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
    [UpdateInGroup(typeof(PieceMoveGroup))]
    // [UpdateAfter(typeof(CountGroup))]
    public class PieceFallMoveSystem : JobComponentSystem
    {
        EntityQuery m_queryField;
        EntityQuery m_queryPiece;
        // EntityQuery m_queryGrid;

        protected override void OnCreateManager()
        {
            m_queryField = GetEntityQuery(
                ComponentType.ReadWrite<FieldBanish>()
            );
            // m_queryGrid = GetEntityQuery(
            //     ComponentType.ReadWrite<GridState>()
            // );
            m_queryPiece = GetEntityQuery(
                ComponentType.ReadWrite<PiecePosition>()
            );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_queryField.AddDependency(inputDeps);
            // m_queryGrid.AddDependency(inputDeps);
            m_queryPiece.AddDependency(inputDeps);

            NativeArray<PiecePosition> piecePositions = m_queryPiece.ToComponentDataArray<PiecePosition>(Allocator.TempJob);
            NativeArray<FieldBanish> fieldBanishs = m_queryField.ToComponentDataArray<FieldBanish>(Allocator.TempJob);
            var job = new PieceMoveJob()
            {
                fieldBanishs = fieldBanishs,
                piecePositions = piecePositions,
                GridSize = Settings.Instance.Common.GridSize,
                GridRowLength = Settings.Instance.Common.GridRowLength,
                GridColumnLength = Settings.Instance.Common.GridColumnLength,
                BorderSpeed = Settings.Instance.Common.BorderSpeed,
            };

            inputDeps = job.Schedule(inputDeps);
            inputDeps.Complete();

            m_queryPiece.CopyFromComponentDataArray(job.piecePositions);
            m_queryField.CopyFromComponentDataArray(job.fieldBanishs);

            piecePositions.Dispose();
            fieldBanishs.Dispose();
            return inputDeps;
        }

        // [BurstCompileAttribute]
        struct PieceMoveJob : IJob
        {
            public NativeArray<PiecePosition> piecePositions;
            public NativeArray<FieldBanish> fieldBanishs;
            // [ReadOnly] public NativeArray<GridState> gridStates;
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
