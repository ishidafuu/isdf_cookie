using System;
using System.Collections.ObjectModel;
// using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace NKPB
{
    [UpdateInGroup(typeof(CountGroup))]
    // [UpdateAfter(typeof(FieldMoveGroup))]
    public class FieldCountSystem : JobComponentSystem
    {
        EntityQuery m_query;

        protected override void OnCreateManager()
        {
            m_query = GetEntityQuery(
                ComponentType.ReadWrite<FieldBanish>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            NativeArray<FieldBanish> fieldBanishs = m_query.ToComponentDataArray<FieldBanish>(Allocator.TempJob);
            var job = new CountJob()
            {
                fieldBanishs = fieldBanishs,
                BanishEndCount = Settings.Instance.Common.BanishEndCount,
            };
            inputDeps = job.Schedule(inputDeps);
            inputDeps.Complete();

            m_query.CopyFromComponentDataArray(job.fieldBanishs);
            fieldBanishs.Dispose();
            return inputDeps;
        }

        // [BurstCompileAttribute]
        struct CountJob : IJob
        {
            public NativeArray<FieldBanish> fieldBanishs;
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
                        fieldBanish.phase = EnumBanishPhase.BanishEnd;
                    }

                    fieldBanishs[i] = fieldBanish;
                }
            }
        }
    }
}
