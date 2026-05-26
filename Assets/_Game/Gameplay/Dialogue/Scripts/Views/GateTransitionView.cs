// autor: Murillo Gomes Yonamine
// data: 25/05/2026

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Dialogue {
    [RequireComponent(typeof(CanvasGroup))]
    public class GateTransitionView : TextViewBase {
        [Header("Gate Transition")]
        [SerializeField] private Image _background;

        public void ShowMessage(string message, Action onComplete = null) {
            if (_background != null) {
                _background.color = Color.black;
            }

            Show();
            AnimateText(message, onComplete);
        }

        public void ClearMessage() {
            SetTextInstantly(string.Empty);
        }

        public override void Hide() {
            ClearMessage();
            base.Hide();
        }
    }
}