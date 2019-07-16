using Unity.Entities;
using Unity.Transforms;
namespace NKPB
{
    public static class ComponentTypes
    {
        public static ComponentType[] FieldComponentType = {
            typeof(FieldInput),
            typeof(FieldBanish),
        };

        public static ComponentType[] GridComponentType = {
            typeof(GridState),
        };

        public static ComponentType[] PieceComponentType = {
            typeof(PieceId),
            typeof(PiecePosition),
            typeof(PieceState),
        };

        public static ComponentType[] EffectComponentType = {
            typeof(EffectState),
        };
    }
}
