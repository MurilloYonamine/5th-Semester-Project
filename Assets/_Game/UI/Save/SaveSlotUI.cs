// Autor: Generated
// Data: 05/05/2026

using System;
using UnityEngine;
using UnityEngine.UI;
using FifthSemester.Core.Services;
using TMPro;

namespace FifthSemester.UI {
    public class SaveSlotUI : MonoBehaviour {
        [SerializeField] private RawImage _snapshotImage;
        [SerializeField] private RawImage _placeholderImageHolder;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _dateText;
        [SerializeField] private Button _loadButton;
        [SerializeField] private Button _deleteButton;

        private string _slotId;

        public void Setup(string slotId, SaveData data, Action onLoad, Action onDelete, Sprite placeholderSprite) {
            _slotId = slotId;

            if (data != null) {
                _titleText.text = data.LastCheckpointId ?? "Saved";

                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime dt = epoch.AddSeconds(data.Timestamp).ToLocalTime();
                _dateText.text = dt.ToString("g");

                // no screenshot field yet; show placeholder
                if (_snapshotImage != null && _placeholderImageHolder != null) {
                    _snapshotImage.gameObject.SetActive(false);
                    _placeholderImageHolder.texture = placeholderSprite != null ? placeholderSprite.texture : null;
                    _placeholderImageHolder.gameObject.SetActive(true);
                }

                _loadButton.interactable = true;
            } else {
                _titleText.text = "Empty";
                _dateText.text = "";
                if (_snapshotImage != null && _placeholderImageHolder != null) {
                    _snapshotImage.gameObject.SetActive(false);
                    _placeholderImageHolder.texture = placeholderSprite != null ? placeholderSprite.texture : null;
                    _placeholderImageHolder.gameObject.SetActive(true);
                }
                _loadButton.interactable = false;
            }

            _loadButton.onClick.RemoveAllListeners();
            _deleteButton.onClick.RemoveAllListeners();

            _loadButton.onClick.AddListener(() => onLoad?.Invoke());
            _deleteButton.onClick.AddListener(() => onDelete?.Invoke());
        }

        private void OnDisable() {
            _loadButton.onClick.RemoveAllListeners();
            _deleteButton.onClick.RemoveAllListeners();
        }
    }
}
