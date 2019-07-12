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
    [UpdateAfter(typeof(InputGroup))]
    public class PieceLineCheckSystem : JobComponentSystem
    {
        ComponentGroup m_groupField;
        ComponentGroup m_groupPiece;
        ComponentGroup m_groupGrid;

        protected override void OnCreateManager()
        {
            m_groupField = GetComponentGroup(
                ComponentType.Create<FieldBanish>()
            );
            m_groupPiece = GetComponentGroup(
                ComponentType.Create<PieceState>()
            );
            m_groupGrid = GetComponentGroup(
                ComponentType.ReadOnly<GridState>()
            );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_groupField.AddDependency(inputDeps);
            m_groupPiece.AddDependency(inputDeps);
            m_groupGrid.AddDependency(inputDeps);

            var job = new CheckLineJob()
            {
                fieldBanishs = m_groupField.GetComponentDataArray<FieldBanish>(),
                pieceStates = m_groupPiece.GetComponentDataArray<PieceState>(),
                gridStates = m_groupGrid.GetComponentDataArray<GridState>(),
                GridSize = Define.Instance.Common.GridSize,
                GridLineLength = Define.Instance.Common.GridLineLength,
            };
            inputDeps = job.Schedule(inputDeps);
            inputDeps.Complete();
            return inputDeps;
        }

        // [BurstCompileAttribute]
        struct CheckLineJob : IJob
        {
            public ComponentDataArray<FieldBanish> fieldBanishs;
            public ComponentDataArray<PieceState> pieceStates;
            [ReadOnly]
            public ComponentDataArray<GridState> gridStates;
            public int GridSize;
            public int GridLineLength;
            int FieldSize;

            public void Execute()
            {
                var fieldBanish = fieldBanishs[0];

                if (fieldBanish.isBanish)
                    return;

                for (int x = 0; x < GridLineLength; x++)
                {
                    bool isSameColor = CheckColor(
                        pieceStates[gridStates[x + GridLineLength * 0].pieceId],
                        pieceStates[gridStates[x + GridLineLength * 1].pieceId],
                        pieceStates[gridStates[x + GridLineLength * 2].pieceId],
                        pieceStates[gridStates[x + GridLineLength * 3].pieceId],
                        pieceStates[gridStates[x + GridLineLength * 4].pieceId]);

                    if (isSameColor)
                    {
                        Banish(x, 0);
                        for (int i = 0; i < GridLineLength; i++)
                        {
                            int index = gridStates[x + GridLineLength * i].pieceId;
                            var piece = pieceStates[index];
                            piece.isBanish = true;
                            piece.count = 0;
                            piece.color = i;
                            pieceStates[index] = piece;
                        }
                        return;
                    }
                }

                for (int y = 0; y < GridLineLength; y++)
                {
                    bool isSameColor = CheckColor(
                        pieceStates[gridStates[GridLineLength * y + 0].pieceId],
                        pieceStates[gridStates[GridLineLength * y + 1].pieceId],
                        pieceStates[gridStates[GridLineLength * y + 2].pieceId],
                        pieceStates[gridStates[GridLineLength * y + 3].pieceId],
                        pieceStates[gridStates[GridLineLength * y + 4].pieceId]);

                    if (isSameColor)
                    {
                        Banish(0, y);
                        for (int i = 0; i < GridLineLength; i++)
                        {
                            int index = gridStates[GridLineLength * y + i].pieceId;
                            var piece = pieceStates[index];
                            piece.isBanish = true;
                            piece.count = 0;
                            piece.color = i;
                            pieceStates[index] = piece;
                        }
                        return;
                    }
                }
            }

            private void Banish(int x, int y)
            {
                FieldBanish fieldBanish = fieldBanishs[0];
                fieldBanish.isBanish = true;
                fieldBanish.banishLine = new Vector2Int(x, y);
                fieldBanish.count = 0;
                fieldBanishs[0] = fieldBanish;
            }

            bool CheckColor(PieceState st0, PieceState st1, PieceState st2, PieceState st3, PieceState st4)
            {

                if (st0.type != EnumPieceType.Normal)
                    return false;
                if (st1.type != EnumPieceType.Normal)
                    return false;
                if (st2.type != EnumPieceType.Normal)
                    return false;
                if (st3.type != EnumPieceType.Normal)
                    return false;
                if (st4.type != EnumPieceType.Normal)
                    return false;

                if (st0.color != st1.color)
                    return false;
                if (st0.color != st2.color)
                    return false;
                if (st0.color != st3.color)
                    return false;
                if (st0.color != st4.color)
                    return false;

                return true;
            }

        }
    }
}
