using System;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private UnityEvent<bool> onSessionChanged;

    private ISession session;

    public void LoadGameplayScene()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
    }

    public void SetSession(ISession session)
    {
        GameManager.Instance.SetSession(session);
    }

    public void OnJoinedSession(ISession session)
    {
        onSessionChanged.Invoke(session.IsHost);
        this.session = session;
        session.Changed += OnSessionChanged;
    }

    public void OnSessionLeaved()
    {
        if (session == null) return;
        session.Changed -= OnSessionChanged;
        session = null;
    }

    private void OnSessionChanged()
    {
        onSessionChanged.Invoke(session.IsHost);
    }
}
