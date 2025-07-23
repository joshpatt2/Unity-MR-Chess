using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MRChess.Chess;

namespace MRChess.AI
{
    /// <summary>
    /// Simple chess AI engine using minimax algorithm with alpha-beta pruning
    /// Provides different difficulty levels for the MR chess game
    /// </summary>
    public class ChessAI : MonoBehaviour
    {
        [Header("AI Configuration")]
        [SerializeField] private DifficultyLevel difficulty = DifficultyLevel.Medium;
        [SerializeField] private float thinkingDelay = 1.0f;
        [SerializeField] private bool showThinking = true;
        
        [Header("Search Parameters")]
        [SerializeField] private int maxDepth = 4; // Base search depth, modified by difficulty level
        [SerializeField] private float timeLimit = 5.0f;
        
        // AI state
        private ChessBoard chessBoard;
        private bool isThinking = false;
        private UnityEngine.Coroutine thinkingCoroutine;
        private System.DateTime searchStartTime;
        
        // Events
        public System.Action<ChessMove> OnMoveSelected;
        public System.Action<bool> OnThinkingChanged;
        
        // Position evaluation tables for piece-square values
        private readonly int[,] pawnTable = new int[8, 8]
        {
            { 0,  0,  0,  0,  0,  0,  0,  0},
            {50, 50, 50, 50, 50, 50, 50, 50},
            {10, 10, 20, 30, 30, 20, 10, 10},
            { 5,  5, 10, 25, 25, 10,  5,  5},
            { 0,  0,  0, 20, 20,  0,  0,  0},
            { 5, -5,-10,  0,  0,-10, -5,  5},
            { 5, 10, 10,-20,-20, 10, 10,  5},
            { 0,  0,  0,  0,  0,  0,  0,  0}
        };
        
        private readonly int[,] knightTable = new int[8, 8]
        {
            {-50,-40,-30,-30,-30,-30,-40,-50},
            {-40,-20,  0,  0,  0,  0,-20,-40},
            {-30,  0, 10, 15, 15, 10,  0,-30},
            {-30,  5, 15, 20, 20, 15,  5,-30},
            {-30,  0, 15, 20, 20, 15,  0,-30},
            {-30,  5, 10, 15, 15, 10,  5,-30},
            {-40,-20,  0,  5,  5,  0,-20,-40},
            {-50,-40,-30,-30,-30,-30,-40,-50}
        };
        
        private void Awake()
        {
            chessBoard = FindObjectOfType<ChessBoard>();
        }
        
        /// <summary>
        /// Initialize the AI with chess board reference
        /// </summary>
        public void Initialize(ChessBoard board)
        {
            chessBoard = board;
        }
        
        /// <summary>
        /// Request AI to make a move
        /// </summary>
        public void MakeMove()
        {
            if (isThinking || chessBoard == null) return;
            
            if (thinkingCoroutine != null)
            {
                StopCoroutine(thinkingCoroutine);
            }
            
            thinkingCoroutine = StartCoroutine(ThinkAndMove());
        }
        
        /// <summary>
        /// Coroutine to handle AI thinking with delay
        /// </summary>
        private System.Collections.IEnumerator ThinkAndMove()
        {
            isThinking = true;
            OnThinkingChanged?.Invoke(true);
            
            if (showThinking)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log("AI is thinking...");
#endif
            }
            
            // Add thinking delay for realism
            yield return new UnityEngine.WaitForSeconds(thinkingDelay);
            
            // Calculate best move
            ChessMove bestMove = CalculateBestMove();
            
            isThinking = false;
            OnThinkingChanged?.Invoke(false);
            
            if (bestMove != null)
            {
                OnMoveSelected?.Invoke(bestMove);
                
                if (showThinking)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"AI selected move: {bestMove.GetAlgebraicNotation()}");
#endif
                }
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("AI could not find a valid move");
#endif
            }
        }
        
        /// <summary>
        /// Calculate the best move using minimax algorithm
        /// </summary>
        private ChessMove CalculateBestMove()
        {
            var validMoves = chessBoard.GetValidMoves(PieceColor.Black); // Assuming AI plays as black
            
            if (validMoves.Count == 0) return null;
            
            // Adjust search depth based on difficulty
            int searchDepth = GetSearchDepth();
            
            ChessMove bestMove = null;
            float bestScore = float.MinValue;
            
            // Start time tracking for the time limit
            searchStartTime = System.DateTime.Now;
            
            foreach (var move in validMoves)
            {
                // Check if we've exceeded the time limit
                var elapsed = (System.DateTime.Now - searchStartTime).TotalSeconds;
                if (elapsed >= timeLimit)
                {
                    if (showThinking)
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.Log($"AI reached time limit of {timeLimit}s, returning best move found so far");
#endif
                    }
                    break;
                }
                
                // Make the move temporarily
                var boardCopy = CreateBoardCopy();
                MakeTemporaryMove(boardCopy, move);
                
                // Evaluate the position
                float score = Minimax(boardCopy, searchDepth - 1, float.MinValue, float.MaxValue, false);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }
            
            return bestMove;
        }
        
        /// <summary>
        /// Minimax algorithm with alpha-beta pruning
        /// </summary>
        private float Minimax(ChessPiece[,] board, int depth, float alpha, float beta, bool isMaximizing)
        {
            // Check time limit to avoid infinite search
            var elapsed = (System.DateTime.Now - searchStartTime).TotalSeconds;
            if (elapsed >= timeLimit)
            {
                return EvaluatePosition(board); // Return current evaluation if time limit reached
            }
            
            if (depth == 0)
            {
                return EvaluatePosition(board);
            }
            
            var moves = GetValidMovesForBoard(board, isMaximizing ? PieceColor.Black : PieceColor.White);
            
            if (moves.Count == 0)
            {
                // Game over or stalemate
                if (IsKingInCheckOnBoard(board, isMaximizing ? PieceColor.Black : PieceColor.White))
                {
                    return isMaximizing ? float.MinValue : float.MaxValue; // Checkmate
                }
                return 0; // Stalemate
            }
            
            if (isMaximizing)
            {
                float maxEval = float.MinValue;
                foreach (var move in moves)
                {
                    var boardCopy = CopyBoard(board);
                    MakeTemporaryMove(boardCopy, move);
                    
                    float eval = Minimax(boardCopy, depth - 1, alpha, beta, false);
                    maxEval = Mathf.Max(maxEval, eval);
                    
                    alpha = Mathf.Max(alpha, eval);
                    if (beta <= alpha) break; // Alpha-beta pruning
                }
                return maxEval;
            }
            else
            {
                float minEval = float.MaxValue;
                foreach (var move in moves)
                {
                    var boardCopy = CopyBoard(board);
                    MakeTemporaryMove(boardCopy, move);
                    
                    float eval = Minimax(boardCopy, depth - 1, alpha, beta, true);
                    minEval = Mathf.Min(minEval, eval);
                    
                    beta = Mathf.Min(beta, eval);
                    if (beta <= alpha) break; // Alpha-beta pruning
                }
                return minEval;
            }
        }
        
        /// <summary>
        /// Evaluate board position from AI perspective (positive = good for AI)
        /// </summary>
        private float EvaluatePosition(ChessPiece[,] board)
        {
            float score = 0;
            
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var piece = board[x, y];
                    if (piece != null)
                    {
                        float pieceValue = GetPieceValue(piece, x, y);
                        
                        if (piece.color == PieceColor.Black) // AI color
                        {
                            score += pieceValue;
                        }
                        else
                        {
                            score -= pieceValue;
                        }
                    }
                }
            }
            
            // Add positional bonuses
            score += EvaluatePositionalFactors(board);
            
            return score;
        }
        
        /// <summary>
        /// Get piece value including positional bonus
        /// </summary>
        private float GetPieceValue(ChessPiece piece, int x, int y)
        {
            float baseValue = piece.GetPieceValue() * 100; // Scale up for precision
            float positionalBonus = 0;
            
            // Apply piece-square tables
            int tableY = piece.color == PieceColor.White ? y : 7 - y; // Flip for black
            
            switch (piece.pieceType)
            {
                case PieceType.Pawn:
                    positionalBonus = pawnTable[tableY, x];
                    break;
                case PieceType.Knight:
                    positionalBonus = knightTable[tableY, x];
                    break;
                // Add more piece-square tables as needed
            }
            
            return baseValue + positionalBonus;
        }
        
        /// <summary>
        /// Evaluate positional factors
        /// </summary>
        private float EvaluatePositionalFactors(ChessPiece[,] board)
        {
            float score = 0;
            
            // King safety
            score += EvaluateKingSafety(board, PieceColor.Black) * 0.1f;
            score -= EvaluateKingSafety(board, PieceColor.White) * 0.1f;
            
            // Center control
            score += EvaluateCenterControl(board) * 0.05f;
            
            // Piece mobility
            score += EvaluateMobility(board) * 0.02f;
            
            return score;
        }
        
        /// <summary>
        /// Evaluate king safety
        /// </summary>
        private float EvaluateKingSafety(ChessPiece[,] board, PieceColor color)
        {
            // Simple king safety: count pawns in front of king
            Vector2Int kingPos = FindKingPosition(board, color);
            if (kingPos == Vector2Int.one * -1) return 0;
            
            float safety = 0;
            int direction = color == PieceColor.White ? 1 : -1;
            
            // Check pawns in front of king
            for (int x = kingPos.x - 1; x <= kingPos.x + 1; x++)
            {
                if (x >= 0 && x < 8)
                {
                    int y = kingPos.y + direction;
                    if (y >= 0 && y < 8 && board[x, y]?.pieceType == PieceType.Pawn && board[x, y].color == color)
                    {
                        safety += 10;
                    }
                }
            }
            
            return safety;
        }
        
        /// <summary>
        /// Evaluate center control
        /// </summary>
        private float EvaluateCenterControl(ChessPiece[,] board)
        {
            float score = 0;
            
            // Center squares (d4, d5, e4, e5)
            int[,] centerSquares = { { 3, 3 }, { 3, 4 }, { 4, 3 }, { 4, 4 } };
            
            for (int i = 0; i < centerSquares.GetLength(0); i++)
            {
                int x = centerSquares[i, 0];
                int y = centerSquares[i, 1];
                
                var piece = board[x, y];
                if (piece != null)
                {
                    if (piece.color == PieceColor.Black)
                        score += 5;
                    else
                        score -= 5;
                }
            }
            
            return score;
        }
        
        /// <summary>
        /// Evaluate piece mobility
        /// </summary>
        private float EvaluateMobility(ChessPiece[,] board)
        {
            int blackMobility = GetValidMovesForBoard(board, PieceColor.Black).Count;
            int whiteMobility = GetValidMovesForBoard(board, PieceColor.White).Count;
            
            return blackMobility - whiteMobility;
        }
        
        /// <summary>
        /// Get search depth based on difficulty level
        /// </summary>
        private int GetSearchDepth()
        {
            int baseDepth = difficulty switch
            {
                DifficultyLevel.Easy => Mathf.Max(1, maxDepth - 2),
                DifficultyLevel.Medium => Mathf.Max(2, maxDepth - 1),
                DifficultyLevel.Hard => maxDepth,
                DifficultyLevel.Expert => Mathf.Max(maxDepth, maxDepth + 1),
                _ => Mathf.Max(2, maxDepth - 1)
            };
            
            // Ensure depth is at least 1 and doesn't exceed reasonable limits
            return Mathf.Clamp(baseDepth, 1, 8);
        }
        
        // Helper methods for board manipulation
        private ChessPiece[,] CreateBoardCopy()
        {
            var boardCopy = new ChessPiece[8, 8];
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var piece = chessBoard.GetPiece(x, y);
                    if (piece != null)
                    {
                        boardCopy[x, y] = new ChessPiece(piece.pieceType, piece.color, piece.boardPosition);
                        boardCopy[x, y].hasMoved = piece.hasMoved;
                    }
                }
            }
            return boardCopy;
        }
        
        private ChessPiece[,] CopyBoard(ChessPiece[,] source)
        {
            var copy = new ChessPiece[8, 8];
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (source[x, y] != null)
                    {
                        var piece = source[x, y];
                        copy[x, y] = new ChessPiece(piece.pieceType, piece.color, piece.boardPosition);
                        copy[x, y].hasMoved = piece.hasMoved;
                    }
                }
            }
            return copy;
        }
        
        private void MakeTemporaryMove(ChessPiece[,] board, ChessMove move)
        {
            var piece = board[move.fromPosition.x, move.fromPosition.y];
            board[move.toPosition.x, move.toPosition.y] = piece;
            board[move.fromPosition.x, move.fromPosition.y] = null;
            
            if (piece != null)
            {
                piece.boardPosition = move.toPosition;
                piece.hasMoved = true;
            }
        }
        
        private List<ChessMove> GetValidMovesForBoard(ChessPiece[,] board, PieceColor color)
        {
            var moves = new List<ChessMove>();
            
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var piece = board[x, y];
                    if (piece != null && piece.color == color)
                    {
                        moves.AddRange(GetValidMovesForPieceOnBoard(board, piece));
                    }
                }
            }
            
            return moves;
        }
        
        private List<ChessMove> GetValidMovesForPieceOnBoard(ChessPiece[,] board, ChessPiece piece)
        {
            var moves = new List<ChessMove>();
            
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var targetPos = new Vector2Int(x, y);
                    if (IsValidMoveOnBoard(board, piece.boardPosition, targetPos))
                    {
                        var move = new ChessMove(piece.boardPosition, targetPos, piece.pieceType, piece.color);
                        moves.Add(move);
                    }
                }
            }
            
            return moves;
        }
        
        private bool IsValidMoveOnBoard(ChessPiece[,] board, Vector2Int from, Vector2Int to)
        {
            // Simplified move validation for AI calculations
            var piece = board[from.x, from.y];
            if (piece == null) return false;
            
            var targetPiece = board[to.x, to.y];
            if (targetPiece != null && targetPiece.color == piece.color) return false;
            
            return piece.CanMovePattern(to);
        }
        
        private Vector2Int FindKingPosition(ChessPiece[,] board, PieceColor color)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var piece = board[x, y];
                    if (piece?.pieceType == PieceType.King && piece.color == color)
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }
            return Vector2Int.one * -1;
        }
        
        private bool IsKingInCheckOnBoard(ChessPiece[,] board, PieceColor kingColor)
        {
            Vector2Int kingPos = FindKingPosition(board, kingColor);
            if (kingPos == Vector2Int.one * -1) return false;
            
            // Check if any opponent piece can attack the king
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var piece = board[x, y];
                    if (piece != null && piece.color != kingColor)
                    {
                        if (piece.CanMovePattern(kingPos))
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
        
        // Properties
        public DifficultyLevel Difficulty 
        { 
            get => difficulty; 
            set => difficulty = value; 
        }
        
        public bool IsThinking => isThinking;
    }
    
    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard,
        Expert
    }
}
