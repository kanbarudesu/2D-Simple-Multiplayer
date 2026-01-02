using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Pool;

public class Wire : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public bool IsLeftWire;
    public Color CustomColor;

    private Image image;
    private LineRenderer lineRenderer;

    private Canvas canvas;
    private bool canDrag = false;
    private WireTask wireTask;
    public bool IsSuccess = false;

    private PointerEventData pointerEventData;
    private Vector2 lastMousePos;

    private void Awake()
    {
        image = GetComponent<Image>();
        lineRenderer = GetComponent<LineRenderer>();
        canvas = GetComponentInParent<Canvas>();

        pointerEventData = new PointerEventData(EventSystem.current);
    }

    public void InitializeWire(WireTask wireTask, Color color, bool isLeftWire = true)
    {
        this.wireTask = wireTask;
        SetColor(color);
        IsLeftWire = isLeftWire;
    }

    private void Update()
    {
        DetectHoveredWire();

        if (!canDrag) return;

        if (!IsSuccess)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out Vector2 movePos);
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position + Vector3.right * 1.5f);
            lineRenderer.SetPosition(2, canvas.transform.TransformPoint(movePos));
            lineRenderer.SetPosition(3, canvas.transform.TransformPoint(movePos));
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    private void DetectHoveredWire()
    {
        if (wireTask.CurrentDraggedWire != null && wireTask.CurrentDraggedWire != this) return;

        Vector2 currentMousePos = Input.mousePosition;

        if (currentMousePos == lastMousePos) return;

        lastMousePos = currentMousePos;
        pointerEventData.position = currentMousePos;
        var raycastResults = ListPool<RaycastResult>.Get();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        foreach (RaycastResult result in raycastResults)
        {
            var wire = result.gameObject.GetComponent<Wire>();
            if (wire != null && wire != this && !wire.IsSuccess && !wire.IsLeftWire)
            {
                wireTask.CurrentHoveredWire = wire;
                ListPool<RaycastResult>.Release(raycastResults);
                return;
            }
        }

        wireTask.CurrentHoveredWire = null;
        ListPool<RaycastResult>.Release(raycastResults);
    }

    public void SetColor(Color color)
    {
        image.color = color;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        CustomColor = color;
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsLeftWire || IsSuccess) return;

        canDrag = true;
        wireTask.CurrentDraggedWire = this;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (wireTask.CurrentHoveredWire != null && wireTask.CurrentHoveredWire.CustomColor == CustomColor && !wireTask.CurrentHoveredWire.IsLeftWire)
        {
            IsSuccess = true;
            wireTask.CurrentHoveredWire.IsSuccess = true;
            lineRenderer.SetPosition(2, wireTask.CurrentHoveredWire.transform.position - Vector3.right * 1.5f);
            lineRenderer.SetPosition(3, wireTask.CurrentHoveredWire.transform.position);
            
            wireTask.SetWireSuccess();
        }

        lineRenderer.enabled = IsSuccess;
        canDrag = false;
        wireTask.CurrentDraggedWire = null;
    }
}