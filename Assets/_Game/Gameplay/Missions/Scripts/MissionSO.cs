// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using UnityEngine;

namespace FifthSemester.Gameplay.Missions {
    public enum MissionType {
        CollectItems = 0,
        CollectAndDeliver = 1
    }

    [CreateAssetMenu(menuName = "Gameplay/Mission", fileName = "NewMission")]
    public class MissionDefinition : ScriptableObject {
        [Header("Identity")]
        public string MissionId;
        public string Title;

        [TextArea(3, 6)]
        public string Description;

        [Header("Type & Completion")]
        public MissionType Type;

        [Tooltip("Item name to collect")]
        public string TargetItemName;

        [Tooltip("Number of items to collect")]
        public int RequiredCount = 1;

        [Tooltip("Delivery point ID for deliver missions")]
        public string DeliveryPointId;

        [Header("Persistence")]
        [Tooltip("Save progress for this mission")]
        public bool PersistProgress = true;

        [Header("Debug Setup")]
        [Tooltip("Events to apply when skipping to this mission")]
        public string[] DebugSetupEvents;
    }
}
