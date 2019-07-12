// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using Unity.Transforms;
// using UnityEngine;

// namespace NKPB
// {
//     [UpdateInGroup(typeof(CountGroup))]
//     public class ToukiMeterCountJobSystem : JobComponentSystem
//     {
//         ComponentGroup m_group;

//         protected override void OnCreateManager()
//         {
//             m_group = GetComponentGroup(
//                 ComponentType.Create<ToukiMeter>()
//             );
//         }

//         protected override JobHandle OnUpdate(JobHandle inputDeps)
//         {
//             m_group.AddDependency(inputDeps);

//             var toukiMeters = m_group.ToComponentDataArray<ToukiMeter>(Allocator.TempJob);
//             JobHandle jobHandle = DoCountToukiJob(ref inputDeps, toukiMeters);

//             // 開放
//             inputDeps = new ReleaseJob
//             {
//                 toukiMeters = toukiMeters
//             }.Schedule(jobHandle);

//             return inputDeps;
//         }

//         private JobHandle DoCountToukiJob(ref JobHandle inputDeps, NativeArray<ToukiMeter> toukiMeters)
//         {
//             Vector2[] uv = Shared.bgFrameMeshMat.meshs["bg00"].uv;
//             inputDeps = new CountToukiJob()
//             {
//                 BgScrollRange = Define.Instance.DrawPos.BgScrollWidth << Define.Instance.DrawPos.BgScrollRangeFactor,
//                     toukiMeters = toukiMeters,
//                     spriteUl = uv[0].x,
//                     spriteUr = uv[1].x,
//             }.Schedule(inputDeps);

//             m_group.AddDependency(inputDeps);
//             m_group.CopyFromComponentDataArray(toukiMeters, out JobHandle jobHandle);
//             return jobHandle;
//         }

//         struct ReleaseJob : IJob
//         {
//             [DeallocateOnJobCompletion]
//             public NativeArray<ToukiMeter> toukiMeters;

//             public void Execute() {}
//         }

//         [BurstCompileAttribute]
//         struct CountToukiJob : IJob
//         {
//             [ReadOnly]
//             public int BgScrollRange;
//             [ReadOnly]
//             public float spriteUl;
//             [ReadOnly]
//             public float spriteUr;
//             public NativeArray<ToukiMeter> toukiMeters;

//             public void Execute()
//             {
//                 float width = (spriteUr - spriteUl) / 2;

//                 for (int i = 0; i < toukiMeters.Length; i++)
//                 {
//                     var toukiMeter = toukiMeters[i];
//                     if (toukiMeter.muki != EnumCrossType.None)
//                     {
//                         toukiMeter.value++;
//                         if (toukiMeter.value > 100)
//                         {
//                             toukiMeter.value = 100;
//                         }
//                     }

//                     // 背景スクロール
//                     switch (toukiMeter.muki)
//                     {
//                         case EnumCrossType.Left:
//                         case EnumCrossType.Down:

//                             toukiMeter.bgScroll--;
//                             if (toukiMeter.bgScroll < 0)
//                             {
//                                 toukiMeter.bgScroll = BgScrollRange;
//                             }
//                             break;
//                         case EnumCrossType.Right:
//                             toukiMeter.bgScroll++;
//                             if (toukiMeter.bgScroll > BgScrollRange)
//                             {
//                                 toukiMeter.bgScroll = 0;
//                             }
//                             break;
//                     }

//                     float u = (float)toukiMeter.bgScroll / (float)BgScrollRange;
//                     toukiMeter.textureUl = spriteUl + (u * width);
//                     toukiMeter.textureUr = toukiMeter.textureUl + width;
//                     toukiMeters[i] = toukiMeter;
//                 }
//             }

//         }

//     }
// }
