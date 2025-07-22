using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MRChess.Setup
{
    /// <summary>
    /// Helper script to automatically set up XR components in the scene
    /// Attach this to an empty GameObject to auto-configure XR setup
    /// </summary>
    public class XRSceneSetup : MonoBehaviour
    {
        [Header("Auto Setup Options")]
        [SerializeField] private bool setupXROrigin = true;
        [SerializeField] private bool setupInteractionManager = true;
        [SerializeField] private bool setupControllers = true;
        [SerializeField] private bool setupHandTracking = true;

        [Header("Prefab References")]
        [SerializeField] private GameObject xrOriginPrefab;
        [SerializeField] private GameObject leftControllerPrefab;
        [SerializeField] private GameObject rightControllerPrefab;

        private void Start()
        {
            if (Application.isEditor)
            {
                SetupXRComponents();
            }
        }

        [ContextMenu("Setup XR Components")]
        public void SetupXRComponents()
        {
            Debug.Log("Setting up XR components for MR Chess...");

            if (setupInteractionManager)
            {
                SetupXRInteractionManager();
            }

            if (setupXROrigin)
            {
                SetupXROriginRig();
            }

            if (setupControllers)
            {
                SetupControllers();
            }

            Debug.Log("XR setup complete!");
        }

        private void SetupXRInteractionManager()
        {
            if (FindObjectOfType<XRInteractionManager>() == null)
            {
                GameObject managerGO = new GameObject("XR Interaction Manager");
                managerGO.AddComponent<XRInteractionManager>();
                Debug.Log("Created XR Interaction Manager");
            }
        }

        private void SetupXROriginRig()
        {
            if (GameObject.Find("XR Origin") == null)
            {
                // Create XR Origin
                GameObject xrOrigin = new GameObject("XR Origin");
                var xrOriginComponent = xrOrigin.AddComponent<XROrigin>();

                // Create Camera Offset
                GameObject cameraOffset = new GameObject("Camera Offset");
                cameraOffset.transform.SetParent(xrOrigin.transform);
                xrOriginComponent.CameraFloorOffsetObject = cameraOffset;

                // Setup Main Camera
                Camera mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    GameObject cameraGO = new GameObject("Main Camera");
                    cameraGO.transform.SetParent(cameraOffset.transform);
                    mainCamera = cameraGO.AddComponent<Camera>();
                    cameraGO.tag = "MainCamera";
                }
                else
                {
                    mainCamera.transform.SetParent(cameraOffset.transform);
                }

                // Add XR Camera components
                if (mainCamera.GetComponent<TrackedPoseDriver>() == null)
                {
                    mainCamera.gameObject.AddComponent<TrackedPoseDriver>();
                }

                xrOriginComponent.Camera = mainCamera;

                Debug.Log("Created XR Origin rig");
            }
        }

        private void SetupControllers()
        {
            GameObject xrOrigin = GameObject.Find("XR Origin");
            if (xrOrigin == null) return;

            Transform cameraOffset = xrOrigin.transform.Find("Camera Offset");
            if (cameraOffset == null) return;

            // Create controller anchors
            if (cameraOffset.Find("LeftHand Controller") == null)
            {
                CreateControllerAnchor("LeftHand Controller", cameraOffset);
            }

            if (cameraOffset.Find("RightHand Controller") == null)
            {
                CreateControllerAnchor("RightHand Controller", cameraOffset);
            }
        }

        private void CreateControllerAnchor(string controllerName, Transform parent)
        {
            GameObject controller = new GameObject(controllerName);
            controller.transform.SetParent(parent);

            // Add basic controller components
            controller.AddComponent<XRController>();
            controller.AddComponent<XRRayInteractor>();
            controller.AddComponent<XRInteractorLineVisual>();
            controller.AddComponent<LineRenderer>();

            Debug.Log($"Created {controllerName}");
        }

        [ContextMenu("Create Chess Scene Structure")]
        public void CreateChessSceneStructure()
        {
            // Create main game structure
            CreateGameObjectIfNotExists("--- GAME STRUCTURE ---", null);
            CreateGameObjectIfNotExists("GameManager", null);
            CreateGameObjectIfNotExists("ChessBoard", null);
            CreateGameObjectIfNotExists("Chess Pieces", null);
            CreateGameObjectIfNotExists("UI Canvas", null);
            CreateGameObjectIfNotExists("Spatial Anchors", null);

            Debug.Log("Chess scene structure created!");
        }

        private GameObject CreateGameObjectIfNotExists(string name, Transform parent)
        {
            GameObject existing = GameObject.Find(name);
            if (existing == null)
            {
                GameObject newGO = new GameObject(name);
                if (parent != null)
                {
                    newGO.transform.SetParent(parent);
                }
                return newGO;
            }
            return existing;
        }
    }
}
