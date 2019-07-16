using Unity.Entities;
using UnityEngine;
namespace NKPB
{

    public struct FieldBanish : IComponentData
    {
        public EnumBanishPhase phase;
        public int combo;
        public int count;
    }
}
