// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FifthSemester.Core.Services;
using FifthSemester.Core.Enums;
using FifthSemester.Gameplay.Save;
using FifthSemester.UI;

namespace FifthSemester.Gameplay.Menu {
    public class LoadGameView : MenuViewBase {
        [SerializeField] private SaveSlotUI[] _slots = new SaveSlotUI[3];
        [SerializeField] private Sprite _noSavePlaceholder;
        [SerializeField] private Button _backButton;

        private ISaveService _saveService;

        protected override MenuScreen MenuScreenType => MenuScreen.LoadGame;

        protected override void Start() {
            _saveService = ServiceLocator.Get<ISaveService>();
            base.Start();
            _backButton.onClick.AddListener(OnBack);
            Hide();
        }

        public override void OnShow() {
            base.OnShow();
            gameObject.SetActive(true);
            Refresh();
        }

        public override void OnHide() {
            base.OnHide();
            Hide();
        }

        private void Hide() {
            gameObject.SetActive(false);
        }

        public void Refresh() {
            if (_saveService == null || _slots == null) return;

            for (int i = 0; i < 3; i++) {
                if (i >= _slots.Length || _slots[i] == null) continue;

                string slotId = $"slot_{i}";
                SaveData data = _saveService.SlotExists(slotId) ? _saveService.LoadFromSlot(slotId) : null;

                _slots[i].Setup(slotId, data,
                    onLoad: () => {
                        PlayMenuPrimarySfx();
                        SaveLoader.SetPendingSave(data);
                        SceneManager.LoadScene("Gym");
                    },
                    onDelete: () => {
                        _saveService.DeleteSlot(slotId);
                        Refresh();
                    },
                    placeholderSprite: _noSavePlaceholder);
            }
        }

        private void OnBack() {
            PlayMenuBackSfx();
            _menuService.Show(MenuScreen.MainMenu);
        }
    }
}