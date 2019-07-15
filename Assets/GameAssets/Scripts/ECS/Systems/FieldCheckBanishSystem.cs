using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace NKPB
{
    [UpdateInGroup(typeof(JudgeGroup))]
    [UpdateAfter(typeof(PieceMoveGroup))]
    public class FieldCheckBanishSystem : JobComponentSystem
    {
        ComponentGroup m_groupField;
        ComponentGroup m_groupPiece;
        ComponentGroup m_groupGrid;
        ComponentGroup m_groupEffect;

        protected override void OnCreateManager()
        {
            m_groupField = GetComponentGroup(
                ComponentType.ReadOnly<FieldInput>(),
                ComponentType.Create<FieldBanish>()
            );
            m_groupPiece = GetComponentGroup(
                ComponentType.Create<PieceState>(),
                ComponentType.ReadOnly<PiecePosition>()
            );
            m_groupGrid = GetComponentGroup(
                ComponentType.ReadOnly<GridState>()
            );
            m_groupEffect = GetComponentGroup(
                ComponentType.Create<EffectState>()
            );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_groupField.AddDependency(inputDeps);
            m_groupPiece.AddDependency(inputDeps);
            m_groupGrid.AddDependency(inputDeps);
            m_groupEffect.AddDependency(inputDeps);

            var job = new CheckLineJob()
            {
                fieldBanishs = m_groupField.GetComponentDataArray<FieldBanish>(),
                fieldInputs = m_groupField.GetComponentDataArray<FieldInput>(),

                pieceStates = m_groupPiece.GetComponentDataArray<PieceState>(),
                piecePositions = m_groupPiece.GetComponentDataArray<PiecePosition>(),

                gridStates = m_groupGrid.GetComponentDataArray<GridState>(),

                effectStates = m_groupEffect.GetComponentDataArray<EffectState>(),

                GridSize = Define.Instance.Common.GridSize,
                GridLineLength = Define.Instance.Common.GridRowLength,
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
            public ComponentDataArray<EffectState> effectStates;
            [ReadOnly] public ComponentDataArray<PiecePosition> piecePositions;
            [ReadOnly] public ComponentDataArray<FieldInput> fieldInputs;
            [ReadOnly] public ComponentDataArray<GridState> gridStates;
            [ReadOnly] public int GridSize;
            [ReadOnly] public int GridLineLength;
            [ReadOnly] int FieldSize;

            public void Execute()
            {
                var fieldBanish = fieldBanishs[0];
                DebugPanel.Log($"fieldBanish.phase", fieldBanish.phase.ToString());
                switch (fieldBanish.phase)
                {
                    case EnumBanishPhase.BanishStart:
                        fieldBanish.phase = EnumBanishPhase.Banish;
                        fieldBanishs[0] = fieldBanish;
                        break;
                    case EnumBanishPhase.FallStart:
                    case EnumBanishPhase.Banish:
                        break;
                    case EnumBanishPhase.None:
                    case EnumBanishPhase.BanishEnd:
                        CheckLine(ref fieldBanish);
                        break;
                }

            }

            private void CheckLine(ref FieldBanish fieldBanish)
            {
                var fieldInput = fieldInputs[0];
                if (!fieldInput.isOnGrid)
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
                        UpdateFieldBanish(ref fieldBanish, x, 0);
                        for (int i = 0; i < GridLineLength; i++)
                        {
                            int index = gridStates[x + GridLineLength * i].pieceId;
                            PieceState pieceState = pieceStates[index];
                            UpdatePieceStateAndEffectState(ref pieceState, i, index);
                        }
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
                        UpdateFieldBanish(ref fieldBanish, 0, y);
                        for (int i = 0; i < GridLineLength; i++)
                        {
                            int index = gridStates[GridLineLength * y + i].pieceId;
                            PieceState pieceState = pieceStates[index];
                            UpdatePieceStateAndEffectState(ref pieceState, i, index);
                        }
                    }
                }

                if (fieldBanish.phase == EnumBanishPhase.BanishEnd)
                {
                    fieldBanish.phase = EnumBanishPhase.FallStart;
                    fieldBanishs[0] = fieldBanish;
                }
            }

            private void UpdatePieceStateAndEffectState(ref PieceState pieceState, int i, int index)
            {
                if (pieceState.isBanish)
                    return;

                pieceState.isBanish = true;
                pieceState.count = 0;
                //TODO:
                pieceState.color = i;
                pieceStates[index] = pieceState;
                UpdateEffectState(piecePositions[index].gridPosition);
            }

            private void UpdateEffectState(Vector2Int gridPosition)
            {
                for (int i = 0; i < effectStates.Length; i++)
                {
                    if (effectStates[i].type != EnumEffectType.None)
                        continue;

                    var effectState = effectStates[i];
                    effectState.type = EnumEffectType.Banish;
                    effectState.position = new Vector2Int(gridPosition.x * GridSize, gridPosition.y * GridSize);
                    effectState.count = 0;
                    effectStates[i] = effectState;
                    break;
                }
            }

            private void UpdateFieldBanish(ref FieldBanish fieldBanish, int x, int y)
            {
                fieldBanish.phase = EnumBanishPhase.BanishStart;
                fieldBanish.count = 0;
                fieldBanishs[0] = fieldBanish;
            }

            bool CheckColor(PieceState st0, PieceState st1, PieceState st2, PieceState st3, PieceState st4)
            {

                if (st0.isBanish && st1.isBanish && st2.isBanish && st3.isBanish && st4.isBanish)
                    return false;

                if (!st0.isBanish && st0.type != EnumPieceType.Normal)
                    return false;
                if (!st1.isBanish && st1.type != EnumPieceType.Normal)
                    return false;
                if (!st2.isBanish && st2.type != EnumPieceType.Normal)
                    return false;
                if (!st3.isBanish && st3.type != EnumPieceType.Normal)
                    return false;
                if (!st4.isBanish && st4.type != EnumPieceType.Normal)
                    return false;

                int lineColor = 0;

                if (!st0.isBanish)
                    lineColor = st0.color;
                else if (!st1.isBanish)
                    lineColor = st1.color;
                else if (!st2.isBanish)
                    lineColor = st2.color;
                else if (!st3.isBanish)
                    lineColor = st3.color;
                else if (!st4.isBanish)
                    lineColor = st4.color;

                if (!st0.isBanish && st0.color != lineColor)
                    return false;
                if (!st1.isBanish && st1.color != lineColor)
                    return false;
                if (!st2.isBanish && st2.color != lineColor)
                    return false;
                if (!st3.isBanish && st3.color != lineColor)
                    return false;
                if (!st4.isBanish && st4.color != lineColor)
                    return false;

                return true;
            }

        }
    }
}
