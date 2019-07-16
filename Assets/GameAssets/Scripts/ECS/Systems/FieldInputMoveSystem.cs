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
    [UpdateInGroup(typeof(FieldMoveGroup))]
    [UpdateAfter(typeof(ScanGroup))]
    public class FieldInputMoveSystem : JobComponentSystem
    {
        ComponentGroup m_groupField;
        static int m_offsetConvertPositionX;
        static int m_offsetConvertPositionY;

        protected override void OnCreateManager()
        {
            m_groupField = GetComponentGroup(
                ComponentType.ReadOnly<FieldScan>(),
                ComponentType.ReadOnly<FieldBanish>(),
                ComponentType.Create<FieldInput>()
            );

            m_offsetConvertPositionX = -Define.Instance.Common.FieldOffsetX - Define.Instance.Common.PieceOffsetX;
            m_offsetConvertPositionY = -Define.Instance.Common.FieldOffsetY - Define.Instance.Common.PieceOffsetY;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            m_groupField.AddDependency(inputDeps);
            var job = new MoveJob()
            {
                fieldInputs = m_groupField.GetComponentDataArray<FieldInput>(),
                fieldScans = m_groupField.GetComponentDataArray<FieldScan>(),
                fieldBanishs = m_groupField.GetComponentDataArray<FieldBanish>(),
                SwipeThreshold = Define.Instance.Common.SwipeThreshold,
                GridSize = Define.Instance.Common.GridSize,
                BorderSpeed = Define.Instance.Common.BorderSpeed,
                BorderOnGridDist = Define.Instance.Common.BorderOnGridDist,
                PixelSize = Define.Instance.PixelSize,
                FieldWidth = Define.Instance.Common.FieldWidth,
                FieldHeight = Define.Instance.Common.FieldHeight,
                offsetConvertPositionX = m_offsetConvertPositionX,
                offsetConvertPositionY = m_offsetConvertPositionY,
            };

            inputDeps = job.Schedule(inputDeps);
            inputDeps.Complete();
            return inputDeps;
        }

        [BurstCompileAttribute]
        struct MoveJob : IJob
        {
            public ComponentDataArray<FieldInput> fieldInputs;
            [ReadOnly] public ComponentDataArray<FieldScan> fieldScans;
            [ReadOnly] public ComponentDataArray<FieldBanish> fieldBanishs;
            [ReadOnly] public float SwipeThreshold;
            [ReadOnly] public int GridSize;
            [ReadOnly] public int BorderSpeed;
            [ReadOnly] public int BorderOnGridDist;
            [ReadOnly] public int PixelSize;
            [ReadOnly] public int FieldWidth;
            [ReadOnly] public int FieldHeight;
            [ReadOnly] public int offsetConvertPositionX;
            [ReadOnly] public int offsetConvertPositionY;

            public void Execute()
            {

                for (int i = 0; i < fieldScans.Length; i++)
                {
                    var fieldScan = fieldScans[i];
                    var fieldInput = fieldInputs[i];
                    var fieldBanish = fieldBanishs[i];

                    Vector2Int gamePosition = ConvertPosition(fieldScan.nowPosition);
                    // DebugPanel.Log($"fieldInput.phase", fieldInput.phase.ToString());

                    if (fieldBanish.phase == EnumBanishPhase.BanishStart)
                    {
                        UpdateFinishAlign(i, fieldScan, fieldInput, gamePosition);
                        break;
                    }
                    else if (fieldBanish.phase != EnumBanishPhase.None)
                    {
                        break;
                    }

                    switch (fieldInput.phase)
                    {
                        case EnumFieldInputPhase.Align:
                            UpdateAlign(i, fieldScan, fieldInput, gamePosition);
                            break;
                        case EnumFieldInputPhase.FinishAlign:
                            UpdateFinishAlign(i, fieldScan, fieldInput, gamePosition);
                            break;
                        default:
                            switch (fieldScan.phase)
                            {
                                case TouchPhase.Began:
                                    UpdateBegan(i, fieldScan, fieldInput, gamePosition);
                                    break;
                                case TouchPhase.Ended:
                                    UpdateEnded(i, fieldScan, fieldInput, gamePosition);
                                    break;
                                case TouchPhase.Moved:
                                    UpdateMoved(i, fieldScan, fieldInput, gamePosition);
                                    break;
                            }
                            break;
                    }
                }
            }

            private void UpdateBegan(int i, FieldScan fieldScan, FieldInput fieldInput, Vector2Int gamePosition)
            {
                if (!CheckInfield(gamePosition))
                    return;

                fieldInput.phase = EnumFieldInputPhase.Hold;
                fieldInput.gridPosition = ConvertGridPosition(gamePosition);
                fieldInput.startPosition = gamePosition;
                fieldInput.swipeVec = EnumSwipeVec.None;
                fieldInput.distPosition = Vector2Int.zero;
                fieldInputs[i] = fieldInput;
            }

            private void UpdateEnded(int i, FieldScan fieldScan, FieldInput fieldInput, Vector2Int gamePosition)
            {
                fieldInput.phase = EnumFieldInputPhase.Align;
                fieldInput.alignVec = GetAlignVec(fieldInput, fieldScan);
                fieldInput.alignDelta = GetAlignDelta(fieldInput.alignVec);
                fieldInput.isOnGrid = CheckOnGrid(fieldInput.distPosition, fieldInput.swipeVec);
                // DebugPanel.Log($"fieldInput.alignVec", fieldInput.alignVec.ToString());
                // DebugPanel.Log($"fieldInput.alignDelta", fieldInput.alignDelta.ToString());
                fieldInputs[i] = fieldInput;
            }

            private void UpdateMoved(int i, FieldScan fieldScan, FieldInput fieldInput, Vector2Int gamePosition)
            {
                if (fieldInput.phase != EnumFieldInputPhase.Hold)
                    return;

                Vector2 distPosition = (fieldScan.nowPosition - fieldScan.startPosition);
                DecideSwipeVec(ref fieldInput, distPosition);

                fieldInput.distPosition = new Vector2Int(
                    (int)(distPosition.x / PixelSize),
                    (int)(distPosition.y / PixelSize));
                fieldInput.isOnGrid = CheckOnGrid(fieldInput.distPosition, fieldInput.swipeVec);
                // DebugPanel.Log($"fieldInput.isOnGrid", fieldInput.isOnGrid.ToString());
                fieldInputs[i] = fieldInput;
            }

            private bool CheckOnGrid(Vector2Int position, EnumSwipeVec swipeType)
            {
                if (swipeType == EnumSwipeVec.Horizontal)
                {
                    int posX = Mathf.Abs(position.x) % GridSize;
                    // DebugPanel.Log($"posX", posX.ToString());
                    return (posX < BorderOnGridDist) || (GridSize - posX < BorderOnGridDist);
                }
                else if (swipeType == EnumSwipeVec.Vertical)
                {
                    int posY = Mathf.Abs(position.y) % GridSize;
                    return (posY < BorderOnGridDist) || (GridSize - posY < BorderOnGridDist);
                }
                return true;
            }

            private void DecideSwipeVec(ref FieldInput fieldInput, Vector2 distPosition)
            {
                if (fieldInput.swipeVec == EnumSwipeVec.None)
                {
                    float distX = Mathf.Abs(distPosition.x);
                    float distY = Mathf.Abs(distPosition.y);

                    if (distX > distY && distX > SwipeThreshold)
                    {
                        fieldInput.swipeVec = EnumSwipeVec.Horizontal;
                    }
                    else if (distX < distY && distY > SwipeThreshold)
                    {
                        fieldInput.swipeVec = EnumSwipeVec.Vertical;
                    }
                }
            }

            private Vector2Int ConvertPosition(Vector2 scanPosition)
            {
                int x = (int)(scanPosition.x / PixelSize) + offsetConvertPositionX;
                int y = (int)(scanPosition.y / PixelSize) + offsetConvertPositionY;
                return new Vector2Int(x, y);
            }

            private bool CheckInfield(Vector2Int position)
            {
                return ((position.x > 0 && position.x < FieldWidth)
                    && (position.y > 0 && position.y < FieldHeight));
            }

            private Vector2Int ConvertGridPosition(Vector2Int position)
            {
                return new Vector2Int(
                    (position.x / GridSize),
                    (position.y / GridSize));
            }

            private void UpdateAlign(int i, FieldScan fieldScan, FieldInput fieldInput, Vector2Int gamePosition)
            {

                Vector2Int newDistPosition = fieldInput.distPosition + fieldInput.alignDelta;

                if (CheckFinishAlign(fieldInput, newDistPosition))
                {
                    fieldInput.phase = EnumFieldInputPhase.FinishAlign;
                }

                fieldInput.distPosition = newDistPosition;
                fieldInput.isOnGrid = CheckOnGrid(fieldInput.distPosition, fieldInput.swipeVec);
                fieldInputs[i] = fieldInput;
            }

            private bool CheckFinishAlign(FieldInput fieldInput, Vector2Int newDistPosition)
            {
                EnumPieceAlignVec lastAlignVec = GetBackAlignVec(fieldInput.distPosition, fieldInput.swipeVec);
                EnumPieceAlignVec nowAlignVec = GetBackAlignVec(newDistPosition, fieldInput.swipeVec);
                // DebugPanel.Log($"fieldInput.distPosition", fieldInput.distPosition.ToString());
                // DebugPanel.Log($"newDistPosition", newDistPosition.ToString());
                // DebugPanel.Log($"lastAlignVec", lastAlignVec.ToString());
                // DebugPanel.Log($"fieldInput.alignVec", fieldInput.alignVec.ToString());
                return (((lastAlignVec != nowAlignVec) && lastAlignVec == fieldInput.alignVec)
                    || lastAlignVec == EnumPieceAlignVec.None);
            }

            private void UpdateFinishAlign(int i, FieldScan fieldScan, FieldInput fieldInput, Vector2Int gamePosition)
            {
                fieldInput.phase = EnumFieldInputPhase.None;
                fieldInput.swipeVec = EnumSwipeVec.None;
                fieldInput.distPosition = Vector2Int.zero;
                fieldInputs[i] = fieldInput;
            }

            private Vector2Int GetAlignDelta(EnumPieceAlignVec alignVec)
            {
                int deltaX = 0;
                int deltaY = 0;
                switch (alignVec)
                {
                    case EnumPieceAlignVec.Left:
                        deltaX = -BorderSpeed;
                        break;
                    case EnumPieceAlignVec.Right:
                        deltaX = +BorderSpeed;
                        break;
                    case EnumPieceAlignVec.Up:
                        deltaY = +BorderSpeed;
                        break;
                    case EnumPieceAlignVec.Down:
                        deltaY = -BorderSpeed;
                        break;
                }
                return new Vector2Int(deltaX, deltaY);
            }

            private EnumPieceAlignVec GetAlignVec(FieldInput fieldInput, FieldScan fieldScan)
            {
                if ((Mathf.Abs(fieldScan.delta.x) > BorderSpeed && fieldInput.swipeVec == EnumSwipeVec.Horizontal)
                    || (Mathf.Abs(fieldScan.delta.y) > BorderSpeed && fieldInput.swipeVec == EnumSwipeVec.Vertical))
                {
                    return GetSlipAlignVec(fieldScan.delta, fieldInput.swipeVec);
                }
                else
                {
                    return GetBackAlignVec(fieldInput.distPosition, fieldInput.swipeVec);
                }
            }
            private EnumPieceAlignVec GetBackAlignVec(Vector2Int position, EnumSwipeVec swipeType)
            {
                int halfPos = (GridSize / 2);
                if (swipeType == EnumSwipeVec.Horizontal)
                {
                    int posX = Mathf.Abs(position.x) % GridSize;
                    if (posX != 0)
                    {
                        return ((posX < halfPos) ^ (position.x < 0))
                            ? EnumPieceAlignVec.Left
                            : EnumPieceAlignVec.Right;
                    }
                }
                else if (swipeType == EnumSwipeVec.Vertical)
                {
                    int posY = Mathf.Abs(position.y) % GridSize;
                    if (posY != 0)
                    {
                        return ((posY < halfPos) ^ (position.y < 0))
                            ? EnumPieceAlignVec.Down
                            : EnumPieceAlignVec.Up;
                    }
                }

                return EnumPieceAlignVec.None;
            }

            private EnumPieceAlignVec GetSlipAlignVec(Vector2 delta, EnumSwipeVec swipeType)
            {
                if (swipeType == EnumSwipeVec.Horizontal)
                {
                    if (delta.x > 0)
                        return EnumPieceAlignVec.Right;
                    if (delta.x < 0)
                        return EnumPieceAlignVec.Left;
                }
                else if (swipeType == EnumSwipeVec.Vertical)
                {
                    if (delta.y > 0)
                        return EnumPieceAlignVec.Up;
                    if (delta.y < 0)
                        return EnumPieceAlignVec.Down;
                }

                return EnumPieceAlignVec.None;
            }

        }
    }
}
