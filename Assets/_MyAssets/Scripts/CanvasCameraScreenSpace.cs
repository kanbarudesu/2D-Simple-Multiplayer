using UnityEngine;

public class CanvasCameraScreenSpace : MonoBehaviour
{
    private void Start()
    {
        var canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
    }
}
