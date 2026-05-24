// autor: Murillo Gomes Yonamine
// data: 24/05/2026

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Dialogue {
    public class DialogueView : TextViewBase {
        [SerializeField] private TMP_Text _speakerNameText;

        public void SetDialogue(string speakerName, string dialogueText, Sprite portrait = null, Color? speakerColor = null, Color? dialogueColor = null) {
            if (_speakerNameText != null) {
                _speakerNameText.text = speakerName ?? string.Empty;
                if (speakerColor.HasValue) {
                    _speakerNameText.color = speakerColor.Value;
                }
            }

            if (_contentText != null && dialogueColor.HasValue) {
                _contentText.color = dialogueColor.Value;
            }

            AnimateText(dialogueText);
        }

        public void ClearDialogue() {
            if (_speakerNameText != null) {
                _speakerNameText.text = string.Empty;
            }

            SetTextInstantly(string.Empty);
        }

        public override void Hide() {
            ClearDialogue();
            base.Hide();
        }
    }
}
