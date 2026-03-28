using TMPro;
using UnityEngine;

namespace FifthSemester.Dialogue {
    [System.Serializable]
    public class DialogueContainer {
        public GameObject root;
        public NameContainer nameContainer;
        public TextMeshProUGUI dialogueText;

        private bool initialized = false;
        public void Initialize() {
            if (initialized)
                return;

            initialized = true;
        }
        public void SetDialogueColor(Color color) => dialogueText.color = color;
        public void SetDialogueFont(TMP_FontAsset font) => dialogueText.font = font;
        public void SetDialogueFontSize(float size) => dialogueText.fontSize = size;
    }
}
