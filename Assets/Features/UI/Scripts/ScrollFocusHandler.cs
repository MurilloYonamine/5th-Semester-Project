using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollFocusHandler : MonoBehaviour {
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private RectTransform _contentPanel;
    [SerializeField] private float _lerpSpeed = 10f;

    private RectTransform _selectedRectTransform;

    void Update() {
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        if (currentSelected == null) return;

        if (!currentSelected.transform.IsChildOf(_contentPanel.transform)) return;

        _selectedRectTransform = currentSelected.GetComponent<RectTransform>();

        if (_selectedRectTransform != null) {
            SnapToSelected();
        }
    }

    private void SnapToSelected() {
        Vector2 localCursor = _scrollRect.transform.InverseTransformPoint(_selectedRectTransform.position);
        Vector2 localContent = _scrollRect.transform.InverseTransformPoint(_contentPanel.position);

        float targetY = localContent.y - localCursor.y;

        Vector2 targetPos = _contentPanel.anchoredPosition;
        targetPos.y = targetY - (_scrollRect.viewport.rect.height / 2f);

        if (targetPos.y < 0f) {
            targetPos.y = 0f;
        }

        _contentPanel.anchoredPosition = Vector2.Lerp(_contentPanel.anchoredPosition, targetPos, Time.deltaTime * _lerpSpeed);
    }
}
