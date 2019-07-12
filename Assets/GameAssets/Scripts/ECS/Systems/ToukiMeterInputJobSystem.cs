// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using Unity.Transforms;
// using UnityEngine;

// namespace NKPB
// {
//     [UpdateInGroup(typeof(UpdateGroup))]
//     public class ToukiMeterInputJobSystem : JobComponentSystem
//     {
//         ComponentGroup m_group;

//         protected override void OnCreateManager()
//         {
//             m_group = GetComponentGroup(
//                 ComponentType.ReadOnly<PadScan>(),
//                 ComponentType.Create<ToukiMeter>()
//             );

//         }

//         protected override JobHandle OnUpdate(JobHandle inputDeps)
//         {
//             m_group.AddDependency(inputDeps);

//             var PadScans = m_group.ToComponentDataArray<PadScan>(Allocator.TempJob, out JobHandle handle1);
//             var toukiMeters = m_group.ToComponentDataArray<ToukiMeter>(Allocator.TempJob, out JobHandle handle2);
//             inputDeps = JobHandle.CombineDependencies(handle1, handle2);

//             var job = new InputToToukiJob()
//             {
//                 PadScans = PadScans,
//                 toukiMeters = toukiMeters,
//             };
//             inputDeps = job.Schedule(inputDeps);

//             m_group.AddDependency(inputDeps);
//             m_group.CopyFromComponentDataArray(toukiMeters, out JobHandle handle3);
//             // inputDeps.Complete();

//             inputDeps = new ReleaseJob
//             {
//                 toukiMeters = toukiMeters
//             }.Schedule(handle3);

//             return inputDeps;
//         }

//         struct ReleaseJob : IJob
//         {
//             [DeallocateOnJobCompletion]
//             public NativeArray<ToukiMeter> toukiMeters;

//             public void Execute() {}
//         }

//         [BurstCompileAttribute]
//         struct InputToToukiJob : IJob
//         {
//             [ReadOnly]
//             [DeallocateOnJobCompletion]
//             public NativeArray<PadScan> PadScans;

//             public NativeArray<ToukiMeter> toukiMeters;

//             public void Execute()
//             {
//                 for (int i = 0; i < PadScans.Length; i++)
//                 {
//                     var toukiMeter = toukiMeters[i];
//                     var a = true;
//                     var b = true;
//                     var c = true;
//                     var v = a || (b && c);
//                     if (toukiMeter.state != EnumToukiMaterState.Active)
//                     {
//                         break;
//                     }

//                     if (toukiMeter.muki != PadScans[i].GetPressCross())
//                     {
//                         toukiMeter.muki = PadScans[i].GetPressCross();
//                         toukiMeter.value = 0;
//                     }

//                     toukiMeters[i] = toukiMeter;
//                 }
//             }

//         }

//     }
// }
