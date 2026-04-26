using UnityEngine;
using UnityEngine.EventSystems;
using FifthSemester.Core.Services;
using FifthSemester.Core.Audio;

namespace FifthSemester.Framework.UI {
    /// <summary>
    /// Selector base genérico para navegação e seleção de opções.
    /// Subclasses devem implementar visualização, persistência e aplicação do valor.
    /// </summary>
    public abstract class Selector<T> : MonoBehaviour,
        ISelectHandler, IDeselectHandler,
        IPointerEnterHandler, IPointerExitHandler,
        IMoveHandler, ISubmitHandler, ICancelHandler, IPointerClickHandler {

        private IAudioService _audioService;

        [Header("Selector State")]
        [SerializeField] protected int _currentIndex = 0;
        [SerializeField] protected T[] _items;

        protected bool _isFocused = false;
        protected bool _isHovered = false;
        private bool _interactionLock = false;

        /// <summary>
        /// Inicialização padrão: carrega valor salvo e atualiza UI.
        /// </summary>
        protected virtual void Start() {
            OnLoad();
            RefreshUI();
            _audioService = ServiceLocator.Get<IAudioService>();
        }

        /// <summary>
        /// Avança para o próximo item.
        /// </summary>
        public virtual void Next() {
            if (_items == null || _items.Length == 0) return;
            _currentIndex = (_currentIndex + 1) % _items.Length;
            OnValueChanged();
        }

        /// <summary>
        /// Volta para o item anterior.
        /// </summary>
        public virtual void Previous() {
            if (_items == null || _items.Length == 0) return;
            _currentIndex = (_currentIndex - 1 + _items.Length) % _items.Length;
            OnValueChanged();
        }

        /// <summary>
        /// Chama ciclo de atualização após mudança de valor.
        /// </summary>
        protected virtual void OnValueChanged() {
            //PlaySound(); // Som agora deve ser controlado externamente (ex: Event Trigger)
            UpdateUI();
            OnItemSelected(_items[_currentIndex]);
            OnSave();
        }

        /// <summary>
        /// Atualiza UI e visuais.
        /// </summary>
        protected virtual void RefreshUI() {
            UpdateUI();
        }

        // --- Métodos a serem implementados pelas subclasses ---

        /// <summary>
        /// O que acontece quando um item é selecionado (aplicar valor, etc).
        /// </summary>
        protected abstract void OnItemSelected(T selectedItem);
        /// <summary>
        /// Salvar valor selecionado (PlayerPrefs, config, etc).
        /// </summary>
        protected abstract void OnSave();
        /// <summary>
        /// Carregar valor salvo.
        /// </summary>
        protected abstract void OnLoad();
        /// <summary>
        /// Atualizar UI textual/visual.
        /// </summary>
        protected abstract void UpdateUI();
        /// <summary>
        /// Atualizar elementos visuais (cor, highlight, etc).
        /// </summary>
        protected abstract void UpdateVisualElements(Color targetColor);

        // --- EventSystem: navegação universal (mouse, teclado, controle) ---


        // Evita duplo disparo de Next/PlaySound

        public virtual void OnPointerClick(PointerEventData eventData) {
            if (_interactionLock) return;
            if (eventData.pointerId >= 0) {
                _interactionLock = true;
                HandleInteraction();
            }
        }

        public virtual void OnSubmit(BaseEventData eventData) {
            if (_interactionLock) return;
            var pointer = eventData as PointerEventData;
            if (pointer == null || pointer.pointerId < 0) {
                _interactionLock = true;
                HandleInteraction();
            }
        }

        private void LateUpdate() {
            // Libera lock a cada frame
            _interactionLock = false;
        }

        /// <summary>
        /// Lógica de interação: foca ou avança.
        /// </summary>
        protected virtual void HandleInteraction() {
            if (!_isFocused) {
                _isFocused = true;
                if (EventSystem.current.currentSelectedGameObject != gameObject)
                    EventSystem.current.SetSelectedGameObject(gameObject);
            }
            else {
                Next();
            }
        }

        public virtual void OnCancel(BaseEventData eventData) {
            if (_isFocused) {
                _isFocused = false;
            }
        }

        public virtual void OnMove(AxisEventData eventData) {
            if (!_isFocused) return;
            if (eventData.moveDir == MoveDirection.Right) Next();
            else if (eventData.moveDir == MoveDirection.Left) Previous();
            eventData.Use();
        }

        public virtual void OnSelect(BaseEventData eventData) {
            _isHovered = true;
        }
        public virtual void OnDeselect(BaseEventData eventData) {
            _isHovered = false;
            _isFocused = false;
        }
        public virtual void OnPointerEnter(PointerEventData eventData) {
            _isHovered = true;
        }
        public virtual void OnPointerExit(PointerEventData eventData) {
            _isHovered = false;
        }
    }
}
