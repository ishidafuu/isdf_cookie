// using Unity.Burst;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using Unity.Transforms;
// using UnityEngine;

// namespace NKPB
// {

//     //座標移動
//     public class MovePosSystem : JobComponentSystem
//     {
//         ComponentGroup m_group;

//         protected override void OnCreateManager()
//         {
//             m_group = GetComponentGroup(
//                 ComponentType.Create<CharaMove>());
//         }

//         protected override JobHandle OnUpdate(JobHandle inputDeps)
//         {
//             var job = new PositionJob()
//             {
//                 m_charaMove = m_group.GetComponentDataArray<CharaMove>(),
//             };
//             inputDeps = job.Schedule(inputDeps);
//             inputDeps.Complete();
//             return inputDeps;
//         }

//         [BurstCompileAttribute]
//         struct PositionJob : IJob
//         {
//             public ComponentDataArray<CharaMove> m_charaMove;
//             public void Execute()
//             {
//                 for (int i = 0; i < m_charaMove.Length; i++)
//                 {
//                     CharaMove charaMove = m_charaMove[i];
//                     charaMove.position += charaMove.delta;
//                     m_charaMove[i] = charaMove;
//                 }
//             }
//         }

//     }
// }
