using UnityEngine;

public class CanvasGroupInteractable : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    public void SetVisibility(bool value)
    {
        canvasGroup.alpha = value ? 1f : 0f;
        canvasGroup.interactable = value;
        canvasGroup.blocksRaycasts = value;
    }
}
