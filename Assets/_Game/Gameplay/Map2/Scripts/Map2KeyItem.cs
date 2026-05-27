using FifthSemester.Gameplay.Inventory;
using UnityEngine;

namespace FifthSemester.Gameplay.Map2 {
    public class Map2KeyItem : Item {
        [field: SerializeField] public Map2KeyDefinitionSO KeyDefinition { get; private set; }
    }
}
