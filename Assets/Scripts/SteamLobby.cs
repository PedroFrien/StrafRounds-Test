using Steamworks;
using UnityEngine;

public class SteamLobby : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    public void HostLobby()
    {

    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {

    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {

    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {

    }

    public void JoinLobby(CSteamID lobbyID)
    {
        SteamMatchmaking.JoinLobby(lobbyID);
    }
}
