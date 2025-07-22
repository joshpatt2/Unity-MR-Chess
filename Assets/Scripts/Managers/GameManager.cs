using UnityEngine;
using UnityEngine.UI;
using MRChess.Chess;
using MRChess.MR;

namespace MRChess.Managers
{
    /// <summary>
    /// Main game manager that coordinates chess logic with MR interactions
    /// Implements MVVM pattern for clean separation of concerns
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game References")]
        [SerializeField] private ChessBoard chessBoard;
        [SerializeField] private SpatialAnchorManager anchorManager;
        
        [Header("Piece Prefabs")]
        [SerializeField] private GameObject whitePawnPrefab;
        [SerializeField] private GameObject whiteRookPrefab;
        [SerializeField] private GameObject whiteKnightPrefab;
        [SerializeField] private GameObject whiteBishopPrefab;
        [SerializeField] private GameObject whiteQueenPrefab;
        [SerializeField] private GameObject whiteKingPrefab;
        [SerializeField] private GameObject blackPawnPrefab;
        [SerializeField] private GameObject blackRookPrefab;
        [SerializeField] private GameObject blackKnightPrefab;
        [SerializeField] private GameObject blackBishopPrefab;
        [SerializeField] private GameObject blackQueenPrefab;
        [SerializeField] private GameObject blackKingPrefab;
        
        [Header("UI References")]
        [SerializeField] private Canvas gameUI;
        [SerializeField] private Text currentPlayerText;
        [SerializeField] private Text gameStatusText;
        [SerializeField] private Text moveHistoryText;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button placeBoardButton;
        [SerializeField] private Button undoMoveButton;
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip movePieceSound;
        [SerializeField] private AudioClip captureSound;
        [SerializeField] private AudioClip checkSound;
        [SerializeField] private AudioClip checkmateSound;
        
        // Game state
        private GameState currentGameState = GameState.WaitingForBoard;
        private ChessPieceInteractable[,] pieceInteractables;
        private bool isPlayerTurn = true;
        
        // Events
        public System.Action<GameState> OnGameStateChanged;
        public System.Action<PieceColor> OnPlayerChanged;
        
        private void Awake()
        {
            // Initialize piece interactables array
            pieceInteractables = new ChessPieceInteractable[8, 8];
            
            // Setup UI button listeners
            if (newGameButton != null)
                newGameButton.onClick.AddListener(StartNewGame);
            
            if (placeBoardButton != null)
                placeBoardButton.onClick.AddListener(StartBoardPlacement);
            
            if (undoMoveButton != null)
                undoMoveButton.onClick.AddListener(UndoLastMove);
        }
        
        private void Start()
        {
            // Subscribe to events
            if (chessBoard != null)
            {
                chessBoard.OnMoveMade += OnMoveMade;
                chessBoard.OnCheck += OnCheck;
                chessBoard.OnCheckmate += OnCheckmate;
                chessBoard.OnStalemate += OnStalemate;
            }
            
            if (anchorManager != null)
            {
                anchorManager.OnBoardPlaced += OnBoardPlaced;
                anchorManager.OnBoardRemoved += OnBoardRemoved;
                anchorManager.OnPlacementModeChanged += OnPlacementModeChanged;
            }
            
            // Initialize game state
            UpdateGameState(GameState.WaitingForBoard);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (chessBoard != null)
            {
                chessBoard.OnMoveMade -= OnMoveMade;
                chessBoard.OnCheck -= OnCheck;
                chessBoard.OnCheckmate -= OnCheckmate;
                chessBoard.OnStalemate -= OnStalemate;
            }
            
            if (anchorManager != null)
            {
                anchorManager.OnBoardPlaced -= OnBoardPlaced;
                anchorManager.OnBoardRemoved -= OnBoardRemoved;
                anchorManager.OnPlacementModeChanged -= OnPlacementModeChanged;
            }
        }
        
        /// <summary>
        /// Start a new chess game
        /// </summary>
        public void StartNewGame()
        {
            if (currentGameState == GameState.WaitingForBoard)
            {
                Debug.LogWarning("Cannot start game: Chess board not placed");
                return;
            }
            
            // Reset chess board
            chessBoard.InitializeBoard();
            
            // Clear existing pieces
            ClearAllPieces();
            
            // Create new piece objects
            CreateAllPieces();
            
            // Update game state
            UpdateGameState(GameState.Playing);
            isPlayerTurn = true;
            
            UpdateUI();
            
            Debug.Log("New chess game started");
        }
        
        /// <summary>
        /// Start board placement mode
        /// </summary>
        public void StartBoardPlacement()
        {
            if (anchorManager != null)
            {
                anchorManager.EnterPlacementMode();
            }
        }
        
        /// <summary>
        /// Undo the last move
        /// </summary>
        public void UndoLastMove()
        {
            // TODO: Implement move undo functionality
            Debug.Log("Undo move functionality not yet implemented");
        }
        
        /// <summary>
        /// Create all chess piece objects
        /// </summary>
        private void CreateAllPieces()
        {
            if (chessBoard == null || anchorManager?.BoardInstance == null) return;
            
            Transform boardTransform = anchorManager.BoardInstance.transform;
            
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var piece = chessBoard.GetPiece(x, y);
                    if (piece != null)
                    {
                        GameObject piecePrefab = GetPiecePrefab(piece.pieceType, piece.color);
                        if (piecePrefab != null)
                        {
                            Vector3 worldPosition = GetWorldPositionFromBoard(new Vector2Int(x, y));
                            GameObject pieceObject = Instantiate(piecePrefab, worldPosition, Quaternion.identity, boardTransform);
                            
                            // Setup interactable component
                            var interactable = pieceObject.GetComponent<ChessPieceInteractable>();
                            if (interactable == null)
                            {
                                interactable = pieceObject.AddComponent<ChessPieceInteractable>();
                            }
                            
                            interactable.Initialize(piece, chessBoard);
                            interactable.OnPieceMoved += OnPieceMovedByPlayer;
                            
                            pieceInteractables[x, y] = interactable;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Clear all piece objects
        /// </summary>
        private void ClearAllPieces()
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (pieceInteractables[x, y] != null)
                    {
                        DestroyImmediate(pieceInteractables[x, y].gameObject);
                        pieceInteractables[x, y] = null;
                    }
                }
            }
        }
        
        /// <summary>
        /// Get the appropriate prefab for a piece
        /// </summary>
        private GameObject GetPiecePrefab(PieceType pieceType, PieceColor color)
        {
            return (pieceType, color) switch
            {
                (PieceType.Pawn, PieceColor.White) => whitePawnPrefab,
                (PieceType.Rook, PieceColor.White) => whiteRookPrefab,
                (PieceType.Knight, PieceColor.White) => whiteKnightPrefab,
                (PieceType.Bishop, PieceColor.White) => whiteBishopPrefab,
                (PieceType.Queen, PieceColor.White) => whiteQueenPrefab,
                (PieceType.King, PieceColor.White) => whiteKingPrefab,
                (PieceType.Pawn, PieceColor.Black) => blackPawnPrefab,
                (PieceType.Rook, PieceColor.Black) => blackRookPrefab,
                (PieceType.Knight, PieceColor.Black) => blackKnightPrefab,
                (PieceType.Bishop, PieceColor.Black) => blackBishopPrefab,
                (PieceType.Queen, PieceColor.Black) => blackQueenPrefab,
                (PieceType.King, PieceColor.Black) => blackKingPrefab,
                _ => null
            };
        }
        
        /// <summary>
        /// Convert board coordinates to world position
        /// </summary>
        private Vector3 GetWorldPositionFromBoard(Vector2Int boardPos)
        {
            if (anchorManager?.BoardInstance == null) return Vector3.zero;
            
            float squareSize = 0.1f; // Adjust based on your board scale
            Vector3 boardCenter = anchorManager.BoardInstance.transform.position;
            
            float x = (boardPos.x - 3.5f) * squareSize;
            float z = (boardPos.y - 3.5f) * squareSize;
            float y = boardCenter.y + 0.05f;
            
            return boardCenter + new Vector3(x, y, z);
        }
        
        /// <summary>
        /// Update game state and notify listeners
        /// </summary>
        private void UpdateGameState(GameState newState)
        {
            if (currentGameState != newState)
            {
                currentGameState = newState;
                OnGameStateChanged?.Invoke(newState);
                UpdateUI();
            }
        }
        
        /// <summary>
        /// Update UI elements based on current game state
        /// </summary>
        private void UpdateUI()
        {
            if (currentPlayerText != null)
            {
                string playerText = chessBoard?.CurrentPlayer.ToString() ?? "None";
                currentPlayerText.text = $"Current Player: {playerText}";
            }
            
            if (gameStatusText != null)
            {
                gameStatusText.text = GetGameStatusText();
            }
            
            if (moveHistoryText != null)
            {
                UpdateMoveHistoryUI();
            }
            
            // Update button states
            if (newGameButton != null)
                newGameButton.interactable = currentGameState != GameState.WaitingForBoard;
            
            if (undoMoveButton != null)
                undoMoveButton.interactable = currentGameState == GameState.Playing && chessBoard?.MoveHistory.Count > 0;
        }
        
        /// <summary>
        /// Get game status text
        /// </summary>
        private string GetGameStatusText()
        {
            return currentGameState switch
            {
                GameState.WaitingForBoard => "Place chess board to start",
                GameState.PlacingBoard => "Position the board and tap to place",
                GameState.Playing => chessBoard?.IsGameOver == true ? "Game Over" : "Game in progress",
                GameState.GameOver => $"Game Over - {chessBoard?.Winner} wins!",
                _ => "Unknown state"
            };
        }
        
        /// <summary>
        /// Update move history UI
        /// </summary>
        private void UpdateMoveHistoryUI()
        {
            if (chessBoard == null) return;
            
            var moves = chessBoard.MoveHistory;
            string historyText = "Move History:\n";
            
            for (int i = 0; i < moves.Count; i++)
            {
                if (i % 2 == 0)
                {
                    historyText += $"{(i / 2) + 1}. ";
                }
                
                historyText += moves[i].GetAlgebraicNotation();
                
                if (i % 2 == 0 && i < moves.Count - 1)
                {
                    historyText += " ";
                }
                else
                {
                    historyText += "\n";
                }
            }
            
            moveHistoryText.text = historyText;
        }
        
        // Event handlers
        private void OnBoardPlaced(Vector3 position, Quaternion rotation)
        {
            UpdateGameState(GameState.Playing);
            Debug.Log("Chess board placed, ready to start game");
        }
        
        private void OnBoardRemoved()
        {
            ClearAllPieces();
            UpdateGameState(GameState.WaitingForBoard);
        }
        
        private void OnPlacementModeChanged(bool isPlacing)
        {
            if (isPlacing)
            {
                UpdateGameState(GameState.PlacingBoard);
            }
        }
        
        private void OnMoveMade(ChessMove move)
        {
            // Update piece positions
            UpdatePiecePositions();
            
            // Play sound
            PlayMoveSound(move);
            
            // Update UI
            UpdateUI();
            
            // Check for game over
            if (chessBoard.IsGameOver)
            {
                UpdateGameState(GameState.GameOver);
            }
        }
        
        private void OnCheck(PieceColor playerInCheck)
        {
            PlaySound(checkSound);
            Debug.Log($"{playerInCheck} is in check!");
        }
        
        private void OnCheckmate(PieceColor playerCheckmated)
        {
            PlaySound(checkmateSound);
            UpdateGameState(GameState.GameOver);
            Debug.Log($"Checkmate! {playerCheckmated} loses.");
        }
        
        private void OnStalemate()
        {
            UpdateGameState(GameState.GameOver);
            Debug.Log("Stalemate! Game is a draw.");
        }
        
        private void OnPieceMovedByPlayer(ChessPieceInteractable piece, Vector2Int newPosition)
        {
            // Move was already validated and executed by the chess board
            // This is just for additional UI feedback if needed
        }
        
        /// <summary>
        /// Update all piece positions after a move
        /// </summary>
        private void UpdatePiecePositions()
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var interactable = pieceInteractables[x, y];
                    if (interactable != null)
                    {
                        var piece = chessBoard.GetPiece(x, y);
                        if (piece == null)
                        {
                            // Piece was captured, remove it
                            DestroyImmediate(interactable.gameObject);
                            pieceInteractables[x, y] = null;
                        }
                        else if (piece.boardPosition != new Vector2Int(x, y))
                        {
                            // Piece moved, update its position
                            interactable.UpdatePosition();
                            
                            // Update array
                            pieceInteractables[piece.boardPosition.x, piece.boardPosition.y] = interactable;
                            pieceInteractables[x, y] = null;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Play sound for move
        /// </summary>
        private void PlayMoveSound(ChessMove move)
        {
            AudioClip soundToPlay = move.isCapture ? captureSound : movePieceSound;
            PlaySound(soundToPlay);
        }
        
        /// <summary>
        /// Play audio clip
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        // Properties
        public GameState CurrentGameState => currentGameState;
        public bool IsPlayerTurn => isPlayerTurn;
        public ChessBoard ChessBoard => chessBoard;
    }
    
    public enum GameState
    {
        WaitingForBoard,
        PlacingBoard,
        Playing,
        GameOver
    }
}
