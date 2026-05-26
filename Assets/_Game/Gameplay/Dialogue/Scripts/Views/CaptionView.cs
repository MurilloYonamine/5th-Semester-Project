// autor: Murillo Gomes Yonamine
// data: 24/05/2026

using System;
using TMPro;
using UnityEngine;

namespace FifthSemester.Gameplay.Dialogue {
    public class CaptionView : TextViewBase {
        public void SetCaption(string captionText) {
            AnimateText(captionText);
        }

        public void SetCaption(string captionText, Action onComplete) {
            AnimateText(captionText, onComplete);
        }

        public void ClearCaption() {
            SetTextInstantly(string.Empty);
        }

        public override void Hide() {
            ClearCaption();
            base.Hide();
        }
    }
}
