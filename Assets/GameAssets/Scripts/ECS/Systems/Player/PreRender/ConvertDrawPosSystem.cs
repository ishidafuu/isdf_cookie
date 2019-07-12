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

//     //ゲーム座標を描画座標に変換
//     public class ConvertDrawPosSystem : JobComponentSystem
//     {
//         ComponentGroup m_group;

//         protected override void OnCreateManager()
//         {
//             m_group = GetComponentGroup(
//                 ComponentType.ReadOnly<CharaMove>(),
//                 ComponentType.Create<Position>()
//             );
//         }

//         protected override JobHandle OnUpdate(JobHandle inputDeps)
//         {
//             var job = new ConvertJob()
//             {
//                 m_charaMoves = m_group.GetComponentDataArray<CharaMove>(),
//                 m_positions = m_group.GetComponentDataArray<Position>(),
//             };
//             inputDeps = job.Schedule(inputDeps);
//             inputDeps.Complete();
//             return inputDeps;
//         }

//         [BurstCompileAttribute]
//         struct ConvertJob : IJob
//         {
//             public ComponentDataArray<Position> m_positions;
//             [ReadOnly]
//             public ComponentDataArray<CharaMove> m_charaMoves;
//             public void Execute()
//             {
//                 for (int i = 0; i < m_positions.Length; i++)
//                 {
//                     var position = m_positions[i];
//                     position.Value.x = m_charaMoves[i].position.x * 0.01f;
//                     position.Value.y = (m_charaMoves[i].position.y + m_charaMoves[i].position.z) * 0.01f;
//                     position.Value.z = -100f + position.Value.y * 0.01f;
//                     m_positions[i] = position;
//                 }
//             }
//         }

//     }
// }
