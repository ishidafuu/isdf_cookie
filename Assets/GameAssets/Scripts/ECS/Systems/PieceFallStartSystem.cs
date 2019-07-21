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
    [UpdateInGroup(typeof(JudgeGroup))]
    // [UpdateAfter(typeof(PieceMoveGroup))]
    public class PieceFallStartSystem : JobComponentSystem
    {
        EntityQuery m_queryPiece;
        EntityQuery m_queryGrid;
        EntityQuery m_queryField;
        protected override void OnCreateManager()
        {
            m_queryField = GetEntityQuery(
                ComponentType.ReadWrite<FieldBanish>()
            );
            m_queryGrid = GetEntityQuery(
                ComponentType.ReadWrite<GridState>()
            );
            m_queryPiece = GetEntityQuery(
                ComponentType.ReadWrite<PieceState>(),
                ComponentType.ReadWrite<PiecePosition>()
            );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            NativeArray<int> fallLength = new NativeArray<int>(Settings.Instance.Common.GridRowLength, Allocator.TempJob);
            NativeArray<FieldBanish> fieldBanishs = m_queryField.ToComponentDataArray<FieldBanish>(Allocator.TempJob);
            NativeArray<GridState> gridStates = m_queryGrid.ToComponentDataArray<GridState>(Allocator.TempJob);
            NativeArray<PieceState> pieceStates = m_queryPiece.ToComponentDataArray<PieceState>(Allocator.TempJob);
            NativeArray<PiecePosition> piecePositions = m_queryPiece.ToComponentDataArray<PiecePosition>(Allocator.TempJob);

            var job = new CountJob()
            {
                fieldBanishs = fieldBanishs,
                gridStates = gridStates,
                pieceStates = pieceStates,
                piecePositions = piecePositions,
                fallCount = fallLength,
                BanishEndCount = Settings.Instance.Common.BanishEndCount,
                BanishImageCount = Settings.Instance.Common.BanishImageCount,
                GridRowLength = Settings.Instance.Common.GridRowLength,
                GridSize = Settings.Instance.Common.GridSize,
                FieldHeight = Settings.Instance.Common.FieldHeight,
            };
            inputDeps = job.Schedule(inputDeps);
            inputDeps.Complete();

            m_queryField.CopyFromComponentDataArray(job.fieldBanishs);
            m_queryPiece.CopyFromComponentDataArray(job.pieceStates);
            m_queryPiece.CopyFromComponentDataArray(job.piecePositions);
            m_queryGrid.CopyFromComponentDataArray(job.gridStates);

            fallLength.Dispose();
            fieldBanishs.Dispose();
            gridStates.Dispose();
            pieceStates.Dispose();
            piecePositions.Dispose();
            return inputDeps;
        }

        // [BurstCompileAttribute]
        struct CountJob : IJob
        {
            public NativeArray<PieceState> pieceStates;
            public NativeArray<PiecePosition> piecePositions;

            public NativeArray<GridState> gridStates;
            public NativeArray<FieldBanish> fieldBanishs;
            public NativeArray<int> fallCount;
            [ReadOnly] public int BanishEndCount;
            [ReadOnly] public int BanishImageCount;
            [ReadOnly] public int GridRowLength;
            [ReadOnly] public int GridSize;
            [ReadOnly] public int FieldHeight;
            public void Execute()
            {
                if (fieldBanishs[0].phase != EnumBanishPhase.FallStart)
                    return;

                UpdateReplacePosition();

                UpdatePieceState();

                UpdateBanish();
            }

            private void UpdateReplacePosition()
            {
                for (int i = 0; i < gridStates.Length; i++)
                {
                    GridState gridState = gridStates[i];
                    PieceState pieceState = pieceStates[gridState.pieceId];
                    PiecePosition piecePosition = piecePositions[gridState.pieceId];
                    int x = i % GridRowLength;

                    if (pieceState.isBanish)
                    {
                        int posY = FieldHeight + (fallCount[x] * GridSize);
                        piecePosition.position = new Vector2Int(piecePosition.position.x, posY);
                        fallCount[x] += 1;
                    }
                    else
                    {
                        piecePosition.fallLength = fallCount[x] * GridSize;
                        piecePosition.fallCount = 0;
                    }
                    piecePositions[gridState.pieceId] = piecePosition;
                }
            }
            private void UpdatePieceState()
            {
                for (int i = 0; i < gridStates.Length; i++)
                {
                    GridState gridState = gridStates[i];
                    PieceState pieceState = pieceStates[gridState.pieceId];

                    if (pieceState.isBanish)
                    {
                        int x = i % GridRowLength;
                        var piecePosition = piecePositions[gridState.pieceId];
                        piecePosition.fallLength = fallCount[x] * GridSize;
                        piecePosition.fallCount = 0;
                        piecePositions[gridState.pieceId] = piecePosition;
                    }

                    pieceState.isBanish = false;
                    pieceStates[gridState.pieceId] = pieceState;
                }
            }

            private void UpdateBanish()
            {
                FieldBanish fieldBanish = fieldBanishs[0];
                fieldBanish.phase = EnumBanishPhase.Fall;
                fieldBanishs[0] = fieldBanish;
            }
        }
    }
}
