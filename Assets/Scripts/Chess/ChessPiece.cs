using UnityEngine;

namespace MRChess.Chess
{
    /// <summary>
    /// Represents a chess piece with its type, color, and position
    /// </summary>
    [System.Serializable]
    public class ChessPiece
    {
        [Header("Piece Properties")]
        public PieceType pieceType;
        public PieceColor color;
        public Vector2Int boardPosition;
        
        [Header("3D Representation")]
        public GameObject pieceObject;
        public bool hasMoved = false;
        
        public ChessPiece(PieceType type, PieceColor pieceColor, Vector2Int position)
        {
            pieceType = type;
            color = pieceColor;
            boardPosition = position;
            hasMoved = false;
        }
        
        /// <summary>
        /// Get the value of this piece for AI evaluation
        /// </summary>
        public int GetPieceValue()
        {
            return pieceType switch
            {
                PieceType.Pawn => 1,
                PieceType.Knight => 3,
                PieceType.Bishop => 3,
                PieceType.Rook => 5,
                PieceType.Queen => 9,
                PieceType.King => 100,
                _ => 0
            };
        }
        
        /// <summary>
        /// Check if this piece can potentially move to target position
        /// (basic movement pattern, doesn't check for obstacles)
        /// </summary>
        public bool CanMovePattern(Vector2Int targetPosition)
        {
            Vector2Int delta = targetPosition - boardPosition;
            int deltaX = Mathf.Abs(delta.x);
            int deltaY = Mathf.Abs(delta.y);
            
            return pieceType switch
            {
                PieceType.Pawn => CanPawnMove(delta),
                PieceType.Rook => deltaX == 0 || deltaY == 0,
                PieceType.Bishop => deltaX == deltaY,
                PieceType.Knight => (deltaX == 2 && deltaY == 1) || (deltaX == 1 && deltaY == 2),
                PieceType.Queen => deltaX == 0 || deltaY == 0 || deltaX == deltaY,
                PieceType.King => deltaX <= 1 && deltaY <= 1,
                _ => false
            };
        }
        
        private bool CanPawnMove(Vector2Int delta)
        {
            int direction = color == PieceColor.White ? 1 : -1;
            
            // Forward move
            if (delta.x == 0)
            {
                if (delta.y == direction) return true;
                if (!hasMoved && delta.y == direction * 2) return true;
            }
            // Diagonal capture
            else if (Mathf.Abs(delta.x) == 1 && delta.y == direction)
            {
                return true;
            }
            
            return false;
        }
    }
    
    public enum PieceType
    {
        None,
        Pawn,
        Rook,
        Knight,
        Bishop,
        Queen,
        King
    }
    
    public enum PieceColor
    {
        White,
        Black
    }
}
