// using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace NKPB
{
    [UpdateInGroup(typeof(JudgeGroup))]
    // [UpdateAfter(typeof(PieceMoveGroup))]
    public class FieldCheckBanishSystem : JobComponentSystem
    {
        EntityQuery m_queryField;
        EntityQuery m_queryPiece;
        EntityQuery m_queryGrid;
        EntityQuery m_queryEffect;

        protected override void OnCreateManager()
        {
            m_queryField = GetEntityQuery(
                ComponentType.ReadOnly<FieldInput>(),
                ComponentType.ReadWrite<FieldBanish>()
            );
            m_queryPiece = GetEntityQuery(
                ComponentType.ReadWrite<PieceState>(),
                ComponentType.ReadOnly<PiecePosition>()
            );
            m_queryGrid = GetEntityQuery(
                ComponentType.ReadOnly<GridState>()
            );
            m_queryEffect = GetEntityQuery(
                ComponentType.ReadWrite<EffectState>()
            );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_queryField.AddDependency(inputDeps);
            m_queryPiece.AddDependency(inputDeps);
            m_queryGrid.AddDependency(inputDeps);
            m_queryEffect.AddDependency(inputDeps);

            int checkLength = Mathf.Max(Settings.Instance.Common.GridRowLength, Settings.Instance.Common.GridColumnLength);
            NativeArray<PieceState> checkStates = new NativeArray<PieceState>(checkLength, Allocator.TempJob);
            NativeArray<FieldBanish> fieldBanishs = m_queryField.ToComponentDataArray<FieldBanish>(Allocator.TempJob);
            NativeArray<PieceState> pieceStates = m_queryPiece.ToComponentDataArray<PieceState>(Allocator.TempJob);
            NativeArray<EffectState> effectStates = m_queryEffect.ToComponentDataArray<EffectState>(Allocator.TempJob);
            NativeArray<PiecePosition> piecePositions = m_queryPiece.ToComponentDataArray<PiecePosition>(Allocator.TempJob);
            NativeArray<FieldInput> fieldInputs = m_queryField.ToComponentDataArray<FieldInput>(Allocator.TempJob);
            NativeArray<GridState> gridStates = m_queryGrid.ToComponentDataArray<GridState>(Allocator.TempJob);

            var job = new CheckLineJob()
            {
                fieldBanishs = fieldBanishs,
                fieldInputs = fieldInputs,
                pieceStates = pieceStates,
                piecePositions = piecePositions,
                gridStates = gridStates,
                effectStates = effectStates,
                checkStates = checkStates,
                GridSize = Settings.Instance.Common.GridSize,
                GridRowLength = Settings.Instance.Common.GridRowLength,
                GridColumnLength = Settings.Instance.Common.GridColumnLength,
            };
            inputDeps = job.Schedule(inputDeps);
            inputDeps.Complete();

            m_queryField.CopyFromComponentDataArray(job.fieldBanishs);
            m_queryPiece.CopyFromComponentDataArray(job.pieceStates);
            m_queryEffect.CopyFromComponentDataArray(job.effectStates);

            fieldBanishs.Dispose();
            fieldInputs.Dispose();
            pieceStates.Dispose();
            piecePositions.Dispose();
            gridStates.Dispose();
            effectStates.Dispose();
            checkStates.Dispose();
            return inputDeps;
        }

        // [BurstCompileAttribute]
        struct CheckLineJob : IJob
        {
            public NativeArray<FieldBanish> fieldBanishs;
            public NativeArray<PieceState> pieceStates;
            public NativeArray<EffectState> effectStates;
            public NativeArray<PieceState> checkStates;

            [ReadOnly] public NativeArray<PiecePosition> piecePositions;
            [ReadOnly] public NativeArray<FieldInput> fieldInputs;
            [ReadOnly] public NativeArray<GridState> gridStates;
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

                int nextCombo = fieldBanish.combo + 1;
                for (int x = 0; x < GridRowLength; x++)
                {
                    for (int y = 0; y < GridColumnLength; y++)
                    {
                        checkStates[y] = pieceStates[gridStates[x + GridRowLength * y].pieceId];
                    }
                    bool isSameColor = CheckLine(checkStates, GridColumnLength, nextCombo);

                    if (isSameColor)
                    {
                        UpdateFieldBanish(ref fieldBanish, x, 0, nextCombo);
                        int col = 0;
                        for (int y = 0; y < GridColumnLength; y++)
                        {
                            int index = gridStates[x + GridRowLength * y].pieceId;
                            PieceState pieceState = pieceStates[index];
                            UpdatePieceStateAndEffectState(ref pieceState, col, index, nextCombo);
                            col++;
                        }
                    }
                }

                for (int y = 0; y < GridColumnLength; y++)
                {
                    for (int x = 0; x < GridRowLength; x++)
                    {
                        checkStates[x] = pieceStates[gridStates[x + GridRowLength * y].pieceId];
                    }
                    bool isSameColor = CheckLine(checkStates, GridRowLength, nextCombo);

                    if (isSameColor)
                    {
                        int col = 0;
                        UpdateFieldBanish(ref fieldBanish, 0, y, nextCombo);
                        for (int x = 0; x < GridRowLength; x++)
                        {
                            int index = gridStates[x + GridRowLength * y].pieceId;
                            PieceState pieceState = pieceStates[index];
                            UpdatePieceStateAndEffectState(ref pieceState, col, index, nextCombo);
                            col++;
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

            private void UpdatePieceStateAndEffectState(ref PieceState pieceState, int col, int index, int combo)
            {
                if (pieceState.isBanish)
                    return;

                pieceState.isBanish = true;
                pieceState.color = col;
                pieceState.combo = combo;
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

            private void UpdateFieldBanish(ref FieldBanish fieldBanish, int x, int y, int combo)
            {
                fieldBanish.phase = EnumBanishPhase.BanishStart;
                fieldBanish.count = 0;
                fieldBanish.combo = combo;
                fieldBanishs[0] = fieldBanish;
            }

            bool CheckLine(NativeArray<PieceState> checkStates, int length, int combo)
            {
                // Debug.Log("CheckLine combo" + combo);
                for (int i = 0; i < length; i++)
                {
                    if (checkStates[i].isBanish
                        && checkStates[i].combo == combo)
                    {
                        return false;
                    }
                }

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

                // Debug.Log(combo);
                return true;
            }

        }
    }
}
