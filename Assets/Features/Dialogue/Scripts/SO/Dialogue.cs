using UnityEngine;

namespace FifthSemester.Dialogue {
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue")]
    public class Dialogue : ScriptableObject {
        [field: SerializeField] public Character Character { get; private set; }
        [field: SerializeField, TextArea] public string[] Sentences { get; private set; }
    }
}
