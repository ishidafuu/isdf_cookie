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
    [UpdateInGroup(typeof(PieceMoveGroup))]
    [UpdateAfter(typeof(CountGroup))]
    public class PieceFallMoveSystem : JobComponentSystem
    {
        ComponentGroup m_groupField;
        ComponentGroup m_groupPiece;
        // ComponentGroup m_groupGrid;

        protected override void OnCreateManager()
        {
            m_groupField = GetComponentGroup(
                ComponentType.Create<FieldBanish>()
            );
            // m_groupGrid = GetComponentGroup(
            //     ComponentType.Create<GridState>()
            // );
            m_groupPiece = GetComponentGroup(
                ComponentType.Create<PiecePosition>()
            );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_groupField.AddDependency(inputDeps);
            // m_groupGrid.AddDependency(inputDeps);
            m_groupPiece.AddDependency(inputDeps);
            var moveJob = new PieceMoveJob()
            {
                fieldBanishs = m_groupField.GetComponentDataArray<FieldBanish>(),
                piecePositions = m_groupPiece.GetComponentDataArray<PiecePosition>(),
                GridSize = Define.Instance.Common.GridSize,
                GridRowLength = Define.Instance.Common.GridRowLength,
                GridColumnLength = Define.Instance.Common.GridColumnLength,
                BorderSpeed = Define.Instance.Common.BorderSpeed,
            };

            inputDeps = moveJob.Schedule(inputDeps);
            inputDeps.Complete();
            return inputDeps;
        }

        [BurstCompileAttribute]
        struct PieceMoveJob : IJob
        {
            public ComponentDataArray<PiecePosition> piecePositions;
            public ComponentDataArray<FieldBanish> fieldBanishs;
            // [ReadOnly] public ComponentDataArray<GridState> gridStates;
            [ReadOnly] public int GridSize;
            [ReadOnly] public int GridRowLength;
            [ReadOnly] public int GridColumnLength;
            [ReadOnly] public int BorderSpeed;
            [ReadOnly] public int FieldSize;

            public void Execute()
            {
                var fieldBanish = fieldBanishs[0];
                if (fieldBanish.phase != EnumBanishPhase.Fall)
                    return;

                bool isFall = false;
                for (int i = 0; i < piecePositions.Length; i++)
                {
                    var piecePosition = piecePositions[i];

                    if (piecePosition.fallLength == 0)
                        continue;

                    isFall = true;
                    piecePosition.fallCount++;
                    int moveY = piecePosition.fallCount / 2;
                    if (moveY > piecePosition.fallLength)
                    {
                        moveY = piecePosition.fallLength;
                    }
                    piecePosition.position = new Vector2Int(piecePosition.position.x, piecePosition.position.y - moveY);
                    piecePosition.fallLength -= moveY;

                    if (piecePosition.fallLength == 0)
                    {
                        piecePosition.gridPosition = new Vector2Int(
                            piecePosition.position.x / GridSize,
                            piecePosition.position.y / GridSize);
                    }

                    piecePositions[i] = piecePosition;
                }

                if (!isFall)
                {
                    fieldBanish.phase = EnumBanishPhase.None;
                    fieldBanishs[0] = fieldBanish;
                }
            }

        }

    }
}
