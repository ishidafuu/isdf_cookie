using Unity.Entities;
namespace NKPB
{
    [UpdateBefore(typeof(FieldMoveGroup))]
    public class ScanGroup : ComponentSystemGroup {}

    [UpdateAfter(typeof(ScanGroup))]
    public class FieldMoveGroup : ComponentSystemGroup {}

    [UpdateAfter(typeof(FieldMoveGroup))]
    public class CountGroup : ComponentSystemGroup {}

    [UpdateAfter(typeof(CountGroup))]
    public class PieceMoveGroup : ComponentSystemGroup {}

    [UpdateAfter(typeof(PieceMoveGroup))]
    public class JudgeGroup : ComponentSystemGroup {}

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class RenderGroup : ComponentSystemGroup {}
}
