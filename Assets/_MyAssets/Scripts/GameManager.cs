using System;
using System.Collections;
using System.Threading.Tasks;
using Kanbarudesu.Utility;
using Unity.Multiplayer.Widgets;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonPersistent<GameManager>
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private WidgetConfiguration widgetConfiguration;

    private ISession session;

    private void Start()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadComplete;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoadComplete;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }

    public void SetSession(ISession session) => this.session = session;

    private void OnSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName == "Gameplay" && IsServer)
        {
            if (session != null)
                PrivateLobbyDelayed();
            GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }

    public void QuitGame()
    {
        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.isPlaying = false;
            return;
        }
        Application.Quit();
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (SceneManager.GetActiveScene().name == "Gameplay")
        {
            //client
            if (!NetworkManager.Singleton.IsServer && clientId == NetworkManager.Singleton.LocalClientId)
            {
                NetworkManager.Singleton.Shutdown();
                SceneManager.LoadScene("MainMenu");
            }
        }
    }


    private async void PrivateLobbyDelayed()
    {
        try
        {
            await Task.Delay(1000);
            await LobbyService.Instance.UpdateLobbyAsync(session.Id, new UpdateLobbyOptions { IsPrivate = true });
            Debug.Log("Lobby deleted successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete lobby: {e.Message}");
        }
    }
}
