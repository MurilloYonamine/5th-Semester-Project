using UnityEngine;

namespace FifthSemester.Dialogue {
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue")]
    public class DialogueData : ScriptableObject {
        [field: SerializeField] public CharacterData Character { get; private set; }
        [field: SerializeField, TextArea] public string[] Sentences { get; private set; }
    }
}
