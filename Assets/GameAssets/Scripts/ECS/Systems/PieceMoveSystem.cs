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
    [UpdateInGroup(typeof(MoveGroup))]
    [UpdateAfter(typeof(CountGroup))]
    public class PieceInputSystem : JobComponentSystem
    {
        ComponentGroup m_groupField;
        ComponentGroup m_groupPiece;
        ComponentGroup m_groupGrid;

        protected override void OnCreateManager()
        {
            m_groupField = GetComponentGroup(
                ComponentType.ReadOnly<FieldInput>()
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
                gridStates = m_groupGrid.GetComponentDataArray<GridState>(),
                piecePositions = m_groupPiece.GetComponentDataArray<PiecePosition>(),
                GridSize = Define.Instance.Common.GridSize,
                GridLineLength = Define.Instance.Common.GridLineLength,
                // PieceLimitSpeed = Define.Instance.Common.PieceLimitSpeed,
                BorderSpeed = Define.Instance.Common.BorderSpeed,
            };

            inputDeps = moveJob.Schedule(inputDeps);

            var reflectJob = new GridReflectJob()
            {
                piecePositions = m_groupPiece.GetComponentDataArray<PiecePosition>(),
                gridStates = m_groupGrid.GetComponentDataArray<GridState>(),
                GridSize = Define.Instance.Common.GridSize,
                GridLineLength = Define.Instance.Common.GridLineLength,
            };

            inputDeps = reflectJob.Schedule(inputDeps);
            inputDeps.Complete();
            return inputDeps;
        }

        // [BurstCompileAttribute]
        struct PieceMoveJob : IJob
        {
            [ReadOnly]
            public ComponentDataArray<FieldInput> fieldInputs;
            public ComponentDataArray<GridState> gridStates;
            public ComponentDataArray<PiecePosition> piecePositions;
            public int GridSize;
            public int GridLineLength;
            // public int PieceLimitSpeed;
            public int BorderSpeed;
            int FieldSize;

            public void Execute()
            {
                FieldSize = GridSize * GridLineLength;
                for (int i = 0; i < fieldInputs.Length; i++)
                {
                    var fieldInput = fieldInputs[i];
                    switch (fieldInput.phase)
                    {
                        case EnumFieldInputPhase.Align:
                            // UpdateAlignment(fieldInput);
                            UpdateHold(fieldInput);
                            break;
                        case EnumFieldInputPhase.FinishAlign:
                            UpdateHold(fieldInput);
                            UpdateAlignFinish(fieldInput);
                            break;
                        case EnumFieldInputPhase.Hold:
                            UpdateHold(fieldInput);
                            break;
                    }
                }
            }

            private void UpdateHold(FieldInput fieldInput)
            {
                // DebugPanel.Log($"fieldInput.distPosition", fieldInput.distPosition.ToString());
                if (fieldInput.swipeType == EnumSwipeType.Horizontal)
                {
                    for (int i = 0; i < piecePositions.Length; i++)
                    {
                        var piecePosition = piecePositions[i];
                        if (piecePosition.gridPosition.y == fieldInput.gridPosition.y)
                        {
                            int posX = RoundPos(piecePosition.startPosition.x + fieldInput.distPosition.x);
                            Vector2Int newPos = new Vector2Int(posX, piecePosition.position.y);
                            piecePositions[i] = MovePosition(piecePosition, newPos); //, delta);
                        }
                    }
                }
                else if (fieldInput.swipeType == EnumSwipeType.Vertical)
                {
                    for (int i = 0; i < piecePositions.Length; i++)
                    {
                        var piecePosition = piecePositions[i];
                        if (piecePosition.gridPosition.x == fieldInput.gridPosition.x)
                        {
                            int posY = RoundPos(piecePosition.startPosition.y + fieldInput.distPosition.y);
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
                    // piecePosition.gridPosition = ToGridPosition(piecePosition.position);
                    piecePositions[i] = piecePosition;
                    if (piecePosition.gridPosition.x == 2)
                    {
                        // DebugPanel.Log($"piecePositions{i}.position grid", piecePositions[i].position.ToString() + " _ " + piecePositions[i].gridPosition.ToString());
                        // DebugPanel.Log($"piecePositions{i}.gridPosition", piecePositions[i].gridPosition.ToString());
                    }
                }
            }

            // private void UpdateAlignment(FieldInput fieldInput)
            // {

            //     EnumPieceAlignVec alignVec = GetPieceAlignVec(fieldInput.distPosition);

            //     for (int i = 0; i < piecePositions.Length; i++)
            //     {
            //         var piecePosition = piecePositions[i];
            //         if (piecePosition.moveType != EnumPieceMoveType.Stop)
            //         {
            //             if (piecePosition.moveType == EnumPieceMoveType.HoldMove)
            //             {
            //                 // DebugPanel.Log($"alignVec{i}", alignVec.ToString());
            //                 int deltaX = 0;
            //                 int deltaY = 0;

            //                 switch (alignVec)
            //                 {
            //                     case EnumPieceAlignVec.Left:
            //                         deltaX = -BorderSpeed;
            //                         break;
            //                     case EnumPieceAlignVec.Right:
            //                         deltaX = +BorderSpeed;
            //                         break;
            //                     case EnumPieceAlignVec.Up:
            //                         deltaY = +BorderSpeed;
            //                         break;
            //                     case EnumPieceAlignVec.Down:
            //                         deltaY = -BorderSpeed;
            //                         break;
            //                 }

            //                 piecePosition.delta = new Vector2Int(deltaX, deltaY);
            //             }

            //             piecePosition.delta = BreakDelta(piecePosition);

            //             Vector2Int newPosition = new Vector2Int(
            //                 RoundPos(piecePosition.position.x + piecePosition.delta.x),
            //                 RoundPos(piecePosition.position.y + piecePosition.delta.y));

            //             EnumPieceAlignVec newPosAlignVec = GetPieceAlignVec(newPosition);
            //             bool isStop = newPosAlignVec != alignVec;

            //             piecePosition.position = (isStop)
            //                 ? FromGridPosition(piecePosition)
            //                 : newPosition;
            //             // piecePosition.position = newPosition;
            //             piecePosition.gridPosition = ToGridPosition(piecePosition);
            //             // piecePosition.moveType = (isStop)
            //             //     ? EnumPieceMoveType.Stop
            //             //     : EnumPieceMoveType.SlipMove;

            //             piecePositions[i] = piecePosition;
            //         }
            //     }
            // }

            // private Vector2Int BreakDelta(PiecePosition piecePosition)
            // {
            //     int deltaX = piecePosition.delta.x;
            //     if (piecePosition.delta.x > PieceLimitSpeed)
            //     {
            //         deltaX = PieceLimitSpeed;
            //     }
            //     // else if (piecePosition.delta.x > 0)
            //     // {
            //     //     deltaX = piecePosition.delta.x - 1;
            //     // }
            //     else if (piecePosition.delta.x < -PieceLimitSpeed)
            //     {
            //         deltaX = -PieceLimitSpeed;
            //     }
            //     // else if (piecePosition.delta.x < 0)
            //     // {
            //     //     deltaX = piecePosition.delta.x + 1;
            //     // }

            //     int deltaY = piecePosition.delta.y;
            //     if (piecePosition.delta.y > PieceLimitSpeed)
            //     {
            //         deltaY = PieceLimitSpeed;
            //     }
            //     // else if (piecePosition.delta.y > 0)
            //     // {
            //     //     deltaY = piecePosition.delta.y - 1;
            //     // }
            //     else if (piecePosition.delta.y < -PieceLimitSpeed)
            //     {
            //         deltaY = -PieceLimitSpeed;
            //     }
            //     // else if (piecePosition.delta.y < 0)
            //     // {
            //     //     deltaY = piecePosition.delta.y + 1;
            //     // }

            //     return new Vector2Int(deltaX, deltaY);
            // }

            // private static void ResetStartPosition(ref PiecePosition piecePosition)
            // {
            //     if (piecePosition.moveType == EnumPieceMoveType.Stop)
            //     {
            //         piecePosition.startPosition = piecePosition.position;
            //     }
            // }

            private Vector2Int ToGridPosition(Vector2Int position)
            {
                int offset = GridSize / 2;
                return new Vector2Int(
                    (RoundPos(position.x + offset) / GridSize) % GridLineLength,
                    (RoundPos(position.y + offset) / GridSize) % GridLineLength);
            }

            private Vector2Int FromGridPosition(Vector2Int gridPosition)
            {
                int offset = 0;
                //GridSize / 2;
                return new Vector2Int(
                    RoundPos((gridPosition.x * GridSize) - offset),
                    RoundPos((gridPosition.y * GridSize) - offset));
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

            private int RoundPos(int pos)
            {
                return (pos >= 0)
                    ? pos % FieldSize
                    : (FieldSize - (Mathf.Abs(pos) % FieldSize));
            }
        }

        // [BurstCompileAttribute]
        struct GridReflectJob : IJob
        {
            public ComponentDataArray<GridState> gridStates;
            [ReadOnly]
            public ComponentDataArray<PiecePosition> piecePositions;
            public int GridSize;
            public int GridLineLength;
            int FieldSize;

            public void Execute()
            {
                for (int i = 0; i < piecePositions.Length; i++)
                {
                    var piecePosition = piecePositions[i];
                    int index = (piecePosition.gridPosition.x + piecePosition.gridPosition.y * GridLineLength);
                    var gridState = gridStates[index];
                    gridState.pieceId = i;
                    gridStates[index] = gridState;
                }
            }

        }
    }
}
