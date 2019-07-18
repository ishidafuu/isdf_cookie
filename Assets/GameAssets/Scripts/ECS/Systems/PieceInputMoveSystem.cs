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
    [UpdateAfter(typeof(CountGroup))]
    public class PieceInputMoveSystem : JobComponentSystem
    {
        ComponentGroup m_groupField;
        ComponentGroup m_groupPiece;
        ComponentGroup m_groupGrid;

        protected override void OnCreateManager()
        {
            m_groupField = GetComponentGroup(
                ComponentType.ReadOnly<FieldInput>(),
                ComponentType.ReadOnly<FieldBanish>()
            );
            m_groupGrid = GetComponentGroup(
                ComponentType.Create<GridState>()
            );
            m_groupPiece = GetComponentGroup(
                ComponentType.Create<PiecePosition>()
            );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_groupField.AddDependency(inputDeps);
            m_groupGrid.AddDependency(inputDeps);
            m_groupPiece.AddDependency(inputDeps);
            var moveJob = new PieceMoveJob()
            {
                fieldInputs = m_groupField.GetComponentDataArray<FieldInput>(),
                fieldBanishs = m_groupField.GetComponentDataArray<FieldBanish>(),
                gridStates = m_groupGrid.GetComponentDataArray<GridState>(),
                piecePositions = m_groupPiece.GetComponentDataArray<PiecePosition>(),
                GridSize = Define.Instance.Common.GridSize,
                GridRowLength = Define.Instance.Common.GridRowLength,
                GridColumnLength = Define.Instance.Common.GridColumnLength,
                FieldWidth = Define.Instance.Common.FieldWidth,
                FieldHeight = Define.Instance.Common.FieldHeight,
                // PieceLimitSpeed = Define.Instance.Common.PieceLimitSpeed,
                BorderSpeed = Define.Instance.Common.BorderSpeed,
            };

            inputDeps = moveJob.Schedule(inputDeps);

            var reflectJob = new GridReflectJob()
            {
                piecePositions = m_groupPiece.GetComponentDataArray<PiecePosition>(),
                gridStates = m_groupGrid.GetComponentDataArray<GridState>(),
                GridSize = Define.Instance.Common.GridSize,
                GridRowLength = Define.Instance.Common.GridRowLength,
                GridColumnLength = Define.Instance.Common.GridColumnLength,
            };

            inputDeps = reflectJob.Schedule(inputDeps);
            inputDeps.Complete();
            return inputDeps;
        }

        // [BurstCompileAttribute]
        struct PieceMoveJob : IJob
        {
            public ComponentDataArray<PiecePosition> piecePositions;
            [ReadOnly] public ComponentDataArray<FieldInput> fieldInputs;
            [ReadOnly] public ComponentDataArray<FieldBanish> fieldBanishs;
            [ReadOnly] public ComponentDataArray<GridState> gridStates;
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
            public ComponentDataArray<GridState> gridStates;
            [ReadOnly] public ComponentDataArray<PiecePosition> piecePositions;
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
