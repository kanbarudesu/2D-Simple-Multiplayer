using EasyTextEffects;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class ColorMemoryTask : BaseTaskArea
{
    [Header("Gameplay Settings")]
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private Button replaySequenceButton;
    [SerializeField] private Button colorButtonPrefab;
    [SerializeField] private Transform buttonParent;
    [SerializeField] private TextEffect completeTextEffect;
    [SerializeField] private Image colorMemoryImage;

    [SerializeField] private float sequenceStartDelay = 1f;
    [SerializeField] private float sequenceDelay = 0.5f;
    [SerializeField] private float difficultyDelayMultiplier = 0.1f;

    [SerializeField] private Color[] colors;

    private Color[] colorSequence;
    private int randomSeed;
    private int currentSequenceIndex = 0;
    private bool isSequenceRunning = false;

    private Sequence colorSequenceTween;

    protected override void Start()
    {
        base.Start();
        contentRect.anchoredPosition = new Vector2(0f, 1080f);
        completeTextEffect.gameObject.SetActive(false);
        randomSeed = Random.Range(0, 999);
    }

    public override void HideTask()
    {
        base.HideTask();
        Tween.UIAnchoredPositionY(contentRect, 1080f, 0.5f).OnComplete(() => isTaskVisible = false);
        colorSequenceTween.Complete();
    }

    public override void ShowTask()
    {
        if (isTaskVisible) return;

        isTaskVisible = true;
        Tween.UIAnchoredPositionY(contentRect, 0f, 0.5f);
        InitializeTask();
        PlayColorSequence();
    }

    private void InitializeTask()
    {
        ClearButtons();
        completeTextEffect.gameObject.SetActive(false);

        foreach (Color color in colors)
        {
            Button button = Instantiate(colorButtonPrefab, buttonParent);
            button.transform.GetChild(0).GetComponent<Image>().color = color;
            button.onClick.AddListener(() =>
            {
                if (colorSequence[currentSequenceIndex] != color)
                {
                    currentSequenceIndex = 0;
                    randomSeed = Random.Range(0, 999);
                    Tween.Color(colorMemoryImage, Color.red, 0.1f, Ease.Default, 6, CycleMode.Yoyo)
                        .OnComplete(() => PlayColorSequence());
                    return;
                }

                currentSequenceIndex++;
                if (currentSequenceIndex == colorSequence.Length)
                {
                    currentSequenceIndex = 0;
                    randomSeed = Random.Range(0, 999);
                    Tween.Color(colorMemoryImage, Color.green, 0.1f, Ease.Default, 6, CycleMode.Yoyo)
                        .OnComplete(() =>
                        {
                            //Task Completed
                            completeTextEffect.gameObject.SetActive(true);
                            completeTextEffect.StartManualEffect("entry");
                        });
                }
            });
        }
    }

    public void PlayColorSequence()
    {
        if (isSequenceRunning) return;
        isSequenceRunning = true;

        int difficulty = sabotageAmount - currentSabotageCount.Value;
        colorSequence = colors.Clone() as Color[];

        System.Random random = new System.Random(randomSeed);
        ShuffleArray(colorSequence, random);

        colorSequenceTween = Sequence.Create()
                                .ChainDelay(sequenceStartDelay);
        foreach (Color color in colorSequence)
        {
            colorSequenceTween.Chain(Tween.Delay(sequenceDelay + (difficulty * difficultyDelayMultiplier))
                                .OnComplete(() => colorMemoryImage.color = color));
        }
        colorSequenceTween.Chain(Tween.Delay(sequenceDelay + (difficulty * difficultyDelayMultiplier))
                            .OnComplete(() => colorMemoryImage.color = Color.white))
                .OnComplete(() => isSequenceRunning = false);
    }

    public override void TaskCompleted()
    {
        base.TaskCompleted();
        randomSeed = Random.Range(0, 999);
    }

    private void ClearButtons()
    {
        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void ShuffleArray<T>(T[] array, System.Random random)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randIndex = random.Next(0, i);
            T temp = array[i];
            array[i] = array[randIndex];
            array[randIndex] = temp;
        }
    }
}
