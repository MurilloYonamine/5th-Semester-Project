using UnityEngine;
using UnityEngine.UI;
using FifthSemester.Items;

namespace FifthSemester.Inventory {
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
            _previewCamera.orthographicSize = 1f;
            _previewCamera.enabled = false;

            if (_itemDisplay != null) {
                _itemDisplay.texture = _renderTexture;
            }
        }

        public void SetItem(IInteractable item) {
            ClearItem();

            _item = item;

            if (_item != null && _item is MonoBehaviour itemMono) {
                CreateItemPreview(itemMono.gameObject);
            }
        }

        private void CreateItemPreview(GameObject itemPrefab) {
            if (_itemContainer == null || itemPrefab == null) return;

            _currentItem = new GameObject("ItemPreview");
            _currentItem.transform.SetParent(_itemContainer);
            _currentItem.transform.localPosition = Vector3.zero;
            _currentItem.transform.localRotation = Quaternion.Euler(_itemRotation);
            _currentItem.transform.localScale = _itemScale;
            _currentItem.layer = LayerMask.NameToLayer("InventoryPreview");

            CopyMeshesAndMaterials(itemPrefab, _currentItem);

            PositionCamera();
            RenderPreview();
        }

        private void CopyMeshesAndMaterials(GameObject source, GameObject destination) {
            MeshFilter[] meshFilters = source.GetComponentsInChildren<MeshFilter>();

            foreach (MeshFilter meshFilter in meshFilters) {
                if (meshFilter.sharedMesh == null) continue;

                GameObject meshObj = new GameObject(meshFilter.gameObject.name);
                meshObj.transform.SetParent(destination.transform);
                meshObj.transform.localPosition = meshFilter.transform.localPosition;
                meshObj.transform.localRotation = meshFilter.transform.localRotation;
                meshObj.transform.localScale = meshFilter.transform.localScale;
                meshObj.layer = LayerMask.NameToLayer("InventoryPreview");

                MeshFilter newMeshFilter = meshObj.AddComponent<MeshFilter>();
                newMeshFilter.sharedMesh = meshFilter.sharedMesh;

                MeshRenderer sourceRenderer = meshFilter.GetComponent<MeshRenderer>();
                if (sourceRenderer != null) {
                    MeshRenderer newRenderer = meshObj.AddComponent<MeshRenderer>();
                    newRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
                }
            }

            SkinnedMeshRenderer[] skinnedRenderers = source.GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer skinnedRenderer in skinnedRenderers) {
                if (skinnedRenderer.sharedMesh == null) continue;

                GameObject meshObj = new GameObject(skinnedRenderer.gameObject.name);
                meshObj.transform.SetParent(destination.transform);
                meshObj.transform.localPosition = skinnedRenderer.transform.localPosition;
                meshObj.transform.localRotation = skinnedRenderer.transform.localRotation;
                meshObj.transform.localScale = skinnedRenderer.transform.localScale;
                meshObj.layer = LayerMask.NameToLayer("InventoryPreview");

                MeshFilter newMeshFilter = meshObj.AddComponent<MeshFilter>();
                newMeshFilter.sharedMesh = skinnedRenderer.sharedMesh;

                MeshRenderer newRenderer = meshObj.AddComponent<MeshRenderer>();
                newRenderer.sharedMaterials = skinnedRenderer.sharedMaterials;
            }
        }

        private void PositionCamera() {
            if (_previewCamera == null || _currentItem == null) return;

            Bounds bounds = CalculateBounds(_currentItem);
            float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

            _previewCamera.transform.position = _itemContainer.position + Vector3.back * (_cameraDistance + maxSize);
            _previewCamera.transform.LookAt(_itemContainer.position + bounds.center);
            _previewCamera.orthographicSize = maxSize * 0.6f;
        }

        private Bounds CalculateBounds(GameObject obj) {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(obj.transform.position, Vector3.one);

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

            if (!_autoRotate) {
                _previewCamera.enabled = false;
            }
        }

        private void Update() {
            if (_autoRotate && _currentItem != null) {
                _currentItem.transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
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
                _renderTexture.Release();
            }
        }
    }
}
