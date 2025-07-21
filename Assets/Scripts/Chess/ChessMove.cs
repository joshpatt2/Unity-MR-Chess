using UnityEngine;
using System.Collections.Generic;

namespace MRChess.Chess
{
    /// <summary>
    /// Represents a chess move with validation and metadata
    /// </summary>
    [System.Serializable]
    public class ChessMove
    {
        [Header("Move Information")]
        public Vector2Int fromPosition;
        public Vector2Int toPosition;
        public PieceType movingPiece;
        public PieceColor playerColor;
        
        [Header("Special Moves")]
        public bool isCapture;
        public bool isCastling;
        public bool isEnPassant;
        public bool isPromotion;
        public PieceType promotionPiece = PieceType.Queen;
        
        [Header("Game State")]
        public bool causesCheck;
        public bool causesCheckmate;
        public ChessPiece capturedPiece;
        
        public ChessMove(Vector2Int from, Vector2Int to, PieceType piece, PieceColor color)
        {
            fromPosition = from;
            toPosition = to;
            movingPiece = piece;
            playerColor = color;
        }
        
        /// <summary>
        /// Get algebraic notation for this move (e.g., "e4", "Nf3", "O-O")
        /// </summary>
        public string GetAlgebraicNotation()
        {
            if (isCastling)
            {
                return toPosition.x > fromPosition.x ? "O-O" : "O-O-O";
            }
            
            string notation = "";
            
            // Piece notation (except pawns)
            if (movingPiece != PieceType.Pawn)
            {
                notation += GetPieceNotation(movingPiece);
            }
            
            // Capture notation
            if (isCapture)
            {
                if (movingPiece == PieceType.Pawn)
                {
                    notation += GetFileNotation(fromPosition.x);
                }
                notation += "x";
            }
            
            // Destination square
            notation += GetSquareNotation(toPosition);
            
            // Promotion
            if (isPromotion)
            {
                notation += "=" + GetPieceNotation(promotionPiece);
            }
            
            // Check/Checkmate
            if (causesCheckmate)
            {
                notation += "#";
            }
            else if (causesCheck)
            {
                notation += "+";
            }
            
            return notation;
        }
        
        private string GetPieceNotation(PieceType piece)
        {
            return piece switch
            {
                PieceType.King => "K",
                PieceType.Queen => "Q",
                PieceType.Rook => "R",
                PieceType.Bishop => "B",
                PieceType.Knight => "N",
                _ => ""
            };
        }
        
        private string GetSquareNotation(Vector2Int position)
        {
            char file = (char)('a' + position.x);
            int rank = position.y + 1;
            return $"{file}{rank}";
        }
        
        private string GetFileNotation(int file)
        {
            return ((char)('a' + file)).ToString();
        }
        
        /// <summary>
        /// Create a copy of this move
        /// </summary>
        public ChessMove Clone()
        {
            var move = new ChessMove(fromPosition, toPosition, movingPiece, playerColor)
            {
                isCapture = isCapture,
                isCastling = isCastling,
                isEnPassant = isEnPassant,
                isPromotion = isPromotion,
                promotionPiece = promotionPiece,
                causesCheck = causesCheck,
                causesCheckmate = causesCheckmate,
                capturedPiece = capturedPiece
            };
            return move;
        }
    }
    
    /// <summary>
    /// Collection of moves with utility functions
    /// </summary>
    public class MoveList : List<ChessMove>
    {
        /// <summary>
        /// Find move from position to position
        /// </summary>
        public ChessMove FindMove(Vector2Int from, Vector2Int to)
        {
            return Find(move => move.fromPosition == from && move.toPosition == to);
        }
        
        /// <summary>
        /// Get all moves for a specific piece type
        /// </summary>
        public List<ChessMove> GetMovesForPiece(PieceType pieceType)
        {
            return FindAll(move => move.movingPiece == pieceType);
        }
        
        /// <summary>
        /// Get all capture moves
        /// </summary>
        public List<ChessMove> GetCaptureMoves()
        {
            return FindAll(move => move.isCapture);
        }
        
        /// <summary>
        /// Get moves that put opponent in check
        /// </summary>
        public List<ChessMove> GetCheckMoves()
        {
            return FindAll(move => move.causesCheck);
        }
    }
}
