using TMPro;
using UnityEngine;

namespace FifthSemester.Dialogue {
    [CreateAssetMenu(fileName = "New Character", menuName = "Dialogue/Character")]
    public class CharacterData : ScriptableObject {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public Sprite Portrait { get; private set; }

        [Header("Text Colors")]
        [field: SerializeField] public Color TextColor { get; private set; } = Color.white;
        [field: SerializeField] public Color NameColor { get; private set; } = Color.white;

        [field: SerializeField] public TMP_FontAsset Font { get; private set; }

        public void ChangeTextColor(Color newTextColor) {
            TextColor = newTextColor;
        }
        public void ChangeNameColor(Color newNameColor) {
            NameColor = newNameColor;
        }
    }
}
