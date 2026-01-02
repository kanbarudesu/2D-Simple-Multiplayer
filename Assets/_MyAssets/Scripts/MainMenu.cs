using System;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startGameButton;

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
        startGameButton.gameObject.SetActive(session.IsHost);
        this.session = session;
        session.Changed += OnSessionChanged;
    }

    public void OnSessionLeaved()
    {
        session.Changed -= OnSessionChanged;
        session = null;
    }

    private void OnSessionChanged()
    {
        startGameButton.gameObject.SetActive(session.IsHost);
    }
}
