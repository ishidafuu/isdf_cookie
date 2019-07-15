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
    [UpdateInGroup(typeof(JudgeGroup))]
    [UpdateAfter(typeof(PieceMoveGroup))]
    public class PieceFallStartSystem : JobComponentSystem
    {
        ComponentGroup m_groupPiece;
        ComponentGroup m_groupGrid;
        ComponentGroup m_groupField;
        protected override void OnCreateManager()
        {
            m_groupField = GetComponentGroup(
                ComponentType.Create<FieldBanish>()
            );
            m_groupGrid = GetComponentGroup(
                ComponentType.Create<GridState>()
            );
            m_groupPiece = GetComponentGroup(
                ComponentType.Create<PieceState>(),
                ComponentType.Create<PiecePosition>()
            );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            NativeArray<int> fallLength = new NativeArray<int>(Define.Instance.Common.GridRowLength, Allocator.TempJob);
            var job = new CountJob()
            {
                fieldBanishs = m_groupField.GetComponentDataArray<FieldBanish>(),
                gridStates = m_groupGrid.GetComponentDataArray<GridState>(),
                pieceStates = m_groupPiece.GetComponentDataArray<PieceState>(),
                piecePositions = m_groupPiece.GetComponentDataArray<PiecePosition>(),
                fallCount = fallLength,
                BanishEndCount = Define.Instance.Common.BanishEndCount,
                BanishImageCount = Define.Instance.Common.BanishImageCount,
                GridRowLength = Define.Instance.Common.GridRowLength,
                GridSize = Define.Instance.Common.GridSize,
                FieldHeight = Define.Instance.Common.FieldHeight,
            };
            inputDeps = job.Schedule(inputDeps);
            inputDeps.Complete();

            fallLength.Dispose();
            return inputDeps;
        }

        [BurstCompileAttribute]
        struct CountJob : IJob
        {
            public ComponentDataArray<PieceState> pieceStates;
            public ComponentDataArray<PiecePosition> piecePositions;

            public ComponentDataArray<GridState> gridStates;
            public ComponentDataArray<FieldBanish> fieldBanishs;
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
