using Unity.Entities;
using UnityEngine;

namespace NKPB
{
    public struct PiecePosition : IComponentData
    {
        public Vector2Int position;
        public Vector2Int startPosition;
        public Vector2Int gridPosition;
        public int fallLength;
        public int fallCount;
    }
}
