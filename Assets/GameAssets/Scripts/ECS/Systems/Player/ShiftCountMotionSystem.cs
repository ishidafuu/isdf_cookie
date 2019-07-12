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
//     /// 時間経過によるモーション変更システム
//     /// </summary>
//     public class ShiftCountMotionSystem : JobComponentSystem
//     {
//         ComponentGroup m_group;

//         protected override void OnCreateManager()
//         {
//             m_group = GetComponentGroup(
//                 ComponentType.Create<CharaMotion>()
//             );
//         }

//         protected override JobHandle OnUpdate(JobHandle inputDeps)
//         {
//             var job = new ShiftCountMotionJob()
//             {
//                 m_charaMotions = m_group.GetComponentDataArray<CharaMotion>()
//             };
//             inputDeps = job.Schedule(inputDeps);
//             inputDeps.Complete();
//             return inputDeps;
//         }

//         [BurstCompileAttribute]
//         struct ShiftCountMotionJob : IJob
//         {
//             public ComponentDataArray<CharaMotion> m_charaMotions;

//             public void Execute()
//             {
//                 for (int i = 0; i < m_charaMotions.Length; i++)
//                 {
//                     CharaMotion charaMotion = m_charaMotions[i];

//                     switch (charaMotion.motionType)
//                     {
//                         case EnumMotion.Idle:
//                             break;
//                         case EnumMotion.Walk:
//                             break;
//                         case EnumMotion.Dash:
//                             break;
//                         case EnumMotion.Slip:
//                             UpdateSlip(ref charaMotion);
//                             break;
//                         case EnumMotion.Jump:
//                             break;
//                         case EnumMotion.Fall:
//                             break;
//                         case EnumMotion.Land:
//                             UpdateLand(ref charaMotion);
//                             break;
//                         case EnumMotion.Damage:
//                             UpdateDamage(ref charaMotion);
//                             break;
//                         case EnumMotion.Fly:
//                             break;
//                         case EnumMotion.Down:
//                             UpdateDown(ref charaMotion);
//                             break;
//                         case EnumMotion.Dead:
//                             UpdateDead(ref charaMotion);
//                             break;
//                         case EnumMotion.Action:
//                             break;
//                         default:
//                             Debug.Assert(false);
//                             break;
//                     }

//                     m_charaMotions[i] = charaMotion;
//                 }

//             }

//             void UpdateSlip(ref CharaMotion motion)
//             {

//             }

//             void UpdateLand(ref CharaMotion motion)
//             {

//             }

//             void UpdateDamage(ref CharaMotion motion)
//             {

//             }

//             void UpdateDown(ref CharaMotion motion)
//             {

//             }

//             void UpdateDead(ref CharaMotion motion)
//             {

//             }
//         }

//     }
// }
