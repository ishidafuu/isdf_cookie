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

            int checkLength = Mathf.Max(Define.Instance.Common.GridRowLength, Define.Instance.Common.GridColumnLength);
            NativeArray<PieceState> checkStates = new NativeArray<PieceState>(checkLength, Allocator.TempJob);

            var job = new CheckLineJob()
            {
                fieldBanishs = m_groupField.GetComponentDataArray<FieldBanish>(),
                fieldInputs = m_groupField.GetComponentDataArray<FieldInput>(),

                pieceStates = m_groupPiece.GetComponentDataArray<PieceState>(),
                piecePositions = m_groupPiece.GetComponentDataArray<PiecePosition>(),

                gridStates = m_groupGrid.GetComponentDataArray<GridState>(),

                effectStates = m_groupEffect.GetComponentDataArray<EffectState>(),
                checkStates = checkStates,
                GridSize = Define.Instance.Common.GridSize,
                GridRowLength = Define.Instance.Common.GridRowLength,
                GridColumnLength = Define.Instance.Common.GridColumnLength,
            };
            inputDeps = job.Schedule(inputDeps);
            inputDeps.Complete();
            checkStates.Dispose();
            return inputDeps;
        }

        [BurstCompileAttribute]
        struct CheckLineJob : IJob
        {
            public ComponentDataArray<FieldBanish> fieldBanishs;
            public ComponentDataArray<PieceState> pieceStates;
            public ComponentDataArray<EffectState> effectStates;
            public NativeArray<PieceState> checkStates;

            [ReadOnly] public ComponentDataArray<PiecePosition> piecePositions;
            [ReadOnly] public ComponentDataArray<FieldInput> fieldInputs;
            [ReadOnly] public ComponentDataArray<GridState> gridStates;
            [ReadOnly] public int GridSize;
            [ReadOnly] public int GridRowLength;
            [ReadOnly] public int GridColumnLength;
            [ReadOnly] int FieldSize;

            public void Execute()
            {
                var fieldBanish = fieldBanishs[0];
                // DebugPanel.Log($"fieldBanish.phase", fieldBanish.phase.ToString());
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

                for (int x = 0; x < GridRowLength; x++)
                {
                    for (int y = 0; y < GridColumnLength; y++)
                    {
                        checkStates[y] = pieceStates[gridStates[x + GridRowLength * y].pieceId];
                    }
                    bool isSameColor = CheckLine(checkStates, GridColumnLength, fieldBanish.combo);

                    if (isSameColor)
                    {
                        UpdateFieldBanish(ref fieldBanish, x, 0);
                        for (int i = 0; i < GridRowLength; i++)
                        {
                            int index = gridStates[x + GridRowLength * i].pieceId;
                            PieceState pieceState = pieceStates[index];
                            UpdatePieceStateAndEffectState(ref pieceState, i, index, fieldBanish.combo);
                        }
                    }
                }

                for (int y = 0; y < GridColumnLength; y++)
                {
                    for (int x = 0; x < GridRowLength; x++)
                    {
                        checkStates[x] = pieceStates[gridStates[x + GridRowLength * y].pieceId];
                    }
                    bool isSameColor = CheckLine(checkStates, GridRowLength, fieldBanish.combo);

                    if (isSameColor)
                    {
                        UpdateFieldBanish(ref fieldBanish, 0, y);
                        for (int i = 0; i < GridRowLength; i++)
                        {
                            int index = gridStates[GridRowLength * y + i].pieceId;
                            PieceState pieceState = pieceStates[index];
                            UpdatePieceStateAndEffectState(ref pieceState, i, index, fieldBanish.combo);
                        }
                    }
                }

                if (fieldBanish.phase == EnumBanishPhase.BanishEnd)
                {
                    fieldBanish.phase = EnumBanishPhase.FallStart;
                    fieldBanish.combo = 0;
                    fieldBanishs[0] = fieldBanish;
                }
            }

            private void UpdatePieceStateAndEffectState(ref PieceState pieceState, int i, int index, int combo)
            {
                if (pieceState.isBanish)
                    return;

                pieceState.isBanish = true;
                // pieceState.count = 0;
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
                fieldBanish.combo++;
                fieldBanishs[0] = fieldBanish;
            }

            bool CheckLine(NativeArray<PieceState> checkStates, int length, int combo)
            {
                int lineBanishCount = 0;
                for (int i = 0; i < length; i++)
                {
                    if (checkStates[i].isBanish
                        && checkStates[i].combo < combo)
                    {
                        lineBanishCount++;
                    }
                }
                if (lineBanishCount >= (length - 1))
                    return false;

                for (int i = 0; i < length; i++)
                {
                    if (!checkStates[i].isBanish
                        && checkStates[i].type != EnumPieceType.Normal)
                    {
                        return false;
                    }
                }

                int lineColor = 0;
                for (int i = 0; i < length; i++)
                {
                    if (!checkStates[i].isBanish)
                    {
                        lineColor = checkStates[i].color;
                        break;
                    }
                }

                for (int i = 0; i < length; i++)
                {
                    if (!checkStates[i].isBanish
                        && lineColor != checkStates[i].color)
                    {
                        return false;
                    }
                }

                return true;
            }

        }
    }
}
