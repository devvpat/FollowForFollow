using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class DraggableWindow : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerDownHandler
{
    [Tooltip("The window root to move. Defaults to this transform's parent.")]
    public RectTransform windowRoot;

    private RectTransform parentRect;
    private RectTransform canvasRect;
    private Vector2 lastLocalPointer;

    void Awake()
    {
        if (windowRoot == null)
            windowRoot = transform.parent as RectTransform;

        if (windowRoot != null)
            parentRect = windowRoot.parent as RectTransform;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            canvasRect = canvas.rootCanvas.transform as RectTransform;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (windowRoot != null)
            windowRoot.SetAsLastSibling();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (parentRect == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, eventData.pressEventCamera, out lastLocalPointer);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (windowRoot == null || parentRect == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, eventData.pressEventCamera, out Vector2 nowLocal))
        {
            windowRoot.anchoredPosition += nowLocal - lastLocalPointer;
            lastLocalPointer = nowLocal;
            ClampInsideCanvas();
        }
    }

    private void ClampInsideCanvas()
    {
        if (canvasRect == null || parentRect == null) return;

        // Clamp the title bar (this transform — the drag handle), not the whole window.
        // The body of the window is allowed to drag off-screen; the title bar stays grabbable.
        RectTransform titleBar = transform as RectTransform;
        if (titleBar == null) return;

        Vector3[] titleCorners = new Vector3[4];
        Vector3[] canvasCorners = new Vector3[4];
        titleBar.GetWorldCorners(titleCorners);
        canvasRect.GetWorldCorners(canvasCorners);

        Vector2 worldOverflow = Vector2.zero;
        if (titleCorners[0].x < canvasCorners[0].x) worldOverflow.x = canvasCorners[0].x - titleCorners[0].x;
        else if (titleCorners[2].x > canvasCorners[2].x) worldOverflow.x = canvasCorners[2].x - titleCorners[2].x;
        if (titleCorners[0].y < canvasCorners[0].y) worldOverflow.y = canvasCorners[0].y - titleCorners[0].y;
        else if (titleCorners[2].y > canvasCorners[2].y) worldOverflow.y = canvasCorners[2].y - titleCorners[2].y;

        if (worldOverflow == Vector2.zero) return;

        Vector3 parentScale = parentRect.lossyScale;
        Vector2 localOverflow = Vector2.zero;
        if (parentScale.x != 0f) localOverflow.x = worldOverflow.x / parentScale.x;
        if (parentScale.y != 0f) localOverflow.y = worldOverflow.y / parentScale.y;

        windowRoot.anchoredPosition += localOverflow;
    }
}
