using UnityEngine;
using FifthSemester.Features.Localization;

namespace FifthSemester.Gameplay.Map2 {
    [CreateAssetMenu(menuName = "Map2/Key Definition", fileName = "Map2KeyDefinition")]
    public class Map2KeyDefinitionSO : ScriptableObject {
        [TextArea] [SerializeField] private string _description = string.Empty;

        [Header("Pickup Dialogue")]
        [SerializeField] private LocalizedTextAsset _pickupDialogue;

        public LocalizedTextAsset PickupDialogue => _pickupDialogue;
    }
}
