// Autor: Generated
// Data: 05/05/2026

using UnityEngine;

namespace FifthSemester.Gameplay.Quests {
    [CreateAssetMenu(menuName = "Gameplay/Mission", fileName = "NewMission")]
    public class MissionSO : ScriptableObject {
        [Header("Identity")]
        public string MissionId;
        public string Title;

        [TextArea(3,6)]
        public string Description;

        [Header("Completion")]
        [Tooltip("String identifier of the event that completes this mission. Example: 'Dialogue_NurseAnna'")]
        public string CompletionEventId;

        [Header("Debug Setup")]
        [Tooltip("List of debug event IDs or item IDs to apply when skipping to this mission. Examples: 'Item:Keycard', 'UnlockDoor:MainDoor', 'Dialogue_NurseAnna'")]
        public string[] DebugSetupEvents;
    }
}
