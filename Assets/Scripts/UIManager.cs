using UnityEngine;

public class UIManager : MonoBehaviour
{
    public LobbyManager lobbyManager;

    public void OnCreateLobbyClicked()
    {
        lobbyManager.CreateLobbyWithRelay();
    }

    public void OnQuickJoinClicked()
    {
        lobbyManager.QuickJoinLobbyAndRelay();
    }
}

