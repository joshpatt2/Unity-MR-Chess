using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace MRChess.MR
{
    /// <summary>
    /// Manages spatial anchoring for the chess board in mixed reality
    /// Handles persistent placement and tracking of the board in real-world space
    /// </summary>
    [RequireComponent(typeof(ARAnchorManager))]
    public class SpatialAnchorManager : MonoBehaviour
    {
        [Header("Anchor Configuration")]
        [SerializeField] private GameObject chessBoardPrefab;
        [SerializeField] private float anchorRadius = 0.1f;
        [SerializeField] private LayerMask anchorableLayers = -1;
        
        [Header("Placement Settings")]
        [SerializeField] private bool autoPlaceOnStart = false;
        [SerializeField] private Vector3 defaultPosition = new Vector3(0, 0, 2);
        [SerializeField] private Vector3 defaultRotation = new Vector3(0, 0, 0);
        
        [Header("Persistence")]
        [SerializeField] private bool saveBoardPosition = true;
        [SerializeField] private string anchorKey = "ChessBoardAnchor";
        
        // Components
        private ARAnchorManager anchorManager;
        private ARRaycastManager raycastManager;
        private Camera arCamera;
        
        // State
        private ARAnchor currentAnchor;
        private GameObject boardInstance;
        private bool isPlacementMode = false;
        
        // Events
        public System.Action<Vector3, Quaternion> OnBoardPlaced;
        public System.Action OnBoardRemoved;
        public System.Action<bool> OnPlacementModeChanged;
        
        private void Awake()
        {
            anchorManager = GetComponent<ARAnchorManager>();
            raycastManager = GetComponent<ARRaycastManager>();
            arCamera = Camera.main;
            
            if (arCamera == null)
                arCamera = FindObjectOfType<Camera>();
        }
        
        private void Start()
        {
            if (autoPlaceOnStart)
            {
                PlaceBoardAtDefault();
            }
            else
            {
                LoadSavedBoardPosition();
            }
        }
        
        private void Update()
        {
            if (isPlacementMode)
            {
                HandlePlacementInput();
            }
        }
        
        /// <summary>
        /// Enter placement mode to position the chess board
        /// </summary>
        public void EnterPlacementMode()
        {
            isPlacementMode = true;
            OnPlacementModeChanged?.Invoke(true);
            
            // Remove existing board if present
            if (boardInstance != null)
            {
                RemoveBoard();
            }
            
            Debug.Log("Entering board placement mode. Look at a flat surface and tap to place.");
        }
        
        /// <summary>
        /// Exit placement mode
        /// </summary>
        public void ExitPlacementMode()
        {
            isPlacementMode = false;
            OnPlacementModeChanged?.Invoke(false);
        }
        
        /// <summary>
        /// Handle input during placement mode
        /// </summary>
        private void HandlePlacementInput()
        {
            // Check for touch input (Quest hand tracking or controller)
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Vector2 touchPosition = Input.GetTouch(0).position;
                TryPlaceBoardAtScreenPosition(touchPosition);
            }
            
            // Check for mouse input (for testing in editor)
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePosition = Input.mousePosition;
                TryPlaceBoardAtScreenPosition(mousePosition);
            }
        }
        
        /// <summary>
        /// Attempt to place board at screen position using raycast
        /// </summary>
        private void TryPlaceBoardAtScreenPosition(Vector2 screenPosition)
        {
            if (raycastManager == null) return;
            
            var hits = new List<ARRaycastHit>();
            
            // Raycast against planes (tables, floors, etc.)
            if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                var hit = hits[0];
                PlaceBoardAtPose(hit.pose);
            }
            else
            {
                // Fallback: place at fixed distance from camera
                Ray ray = arCamera.ScreenPointToRay(screenPosition);
                Vector3 position = ray.origin + ray.direction * 2.0f;
                Pose pose = new Pose(position, Quaternion.LookRotation(Vector3.forward, Vector3.up));
                PlaceBoardAtPose(pose);
            }
        }
        
        /// <summary>
        /// Place chess board at specified pose with spatial anchor
        /// </summary>
        public void PlaceBoardAtPose(Pose pose)
        {
            // Remove existing board
            RemoveBoard();
            
            // Create anchor
            currentAnchor = anchorManager.AddAnchor(pose);
            
            if (currentAnchor != null)
            {
                // Instantiate board at anchor position
                boardInstance = Instantiate(chessBoardPrefab, currentAnchor.transform);
                boardInstance.transform.localPosition = Vector3.zero;
                boardInstance.transform.localRotation = Quaternion.identity;
                
                // Save position for persistence
                if (saveBoardPosition)
                {
                    SaveBoardPosition(pose);
                }
                
                // Exit placement mode
                ExitPlacementMode();
                
                // Notify listeners
                OnBoardPlaced?.Invoke(pose.position, pose.rotation);
                
                Debug.Log($"Chess board placed at {pose.position}");
            }
            else
            {
                Debug.LogWarning("Failed to create spatial anchor for chess board");
            }
        }
        
        /// <summary>
        /// Place board at default position (for testing)
        /// </summary>
        public void PlaceBoardAtDefault()
        {
            Vector3 position = arCamera.transform.position + arCamera.transform.forward * defaultPosition.magnitude;
            Quaternion rotation = Quaternion.Euler(defaultRotation) * arCamera.transform.rotation;
            
            Pose pose = new Pose(position, rotation);
            PlaceBoardAtPose(pose);
        }
        
        /// <summary>
        /// Remove current chess board and anchor
        /// </summary>
        public void RemoveBoard()
        {
            if (boardInstance != null)
            {
                DestroyImmediate(boardInstance);
                boardInstance = null;
            }
            
            if (currentAnchor != null)
            {
                anchorManager.RemoveAnchor(currentAnchor);
                currentAnchor = null;
            }
            
            OnBoardRemoved?.Invoke();
        }
        
        /// <summary>
        /// Save board position for persistence across sessions
        /// </summary>
        private void SaveBoardPosition(Pose pose)
        {
            PlayerPrefs.SetFloat($"{anchorKey}_PosX", pose.position.x);
            PlayerPrefs.SetFloat($"{anchorKey}_PosY", pose.position.y);
            PlayerPrefs.SetFloat($"{anchorKey}_PosZ", pose.position.z);
            PlayerPrefs.SetFloat($"{anchorKey}_RotX", pose.rotation.x);
            PlayerPrefs.SetFloat($"{anchorKey}_RotY", pose.rotation.y);
            PlayerPrefs.SetFloat($"{anchorKey}_RotZ", pose.rotation.z);
            PlayerPrefs.SetFloat($"{anchorKey}_RotW", pose.rotation.w);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Load saved board position
        /// </summary>
        private void LoadSavedBoardPosition()
        {
            if (PlayerPrefs.HasKey($"{anchorKey}_PosX"))
            {
                Vector3 position = new Vector3(
                    PlayerPrefs.GetFloat($"{anchorKey}_PosX"),
                    PlayerPrefs.GetFloat($"{anchorKey}_PosY"),
                    PlayerPrefs.GetFloat($"{anchorKey}_PosZ")
                );
                
                Quaternion rotation = new Quaternion(
                    PlayerPrefs.GetFloat($"{anchorKey}_RotX"),
                    PlayerPrefs.GetFloat($"{anchorKey}_RotY"),
                    PlayerPrefs.GetFloat($"{anchorKey}_RotZ"),
                    PlayerPrefs.GetFloat($"{anchorKey}_RotW")
                );
                
                Pose savedPose = new Pose(position, rotation);
                PlaceBoardAtPose(savedPose);
                
                Debug.Log("Loaded saved chess board position");
            }
        }
        
        /// <summary>
        /// Clear saved board position
        /// </summary>
        public void ClearSavedPosition()
        {
            PlayerPrefs.DeleteKey($"{anchorKey}_PosX");
            PlayerPrefs.DeleteKey($"{anchorKey}_PosY");
            PlayerPrefs.DeleteKey($"{anchorKey}_PosZ");
            PlayerPrefs.DeleteKey($"{anchorKey}_RotX");
            PlayerPrefs.DeleteKey($"{anchorKey}_RotY");
            PlayerPrefs.DeleteKey($"{anchorKey}_RotZ");
            PlayerPrefs.DeleteKey($"{anchorKey}_RotW");
            PlayerPrefs.Save();
        }
        
        // Properties
        public bool IsPlacementMode => isPlacementMode;
        public bool HasBoard => boardInstance != null;
        public GameObject BoardInstance => boardInstance;
        public Vector3 BoardPosition => boardInstance != null ? boardInstance.transform.position : Vector3.zero;
        public Quaternion BoardRotation => boardInstance != null ? boardInstance.transform.rotation : Quaternion.identity;
    }
}
