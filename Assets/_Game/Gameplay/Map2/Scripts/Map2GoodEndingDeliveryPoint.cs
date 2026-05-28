using System.Collections;
using System.Collections.Generic;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using FifthSemester.Gameplay.Inventory;
using FifthSemester.Gameplay.Shared;
using TMPro;
using ThirdParty.QuickOutline;
using UnityEngine;
using UnityEngine.Video;

namespace FifthSemester.Gameplay.Map2 {
    [RequireComponent(typeof(Outline))]
    public class Map2GoodEndingDeliveryPoint : MonoBehaviour, IInteractable {
        private const string TAG = "<color=green>[GoodEndingDelivery]</color>";

        [Header("Configurações do Item")]
        [Tooltip("ID do item necessário para concluir o final bom (ex: gasoline_canister).")]
        [SerializeField] private string _requiredItemId = "gasoline_canister";

        [Header("Final Bom (Cutscene)")]
        [Tooltip("Prefab contendo a RawImage e o VideoPlayer para o Final Bom.")]
        [SerializeField] private GameObject _goodEndingPrefab;
        [SerializeField] private float _fadeDuration = 2f;

        [Header("Configurações Visuais e UI")]
        [SerializeField] private TextMeshPro _interactionPromptText;
        [SerializeField] private string _deliverPromptText = "Entregar Gasolina";
        [SerializeField] private string _completedPromptText = "Área Limpa";

        [Header("Áudio")]
        [SerializeField] private AudioClip _successSound;
        [SerializeField] private AudioClip _failureSound;

        private Outline _outline;
        private IInventoryService<Item> _inventoryService;
        private IAudioService _audioService;
        private Color _unlockedColor;
        private bool _isCompleted = false;

        public bool IsInteractable => !_isCompleted;
        public string Id => gameObject.name;

        private void Awake() {
            _outline = GetComponent<Outline>();
            if (_outline != null) {
                _outline.enabled = false;
            }
            _unlockedColor = new Color32(105, 255, 144, 255);
            UpdateInteractionPrompt();
        }

        private void Start() {
            ServiceLocator.TryGet<IInventoryService<Item>>(out _inventoryService);
            ServiceLocator.TryGet<IAudioService>(out _audioService);
            UpdateInteractionPrompt();
        }

        public void Highlight(bool value) {
            if (_outline != null) {
                _outline.enabled = value;
                _outline.OutlineColor = HasRequiredItem() ? _unlockedColor : Color.red;
            }

            if (_interactionPromptText != null) {
                _interactionPromptText.gameObject.SetActive(value);
                _interactionPromptText.color = HasRequiredItem() ? _unlockedColor : Color.red;
            }
        }

        public void Interact() {
            if (_isCompleted) return;

            if (TryConsumeRequiredItem()) {
                _isCompleted = true;
                UpdateInteractionPrompt();
                PlaySound(_successSound);
                TriggerGoodEnding();
            }
            else {
                PlaySound(_failureSound);
                Debug.LogWarning($"{TAG} O jogador não possui o item necessário: {_requiredItemId}");
            }
        }

        public void StopInteract() {
        }

        private bool HasRequiredItem() {
            if (_inventoryService == null) return false;

            IReadOnlyList<Item> items = _inventoryService.GetItems();
            if (items == null) return false;

            for (int i = 0; i < items.Count; i++) {
                if (items[i] != null && items[i].Id == _requiredItemId) {
                    return true;
                }
            }

            return false;
        }

        private bool TryConsumeRequiredItem() {
            if (_inventoryService == null) return false;

            IReadOnlyList<Item> items = _inventoryService.GetItems();
            if (items == null) return false;

            for (int i = 0; i < items.Count; i++) {
                var item = items[i];
                if (item != null && item.Id == _requiredItemId) {
                    _inventoryService.RemoveItem(item);
                    return true;
                }
            }

            return false;
        }

        private void UpdateInteractionPrompt() {
            if (_interactionPromptText == null) return;
            _interactionPromptText.text = _isCompleted ? _completedPromptText : _deliverPromptText;
        }

        private void PlaySound(AudioClip clip) {
            if (clip == null || _audioService == null) return;
            _audioService.PlaySFX(clip);
        }

        private void TriggerGoodEnding() {
            if (_goodEndingPrefab == null) {
                Debug.LogError($"{TAG} Prefab de Final Bom não configurado!");
                LoadMainMenu();
                return;
            }

            if (ServiceLocator.TryGet<IGameStateService>(out var gameStateService)) {
                gameStateService.ChangeState(GameState.Cutscene);
            }

            GameObject instance = Instantiate(_goodEndingPrefab);
            VideoPlayer videoPlayer = instance.GetComponentInChildren<VideoPlayer>();

            if (videoPlayer != null) {
                StartCoroutine(PlayGoodEndingVideo(videoPlayer, instance));
            }
            else {
                Debug.LogError($"{TAG} Prefab de Final Bom não contém um VideoPlayer!");
                LoadMainMenu();
            }
        }

        private IEnumerator PlayGoodEndingVideo(VideoPlayer videoPlayer, GameObject instance) {
            yield return null;

            bool videoFinished = false;
            videoPlayer.loopPointReached += (vp) => {
                videoFinished = true;
            };

            videoPlayer.Play();

            yield return new WaitUntil(() => videoFinished);

            if (ServiceLocator.TryGet<IFadeService>(out var fadeService)) {
                bool fadeComplete = false;
                fadeService.FadeOut(_fadeDuration, () => fadeComplete = true);
                yield return new WaitUntil(() => fadeComplete);
            }
            else {
                yield return new WaitForSeconds(_fadeDuration);
            }

            Destroy(instance);
            LoadMainMenu();
        }

        private void LoadMainMenu() {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
