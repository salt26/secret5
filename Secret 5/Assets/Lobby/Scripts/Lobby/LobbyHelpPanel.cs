using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prototype.NetworkLobby
{
    public class LobbyHelpPanel : MonoBehaviour
    {
        public LobbyManager lobbyManager;

        public RectTransform prevHelpPanel;
        public RectTransform nextHelpPanel;

        public void OnClickPrev()
        {
            lobbyManager.ChangeTo(prevHelpPanel);
            lobbyManager.backDelegate = lobbyManager.SimpleBackClbk;
        }

        public void OnClickNext()
        {
            lobbyManager.ChangeTo(nextHelpPanel);
            lobbyManager.backDelegate = lobbyManager.SimpleBackClbk;
        }
    }
}
