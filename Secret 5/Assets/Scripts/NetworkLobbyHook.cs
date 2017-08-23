using UnityEngine;
using Prototype.NetworkLobby;
using System.Collections;
using UnityEngine.Networking;

public class NetworkLobbyHook : LobbyHook
{
    //static private int count = 0;

    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
    {
        LobbyPlayer lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
        PlayerControl player = gamePlayer.GetComponent<PlayerControl>();

        player.name = lobby.name;
        player.playerName = lobby.playerName;
        //player.playerNum = count;
        player.color = lobby.playerColor;

        //count++;
    }
}
