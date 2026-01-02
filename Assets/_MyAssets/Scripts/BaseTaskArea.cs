using System;
using Unity.Netcode;
using UnityEngine;

public abstract class BaseTaskArea : NetworkBehaviour
{
    [Header("Task Settings")]
    [SerializeField] protected KeyCode interactKey = KeyCode.E;
    [SerializeField] protected int sabotageAmount = 2;
    [SerializeField] protected SpriteRenderer taskMarkerIcon;
    [SerializeField] protected SpriteRenderer taskSabotageIcon;

    protected bool isInsideArea = false;
    protected bool isTaskVisible = false;
    protected NetworkVariable<bool> canSabotageTask = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    protected NetworkVariable<bool> isTaskCompleted = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    protected NetworkVariable<int> currentSabotageCount = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public int SabotageAmount => sabotageAmount;

    protected PlayerController player;

    protected virtual void Start()
    {
        HideTask();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        canSabotageTask.OnValueChanged += OnAnyNetworkVariableChanged;
        isTaskCompleted.OnValueChanged += OnAnyNetworkVariableChanged;
        currentSabotageCount.OnValueChanged += OnAnyNetworkVariableChanged;

        UpdateIconVisibility(); // In case spawn happens after initial value set
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        canSabotageTask.OnValueChanged -= OnAnyNetworkVariableChanged;
        isTaskCompleted.OnValueChanged -= OnAnyNetworkVariableChanged;
        currentSabotageCount.OnValueChanged -= OnAnyNetworkVariableChanged;
    }

    private void OnAnyNetworkVariableChanged<T>(T previousValue, T newValue)
    {
        UpdateIconVisibility();
    }

    private void UpdateIconVisibility()
    {
        if (!IsSpawned) return;

        taskMarkerIcon.enabled = IsOwner && !isTaskCompleted.Value;
        taskSabotageIcon.enabled = !IsOwner && canSabotageTask.Value && currentSabotageCount.Value > 0;
    }

    [ClientRpc]
    public void InitializeBaseTaskClientRpc(ulong ownerId)
    {
        bool isOwnedByThisClient = NetworkManager.Singleton.LocalClientId == ownerId;
        if (!isOwnedByThisClient) return;

        taskMarkerIcon.enabled = isOwnedByThisClient;
        taskSabotageIcon.enabled = false;
        currentSabotageCount.Value = sabotageAmount;
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(interactKey) && isInsideArea)
        {
            if (IsOwner && !isTaskCompleted.Value)
            {
                ShowTask();
                player.canControl = false;
            }
            else if (!IsOwner && canSabotageTask.Value && currentSabotageCount.Value > 0)
            {
                PerformSabotageRpc();
            }
        }
    }

    public virtual void TaskCompleted()
    {
        if (!IsOwner) return;

        HideTask();
        player.canControl = true;
        isTaskCompleted.Value = true;
        canSabotageTask.Value = true;
        TaskCounterEvent.Trigger(OwnerClientId, 1);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController player))
        {
            if (player.IsLocalPlayer)
            {
                isInsideArea = CanShowTask();
                this.player = player;
            }
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out PlayerController player))
        {
            if (player.IsLocalPlayer)
            {
                isInsideArea = false;
                this.player = null;
            }
        }
    }

    protected virtual void ResetTask() { }
    public abstract void ShowTask();
    public virtual void HideTask() { if (player != null) player.canControl = true; }
    public virtual bool CanShowTask() => IsOwner || (!IsOwner && canSabotageTask.Value);

    [Rpc(SendTo.Owner)]
    private void PerformSabotageRpc()
    {
        if (!IsOwner) return;
        if (canSabotageTask.Value && currentSabotageCount.Value > 0)
        {
            currentSabotageCount.Value--;
            canSabotageTask.Value = false;
            isTaskCompleted.Value = false;
        }
        TaskCounterEvent.Trigger(OwnerClientId, -1);
        ResetTask();
    }
}
