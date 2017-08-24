using System.Collections;
using System.Collections.Generic;
using Prototype.NetworkLobby;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class BattleManager : NetworkBehaviour {

    static public BattleManager bm;

    public GameObject player;
    [SerializeField] private List<GameObject> cards = new List<GameObject>();
    // cards의 i번째 값은 cardCode가 i인 Card 컴포넌트를 포함하는 게임오브젝트입니다.
    // 이제 cards의 순서는 바뀌지 않고 카드 교환 시 cardcode의 순서가 바뀝니다.
    
    public Pusher pusher;

    public List<PlayerControl> players = new List<PlayerControl>(){
            null, null, null, null, null
        };
    //private List<TargetGraph> playerPermutation = new List<TargetGraph>();
    public SyncListInt playerPermutation = new SyncListInt(); // TODO private로 바꾸기
    // playerPermutation은 각 PlayerControl의 인덱스 값(int)을 갖는 배열입니다.
    // players[playerPermutation[0]]의 목표는 players[playerPermutation[1]] 또는 players[playerPermutation[2]]를 잡는 것입니다.
    // 1의 목표는 2 또는 4를 잡는 것입니다. (반복은 생략)
    // 2의 목표는 3 또는 1을 잡는 것입니다.
    // 3의 목표는 4 또는 0을 잡는 것입니다.
    // 4의 목표는 0 또는 3을 잡는 것입니다.

    public SyncListBool isWin = new SyncListBool(); // 인덱스는 players의 인덱스 기준, 그 플레이어가 승리하면 true // TODO private로 바꾸기
    //private List<bool> isWin = new List<bool>();
    [SyncVar] private int turnPlayer = 0; // 현재 자신의 턴을 진행하는 플레이어 번호
    [SyncVar] private int turnStep;   // 턴의 단계 (0: 대전 시작, 1: 턴 시작, 2: 턴 진행자의 교환 상대 선택과 교환할 카드 선택,
                                      //           3: 교환당하는 자의 교환할 카드 선택, 4: 교환 중(카드를 낼 때와 받을 때 효과 발동),
                                      //           5: 빙결 발동, 6: 턴이 끝날 때 효과 발동, 7: 턴 종료)
    [SyncVar] private int objectPlayer = -1;     // 교환당하는 플레이어
    [SyncVar] private int turnPlayerCard = -1;            // 턴을 진행한 플레이어가 낸 카드
    [SyncVar] private int objectPlayerCard = -1;          // 교환당하는 플레이어가 낸 카드
    private Exchange exchange;
    //private int cameraPlayer = 0;

    private int tpcIndex;
    private int opcIndex;
    private Vector3 tpcPosition;
    private Vector3 opcPosition;
    private Quaternion tpcRotation;
    private Quaternion opcRotation;

    private static CardDatabase cd;

    public SyncListInt cardcode = new SyncListInt(); // TODO private로 바꾸기
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

        /*
        List<PlayerControl> tempPlayers = new List<PlayerControl>
        {
            players[0],
            players[1],
            players[2],
            players[3],
            players[4]
        };

        // 목표 그래프에 5명의 플레이어를 랜덤한 순서로 배치한다.
        for (int i = 0; i < 5; i++)
        {
            isWin.Add(false);
            playerPermutation.Add(new TargetGraph(i));
            int r = Random.Range(0, tempPlayers.Count);
            playerPermutation[i].player = tempPlayers[r];
            tempPlayers.RemoveAt(r);
            //NetworkServer.Spawn(players[i].gameObject);
        }
        */
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
            // cards[i].GetComponent<Card>().MoveCard(100 + i); // 이전 코드
            // TODO 개별 클라이언트마다 카드 앞면으로 뒤집기
        }
        turnPlayer = Random.Range(0, 5);
        //cameraPlayer = turnPlayer;

        RpcPrintLog("Battle starts.");
        /*
        for (int j = 0; j < 5; j++)
        {
            RpcPrintLog(players[j].GetName() + "'s objective is to eliminate "
                + players[GetTarget(j)[0]].GetName() + " and "
                + players[GetTarget(j)[1]].GetName());
        }
        */
        RpcPrintLog("turnStep 1(" + players[turnPlayer].GetName() + " turn starts)");
        //SetCameraVisible(cameraPlayer);

        turnStep = 1;
    }

    private void Awake()
    {
        bm = this;

        turnStep = 0;
        cd = GetComponent<CardDatabase>();
    }
    /*
        turnStep = 0;
        cd = GetComponent<CardDatabase>();
        pushingcard = GameObject.Find("CardPanel").GetComponentsInChildren<PushingCard>();

        List<PlayerControl> tempPlayers = new List<PlayerControl>();

        /*
        players.Add(Instantiate(player, new Vector3(0f, 0f, 0f), Quaternion.identity).GetComponent<PlayerControl>());
        players.Add(Instantiate(player, new Vector3(3.236f, 0f, 2.351f), Quaternion.Euler(0f, -72f, 0f)).GetComponent<PlayerControl>());
        players.Add(Instantiate(player, new Vector3(2f, 0f, 6.155f), Quaternion.Euler(0f, -144f, 0f)).GetComponent<PlayerControl>());
        players.Add(Instantiate(player, new Vector3(-2f, 0f, 6.155f), Quaternion.Euler(0f, 144f, 0f)).GetComponent<PlayerControl>());
        players.Add(Instantiate(player, new Vector3(-3.236f, 0f, 2.351f), Quaternion.Euler(0f, 72f, 0f)).GetComponent<PlayerControl>());
        players[0].gameObject.GetComponentInChildren<Camera>().targetDisplay = 0;
        players[0].SetPlayerNum(1);
        players[0].SetName("Player 1");
        tempPlayers.Add(players[0]);

        players[1].gameObject.GetComponentInChildren<Camera>().targetDisplay = 0;
        players[1].SetPlayerNum(2);
        players[1].SetName("Player 2");
        tempPlayers.Add(players[1]);

        players[2].gameObject.GetComponentInChildren<Camera>().targetDisplay = 0;
        players[2].SetPlayerNum(3);
        players[2].SetName("Player 3");
        tempPlayers.Add(players[2]);

        players[3].gameObject.GetComponentInChildren<Camera>().targetDisplay = 0;
        players[3].SetPlayerNum(4);
        players[3].SetName("Player 4");
        tempPlayers.Add(players[3]);

        players[4].gameObject.GetComponentInChildren<Camera>().targetDisplay = 0;
        players[4].SetPlayerNum(5);
        players[4].SetName("Player 5");
        tempPlayers.Add(players[4]);

        // 목표 그래프에 5명의 플레이어를 랜덤한 순서로 배치한다.
        for (int i = 0; i < 5; i++)
        {
            isWin.Add(false);
            playerPermutation.Add(new TargetGraph(i));
            int r = Random.Range(0, tempPlayers.Count);
            playerPermutation[i].player = tempPlayers[r];
            tempPlayers.RemoveAt(r);
            //NetworkServer.Spawn(players[0].gameObject);
        }
    }
    void Start ()
    {
        CardPermutation();
        for (int i = 0; i < 10; i++)
        {
            cards[i].GetComponent<Card>().MoveCard(100 + i);
        }
        turnPlayer = Random.Range(0, 5);
        cameraPlayer = turnPlayer;

        turnStep = 1;
        Debug.Log("Battle starts.");
        Debug.Log("turnStep 1(" + players[turnPlayer].GetName() + " turn starts)");
        SetCameraVisible(cameraPlayer);
    }
    */

    void FixedUpdate() {
        if (!isServer) return;
        /*
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = players[cameraPlayer].GetComponentInChildren<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 8)))
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green, 3f);
                if (hit.collider.gameObject.GetComponentInParent<PlayerControl>() != null
                    && !hit.collider.gameObject.GetComponentInParent<PlayerControl>().Equals(players[cameraPlayer]))
                {
                    cameraPlayer = hit.collider.gameObject.GetComponentInParent<PlayerControl>().GetPlayerNum() - 1;
                    SetCameraVisible(cameraPlayer);
                    Debug.Log(hit.collider.gameObject.GetComponentInParent<PlayerControl>().GetName() + "'s camera.");
                    pusher.SetCardChange();
                }
            }
        }
        */
        if (turnStep == 1)
        {
            // 빙결된 상태이면 교환을 할 수 없다.
            if (players[turnPlayer].HasFreezed())
            {
                turnStep = 5;
                RpcPrintLog("turnStep 5(freezed)");
            }
            else
            {
                turnStep = 2;
                RpcPrintLog("turnStep 2(select a player to exchange with, and select a card to play)");
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
                RpcPrintLog("turnStep 4(preprocessing)");
            }
        }
        else if (turnStep == 4)
        {
            exchange = new Exchange(players[turnPlayer], players[objectPlayer], GetCard(turnPlayerCard), GetCard(objectPlayerCard));
            RpcSetOpponentCard(turnPlayer, objectPlayer, turnPlayerCard, objectPlayerCard);
            if (exchange.GetTurnNum() == 0)
            {
                RpcPrintLog("Exception throwed while making an exchange!");
                turnStep = 10;
                return;
            }
            //RpcPrintLog("Create Exchange");
            tpcIndex = cardcode.IndexOf(exchange.GetTurnPlayerCard().GetCardCode());
            opcIndex = cardcode.IndexOf(exchange.GetObjectPlayerCard().GetCardCode());

            /* 애니메이션
            cards[tpcIndex].GetComponent<Animator>().SetInteger("Destination", opcIndex);
            cards[opcIndex].GetComponent<Animator>().SetInteger("Destination", tpcIndex);
            if (cameraPlayer == tpcIndex / 2)
            {
                cards[tpcIndex].GetComponent<Animator>().SetBool("Play", true);
                cards[tpcIndex].GetComponent<Animator>().SetBool("Receive", false);
                cards[opcIndex].GetComponent<Animator>().SetBool("Play", false);
                cards[opcIndex].GetComponent<Animator>().SetBool("Receive", true);
            }
            else if (cameraPlayer == opcIndex / 2)
            {
                cards[tpcIndex].GetComponent<Animator>().SetBool("Play", false);
                cards[tpcIndex].GetComponent<Animator>().SetBool("Receive", true);
                cards[opcIndex].GetComponent<Animator>().SetBool("Play", true);
                cards[opcIndex].GetComponent<Animator>().SetBool("Receive", false);
            }
            else
            {
                cards[tpcIndex].GetComponent<Animator>().SetBool("Play", false);
                cards[tpcIndex].GetComponent<Animator>().SetBool("Receive", false);
                cards[opcIndex].GetComponent<Animator>().SetBool("Play", false);
                cards[opcIndex].GetComponent<Animator>().SetBool("Receive", false);
            }
            cards[tpcIndex].GetComponent<Animator>().SetTrigger("Exchange");
            cards[opcIndex].GetComponent<Animator>().SetTrigger("Exchange");
            */

            // 카드 위치 변경 및 카드 이동 애니메이션 재생
            /*
            foreach (PlayerControl p in players)
            {
                if (p.GetPlayerIndex() == tpcIndex / 2)
                {
                    // TODO 아마도 Rpc를 쓰면 모든 클라이언트에서 재생되지 않을까?
                    cards[cardcode[tpcIndex]].GetComponent<Card>().RpcFlipCard(tpcIndex, true);
                }
                else if (p.GetPlayerIndex() == opcIndex / 2)
                {
                    cards[cardcode[opcIndex]].GetComponent<Card>().RpcFlipCard(opcIndex, true);
                }
            }
            */

            cards[cardcode[tpcIndex]].GetComponent<Card>().RpcMoveCard(tpcIndex * 10 + opcIndex);
            cards[cardcode[opcIndex]].GetComponent<Card>().RpcMoveCard(opcIndex * 10 + tpcIndex);

            /*
            foreach (PlayerControl p in players)
            {
                if (p.GetPlayerIndex() == tpcIndex / 2)
                {
                    cards[cardcode[opcIndex]].GetComponent<Card>().RpcFlipCard(tpcIndex, false);
                }
                else if (p.GetPlayerIndex() == opcIndex / 2)
                {
                    cards[cardcode[tpcIndex]].GetComponent<Card>().RpcFlipCard(opcIndex, false);
                }
            }
            */

            // 손패 교환
            int temp = cardcode[tpcIndex];
            cardcode[tpcIndex] = cardcode[opcIndex];
            cardcode[opcIndex] = temp;
            RpcExchangeCardIndex(tpcIndex, opcIndex);
            

            string m = "cardcode";
            for (int i = 0; i < 10; i++)
            {
                m += " " + cardcode[i];
            }
            RpcPrintLog(m);

            turnStep = 9;

        }
        else if (turnStep == 5)
        {
            // TODO 빙결된 이펙트 보여주고 잠시 딜레이

            turnStep = 6;

            // 턴을 진행한 플레이어가 빙결된 상태였으면 해동된다.
            players[turnPlayer].Thawed();
            //RpcPrintLog("turnStep 6(postprocessing)");
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
                        isEnd = true;
                        break;
                    }
                    // 한 명만 사망하였거나, 두 명이 동시에 사망하더라도 그 두 명을 모두 목표로 하는 플레이어가 없으면, 사망자들 중 한 명을 목표로 하는 모든 플레이어들의 공동 승리!
                    else if (players[GetTarget(i)[0]].HasDead()
                        || players[GetTarget(i)[1]].HasDead())
                    {
                        isWin[i] = true;
                        isEnd = true;
                    }
                }
            }
            if (isEnd)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (isWin[i]) RpcPrintLog(players[i].GetName() + " wins!");
                }
                RpcPrintLog("Battle ends.");
                turnStep = 8;
            }
            else
            {
                RpcPrintLog("Turn ends.");
                turnPlayer += 1;
                if (turnPlayer >= 5) turnPlayer = 0;
                turnStep = 1;
                RpcPrintLog("turnStep 1(" + players[turnPlayer].GetName() + " turn starts)");
            }
        }
        else if (turnStep == 9)
        {
            // 애니메이션이 끝나면 카드의 위치가 바뀌고 turnStep = 6 이 됩니다.
        }

	}

    public void SetObjectPlayer(int objectTargetIndex)
    {
        if (turnStep != 2 || objectTargetIndex < 0 || objectTargetIndex >= 5) return;
        objectPlayer = objectTargetIndex;
        turnStep = 3;
        RpcPrintLog("turnStep 3(select a card to play)");
    }

    public void SetCardToPlay(int cardCode, int playerIndex)
    {
        if (turnStep != 3 || cardCode < 0 || cardCode >= 10 || playerIndex < 0 || playerIndex >= 5) return;
        if (playerIndex == turnPlayer && turnPlayerCard == -1)
        {
            turnPlayerCard = cardCode;
            RpcPrintLog(players[playerIndex].GetName() + " sets " + GetCard(cardCode).GetCardName() + " card to play.");
        }
        else if (playerIndex == objectPlayer && objectPlayerCard == -1)
        {
            objectPlayerCard = cardCode;
            RpcPrintLog(players[playerIndex].GetName() + " sets " + GetCard(cardCode).GetCardName() + " card to play.");
        }
    }

    /*
    [Client]
    public void SetCameraVisible(int cp)
    {
        Debug.Log(cp);
        foreach (PlayerControl p in players)
        {
            p.gameObject.GetComponentInChildren<Camera>().enabled = false;
        }

        for (int i = 0; i < 10; i++)
        {
            cards[i].GetComponent<Card>().RpcFlipCardImmediate(i, true);
        }
        cards[cp * 2].GetComponent<Card>().RpcFlipCardImmediate(cp * 2, false);
        cards[cp * 2 + 1].GetComponent<Card>().RpcFlipCardImmediate(cp * 2 + 1, false);

        players[cp].gameObject.GetComponentInChildren<Camera>().enabled = true;
        for (int i = 0; i < playerPermutation.Count; i++)
        {
            if (playerPermutation[i].player.Equals(players[cp]))
            {
                Debug.Log(players[cp].GetName() + "'s objective is to eliminate "
                    + playerPermutation[playerPermutation[i].GetTargetIndex()[0]].player.GetName() + " and "
                    + playerPermutation[playerPermutation[i].GetTargetIndex()[1]].player.GetName());
                break;
            }
        }
    }
    */
    

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

    /*
    public PlayerControl GetCameraPlayer()
    {
        return players[cameraPlayer];
    }
    */
    /*
    public void DecideClick()
    {
        foreach (PlayerControl p in players)
        {
            p.DecideClicked();
        }
    }
    */

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
            Debug.Log("a");
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
        Debug.Log("turnStep 6(postprocessing)");
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
        /*
        foreach (GameObject c in cards)
        {
            if (c.GetComponent<Card>().GetCardCode() == cardCode)
            {
                return c.GetComponent<Card>();
            }
        }
        */
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

    [ClientRpc]
    public void RpcPrintLog(string msg)
    {
        LogDisplay.AddText(msg);
        Debug.Log(msg);
    }

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
    private void RpcSetExchangeComplete()
    {
        pusher.SetExchangeComplete();
    }

    [ClientRpc]
    private void RpcSetOpponentCard(int TP, int OP, int TPCardCode, int OPCardCode)
    {
        pusher.SetOpponentCard(TP, OP, TPCardCode, OPCardCode);
    }
}
