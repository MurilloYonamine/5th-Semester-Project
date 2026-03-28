using TMPro;
using UnityEngine;

namespace FifthSemester.Dialogue {
    [System.Serializable]
    public class NameContainer {
        [SerializeField] private GameObject root;
        [SerializeField] public TextMeshProUGUI _nameText;

        public void Show(string nameToShow = "") {
            root.SetActive(true);

            if (nameToShow != string.Empty)
                _nameText.text = nameToShow;
        }

        public void Hide() {
            root.SetActive(false);
        }

        public void SetNameColor(Color color) => _nameText.color = color;
        public void SetNameFont(TMP_FontAsset font) => _nameText.font = font;
        public void SetNameFontSize(float size) => _nameText.fontSize = size;
    }
}
