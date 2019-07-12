using Unity.Entities;
using UnityEngine;
namespace NKPB
{

    public struct FieldInput : IComponentData
    {
        public boolean isHold;
        public Vector2Int gridPosition;
        public EnumSwipeType swipeType;
        public Vector2 startPosition;
        public Vector2 distPosition;
    }
}
