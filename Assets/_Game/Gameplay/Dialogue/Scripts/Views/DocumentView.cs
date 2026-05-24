// autor: Murillo Gomes Yonamine
// data: 24/05/2026

using TMPro;
using UnityEngine;

namespace FifthSemester.Gameplay.Dialogue {
    public class DocumentView : TextViewBase {
        [Header("Document")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _pageCounterText;

        private string[] _pages = new string[0];
        private int _currentPageIndex;

        public void SetDocument(DocumentData documentData) {
            _pages = documentData.Pages != null ? documentData.Pages : new string[0];
            _currentPageIndex = 0;

            if (_titleText != null) {
                _titleText.text = documentData.Title;
            }

            RefreshPage();
        }

        public void NextPage() {
            if (_pages == null || _pages.Length == 0 || _currentPageIndex >= _pages.Length - 1) {
                return;
            }

            _currentPageIndex++;
            RefreshPage();
        }

        public void PreviousPage() {
            if (_pages == null || _pages.Length == 0 || _currentPageIndex <= 0) {
                return;
            }

            _currentPageIndex--;
            RefreshPage();
        }

        public void ClearDocument() {
            _pages = new string[0];
            _currentPageIndex = 0;

            if (_titleText != null) {
                _titleText.text = string.Empty;
            }

            if (_pageCounterText != null) {
                _pageCounterText.text = string.Empty;
            }

            SetTextInstantly(string.Empty);
        }

        public override void Hide() {
            ClearDocument();
            base.Hide();
        }

        private void RefreshPage() {
            if (_pages == null || _pages.Length == 0) {
                SetTextInstantly(string.Empty);
                if (_pageCounterText != null) {
                    _pageCounterText.text = string.Empty;
                }
                return;
            }

            string pageText = _pages[_currentPageIndex] != null ? _pages[_currentPageIndex].Trim() : string.Empty;
            AnimateText(pageText);

            if (_pageCounterText != null) {
                _pageCounterText.text = string.Format("{0} / {1}", _currentPageIndex + 1, _pages.Length);
            }
        }
    }
}
