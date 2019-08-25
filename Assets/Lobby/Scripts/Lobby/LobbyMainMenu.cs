using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Prototype.NetworkLobby
{
    //Main menu, mainly only a bunch of callback called by the UI (setup throught the Inspector)
    public class LobbyMainMenu : MonoBehaviour
    {
        public LobbyManager lobbyManager;

        public RectTransform lobbyServerList;
        public RectTransform lobbyPanel;
        public RectTransform helpPanel;

        public InputField ipInput;
        public InputField matchNameInput;

        public void OnEnable()
        {
            lobbyManager.topPanel.ToggleVisibility(true);

            ipInput.onEndEdit.RemoveAllListeners();
            ipInput.onEndEdit.AddListener(onEndEditIP);

            matchNameInput.onEndEdit.RemoveAllListeners();
            matchNameInput.onEndEdit.AddListener(onEndEditGameName);
        }

        public void OnClickHost()
        {
            /*
            if (Time.time < lobbyManager.startTime + 90f)
            {
                lobbyManager.infoPanel.Display("Please wait while loading.\n"
                    + (int)(lobbyManager.startTime + 90f - Time.time) + " seconds left.", "Close", null);
                return;
            }
            */
            lobbyManager.minPlayers = 1;
            lobbyManager.maxPlayers = 1;
            lobbyManager.prematchCountdown = 0f;
            lobbyManager.networkAddress = "localhost"; //ipInput.text;
            lobbyManager.StartHost();
        }

        public void OnClickJoin()
        {
            /*
            if (Time.time < lobbyManager.startTime + 45f)
            {
                lobbyManager.infoPanel.Display("Please wait while loading.\n"
                    + (int)(lobbyManager.startTime + 45f - Time.time) + " seconds left.", "Close", null);
                return;
            }
            */
            lobbyManager.ChangeTo(lobbyPanel);

            lobbyManager.minPlayers = 2;
            lobbyManager.maxPlayers = 5;
            lobbyManager.prematchCountdown = 5f;
            lobbyManager.networkAddress = "uriel.upnl.org"; //ipInput.text;
            lobbyManager.StartClient();

            lobbyManager.backDelegate = lobbyManager.StopClientClbk;
            lobbyManager.DisplayIsConnecting();

            lobbyManager.SetServerInfo("Connecting...", lobbyManager.networkAddress);
        }

        public void OnClickJoinLocal()
        {
            /*
            if (Time.time < lobbyManager.startTime + 45f)
            {
                lobbyManager.infoPanel.Display("Please wait while loading.\n"
                    + (int)(lobbyManager.startTime + 45f - Time.time) + " seconds left.", "Close", null);
                return;
            }
            */
            lobbyManager.ChangeTo(lobbyPanel);

            lobbyManager.minPlayers = 2;
            lobbyManager.maxPlayers = 5;
            lobbyManager.prematchCountdown = 5f;
            lobbyManager.networkAddress = "localhost"; //ipInput.text;
            lobbyManager.StartClient();

            lobbyManager.backDelegate = lobbyManager.StopClientClbk;
            lobbyManager.DisplayIsConnecting();

            lobbyManager.SetServerInfo("Connecting...", lobbyManager.networkAddress);
        }

        public void OnClickDedicated()
        {
            /*
            if (Time.time < lobbyManager.startTime + 90f)
            {
                lobbyManager.infoPanel.Display("Please wait while loading.\n"
                    + (int)(lobbyManager.startTime + 90f - Time.time) + " seconds left.", "Close", null);
                return;
            }
            */
            lobbyManager.ChangeTo(null);

            lobbyManager.minPlayers = 2;
            lobbyManager.maxPlayers = 5;
            lobbyManager.prematchCountdown = 5f;
            lobbyManager.networkAddress = "localhost"; //ipInput.text;
            lobbyManager.StartServer();

            lobbyManager.backDelegate = lobbyManager.StopServerClbk;

            lobbyManager.SetServerInfo("Dedicated Server", lobbyManager.networkAddress);
        }

        public void OnClickCreateMatchmakingGame()
        {
            lobbyManager.StartMatchMaker();
            lobbyManager.matchMaker.CreateMatch(
                matchNameInput.text,
                (uint)lobbyManager.maxPlayers,
                true,
                "", "", "", 0, 0,
                lobbyManager.OnMatchCreate);

            lobbyManager.backDelegate = lobbyManager.StopHost;
            lobbyManager._isMatchmaking = true;
            lobbyManager.DisplayIsConnecting();

            lobbyManager.SetServerInfo("Matchmaker Host", lobbyManager.matchHost);
        }

        public void OnClickOpenServerList()
        {
            lobbyManager.StartMatchMaker();
            lobbyManager.backDelegate = lobbyManager.SimpleBackClbk;
            lobbyManager.ChangeTo(lobbyServerList);
        }

        public void OnClickHelpButton()
        {
            lobbyManager.ChangeTo(helpPanel);
            lobbyManager.backDelegate = lobbyManager.SimpleBackClbk;
        }

        public void OnClickQuitGame()
        {
            Application.Quit();
        }

        void onEndEditIP(string text)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnClickJoin();
            }
        }

        void onEndEditGameName(string text)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnClickCreateMatchmakingGame();
            }
        }

    }
}
