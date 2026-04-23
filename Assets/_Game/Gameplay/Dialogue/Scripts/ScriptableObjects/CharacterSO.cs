// autor: Murillo Gomes Yonamine
// data: 30/03/2026

using UnityEngine;

namespace FifthSemester.Gameplay.Dialogue {
    [CreateAssetMenu(menuName = "Dialogue/Character")]
    public class CharacterSO : ScriptableObject {
        public string characterName;
        public Color nameColor = Color.white;
        public Color textColor = Color.white;
    }
}