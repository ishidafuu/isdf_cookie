using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace NKPB
{
    [UpdateInGroup(typeof(RenderGroup))]
    public class ToukiMeterDebugSystem : ComponentSystem
    {
        ComponentGroup m_groupField;
        ComponentGroup m_groupGrid;
        ComponentGroup m_groupPiece;

        protected override void OnCreateManager()
        {
            m_groupField = GetComponentGroup(
                ComponentType.ReadOnly<FieldBanish>()
                // ComponentType.ReadOnly<FieldInput>(),
                // ComponentType.ReadOnly<PiecePosition>()
            );
        }

        protected override void OnUpdate()
        {
            var fieldBanishs = m_groupField.GetComponentDataArray<FieldBanish>();
            // var fieldScans = m_group.GetComponentDataArray<FieldScan>();
            // var piecePositions = m_group.GetComponentDataArray<PiecePosition>();
            for (int i = 0; i < fieldBanishs.Length; i++)
            {
                // var fieldInput = fieldInputs[i];
                // var fieldScan = fieldScans[i];
                // Debug.Log(toukiMeter.value);
                // Debug.Log(toukiMeter.muki);
                // DebugPanel.Log("ToukiMeter.value", toukiMeter.value.ToString());
                // DebugPanel.Log($"piecePositions{i}", piecePositions[i].position.ToString());
                DebugPanel.Log($"fieldBanishs[i].isBanish{i}", fieldBanishs[i].isBanish.ToString());
                // DebugPanel.Log($"fieldInput.pieceId{i}", fieldInput.pieceId.ToString());
                // DebugPanel.Log($"fieldInput.isInfield{i}", fieldInput.isInfield);
                // fieldInputs[i] = fieldInput;
                // break;
            }
        }

    }
}
