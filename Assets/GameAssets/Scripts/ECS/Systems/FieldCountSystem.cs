using System;
using System.Collections.ObjectModel;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace NKPB
{
    [UpdateInGroup(typeof(CountGroup))]
    [UpdateAfter(typeof(InputGroup))]
    public class FieldCountSystem : JobComponentSystem
    {
        ComponentGroup m_group;

        protected override void OnCreateManager()
        {
            m_group = GetComponentGroup(
                ComponentType.Create<FieldBanish>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new CountJob()
            {
                fieldBanishs = m_group.GetComponentDataArray<FieldBanish>(),
                BanishEndCount = Define.Instance.Common.BanishEndCount,
            };
            inputDeps = job.Schedule(inputDeps);
            inputDeps.Complete();
            return inputDeps;
        }

        [BurstCompileAttribute]
        struct CountJob : IJob
        {
            public ComponentDataArray<FieldBanish> fieldBanishs;
            [ReadOnly] public int BanishEndCount;

            public void Execute()
            {
                for (int i = 0; i < fieldBanishs.Length; i++)
                {
                    var fieldBanish = fieldBanishs[i];
                    if (fieldBanish.phase != EnumBanishPhase.Banish)
                        break;

                    fieldBanish.count++;

                    if (fieldBanish.count > BanishEndCount)
                    {
                        fieldBanish.phase = EnumBanishPhase.None;
                    }

                    fieldBanishs[i] = fieldBanish;
                }
            }
        }
    }
}
