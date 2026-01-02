using System.Collections.Generic;
using MoreMountains.Tools;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TaskManager : NetworkBehaviour, MMEventListener<TaskCounterEvent>
{
    public List<Transform> taskSpawnPoints = new List<Transform>();
    public List<NetworkObject> taskNetworkObjects = new List<NetworkObject>();

    [Header("UI")]
    [SerializeField] private TMP_Text player1TaskCounter;
    [SerializeField] private TMP_Text player2TaskCounter;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private Button restartButton;

    private NetworkVariable<int> player1TaskCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> player2TaskCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Stack<Transform> availableSpawnPoints;

    private void Start()
    {
        restartButton.onClick.AddListener(RestartGame);
        winnerPanel.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            availableSpawnPoints ??= new Stack<Transform>(Shuffle(taskSpawnPoints));
            NetworkManager.SceneManager.OnLoadComplete += OnClientSceneLoaded;
        }

        if (!IsServer)
        {
            restartButton.gameObject.SetActive(false);
        }

        player1TaskCount.OnValueChanged += Player1TaskCountChanged;
        player2TaskCount.OnValueChanged += Player2TaskCountChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            availableSpawnPoints.Clear();
            NetworkManager.SceneManager.OnLoadComplete -= OnClientSceneLoaded;
        }
    }

    public void LeaveSession()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnMMEvent(TaskCounterEvent eventType)
    {
        TaskCountChangedRpc(eventType.clientId, eventType.taskCount);
    }

    [Rpc(SendTo.Server)]
    private void TaskCountChangedRpc(ulong clientId, int taskCount)
    {
        if (clientId == 0)
        {
            player1TaskCount.Value += taskCount;
        }
        else if (clientId == 1)
        {
            player2TaskCount.Value += taskCount;
        }
    }

    private void OnClientSceneLoaded(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        if (sceneName != "Gameplay")
            return;

        foreach (var task in taskNetworkObjects)
        {
            var spawn = availableSpawnPoints.Pop();
            var taskObject = Instantiate(task, spawn.position, Quaternion.identity);
            taskObject.SpawnWithOwnership(clientId, true);

            if (taskObject.TryGetComponent(out BaseTaskArea taskArea))
            {
                taskArea.InitializeBaseTaskClientRpc(clientId);
            }
        }
    }

    private IList<T> Shuffle<T>(IList<T> source)
    {
        for (int i = 0; i < source.Count - 1; ++i)
        {
            var indexToSwap = Random.Range(i, source.Count);
            (source[i], source[indexToSwap]) = (source[indexToSwap], source[i]);
        }
        return source;
    }

    private void OnEnable()
    {
        this.MMEventStartListening<TaskCounterEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<TaskCounterEvent>();
    }

    private void Player2TaskCountChanged(int previousValue, int newValue)
    {
        player2TaskCounter.text = $"Player 2: {newValue} / {taskNetworkObjects.Count}";
        if (newValue == taskNetworkObjects.Count)
        {
            SetWinner("Player 2");
        }
    }

    private void Player1TaskCountChanged(int previousValue, int newValue)
    {
        player1TaskCounter.text = $" Player 1: {newValue} / {taskNetworkObjects.Count}";
        if (newValue == taskNetworkObjects.Count)
        {
            SetWinner("Player 1");
        }
    }

    private void SetWinner(string winner)
    {
        winnerText.text = $"{winner} Win";
        winnerPanel.SetActive(true);
    }

    private void RestartGame()
    {
        NetworkManager.SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
    }
}

public struct TaskCounterEvent
{
    public ulong clientId;
    public int taskCount;

    public TaskCounterEvent(ulong clientId, int taskCount)
    {
        this.clientId = clientId;
        this.taskCount = taskCount;
    }

    static TaskCounterEvent e;
    public static void Trigger(ulong clientId, int taskCount)
    {
        e.clientId = clientId;
        e.taskCount = taskCount;
        MMEventManager.TriggerEvent(e);
    }
}
