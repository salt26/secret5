using System.Collections;
using System.Collections.Generic;
using Prototype.NetworkLobby;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class BattleManager : NetworkBehaviour
{

    static public BattleManager bm;

    public GameObject player;
    [SerializeField] private List<GameObject> cards = new List<GameObject>();
    // cards의 i번째 값은 cardCode가 i인 Card 컴포넌트를 포함하는 게임오브젝트입니다.
    // 이제 cards의 순서는 바뀌지 않고 카드 교환 시 cardcode의 순서가 바뀝니다.

    public Pusher pusher;

    public List<PlayerControl> players = new List<PlayerControl>(){
            null, null, null, null, null
        };
    private SyncListInt playerPermutation = new SyncListInt();
    // playerPermutation은 각 PlayerControl의 인덱스 값(int)을 갖는 배열입니다.
    // players[playerPermutation[0]]의 목표는 players[playerPermutation[1]] 또는 players[playerPermutation[2]]를 잡는 것입니다.
    // 1의 목표는 2 또는 4를 잡는 것입니다. (반복은 생략)
    // 2의 목표는 3 또는 1을 잡는 것입니다.
    // 3의 목표는 4 또는 0을 잡는 것입니다.
    // 4의 목표는 0 또는 3을 잡는 것입니다.

    private SyncListBool isWin = new SyncListBool(); // 인덱스는 players의 인덱스 기준, 그 플레이어가 승리하면 true
    [SyncVar] private int turnPlayer = 0; // 현재 자신의 턴을 진행하는 플레이어 번호
    [SyncVar] private int turnStep;   // 턴의 단계 (0: 대전 시작, 1: 턴 시작, 2: 턴 진행자의 교환 상대 선택과 교환할 카드 선택,
                                      //           3: 교환당하는 자의 교환할 카드 선택, 4: 교환 중(카드를 낼 때와 받을 때 효과 발동),
                                      //           5: 빙결 발동, 6: 턴이 끝날 때 효과 발동, 7: 턴 종료)
    [SyncVar] private int objectPlayer = -1;     // 교환당하는 플레이어
    [SyncVar] private int turnPlayerCard = -1;            // 턴을 진행한 플레이어가 낸 카드
    [SyncVar] private int objectPlayerCard = -1;          // 교환당하는 플레이어가 낸 카드
    private Exchange exchange;

    private int tpcIndex;
    private int opcIndex;
    private Vector3 tpcPosition;
    private Vector3 opcPosition;
    private Quaternion tpcRotation;
    private Quaternion opcRotation;

    private static CardDatabase cd;

    private SyncListInt cardcode = new SyncListInt();
    // cardcode의 인덱스는 어떤 플레이어의 손패에 카드가 있는지를 나타낸다.
    // 0 ~ 1: players[0]의 손패, 2 ~ 3: players[1]의 손패, 4 ~ 5: players[2]의 손패,
    // 6 ~ 7: players[3]의 손패, 8 ~ 9: players[4]의 손패
    // cardcode[i]의 값은 cardCode가 i인 Card의 cards에서의 인덱스와 일치한다.
    // 다시 말해, cardcode의 인덱스는 카드의 위치, cardcode의 값은 카드의 종류이다.

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine("StartGame");
    }

    private bool IsPlayerEmpty()
    {
        for (int i = 0; i < 5; i++)
        {
            if (players[i] == null)
                return true;
        }
        return false;
    }

    IEnumerator StartGame()
    {
        yield return new WaitWhile(() => !IsPlayerEmpty());
        
        List<int> temp = RandomListGenerator(5);
        for (int i = 0; i < 5; i++)
        {
            isWin.Add(false);
            playerPermutation.Add(temp[i]);
        }

        for (int i = 0; i < 10; i++)
        {
            cardcode.Add(i);
        }
        CardPermutation();

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 10; i++)
        {
            cards[cardcode[i]].GetComponent<Card>().RpcMoveCard(100 + i);
        }
        turnPlayer = Random.Range(0, 5);

        //RpcPrintLog("Battle starts.");
        //RpcPrintLog("turnStep 1(" + players[turnPlayer].GetName() + " turn starts)");

        turnStep = 1;
    }
    
    private void OnPlayerDisconnected(NetworkPlayer player)
    {
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
        //RpcPrintLog("A player has disconnected. Battle ends.");
        StartCoroutine(ReturnToLobby(3f));
    }

    /*
    // called when a player is removed for a client
    public override void OnServerRemovePlayer(NetworkConnection conn, short playerControllerId)
    {
        PlayerController player;
        if (conn.GetPlayer(playerControllerId, out player))
        {
            if (player.NetworkIdentity != null && player.NetworkIdentity.gameObject != null)
                NetworkServer.Destroy(player.NetworkIdentity.gameObject);
        }
    }
    */

    private void Awake()
    {
        bm = this;

        turnStep = 0;
        cd = GetComponent<CardDatabase>();
    }

    void FixedUpdate()
    {
        if (!isServer) return;

        if (turnStep == 1)
        {
            // 빙결된 상태이면 교환을 할 수 없다.
            if (players[turnPlayer].HasFreezed())
            {
                turnStep = 5;
                //RpcPrintLog("turnStep 5(freezed)");
            }
            else
            {
                turnStep = 2;
                //RpcPrintLog("turnStep 2(select a player to exchange with, and select a card to play)");
            }
        }
        else if (turnStep == 2)
        {
            // TODO 플레이어의 응답을 기다리고 제한 시간 측정하기
        }
        else if (turnStep == 3)
        {
            // TODO 플레이어의 응답을 기다리고 제한 시간 측정하기
            if (turnPlayerCard != -1 && objectPlayerCard != -1)
            {
                turnStep = 4;
                //RpcPrintLog("turnStep 4(preprocessing)");
            }
        }
        else if (turnStep == 4)
        {
            exchange = new Exchange(players[turnPlayer], players[objectPlayer], GetCard(turnPlayerCard), GetCard(objectPlayerCard));
            RpcSetOpponentCard(turnPlayer, objectPlayer, turnPlayerCard, objectPlayerCard);
            if (exchange.GetTurnNum() == 0)
            {
                //RpcPrintLog("Exception throwed while making an exchange!");
                turnStep = 10;
                return;
            }
            tpcIndex = cardcode.IndexOf(exchange.GetTurnPlayerCard().GetCardCode());
            opcIndex = cardcode.IndexOf(exchange.GetObjectPlayerCard().GetCardCode());
            
            cards[cardcode[tpcIndex]].GetComponent<Card>().RpcMoveCard(tpcIndex * 10 + opcIndex);
            cards[cardcode[opcIndex]].GetComponent<Card>().RpcMoveCard(opcIndex * 10 + tpcIndex);
            
            // 손패 교환
            int temp = cardcode[tpcIndex];
            cardcode[tpcIndex] = cardcode[opcIndex];
            cardcode[opcIndex] = temp;
            RpcExchangeCardIndex(tpcIndex, opcIndex);

            turnStep = 9;

        }
        else if (turnStep == 5)
        {
            // TODO 빙결된 이펙트 보여주고 잠시 딜레이
            players[turnPlayer].RpcFreeze();
            turnStep = 11;
        }
        else if (turnStep == 6)
        {
            List<Card> hand = GetPlayerHand(players[turnPlayer]);
            // 턴을 진행한 플레이어가 폭탄 카드를 들고 있으면 펑!
            if (hand[0].GetCardName() == "Bomb" || hand[1].GetCardName() == "Bomb")
            {
                players[turnPlayer].Damaged();
            }

            turnStep = 7;
            //RpcPrintLog("turnStep 7(turn ends)");
        }
        else if (turnStep == 7)
        {
            bool isEnd = false;
            for (int i = 0; i < 5; i++)
            {
                players[i].UpdateHealth();
            }

            // 대전 규칙: 한 명이라도 사망하면 게임이 끝나고, 게임이 끝나는 시점에 "자신의 목표 중 사망자의 수"가 가장 많은 플레이어들이 승리한다.
            for (int i = 0; i < 5; i++)
            {
                if (!players[i].HasDead())
                {
                    // 두 명이 동시에 사망하였는데 그 두 명을 모두 목표로 하는 플레이어가 있다면 그 플레이어의 단독 승리!
                    if (players[GetTarget(i)[0]].HasDead()
                        && players[GetTarget(i)[1]].HasDead())
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            isWin[j] = false;
                        }
                        isWin[i] = true;
                        RpcPlayerWinIndex(i);
                        isEnd = true;
                        break;
                    }
                    // 한 명만 사망하였거나, 두 명이 동시에 사망하더라도 그 두 명을 모두 목표로 하는 플레이어가 없으면, 사망자들 중 한 명을 목표로 하는 모든 플레이어들의 공동 승리!
                    else if (players[GetTarget(i)[0]].HasDead()
                        || players[GetTarget(i)[1]].HasDead())
                    {
                        isWin[i] = true;
                        RpcPlayerWinIndex(i);
                        isEnd = true;
                    }
                }
            }
            if (isEnd)
            {
                /*
                for (int i = 0; i < 5; i++)
                {
                    if (isWin[i]) RpcPrintLog(players[i].GetName() + " wins!");
                }
                RpcPrintLog("Battle ends.");
                */
                StartCoroutine(ReturnToLobby(5f));
                turnStep = 8;
            }
            else
            {
                //RpcPrintLog("Turn ends.");
                turnPlayer += 1;
                if (turnPlayer >= 5) turnPlayer = 0;
                turnStep = 1;
                //RpcPrintLog("turnStep 1(" + players[turnPlayer].GetName() + " turn starts)");
            }
        }
        else if (turnStep == 9)
        {
            // 카드 교환 애니메이션이 끝나면 카드의 위치가 바뀌고 turnStep 6이 됩니다.
        }
        else if (turnStep == 10)
        {
            // 교환(turnStep 4) 단계에서 오류가 생긴 경우
        }
        else if (turnStep == 11)
        {
            // 빙결 애니메이션이 끝나면 플레이어가 해동되고 turnStep 6이 됩니다.
        }

    }

    public void SetObjectPlayer(int objectTargetIndex)
    {
        if (turnStep != 2 || objectTargetIndex < 0 || objectTargetIndex >= 5) return;
        objectPlayer = objectTargetIndex;
        turnStep = 3;
        //RpcPrintLog("turnStep 3(select a card to play)");
    }

    public void SetCardToPlay(int cardCode, int playerIndex)
    {
        if (turnStep != 3 || cardCode < 0 || cardCode >= 10 || playerIndex < 0 || playerIndex >= 5) return;
        if (playerIndex == turnPlayer && turnPlayerCard == -1)
        {
            turnPlayerCard = cardCode;
            //RpcPrintLog(players[playerIndex].GetName() + " sets " + GetCard(cardCode).GetCardName() + " card to play.");
        }
        else if (playerIndex == objectPlayer && objectPlayerCard == -1)
        {
            objectPlayerCard = cardCode;
            //RpcPrintLog(players[playerIndex].GetName() + " sets " + GetCard(cardCode).GetCardName() + " card to play.");
        }
    }

    public List<Card> GetPlayerHand(PlayerControl player)
    {
        List<Card> hand = new List<Card>();
        int playerNum = players.IndexOf(player);
        if (playerNum != -1 && turnStep > 0)
        {
            hand.Add(cards[cardcode[playerNum * 2]].GetComponent<Card>());
            hand.Add(cards[cardcode[playerNum * 2 + 1]].GetComponent<Card>());
            return hand;
        }
        else return null;
    }

    public PlayerControl GetTurnPlayer()
    {
        return players[turnPlayer];
    }

    public PlayerControl GetObjectPlayer()
    {
        if (objectPlayer == -1) return null;
        return players[objectPlayer];
    }

    public int GetTurnStep()
    {
        return turnStep;
    }

    public SyncListBool GetIsWin()
    {
        return isWin;
    }

    /// <summary>
    /// 카드들의 목록을 반환합니다. 주의: 이 리스트의 순서는 카드 배치 순서를 반영하지 않습니다! 카드 배치 순서를 얻고 싶다면 GetCardCode()를 사용하십시오.
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetCardsInHand()
    {
        return cards;
    }


    public Card GetPlayerSelectedCard(PlayerControl player)
    {
        if (turnStep != 3 || player == null) return null;
        else if (player.Equals(players[turnPlayer]))
        {
            return GetCard(turnPlayerCard);
        }
        else if (player.Equals(players[objectPlayer]))
        {
            return GetCard(objectPlayerCard);
        }
        else
        {
            //Debug.Log("a");
            return null;
        }
    }
    /// <summary>
    /// 교환 처리 이후의 단계로 넘어가기 위해 호출되는 함수입니다.
    /// </summary>
    public void AfterExchange()
    {
        if (turnStep != 9) return;

        RpcSetExchangeComplete();
        objectPlayer = -1;
        turnPlayerCard = -1;
        objectPlayerCard = -1;

        turnStep = 6;
        //Debug.Log("turnStep 6(postprocessing)");
    }

    /// <summary>
    /// 빙결 처리 이후의 단계로 넘어가기 위해 호출되는 함수입니다.
    /// </summary>
    public void AfterFreezed()
    {
        if (turnStep != 11) return;
        //RpcPrintLog("AfterFreezed");

        turnStep = 6;

        // 턴을 진행한 플레이어가 빙결된 상태였으면 해동된다.
        players[turnPlayer].Thawed();
        //Debug.Log("turnStep 6(postprocessing)");
    }


    private List<int> RandomListGenerator(int n)
    {
        List<int> rand = new List<int>();
        bool[] check = new bool[n];
        int r;
        for (int i = n; i > 0; i--)
        {
            r = Random.Range(0, i);
            for (int j = 0; j < n; j++)
            {
                if (!check[j])
                {
                    if (r == 0)
                    {
                        rand.Add(j);
                        check[j] = true;
                        break;
                    }
                    r--;
                }
            }
        }
        return rand;
    }

    private void CardPermutation()
    {
        List<int> rand1 = RandomListGenerator(5);
        List<int> rand2 = RandomListGenerator(5);
        SyncListInt c = new SyncListInt();
        for (int i = 0; i < 5; i++)
        {
            c.Add(cardcode[2 * rand1[i]]);
            c.Add(cardcode[2 * rand2[i] + 1]);
        }
        cardcode = c;
    }

    /// <summary>
    /// 인자로 주어진 cardCode와 같은 코드를 갖는 카드를 반환합니다.
    /// </summary>
    /// <param name="cardCode"></param>
    /// <returns></returns>
    public Card GetCard(int cardCode)
    {
        if (cardCode < 0 || cardCode >= 10)
            return null;
        return cards[cardCode].GetComponent<Card>();
    }

    public Card GetCardInPosition(int cardPosition)
    {
        if (cardPosition < 0 || cardPosition >= 10 || turnStep <= 0) return null;
        return cards[cardcode[cardPosition]].GetComponent<Card>();
    }

    /// <summary>
    /// 카드의 배치 상태 리스트를 반환합니다.
    /// 만약 i번째 자리에 배치된 카드에 접근하고 싶다면 GetCardsInHand()[GetCardCode()[i]].GetComponent<Card>()를 사용하십시오.
    /// </summary>
    /// <returns></returns>
    public SyncListInt GetCardCode()
    {
        return cardcode;
    }

    /*
    // TODO 임시 코드
    [ClientRpc]
    public void RpcPrintLog(string msg)
    {
        LogDisplay.AddText(msg);
        Debug.Log(msg);
    }
    */

    /// <summary>
    /// 인자로 주어진 플레이어가 잡아야 하는 목표의 번호 리스트를 반환합니다.
    /// 잘못된 입력이 주어지지 않는다면, 반환되는 리스트의 크기는 2입니다.
    /// </summary>
    /// <param name="playerIndex">플레이어 번호</param>
    /// <returns></returns>
    public List<int> GetTarget(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= 5 || playerPermutation.Count != 5) return null;

        List<int> t = new List<int>();
        int i = playerPermutation.IndexOf(playerIndex);

        switch (i)
        {
            case 0:
                t.Add(playerPermutation[1]);
                t.Add(playerPermutation[2]);
                break;
            case 1:
                t.Add(playerPermutation[2]);
                t.Add(playerPermutation[4]);
                break;
            case 2:
                t.Add(playerPermutation[3]);
                t.Add(playerPermutation[1]);
                break;
            case 3:
                t.Add(playerPermutation[4]);
                t.Add(playerPermutation[0]);
                break;
            case 4:
                t.Add(playerPermutation[0]);
                t.Add(playerPermutation[3]);
                break;
        }
        return t;
    }

    public List<PlayerControl> GetPlayers()
    {
        return players;
    }

    [ClientRpc]
    private void RpcExchangeCardIndex(int tpc, int opc)
    {
        int temp = cardcode[tpc];
        cardcode[tpc] = cardcode[opc];
        cardcode[opc] = temp;
    }

    [ClientRpc]
    private void RpcPlayerWinIndex(int i)
    {
        isWin[i] = true;
    }

    [ClientRpc]
    private void RpcSetExchangeComplete()
    {
        pusher.SetExchangeComplete();
    }

    [ClientRpc]
    private void RpcSetOpponentCard(int TP, int OP, int TPCardCode, int OPCardCode)
    {
        pusher.SetOpponentCard(TP, OP, TPCardCode, OPCardCode);
    }
    /*
    [ClientRpc]
    private void RpcAlert(int i)
    {
        for (int j = 0; j < 5; j++)
        {
            players[j].CAlert(i);
        }
    }
    */

    IEnumerator ReturnToLobby(float timing)
    {
        yield return new WaitForSeconds(timing);
        LobbyManager.s_Singleton.ServerReturnToLobby();
    }
}
