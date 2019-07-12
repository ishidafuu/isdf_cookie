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
    public class PieceCountSystem : JobComponentSystem
    {
        ComponentGroup m_groupPiece;

        protected override void OnCreateManager()
        {
            m_groupPiece = GetComponentGroup(
                ComponentType.Create<PieceState>()
            );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new CountJob()
            {
                pieceStates = m_groupPiece.GetComponentDataArray<PieceState>(),
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
            public ComponentDataArray<PieceState> pieceStates;
            public int BanishEndCount;
            public int BanishImageCount;

            public void Execute()
            {
                int imageFrame = (BanishEndCount / BanishImageCount);
                for (int i = 0; i < pieceStates.Length; i++)
                {
                    var pieceState = pieceStates[i];
                    if (!pieceState.isBanish)
                        continue;

                    pieceState.count++;
                    if (pieceState.count >= BanishEndCount)
                    {
                        pieceState.isBanish = false;
                        pieceState.imageNo = 0;
                    }
                    else
                    {
                        pieceState.imageNo = pieceState.count / imageFrame;
                    }

                    pieceStates[i] = pieceState;
                }
            }
        }
    }
}
