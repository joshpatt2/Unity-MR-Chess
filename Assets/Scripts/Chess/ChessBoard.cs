using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MRChess.Chess
{
    /// <summary>
    /// Core chess board logic with move validation and game state management
    /// </summary>
    public class ChessBoard : MonoBehaviour
    {
        [Header("Board Configuration")]
        [SerializeField] private int boardSize = 8;
        [SerializeField] private ChessPiece[,] board;
        [SerializeField] private PieceColor currentPlayer = PieceColor.White;
        
        [Header("Game State")]
        [SerializeField] private bool isGameOver = false;
        [SerializeField] private PieceColor winner = PieceColor.White;
        [SerializeField] private List<ChessMove> moveHistory = new List<ChessMove>();
        
        [Header("Special Rules")]
        [SerializeField] private Vector2Int enPassantTarget = new Vector2Int(-1, -1);
        [SerializeField] private bool whiteKingMoved = false;
        [SerializeField] private bool blackKingMoved = false;
        [SerializeField] private bool whiteKingSideRookMoved = false;
        [SerializeField] private bool whiteQueenSideRookMoved = false;
        [SerializeField] private bool blackKingSideRookMoved = false;
        [SerializeField] private bool blackQueenSideRookMoved = false;
        
        // Events
        public System.Action<ChessMove> OnMoveMade;
        public System.Action<PieceColor> OnCheck;
        public System.Action<PieceColor> OnCheckmate;
        public System.Action OnStalemate;
        
        private void Awake()
        {
            InitializeBoard();
        }
        
        /// <summary>
        /// Initialize the chess board with starting positions
        /// </summary>
        public void InitializeBoard()
        {
            board = new ChessPiece[boardSize, boardSize];
            SetupStartingPosition();
            currentPlayer = PieceColor.White;
            isGameOver = false;
            moveHistory.Clear();
        }
        
        /// <summary>
        /// Set up the standard chess starting position
        /// </summary>
        private void SetupStartingPosition()
        {
            // Clear board
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    board[x, y] = null;
                }
            }
            
            // White pieces
            PlacePiece(new ChessPiece(PieceType.Rook, PieceColor.White, new Vector2Int(0, 0)), 0, 0);
            PlacePiece(new ChessPiece(PieceType.Knight, PieceColor.White, new Vector2Int(1, 0)), 1, 0);
            PlacePiece(new ChessPiece(PieceType.Bishop, PieceColor.White, new Vector2Int(2, 0)), 2, 0);
            PlacePiece(new ChessPiece(PieceType.Queen, PieceColor.White, new Vector2Int(3, 0)), 3, 0);
            PlacePiece(new ChessPiece(PieceType.King, PieceColor.White, new Vector2Int(4, 0)), 4, 0);
            PlacePiece(new ChessPiece(PieceType.Bishop, PieceColor.White, new Vector2Int(5, 0)), 5, 0);
            PlacePiece(new ChessPiece(PieceType.Knight, PieceColor.White, new Vector2Int(6, 0)), 6, 0);
            PlacePiece(new ChessPiece(PieceType.Rook, PieceColor.White, new Vector2Int(7, 0)), 7, 0);
            
            for (int x = 0; x < boardSize; x++)
            {
                PlacePiece(new ChessPiece(PieceType.Pawn, PieceColor.White, new Vector2Int(x, 1)), x, 1);
            }
            
            // Black pieces
            PlacePiece(new ChessPiece(PieceType.Rook, PieceColor.Black, new Vector2Int(0, 7)), 0, 7);
            PlacePiece(new ChessPiece(PieceType.Knight, PieceColor.Black, new Vector2Int(1, 7)), 1, 7);
            PlacePiece(new ChessPiece(PieceType.Bishop, PieceColor.Black, new Vector2Int(2, 7)), 2, 7);
            PlacePiece(new ChessPiece(PieceType.Queen, PieceColor.Black, new Vector2Int(3, 7)), 3, 7);
            PlacePiece(new ChessPiece(PieceType.King, PieceColor.Black, new Vector2Int(4, 7)), 4, 7);
            PlacePiece(new ChessPiece(PieceType.Bishop, PieceColor.Black, new Vector2Int(5, 7)), 5, 7);
            PlacePiece(new ChessPiece(PieceType.Knight, PieceColor.Black, new Vector2Int(6, 7)), 6, 7);
            PlacePiece(new ChessPiece(PieceType.Rook, PieceColor.Black, new Vector2Int(7, 7)), 7, 7);
            
            for (int x = 0; x < boardSize; x++)
            {
                PlacePiece(new ChessPiece(PieceType.Pawn, PieceColor.Black, new Vector2Int(x, 6)), x, 6);
            }
        }
        
        /// <summary>
        /// Place a piece on the board
        /// </summary>
        private void PlacePiece(ChessPiece piece, int x, int y)
        {
            if (IsValidPosition(x, y))
            {
                board[x, y] = piece;
                piece.boardPosition = new Vector2Int(x, y);
            }
        }
        
        /// <summary>
        /// Get piece at position
        /// </summary>
        public ChessPiece GetPiece(Vector2Int position)
        {
            return GetPiece(position.x, position.y);
        }
        
        public ChessPiece GetPiece(int x, int y)
        {
            if (IsValidPosition(x, y))
            {
                return board[x, y];
            }
            return null;
        }
        
        /// <summary>
        /// Check if position is valid on board
        /// </summary>
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < boardSize && y >= 0 && y < boardSize;
        }
        
        public bool IsValidPosition(Vector2Int position)
        {
            return IsValidPosition(position.x, position.y);
        }
        
        /// <summary>
        /// Attempt to make a move
        /// </summary>
        public bool TryMakeMove(Vector2Int from, Vector2Int to)
        {
            var move = new ChessMove(from, to, PieceType.None, currentPlayer);
            
            if (IsValidMove(move))
            {
                ExecuteMove(move);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Check if a move is valid
        /// </summary>
        public bool IsValidMove(ChessMove move)
        {
            var piece = GetPiece(move.fromPosition);
            
            // Basic validation
            if (piece == null || piece.color != currentPlayer)
                return false;
            
            if (!IsValidPosition(move.toPosition))
                return false;
            
            var targetPiece = GetPiece(move.toPosition);
            if (targetPiece != null && targetPiece.color == piece.color)
                return false;
            
            // Check piece movement pattern
            if (!piece.CanMovePattern(move.toPosition))
                return false;
            
            // Check if path is clear (except for knights)
            if (piece.pieceType != PieceType.Knight && !IsPathClear(move.fromPosition, move.toPosition))
                return false;
            
            // Check if move would put own king in check
            if (WouldMoveExposeKing(move))
                return false;
            
            return true;
        }
        
        /// <summary>
        /// Check if path between two positions is clear
        /// </summary>
        private bool IsPathClear(Vector2Int from, Vector2Int to)
        {
            Vector2Int direction = new Vector2Int(
                System.Math.Sign(to.x - from.x),
                System.Math.Sign(to.y - from.y)
            );
            
            Vector2Int current = from + direction;
            
            while (current != to)
            {
                if (GetPiece(current) != null)
                    return false;
                
                current += direction;
            }
            
            return true;
        }
        
        /// <summary>
        /// Check if making this move would expose own king to check
        /// </summary>
        private bool WouldMoveExposeKing(ChessMove move)
        {
            // Temporarily make the move
            var movingPiece = GetPiece(move.fromPosition);
            var capturedPiece = GetPiece(move.toPosition);
            
            board[move.toPosition.x, move.toPosition.y] = movingPiece;
            board[move.fromPosition.x, move.fromPosition.y] = null;
            movingPiece.boardPosition = move.toPosition;
            
            // Check if king is in check
            bool kingInCheck = IsKingInCheck(currentPlayer);
            
            // Undo the move
            board[move.fromPosition.x, move.fromPosition.y] = movingPiece;
            board[move.toPosition.x, move.toPosition.y] = capturedPiece;
            movingPiece.boardPosition = move.fromPosition;
            
            return kingInCheck;
        }
        
        /// <summary>
        /// Check if the king of specified color is in check
        /// </summary>
        public bool IsKingInCheck(PieceColor kingColor)
        {
            Vector2Int kingPosition = FindKing(kingColor);
            if (kingPosition == new Vector2Int(-1, -1))
                return false;
            
            // Check if any opponent piece can attack the king
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    var piece = GetPiece(x, y);
                    if (piece != null && piece.color != kingColor)
                    {
                        if (CanPieceAttackSquare(piece, kingPosition))
                            return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Find the king of specified color
        /// </summary>
        private Vector2Int FindKing(PieceColor color)
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    var piece = GetPiece(x, y);
                    if (piece != null && piece.pieceType == PieceType.King && piece.color == color)
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }
            return new Vector2Int(-1, -1);
        }
        
        /// <summary>
        /// Check if piece can attack specified square
        /// </summary>
        private bool CanPieceAttackSquare(ChessPiece piece, Vector2Int targetSquare)
        {
            if (!piece.CanMovePattern(targetSquare))
                return false;
            
            if (piece.pieceType != PieceType.Knight && !IsPathClear(piece.boardPosition, targetSquare))
                return false;
            
            return true;
        }
        
        /// <summary>
        /// Execute a validated move
        /// </summary>
        private void ExecuteMove(ChessMove move)
        {
            var movingPiece = GetPiece(move.fromPosition);
            var capturedPiece = GetPiece(move.toPosition);
            
            // Update move information
            move.movingPiece = movingPiece.pieceType;
            move.isCapture = capturedPiece != null;
            move.capturedPiece = capturedPiece;
            
            // Move the piece
            board[move.toPosition.x, move.toPosition.y] = movingPiece;
            board[move.fromPosition.x, move.fromPosition.y] = null;
            movingPiece.boardPosition = move.toPosition;
            movingPiece.hasMoved = true;
            
            // Update castling rights
            UpdateCastlingRights(move);
            
            // Check for check/checkmate
            var opponentColor = currentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White;
            move.causesCheck = IsKingInCheck(opponentColor);
            
            if (move.causesCheck)
            {
                move.causesCheckmate = IsCheckmate(opponentColor);
                OnCheck?.Invoke(opponentColor);
                
                if (move.causesCheckmate)
                {
                    isGameOver = true;
                    winner = currentPlayer;
                    OnCheckmate?.Invoke(opponentColor);
                }
            }
            else if (IsStalemate(opponentColor))
            {
                isGameOver = true;
                OnStalemate?.Invoke();
            }
            
            // Add to move history
            moveHistory.Add(move);
            
            // Switch players
            currentPlayer = opponentColor;
            
            // Invoke move event
            OnMoveMade?.Invoke(move);
        }
        
        /// <summary>
        /// Update castling rights based on move
        /// </summary>
        private void UpdateCastlingRights(ChessMove move)
        {
            var piece = GetPiece(move.toPosition);
            
            if (piece.pieceType == PieceType.King)
            {
                if (piece.color == PieceColor.White)
                    whiteKingMoved = true;
                else
                    blackKingMoved = true;
            }
            else if (piece.pieceType == PieceType.Rook)
            {
                if (piece.color == PieceColor.White)
                {
                    if (move.fromPosition.x == 0) whiteQueenSideRookMoved = true;
                    if (move.fromPosition.x == 7) whiteKingSideRookMoved = true;
                }
                else
                {
                    if (move.fromPosition.x == 0) blackQueenSideRookMoved = true;
                    if (move.fromPosition.x == 7) blackKingSideRookMoved = true;
                }
            }
        }
        
        /// <summary>
        /// Check if castling is possible for the given color and side
        /// </summary>
        public bool CanCastle(PieceColor color, bool kingSide)
        {
            // Check if king has moved
            if (color == PieceColor.White && whiteKingMoved) return false;
            if (color == PieceColor.Black && blackKingMoved) return false;
            
            // Check if the relevant rook has moved
            if (color == PieceColor.White)
            {
                if (kingSide && whiteKingSideRookMoved) return false;
                if (!kingSide && whiteQueenSideRookMoved) return false;
            }
            else
            {
                if (kingSide && blackKingSideRookMoved) return false;
                if (!kingSide && blackQueenSideRookMoved) return false;
            }
            
            // Additional castling checks (squares between king and rook must be empty, etc.)
            // would be implemented here in a full chess implementation
            
            return true;
        }
        
        /// <summary>
        /// Check if player is in checkmate
        /// </summary>
        public bool IsCheckmate(PieceColor playerColor)
        {
            if (!IsKingInCheck(playerColor))
                return false;
            
            return GetValidMoves(playerColor).Count == 0;
        }
        
        /// <summary>
        /// Check if player is in stalemate
        /// </summary>
        public bool IsStalemate(PieceColor playerColor)
        {
            if (IsKingInCheck(playerColor))
                return false;
            
            return GetValidMoves(playerColor).Count == 0;
        }
        
        /// <summary>
        /// Get all valid moves for a player
        /// </summary>
        public MoveList GetValidMoves(PieceColor playerColor)
        {
            var moves = new MoveList();
            
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    var piece = GetPiece(x, y);
                    if (piece != null && piece.color == playerColor)
                    {
                        moves.AddRange(GetValidMovesForPiece(piece));
                    }
                }
            }
            
            return moves;
        }
        
        /// <summary>
        /// Get valid moves for a specific piece
        /// </summary>
        public List<ChessMove> GetValidMovesForPiece(ChessPiece piece)
        {
            var moves = new List<ChessMove>();
            
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    var targetPosition = new Vector2Int(x, y);
                    var move = new ChessMove(piece.boardPosition, targetPosition, piece.pieceType, piece.color);
                    
                    if (IsValidMove(move))
                    {
                        moves.Add(move);
                    }
                }
            }
            
            return moves;
        }
        
        // Properties
        public PieceColor CurrentPlayer => currentPlayer;
        public bool IsGameOver => isGameOver;
        public PieceColor Winner => winner;
        public List<ChessMove> MoveHistory => moveHistory.ToList();
        public int BoardSize => boardSize;
    }
}
