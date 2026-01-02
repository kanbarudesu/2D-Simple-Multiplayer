using System.Collections.Generic;
using EasyTextEffects;
using PrimeTween;
using UnityEngine;

public class WireTask : BaseTaskArea
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Material wireMaterial;

    [SerializeField] private Wire wirePrefab;
    [SerializeField] private List<Color> wireColors = new List<Color>();

    [SerializeField] private Transform wireParentLeft, wireParentRight;

    [SerializeField] private TextEffect completeTextEffect;

    private List<Wire> leftWires = new List<Wire>();
    private List<Wire> rightWires = new List<Wire>();

    public Wire CurrentDraggedWire { get; set; }
    public Wire CurrentHoveredWire { get; set; }

    private int currentSuccessWires = 0;
    private int randomSeed;

    protected override void Start()
    {
        Hide(0f);
        randomSeed = Random.Range(0, 999);
        ResetTask();
    }

    private void InitializeTask()
    {
        ClearChildObjects(wireParentLeft);
        ClearChildObjects(wireParentRight);

        int difficulty = sabotageAmount - currentSabotageCount.Value;

        for (int i = 0; i < wireColors.Count - difficulty; i++)
        {
            var leftWire = Instantiate(wirePrefab, wireParentLeft);
            leftWire.InitializeWire(this, wireColors[i]);
            leftWires.Add(leftWire);

            var rightWire = Instantiate(wirePrefab, wireParentRight);
            rightWire.InitializeWire(this, wireColors[i], false);
            rightWires.Add(rightWire);
        }

        System.Random random = new System.Random(randomSeed);
        RandomizeChildren(wireParentLeft, random);
        RandomizeChildren(wireParentRight, random);
    }

    private void ClearChildObjects(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }

    private void RandomizeChildren(Transform parent, System.Random random)
    {
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < parent.childCount; i++)
        {
            children.Add(parent.GetChild(i));
        }

        for (int i = 0; i < children.Count; i++)
        {
            int randomIndex = random.Next(i, children.Count);
            (children[i], children[randomIndex]) = (children[randomIndex], children[i]);
        }

        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetSiblingIndex(i);
        }
    }

    protected override void ResetTask()
    {
        currentSuccessWires = 0;
        completeTextEffect.gameObject.SetActive(false);
    }

    public void SetWireSuccess()
    {
        currentSuccessWires++;
        int difficulty = sabotageAmount - currentSabotageCount.Value;

        if (currentSuccessWires >= wireColors.Count - difficulty)
        {
            completeTextEffect.gameObject.SetActive(true);
            completeTextEffect.StartManualEffect("entry");
        }
    }

    public override void HideTask()
    {
        base.HideTask();
        Hide();
    }

    private void Hide(float duration = 0.5f)
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        Tween.MaterialAlpha(wireMaterial, 0f, duration);
        Tween.Alpha(canvasGroup, 0f, duration).OnComplete(() => { isTaskVisible = false; });
    }

    public override void ShowTask()
    {
        if (isTaskVisible) return;
        isTaskVisible = true;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        Tween.MaterialAlpha(wireMaterial, 1f, 0.5f);
        Tween.Alpha(canvasGroup, 1f, 0.5f);

        InitializeTask();
    }

    public override void TaskCompleted()
    {
        base.TaskCompleted();
        randomSeed = Random.Range(0, 999);
    }
}
