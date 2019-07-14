using Unity.Entities;
using UnityEngine;
namespace NKPB
{

    public struct FieldInput : IComponentData
    {
        public EnumFieldInputPhase phase;
        public Vector2Int gridPosition;
        public EnumSwipeType swipeType;
        public Vector2 startPosition;
        public Vector2Int distPosition;
        // public Vector2 delta;
        public EnumPieceAlignVec alignVec;
        public Vector2Int alignDelta;
    }
}
