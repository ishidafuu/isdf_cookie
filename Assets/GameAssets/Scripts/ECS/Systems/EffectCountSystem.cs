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
    [UpdateAfter(typeof(FieldMoveGroup))]
    public class EffectCountSystem : JobComponentSystem
    {
        ComponentGroup m_group;

        protected override void OnCreateManager()
        {
            m_group = GetComponentGroup(
                ComponentType.Create<EffectState>()
            );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new CountJob()
            {
                effectStates = m_group.GetComponentDataArray<EffectState>(),
                BanishEndCount = Define.Instance.Common.BanishEndCount,
                BanishImageCount = Define.Instance.Common.BanishImageCount,
            };
            inputDeps = job.Schedule(inputDeps);
            inputDeps.Complete();
            return inputDeps;
        }

        [BurstCompileAttribute]
        struct CountJob : IJob
        {
            public ComponentDataArray<EffectState> effectStates;
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
