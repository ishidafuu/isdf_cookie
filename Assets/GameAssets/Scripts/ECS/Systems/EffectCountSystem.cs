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
    public class EffectCountSystem : JobComponentSystem
    {
        EntityQuery m_query;

        protected override void OnCreateManager()
        {
            m_query = GetEntityQuery(
                ComponentType.ReadWrite<EffectState>()
            );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            NativeArray<EffectState> effectStates = m_query.ToComponentDataArray<EffectState>(Allocator.TempJob);
            var job = new CountJob()
            {
                effectStates = effectStates,
                BanishEndCount = Settings.Instance.Common.BanishEndCount,
                BanishImageCount = Settings.Instance.Common.BanishImageCount,
            };
            inputDeps = job.Schedule(inputDeps);
            inputDeps.Complete();

            m_query.CopyFromComponentDataArray(job.effectStates);

            effectStates.Dispose();
            return inputDeps;
        }

        // [BurstCompileAttribute]
        struct CountJob : IJob
        {
            public NativeArray<EffectState> effectStates;
            [ReadOnly] public int BanishEndCount;
            [ReadOnly] public int BanishImageCount;

            public void Execute()
            {
                int imageFrame = (BanishEndCount / BanishImageCount);
                for (int i = 0; i < effectStates.Length; i++)
                {
                    var effectState = effectStates[i];

                    switch (effectState.type)
                    {
                        case EnumEffectType.Banish:
                            UpdateBanish(imageFrame, i, effectState);
                            break;
                    }
                }
            }

            private void UpdateBanish(int imageFrame, int i, EffectState effectState)
            {
                effectState.count++;
                effectState.imageNo = effectState.count / imageFrame;

                if (effectState.count >= BanishEndCount)
                {
                    effectState.type = EnumEffectType.None;
                    effectState.imageNo = 0;
                }

                effectStates[i] = effectState;
            }
        }
    }
}
