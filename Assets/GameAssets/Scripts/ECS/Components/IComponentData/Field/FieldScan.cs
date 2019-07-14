using Unity.Entities;
using UnityEngine;
namespace NKPB
{
    public struct FieldScan : IComponentData
    {
        public TouchPhase phase;
        public boolean isTouch;
        public EnumSwipeType swipeType;
        // public boolean isInfield;
        // public Vector2 gridPosition;
        public Vector2 startPosition;
        public Vector2 nowPosition;
        public Vector2 delta;
    }
}
