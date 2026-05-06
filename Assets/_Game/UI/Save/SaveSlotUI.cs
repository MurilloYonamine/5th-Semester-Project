// Autor: Generated
// Data: 05/05/2026

using System;
using UnityEngine;
using UnityEngine.UI;
using FifthSemester.Core.Services;
using TMPro;

namespace FifthSemester.UI {
    public class SaveSlotUI : MonoBehaviour {
        [Header("Images")]
        [SerializeField] private RawImage _snapshotImage;
        [SerializeField] private RawImage _placeholderImageHolder;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _dateText;

        [Header("Buttons")]
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

                if (_snapshotImage != null && _placeholderImageHolder != null) {
                    if (!string.IsNullOrEmpty(data.ScreenshotBase64)) {
                        try {
                            byte[] bytes = Convert.FromBase64String(data.ScreenshotBase64);
                            Texture2D tex = new Texture2D(2, 2);
                            if (tex.LoadImage(bytes)) {
                                _snapshotImage.texture = tex;
                                _snapshotImage.gameObject.SetActive(true);
                                _placeholderImageHolder.gameObject.SetActive(false);
                            } else {
                                UnityEngine.Object.Destroy(tex);
                                _snapshotImage.gameObject.SetActive(false);
                                _placeholderImageHolder.texture = placeholderSprite != null ? placeholderSprite.texture : null;
                                _placeholderImageHolder.gameObject.SetActive(true);
                            }
                        } catch (Exception) {
                            _snapshotImage.gameObject.SetActive(false);
                            _placeholderImageHolder.texture = placeholderSprite != null ? placeholderSprite.texture : null;
                            _placeholderImageHolder.gameObject.SetActive(true);
                        }
                    } else {
                        _snapshotImage.gameObject.SetActive(false);
                        _placeholderImageHolder.texture = placeholderSprite != null ? placeholderSprite.texture : null;
                        _placeholderImageHolder.gameObject.SetActive(true);
                    }
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
