// using System;
// using System.Collections.ObjectModel;
// 
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using Unity.Transforms;
// using UnityEngine;

// namespace NKPB
// {

//     public class LookSystem : JobComponentSystem
//     {
//         ComponentGroup m_group;

//         protected override void OnCreateManager()
//         {
//             m_group = GetComponentGroup(
//                 ComponentType.Create<CharaLook>(),
//                 ComponentType.ReadOnly<CharaMuki>()
//             );
//         }

//         protected override JobHandle OnUpdate(JobHandle inputDeps)
//         {
//             var job = new ConvertJob()
//             {
//                 m_charaLooks = m_group.GetComponentDataArray<CharaLook>(),
//                 m_charaMukis = m_group.GetComponentDataArray<CharaMuki>(),
//             };
//             inputDeps = job.Schedule(inputDeps);
//             inputDeps.Complete();
//             return inputDeps;
//         }

//         [BurstCompileAttribute]
//         struct ConvertJob : IJob
//         {
//             public ComponentDataArray<CharaLook> m_charaLooks;
//             [ReadOnly]
//             public ComponentDataArray<CharaMuki> m_charaMukis;
//             public void Execute()
//             {
//                 for (int i = 0; i < m_charaLooks.Length; i++)
//                 {
//                     var look = m_charaLooks[i];
//                     look.isLeft = (m_charaMukis[i].muki == EnumMuki.Left)
//                         ? 1
//                         : 0;
//                     m_charaLooks[i] = look;
//                 }
//             }
//         }

//     }
// }
