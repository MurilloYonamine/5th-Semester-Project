using FifthSemester.Gameplay.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Inventory {
    public class InventorySlot : MonoBehaviour {
        [Header("UI References")]
        [SerializeField] private RawImage _itemDisplay;
        [SerializeField] private RenderTexture _renderTexture;

        [Header("3D Preview Setup")]
        [SerializeField] private Camera _previewCamera;
        [SerializeField] private Transform _itemContainer;

        [Header("Item Display Settings")]
        [SerializeField] private Vector3 _itemScale = Vector3.one;
        [SerializeField] private Vector3 _itemRotation = new Vector3(-15, 45, 0);
        [SerializeField] private float _cameraDistance = 2f;
        [SerializeField] private bool _autoRotate = false;
        [SerializeField] private float _rotationSpeed = 30f;

        [Header("Background Settings")]
        [SerializeField] private Color _backgroundColor = new Color(0, 0, 0, 0);

        private GameObject _currentItem;
        private IInteractable _item;

        private void Awake() {
            SetupPreviewCamera();
        }

        private void SetupPreviewCamera() {
            if (_previewCamera == null) return;

            if (_renderTexture == null) {
                _renderTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
                _renderTexture.antiAliasing = 4;
            }

            _previewCamera.targetTexture = _renderTexture;
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = _backgroundColor;
            _previewCamera.cullingMask = 1 << LayerMask.NameToLayer("InventoryPreview");
            _previewCamera.orthographic = true;
            _previewCamera.enabled = false;

            if (_itemDisplay != null) {
                _itemDisplay.texture = _renderTexture;
            }
        }

        public void SetItem(IInteractable item) {
            ClearItem();

            _item = item;

            if (_item is MonoBehaviour mono) {
                CreateItemPreview(mono.gameObject);
            }
        }

        private void CreateItemPreview(GameObject original) {
            if (_itemContainer == null || original == null) return;

            _currentItem = Instantiate(original, _itemContainer);
            _currentItem.SetActive(true);

            _currentItem.transform.localPosition = Vector3.zero;
            _currentItem.transform.localRotation = Quaternion.Euler(_itemRotation);
            _currentItem.transform.localScale = _itemScale;

            SetLayerRecursively(_currentItem, LayerMask.NameToLayer("InventoryPreview"));
            DisableGameplayComponents(_currentItem);

            PositionCamera();
            RenderPreview();
        }

        private void DisableGameplayComponents(GameObject obj) {
            foreach (var mono in obj.GetComponentsInChildren<MonoBehaviour>()) {
                mono.enabled = false;
            }

            foreach (var col in obj.GetComponentsInChildren<Collider>()) {
                col.enabled = false;
            }

            foreach (var rb in obj.GetComponentsInChildren<Rigidbody>()) {
                rb.isKinematic = true;
            }
        }

        private void SetLayerRecursively(GameObject obj, int newLayer) {
            obj.layer = newLayer;
            foreach (Transform child in obj.transform) {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        private void PositionCamera() {
            if (_previewCamera == null || _currentItem == null) return;

            Bounds bounds = CalculateBounds(_currentItem);
            float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

            _previewCamera.transform.position =
                _itemContainer.position + Vector3.back * (_cameraDistance + maxSize);

            _previewCamera.transform.LookAt(bounds.center);
            _previewCamera.orthographicSize = maxSize * 0.6f;
        }

        private Bounds CalculateBounds(GameObject obj) {
            var renderers = obj.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
                return new Bounds(obj.transform.position, Vector3.one);

            Bounds bounds = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++) {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private void RenderPreview() {
            if (_previewCamera == null) return;

            _previewCamera.enabled = true;
            _previewCamera.Render();

            if (!_autoRotate)
                _previewCamera.enabled = false;
        }

        private void Update() {
            if (_autoRotate && _currentItem != null) {
                _currentItem.transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
                _previewCamera.Render();
            }
        }

        public void ClearItem() {
            if (_currentItem != null) {
                Destroy(_currentItem);
                _currentItem = null;
            }

            _item = null;
        }

        private void OnDestroy() {
            ClearItem();

            if (_renderTexture != null) {
                _previewCamera.targetTexture = null;
                _renderTexture.Release();
            }
        }
    }
}