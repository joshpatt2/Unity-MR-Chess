using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using MRChess.Chess;

namespace MRChess.MR
{
    /// <summary>
    /// Handles XR interactions for chess pieces using hand tracking and controllers
    /// Integrates with Unity's XR Interaction Toolkit for natural piece manipulation
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class ChessPieceInteractable : MonoBehaviour
    {
        [Header("Chess Piece Reference")]
        [SerializeField] private ChessPiece chessPiece;
        
        [Header("Interaction Settings")]
        [SerializeField] private bool allowGrabbing = true;
        [SerializeField] private bool showValidMoves = true;
        [SerializeField] private float hoverHeight = 0.02f; // Height to lift piece when hovered (in meters)
        [SerializeField] private float moveSpeed = 5.0f;
        
        [Header("Visual Feedback")]
        [SerializeField] private GameObject highlightObject;
        [SerializeField] private Material selectedMaterial;
        [SerializeField] private Material validMoveMaterial;
        [SerializeField] private Material invalidMoveMaterial;
        
        // Components
        private XRGrabInteractable grabInteractable;
        private Rigidbody pieceRigidbody;
        private Collider pieceCollider;
        private Renderer pieceRenderer;
        private Material originalMaterial;
        
        // State
        private Vector3 originalPosition;
        private Vector3 boardPosition;
        private bool isSelected = false;
        private bool isMoving = false;
        private ChessBoard chessBoard;
        
        // Valid moves visualization
        private GameObject[] validMoveIndicators;
        
        // Events
        public System.Action<ChessPieceInteractable> OnPieceSelected;
        public System.Action<ChessPieceInteractable> OnPieceDeselected;
        public System.Action<ChessPieceInteractable, Vector2Int> OnPieceMoved;
        
        private void Awake()
        {
            // Get components
            grabInteractable = GetComponent<XRGrabInteractable>();
            pieceRigidbody = GetComponent<Rigidbody>();
            pieceCollider = GetComponent<Collider>();
            pieceRenderer = GetComponent<Renderer>();
            
            // Store original material
            if (pieceRenderer != null)
            {
                originalMaterial = pieceRenderer.material;
            }
            
            // Set up grab interactable
            if (grabInteractable != null)
            {
                grabInteractable.selectEntered.AddListener(OnGrabStart);
                grabInteractable.selectExited.AddListener(OnGrabEnd);
                grabInteractable.hoverEntered.AddListener(OnHoverStart);
                grabInteractable.hoverExited.AddListener(OnHoverEnd);
            }
        }
        
        private void Start()
        {
            // Find chess board reference
            chessBoard = FindObjectOfType<ChessBoard>();
            
            // Store initial position
            originalPosition = transform.position;
            UpdateBoardPosition();
        }
        
        private void Update()
        {
            if (isMoving)
            {
                UpdateMovement();
            }
        }
        
        /// <summary>
        /// Initialize the chess piece interactable
        /// </summary>
        public void Initialize(ChessPiece piece, ChessBoard board)
        {
            chessPiece = piece;
            chessBoard = board;
            chessPiece.pieceObject = gameObject;
            
            UpdateBoardPosition();
        }
        
        /// <summary>
        /// Update the board position based on chess piece data
        /// </summary>
        private void UpdateBoardPosition()
        {
            if (chessPiece != null && chessBoard != null)
            {
                boardPosition = GetWorldPositionFromBoard(chessPiece.boardPosition);
            }
        }
        
        /// <summary>
        /// Convert board coordinates to world position
        /// </summary>
        private Vector3 GetWorldPositionFromBoard(Vector2Int boardPos)
        {
            if (chessBoard == null) return transform.position;
            
            // Calculate world position based on board size and position
            float squareSize = 0.1f; // Adjust based on your board scale
            Vector3 boardCenter = chessBoard.transform.position;
            
            float x = (boardPos.x - 3.5f) * squareSize;
            float z = (boardPos.y - 3.5f) * squareSize;
            float y = boardCenter.y + 0.05f; // Slightly above board surface
            
            return boardCenter + new Vector3(x, y, z);
        }
        
        /// <summary>
        /// Convert world position to board coordinates
        /// </summary>
        private Vector2Int GetBoardPositionFromWorld(Vector3 worldPos)
        {
            if (chessBoard == null) return Vector2Int.zero;
            
            float squareSize = 0.1f;
            Vector3 boardCenter = chessBoard.transform.position;
            Vector3 localPos = worldPos - boardCenter;
            
            int x = Mathf.RoundToInt(localPos.x / squareSize + 3.5f);
            int z = Mathf.RoundToInt(localPos.z / squareSize + 3.5f);
            
            return new Vector2Int(x, z);
        }
        
        /// <summary>
        /// Called when piece is grabbed
        /// </summary>
        private void OnGrabStart(SelectEnterEventArgs args)
        {
            if (!allowGrabbing) return;
            
            isSelected = true;
            SetHighlight(true);
            
            // Lift piece slightly
            if (pieceRigidbody != null)
            {
                pieceRigidbody.isKinematic = true;
            }
            
            // Show valid moves
            if (showValidMoves)
            {
                ShowValidMoves();
            }
            
            OnPieceSelected?.Invoke(this);
        }
        
        /// <summary>
        /// Called when piece is released
        /// </summary>
        private void OnGrabEnd(SelectExitEventArgs args)
        {
            if (!isSelected) return;
            
            isSelected = false;
            SetHighlight(false);
            
            // Hide valid moves
            HideValidMoves();
            
            // Attempt to place piece
            TryPlacePiece();
            
            OnPieceDeselected?.Invoke(this);
        }
        
        /// <summary>
        /// Called when piece is hovered
        /// </summary>
        private void OnHoverStart(HoverEnterEventArgs args)
        {
            if (!isSelected)
            {
                SetHighlight(true, 0.5f);
                
                // Lift piece slightly when hovered
                Vector3 hoverPosition = originalPosition + Vector3.up * hoverHeight;
                transform.position = hoverPosition;
            }
        }
        
        /// <summary>
        /// Called when piece hover ends
        /// </summary>
        private void OnHoverEnd(HoverExitEventArgs args)
        {
            if (!isSelected)
            {
                SetHighlight(false);
                
                // Return piece to original position
                transform.position = originalPosition;
            }
        }
        
        /// <summary>
        /// Set piece highlight state
        /// </summary>
        private void SetHighlight(bool highlighted, float intensity = 1.0f)
        {
            if (highlightObject != null)
            {
                highlightObject.SetActive(highlighted);
            }
            
            if (pieceRenderer != null && selectedMaterial != null)
            {
                if (highlighted)
                {
                    Color color = selectedMaterial.color;
                    color.a = intensity;
                    selectedMaterial.color = color;
                    pieceRenderer.material = selectedMaterial;
                }
                else
                {
                    pieceRenderer.material = originalMaterial;
                }
            }
        }
        
        /// <summary>
        /// Show valid move indicators
        /// </summary>
        private void ShowValidMoves()
        {
            if (chessPiece == null || chessBoard == null) return;
            
            var validMoves = chessBoard.GetValidMovesForPiece(chessPiece);
            validMoveIndicators = new GameObject[validMoves.Count];
            
            for (int i = 0; i < validMoves.Count; i++)
            {
                Vector3 movePosition = GetWorldPositionFromBoard(validMoves[i].toPosition);
                GameObject indicator = CreateMoveIndicator(movePosition, validMoves[i].isCapture);
                validMoveIndicators[i] = indicator;
            }
        }
        
        /// <summary>
        /// Hide valid move indicators
        /// </summary>
        private void HideValidMoves()
        {
            if (validMoveIndicators != null)
            {
                foreach (var indicator in validMoveIndicators)
                {
                    if (indicator != null)
                    {
                        DestroyImmediate(indicator);
                    }
                }
                validMoveIndicators = null;
            }
        }
        
        /// <summary>
        /// Create visual indicator for valid move
        /// </summary>
        private GameObject CreateMoveIndicator(Vector3 position, bool isCapture)
        {
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicator.transform.position = position;
            indicator.transform.localScale = new Vector3(0.08f, 0.01f, 0.08f);
            
            // Remove collider to avoid interference
            DestroyImmediate(indicator.GetComponent<Collider>());
            
            // Set material based on move type
            Renderer renderer = indicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = isCapture ? invalidMoveMaterial : validMoveMaterial;
            }
            
            return indicator;
        }
        
        /// <summary>
        /// Attempt to place piece at current position
        /// </summary>
        private void TryPlacePiece()
        {
            Vector2Int targetBoardPosition = GetBoardPositionFromWorld(transform.position);
            
            // Validate the move
            if (chessBoard != null && chessBoard.TryMakeMove(chessPiece.boardPosition, targetBoardPosition))
            {
                // Move was successful
                StartMoveToBoardPosition(targetBoardPosition);
                OnPieceMoved?.Invoke(this, targetBoardPosition);
            }
            else
            {
                // Invalid move, return to original position
                StartMoveToPosition(boardPosition);
            }
        }
        
        /// <summary>
        /// Start smooth movement to board position
        /// </summary>
        private void StartMoveToBoardPosition(Vector2Int newBoardPos)
        {
            Vector3 targetWorldPos = GetWorldPositionFromBoard(newBoardPos);
            StartMoveToPosition(targetWorldPos);
            
            // Update board position reference
            boardPosition = targetWorldPos;
        }
        
        /// <summary>
        /// Start smooth movement to world position
        /// </summary>
        private void StartMoveToPosition(Vector3 targetPosition)
        {
            isMoving = true;
            
            // Set rigidbody kinematic during movement
            if (pieceRigidbody != null)
            {
                pieceRigidbody.isKinematic = true;
            }
        }
        
        /// <summary>
        /// Update smooth movement
        /// </summary>
        private void UpdateMovement()
        {
            if (!isMoving) return;
            
            float distance = Vector3.Distance(transform.position, boardPosition);
            
            if (distance < 0.01f)
            {
                // Movement complete
                transform.position = boardPosition;
                originalPosition = boardPosition; // Update original position for hover effects
                isMoving = false;
                
                // Re-enable physics
                if (pieceRigidbody != null)
                {
                    pieceRigidbody.isKinematic = false;
                }
            }
            else
            {
                // Continue movement
                transform.position = Vector3.MoveTowards(transform.position, boardPosition, moveSpeed * Time.deltaTime);
            }
        }
        
        /// <summary>
        /// Force position update (called when move is made programmatically)
        /// </summary>
        public void UpdatePosition()
        {
            UpdateBoardPosition();
            StartMoveToPosition(boardPosition);
        }
        
        // Properties
        public ChessPiece ChessPiece => chessPiece;
        public bool IsSelected => isSelected;
        public bool IsMoving => isMoving;
    }
}
