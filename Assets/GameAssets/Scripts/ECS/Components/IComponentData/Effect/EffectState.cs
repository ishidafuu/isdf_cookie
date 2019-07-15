using Unity.Entities;
using UnityEngine;

namespace NKPB
{
    public struct EffectState : IComponentData
    {
        public EnumEffectType type;
        public Vector2Int position;
        public int count;
        public int imageNo;
    }
}
