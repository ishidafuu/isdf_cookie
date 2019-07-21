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
    public class PieceCountSystem : JobComponentSystem
    {
        EntityQuery m_queryPiece;

        protected override void OnCreateManager()
        {
            m_queryPiece = GetEntityQuery(
                ComponentType.ReadWrite<PieceState>()
            );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            NativeArray<PieceState> pieceStates = m_queryPiece.ToComponentDataArray<PieceState>(Allocator.TempJob);
            var job = new CountJob()
            {
                pieceStates = pieceStates,
                BanishEndCount = Settings.Instance.Common.BanishEndCount,
                BanishImageCount = Settings.Instance.Common.BanishImageCount,
            };
            inputDeps = job.Schedule(inputDeps);
            inputDeps.Complete();

            m_queryPiece.CopyFromComponentDataArray(job.pieceStates);
            pieceStates.Dispose();
            return inputDeps;
        }

        // [BurstCompileAttribute]
        struct CountJob : IJob
        {
            public NativeArray<PieceState> pieceStates;
            [ReadOnly] public int BanishEndCount;
            [ReadOnly] public int BanishImageCount;

            public void Execute()
            {
                // int imageFrame = (BanishEndCount / BanishImageCount);
                // for (int i = 0; i < pieceStates.Length; i++)
                // {
                //     var pieceState = pieceStates[i];
                //     if (!pieceState.isBanish)
                //         continue;

                //     pieceState.count++;
                //     if (pieceState.count >= BanishEndCount)
                //     {
                //         pieceState.isBanish = false;
                //         pieceState.imageNo = 0;
                //     }
                //     else
                //     {
                //         pieceState.imageNo = pieceState.count / imageFrame;
                //     }

                //     pieceStates[i] = pieceState;
                // }
            }
        }
    }
}
