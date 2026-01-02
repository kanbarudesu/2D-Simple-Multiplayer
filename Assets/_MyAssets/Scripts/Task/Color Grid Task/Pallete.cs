using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class Pallete : MonoBehaviour
{
    [SerializeField] private Image palleteImage;
    [SerializeField] private Image palleteBorderImage;
    [SerializeField] private float tweenDelay = 0.25f;

    public Color Color { get; private set; }
    public Vector2 gridPosition { get; private set; }

    public bool IsColorPallete { get; set; }

    public void InitializePallete(Color color, Vector2 gridPosition)
    {
        Color = color;
        palleteImage.color = color;
        this.gridPosition = gridPosition;

        if (palleteBorderImage != null)
            palleteBorderImage.enabled = false;
    }

    public void EnableSelected(bool isSelected)
    {
        if (palleteBorderImage == null) return;
        palleteBorderImage.enabled = isSelected;
    }

    public void SetColor(Color color)
    {
        Color = color;
        Tween.Color(palleteImage, color, tweenDelay);
        Tween.Scale(transform as RectTransform, new Vector2(1.2f, 1.2f), tweenDelay, Ease.InBounce, 2, CycleMode.Rewind);
    }
}