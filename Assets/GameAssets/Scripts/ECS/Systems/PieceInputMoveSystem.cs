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
    public class PieceInputMoveSystem : JobComponentSystem
    {
        EntityQuery m_queryField;
        EntityQuery m_queryPiece;
        EntityQuery m_queryGrid;

        protected override void OnCreateManager()
        {
            m_queryField = GetEntityQuery(
                ComponentType.ReadOnly<FieldInput>(),
                ComponentType.ReadOnly<FieldBanish>()
            );
            m_queryGrid = GetEntityQuery(
                ComponentType.ReadWrite<GridState>()
            );
            m_queryPiece = GetEntityQuery(
                ComponentType.ReadWrite<PiecePosition>()
            );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_queryField.AddDependency(inputDeps);
            m_queryGrid.AddDependency(inputDeps);
            m_queryPiece.AddDependency(inputDeps);

            NativeArray<FieldInput> fieldInputs = m_queryField.ToComponentDataArray<FieldInput>(Allocator.TempJob);
            NativeArray<FieldBanish> fieldBanishs = m_queryField.ToComponentDataArray<FieldBanish>(Allocator.TempJob);
            NativeArray<GridState> gridStates = m_queryGrid.ToComponentDataArray<GridState>(Allocator.TempJob);
            NativeArray<PiecePosition> piecePositions = m_queryPiece.ToComponentDataArray<PiecePosition>(Allocator.TempJob);

            var moveJob = new PieceMoveJob()
            {
                fieldInputs = fieldInputs,
                fieldBanishs = fieldBanishs,
                gridStates = gridStates,
                piecePositions = piecePositions,
                GridSize = Settings.Instance.Common.GridSize,
                GridRowLength = Settings.Instance.Common.GridRowLength,
                GridColumnLength = Settings.Instance.Common.GridColumnLength,
                FieldWidth = Settings.Instance.Common.FieldWidth,
                FieldHeight = Settings.Instance.Common.FieldHeight,
                // PieceLimitSpeed = Define.Instance.Common.PieceLimitSpeed,
                BorderSpeed = Settings.Instance.Common.BorderSpeed,
            };

            inputDeps = moveJob.Schedule(inputDeps);

            var reflectJob = new GridReflectJob()
            {
                piecePositions = piecePositions,
                gridStates = gridStates,
                GridSize = Settings.Instance.Common.GridSize,
                GridRowLength = Settings.Instance.Common.GridRowLength,
                GridColumnLength = Settings.Instance.Common.GridColumnLength,
            };

            inputDeps = reflectJob.Schedule(inputDeps);
            inputDeps.Complete();

            m_queryPiece.CopyFromComponentDataArray(moveJob.piecePositions);
            m_queryGrid.CopyFromComponentDataArray(reflectJob.gridStates);

            fieldInputs.Dispose();
            fieldBanishs.Dispose();
            gridStates.Dispose();
            piecePositions.Dispose();
            return inputDeps;
        }

        // [BurstCompileAttribute]
        struct PieceMoveJob : IJob
        {
            public NativeArray<PiecePosition> piecePositions;
            [ReadOnly] public NativeArray<FieldInput> fieldInputs;
            [ReadOnly] public NativeArray<FieldBanish> fieldBanishs;
            [ReadOnly] public NativeArray<GridState> gridStates;
            [ReadOnly] public int GridSize;
            [ReadOnly] public int GridRowLength;
            [ReadOnly] public int GridColumnLength;
            [ReadOnly] public int BorderSpeed;
            [ReadOnly] public int FieldWidth;
            [ReadOnly] public int FieldHeight;

            public void Execute()
            {
                for (int i = 0; i < fieldInputs.Length; i++)
                {
                    var fieldInput = fieldInputs[i];
                    var fieldBanish = fieldBanishs[i];
                    if (fieldBanish.phase == EnumBanishPhase.BanishStart)
                    {
                        UpdateAlignFinish(fieldInput);
                    }
                    else
                    {
                        switch (fieldInput.phase)
                        {
                            case EnumFieldInputPhase.Align:
                                UpdateInputMove(fieldInput);
                                break;
                            case EnumFieldInputPhase.FinishAlign:
                                UpdateInputMove(fieldInput);
                                UpdateAlignFinish(fieldInput);
                                break;
                            case EnumFieldInputPhase.Hold:
                                UpdateInputMove(fieldInput);
                                break;
                        }
                    }
                }
            }

            private void UpdateInputMove(FieldInput fieldInput)
            {
                // DebugPanel.Log($"fieldInput.distPosition", fieldInput.distPosition.ToString());
                if (fieldInput.swipeVec == EnumSwipeVec.Horizontal)
                {
                    for (int i = 0; i < piecePositions.Length; i++)
                    {
                        var piecePosition = piecePositions[i];
                        if (piecePosition.gridPosition.y == fieldInput.gridPosition.y)
                        {
                            int posX = RoundPos(piecePosition.startPosition.x + fieldInput.distPosition.x, FieldWidth);
                            Vector2Int newPos = new Vector2Int(posX, piecePosition.position.y);
                            piecePositions[i] = MovePosition(piecePosition, newPos); //, delta);
                        }
                    }
                }
                else if (fieldInput.swipeVec == EnumSwipeVec.Vertical)
                {
                    for (int i = 0; i < piecePositions.Length; i++)
                    {
                        var piecePosition = piecePositions[i];
                        if (piecePosition.gridPosition.x == fieldInput.gridPosition.x)
                        {
                            int posY = RoundPos(piecePosition.startPosition.y + fieldInput.distPosition.y, FieldHeight);
                            Vector2Int newPos = new Vector2Int(piecePosition.position.x, posY);
                            piecePositions[i] = MovePosition(piecePosition, newPos);
                            if (piecePosition.gridPosition.x == 2)
                            {
                                // DebugPanel.Log($"piecePositions{i}.position grid", piecePositions[i].position.ToString() + " _ " + piecePositions[i].gridPosition.ToString());
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < piecePositions.Length; i++)
                    {
                        var piecePosition = piecePositions[i];
                        piecePosition.startPosition = FromGridPosition(piecePosition.gridPosition);
                        piecePositions[i] = piecePosition;
                        if (piecePosition.gridPosition.x == 2)
                        {
                            // DebugPanel.Log($"piecePositions{i}.position grid", piecePositions[i].position.ToString() + " _ " + piecePositions[i].gridPosition.ToString());
                        }
                    }
                }
            }

            private PiecePosition MovePosition(PiecePosition piecePosition, Vector2Int newPos) //, Vector2Int delta)
            {
                // piecePosition.delta = delta;
                // DebugPanel.Log("delta", delta.ToString());
                piecePosition.position = newPos;
                piecePosition.gridPosition = ToGridPosition(piecePosition.position);
                // piecePosition.moveType = EnumPieceMoveType.HoldMove;
                return piecePosition;
            }

            private void UpdateAlignFinish(FieldInput fieldInput)
            {
                for (int i = 0; i < piecePositions.Length; i++)
                {
                    var piecePosition = piecePositions[i];
                    piecePosition.position = FromGridPosition(piecePosition.gridPosition);
                    piecePositions[i] = piecePosition;
                    if (piecePosition.gridPosition.x == 2)
                    {
                        // DebugPanel.Log($"piecePositions{i}.position grid", piecePositions[i].position.ToString() + " _ " + piecePositions[i].gridPosition.ToString());
                        // DebugPanel.Log($"piecePositions{i}.gridPosition", piecePositions[i].gridPosition.ToString());
                    }
                }
            }

            private Vector2Int ToGridPosition(Vector2Int position)
            {
                int offset = GridSize / 2;
                return new Vector2Int(
                    (RoundPos(position.x + offset, FieldWidth) / GridSize) % GridRowLength,
                    (RoundPos(position.y + offset, FieldHeight) / GridSize) % GridColumnLength);
            }

            private Vector2Int FromGridPosition(Vector2Int gridPosition)
            {
                int offset = 0;
                //GridSize / 2;
                return new Vector2Int(
                    RoundPos((gridPosition.x * GridSize) - offset, FieldWidth),
                    RoundPos((gridPosition.y * GridSize) - offset, FieldHeight));
            }

            private EnumPieceAlignVec GetPieceAlignVec(Vector2Int position)
            {
                int halfPos = (GridSize / 2);
                int posX = position.x % GridSize;
                if (posX != 0)
                {
                    return (posX < halfPos)
                        ? EnumPieceAlignVec.Left
                        : EnumPieceAlignVec.Right;
                }

                int posY = position.y % GridSize;
                if (posY != 0)
                {
                    return (posY < halfPos)
                        ? EnumPieceAlignVec.Down
                        : EnumPieceAlignVec.Up;
                }
                return EnumPieceAlignVec.None;
            }

            private int RoundPos(int pos, int fieldSize)
            {
                return (pos >= 0)
                    ? pos % fieldSize
                    : (fieldSize - (Mathf.Abs(pos) % fieldSize));
            }
        }

        // [BurstCompileAttribute]
        struct GridReflectJob : IJob
        {
            public NativeArray<GridState> gridStates;
            [ReadOnly] public NativeArray<PiecePosition> piecePositions;
            [ReadOnly] public int GridSize;
            [ReadOnly] public int GridRowLength;
            [ReadOnly] public int GridColumnLength;
            [ReadOnly] public int FieldSize;

            public void Execute()
            {
                for (int i = 0; i < piecePositions.Length; i++)
                {
                    var piecePosition = piecePositions[i];
                    int index = (piecePosition.gridPosition.x + piecePosition.gridPosition.y * GridRowLength);
                    var gridState = gridStates[index];
                    gridState.pieceId = i;
                    gridStates[index] = gridState;
                }
            }

        }
    }
}
