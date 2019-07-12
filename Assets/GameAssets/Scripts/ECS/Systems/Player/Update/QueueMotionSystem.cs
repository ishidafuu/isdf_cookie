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
//     /// <summary>
//     /// 入力による状態変化システム
//     /// </summary>
//     public class QueueMotionSystem : JobComponentSystem
//     {
//         ComponentGroup m_group;

//         protected override void OnCreateManager()
//         {
//             m_group = GetComponentGroup(
//                 ComponentType.Create<CharaMotion>(),
//                 ComponentType.Create<CharaFlag>(),
//                 ComponentType.Create<CharaQueue>());
//         }

//         protected override JobHandle OnUpdate(JobHandle inputDeps)
//         {
//             var job = new InputJob()
//             {
//                 m_charaMotions = m_group.GetComponentDataArray<CharaMotion>(),
//                 m_charaFlags = m_group.GetComponentDataArray<CharaFlag>(),
//                 m_charaQueues = m_group.GetComponentDataArray<CharaQueue>(),
//             };

//             inputDeps = job.Schedule(inputDeps);
//             inputDeps.Complete();
//             return inputDeps;
//         }

//         [BurstCompileAttribute]
//         struct InputJob : IJob
//         {
//             public ComponentDataArray<CharaMotion> m_charaMotions;
//             public ComponentDataArray<CharaFlag> m_charaFlags;
//             [ReadOnly]
//             public ComponentDataArray<CharaQueue> m_charaQueues;

//             public void Execute()
//             {
//                 for (int i = 0; i < m_charaQueues.Length; i++)
//                 {
//                     var charaQueue = m_charaQueues[i];

//                     if (charaQueue.isQueue)
//                     {
//                         var charaMotion = m_charaMotions[i];
//                         charaMotion.SwitchMotion(charaQueue.motionType);

//                         switch (charaQueue.motionType)
//                         {
//                             case EnumMotion.Idle:
//                                 UpdateIdleFlags(i);
//                                 break;
//                             case EnumMotion.Walk:
//                                 break;
//                             case EnumMotion.Dash:
//                                 break;
//                             case EnumMotion.Slip:
//                                 break;
//                             case EnumMotion.Jump:
//                                 break;
//                             case EnumMotion.Fall:
//                                 break;
//                             case EnumMotion.Land:
//                                 break;
//                             case EnumMotion.Damage:
//                                 break;
//                             case EnumMotion.Fly:
//                                 break;
//                             case EnumMotion.Down:
//                                 break;
//                             case EnumMotion.Dead:
//                                 break;
//                             case EnumMotion.Action:
//                                 break;
//                             default:
//                                 Debug.Assert(false);
//                                 break;
//                         }
//                     }

//                     charaQueue.ClearQueue();
//                     m_charaQueues[i] = charaQueue;
//                 }
//             }
//             // Jump = 0x0001,
//             //     Dash = 0x0002,
//             //     Walk = 0x0004,
//             //     Slip = 0x0008,
//             //     Idle = 0x0010,
//             void UpdateIdleFlags(int i)
//             {
//                 var charaFlag = m_charaFlags[i];
//                 charaFlag.inputCheckFlag = FlagInputCheck.Jump | FlagInputCheck.Dash | FlagInputCheck.Walk;
//                 charaFlag.moveFlag = FlagMove.Stop;
//                 charaFlag.motionFlag = FlagMotion.None;
//                 charaFlag.mukiFlag = true;
//                 m_charaFlags[i] = charaFlag;
//             }

//         }
//     }
// }
