using Unity.Entities;
using UnityEngine;

namespace NKPB
{
    public struct PiecePosition : IComponentData
    {
        public Vector2Int position;
        public Vector2 startPosition;
        public Vector2Int gridPosition;
        public boolean isMove;
    }
}
