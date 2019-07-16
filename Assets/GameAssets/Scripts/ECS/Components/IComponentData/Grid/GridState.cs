using Unity.Entities;
using UnityEngine;

namespace NKPB
{
    public struct GridState : IComponentData
    {
        public int gridId;
        public int fieldId;
        public int pieceId;
    }
}
