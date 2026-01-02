using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CinemachineTarget : NetworkBehaviour
{
    private CinemachineCamera cinemachineCamera;

    private void Start()
    {
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (NetworkManager.Singleton.LocalClientId == OwnerClientId)
        {
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
            cinemachineCamera.Target.TrackingTarget = this.transform;
        }
    }
}
