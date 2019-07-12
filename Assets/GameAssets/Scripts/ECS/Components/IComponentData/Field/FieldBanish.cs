using Unity.Entities;
using UnityEngine;
namespace NKPB
{

    public struct FieldBanish : IComponentData
    {
        public boolean isBanish;
        public Vector2Int banishLine;
        public int count;
    }
}
