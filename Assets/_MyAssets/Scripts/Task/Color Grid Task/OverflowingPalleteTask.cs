using System.Collections.Generic;
using System.Threading.Tasks;
using EasyTextEffects;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OverflowingPalleteTask : BaseTaskArea
{
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private GraphicRaycaster raycaster;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private Pallete palletePrefab;
    [SerializeField] private Pallete selectedPalletePrefab;
    [SerializeField] private Transform gridParent;
    [SerializeField] private Transform palleteParent;
    [SerializeField] private Vector2 gridSize;
    [SerializeField] private float fillDelay = 0.1f;

    [Header("Completed Text")]
    [SerializeField] private TextEffect completeTextEffect;

    [SerializeField] private Color[] colors;

    private Pallete[,] palleteGrid;
    private Pallete currentSelectedPallete;

    private int randomSeed;

    private bool canStartFill;

    protected override void Start()
    {
        base.Start();
        contentRect.anchoredPosition = new Vector2(0f, 1080f);
        randomSeed = Random.Range(0, 999);
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.TryGetComponent(out Pallete pallete))
                {
                    if (pallete.IsColorPallete)
                    {
                        currentSelectedPallete?.EnableSelected(false);
                        currentSelectedPallete = pallete;
                        currentSelectedPallete.EnableSelected(true);
                    }
                    if (currentSelectedPallete != null && !pallete.IsColorPallete)
                    {
                        FloodFill((int)pallete.gridPosition.x, (int)pallete.gridPosition.y, currentSelectedPallete.Color);
                    }
                    return;
                }
            }
        }
    }

    private void StartTask()
    {
        foreach (Transform transform in gridParent)
        {
            Destroy(transform.gameObject);
        }
        foreach (Transform transform in palleteParent)
        {
            Destroy(transform.gameObject);
        }

        InitializeTask();
        InitializePallete();
        completeTextEffect.gameObject.SetActive(false);
        canStartFill = true;
    }

    private void InitializeTask()
    {
        palleteGrid = new Pallete[(int)gridSize.x, (int)gridSize.y];
        System.Random random = new System.Random(randomSeed);

        bool isHorizontal = gridSize.x > gridSize.y;
        gridLayoutGroup.constraint = isHorizontal ? GridLayoutGroup.Constraint.FixedColumnCount : GridLayoutGroup.Constraint.FixedRowCount;
        gridLayoutGroup.constraintCount = isHorizontal ? (int)gridSize.x : (int)gridSize.y;

        for (int x = 0; x < gridSize.y; x++)
        {
            for (int y = 0; y < gridSize.x; y++)
            {
                var pallete = Instantiate(palletePrefab, gridParent);
                pallete.InitializePallete(colors[random.Next(0, colors.Length)], new Vector2(y, x));
                palleteGrid[y, x] = pallete;
            }
        }
    }

    private void InitializePallete()
    {
        foreach (var color in colors)
        {
            var pallete = Instantiate(selectedPalletePrefab, palleteParent);
            pallete.InitializePallete(color, Vector2.zero);
            pallete.IsColorPallete = true;
        }
    }

    private async void FloodFill(int row, int colomn, Color newColor)
    {
        if (!canStartFill) return;
        canStartFill = false;

        var oldColor = palleteGrid[row, colomn].Color;
        if (oldColor.Equals(newColor))
        {
            canStartFill = true;
            return;
        }

        var palleteQ = new Queue<(int, int)>();
        palleteQ.Enqueue((row, colomn));

        // Change the starting pallete color
        palleteGrid[row, colomn].SetColor(newColor);

        // Direction vectors for adjacent pixels
        int[] dirX = { -1, 1, 0, 0 };
        int[] dirY = { 0, 0, -1, 1 };

        while (palleteQ.Count > 0)
        {
            var (x, y) = palleteQ.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                int neighbourX = x + dirX[i];
                int neighbourY = y + dirY[i];

                if (neighbourX >= 0 &&
                    neighbourX < gridSize.x &&
                    neighbourY >= 0 &&
                    neighbourY < gridSize.y &&
                    ColorsAreEqual(oldColor, palleteGrid[neighbourX, neighbourY].Color))
                {
                    palleteGrid[neighbourX, neighbourY].SetColor(newColor);
                    palleteQ.Enqueue((neighbourX, neighbourY));
                    await Task.Delay((int)(fillDelay * 1000));
                }
            }
        }

        if (IsPalleteCompleted(newColor))
        {
            completeTextEffect.gameObject.SetActive(true);
            completeTextEffect.StartManualEffect("entry");
            return;
        }

        canStartFill = true;
    }

    private bool IsPalleteCompleted(Color color)
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                if (!ColorsAreEqual(palleteGrid[x, y].Color, color))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private bool ColorsAreEqual(Color a, Color b, float tolerance = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance &&
               Mathf.Abs(a.a - b.a) < tolerance;
    }

    public override void HideTask()
    {
        base.HideTask();
        Tween.UIAnchoredPositionY(contentRect, 1080f, 0.5f).OnComplete(() => isTaskVisible = false);
    }

    public override void ShowTask()
    {
        if (isTaskVisible) return;

        isTaskVisible = true;
        StartTask();
        Tween.UIAnchoredPositionY(contentRect, 0f, 0.5f);
    }

    public override void TaskCompleted()
    {
        base.TaskCompleted();
        randomSeed = Random.Range(0, 999);
    }
}
