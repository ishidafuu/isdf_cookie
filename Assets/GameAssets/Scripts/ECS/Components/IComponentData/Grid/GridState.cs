using Unity.Entities;
using UnityEngine;

namespace NKPB
{
    /// <summary>
    /// グリッドの状態
    /// </summary>
    public struct GridState : IComponentData
    {
        public int gridId;
        public int fieldId;
        // public Vector2Int position;
        public int pieceId;
    }
}
