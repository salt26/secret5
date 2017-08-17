using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class BattleManager : NetworkBehaviour {

    public GameObject player;
    [SerializeField] private List<GameObject> cards = new List<GameObject>();
    // cards의 인덱스는 어떤 플레이어의 손패에 카드가 있는지를 나타낸다.
    // 0 ~ 1: players[0]의 손패, 2 ~ 3: players[1]의 손패, 4 ~ 5: players[2]의 손패,
    // 6 ~ 7: players[3]의 손패, 8 ~ 9: players[4]의 손패
    
    private PushingCard[] pushingcard = new PushingCard[2];

    private List<PlayerController> players = new List<PlayerController>();
    private List<TargetGraph> playerPermutation = new List<TargetGraph>();
    private List<bool> isWin = new List<bool>();
    private int turnPlayer = 0; // 현재 자신의 턴을 진행하는 플레이어 번호
    private int turnStep;   // 턴의 단계 (0: 대전 시작, 1: 턴 시작, 2: 턴 진행자의 교환 상대 선택과 교환할 카드 선택,
                            //           3: 교환당하는 자의 교환할 카드 선택, 4: 교환 중(카드를 낼 때와 받을 때 효과 발동),
                            //           5: 빙결 발동, 6: 턴이 끝날 때 효과 발동, 7: 턴 종료)
    private PlayerController objectPlayer;  // 교환당하는 플레이어
    private Card turnPlayerCard;            // 턴을 진행한 플레이어가 낸 카드
    private Card objectPlayerCard;          // 교환당하는 플레이어가 낸 카드
    private Exchange exchange;
    private int cameraPlayer = 0;

    private int tpcIndex;
    private int opcIndex;
    private Vector3 tpcPosition;
    private Vector3 opcPosition;
    private Quaternion tpcRotation;
    private Quaternion opcRotation;
    private int count;

    private static CardDatabase cd;

    public override void OnStartServer()
    {
        base.OnStartServer(); turnStep = 0;
        cd = GetComponent<CardDatabase>();
        pushingcard = GameObject.Find("CardPanel").GetComponentsInChildren<PushingCard>();

        List<PlayerController> tempPlayers = new List<PlayerController>();

        players.Add(Instantiate(player, new Vector3(0f, 0f, 0f), Quaternion.identity).GetComponent<PlayerController>());
        players.Add(Instantiate(player, new Vector3(3.236f, 0f, 2.351f), Quaternion.Euler(0f, -72f, 0f)).GetComponent<PlayerController>());
        players.Add(Instantiate(player, new Vector3(2f, 0f, 6.155f), Quaternion.Euler(0f, -144f, 0f)).GetComponent<PlayerController>());
        players.Add(Instantiate(player, new Vector3(-2f, 0f, 6.155f), Quaternion.Euler(0f, 144f, 0f)).GetComponent<PlayerController>());
        players.Add(Instantiate(player, new Vector3(-3.236f, 0f, 2.351f), Quaternion.Euler(0f, 72f, 0f)).GetComponent<PlayerController>());
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
            NetworkServer.Spawn(players[i].gameObject);
        }

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
    /*
    private void Awake()
    {
        turnStep = 0;
        cd = GetComponent<CardDatabase>();
        pushingcard = GameObject.Find("CardPanel").GetComponentsInChildren<PushingCard>();

        List<PlayerController> tempPlayers = new List<PlayerController>();

        /*
        List<int> rand = RandomListGenerator(5);
        List<Vector3> pos = new List<Vector3>();
        List<Quaternion> rot = new List<Quaternion>();
        for (int i = 0; i < 5; i++)
        {
            Debug.Log("rand[" + i + "] = " + rand[i]);
        }

        pos.Add(new Vector3(0f, 0f, 0f));
        pos.Add(new Vector3(3.236f, 0f, 2.351f));
        pos.Add(new Vector3(2f, 0f, 6.155f));
        pos.Add(new Vector3(-2f, 0f, 6.155f));
        pos.Add(new Vector3(-3.236f, 0f, 2.351f));

        rot.Add(Quaternion.identity);
        rot.Add(Quaternion.Euler(0f, -72f, 0f));
        rot.Add(Quaternion.Euler(0f, -144f, 0f));
        rot.Add(Quaternion.Euler(0f, 144f, 0f));
        rot.Add(Quaternion.Euler(0f, 72f, 0f));
        */
        /*
        players.Add(Instantiate(player, new Vector3(0f, 0f, 0f), Quaternion.identity).GetComponent<PlayerController>());
        players.Add(Instantiate(player, new Vector3(3.236f, 0f, 2.351f), Quaternion.Euler(0f, -72f, 0f)).GetComponent<PlayerController>());
        players.Add(Instantiate(player, new Vector3(2f, 0f, 6.155f), Quaternion.Euler(0f, -144f, 0f)).GetComponent<PlayerController>());
        players.Add(Instantiate(player, new Vector3(-2f, 0f, 6.155f), Quaternion.Euler(0f, 144f, 0f)).GetComponent<PlayerController>());
        players.Add(Instantiate(player, new Vector3(-3.236f, 0f, 2.351f), Quaternion.Euler(0f, 72f, 0f)).GetComponent<PlayerController>());
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
            NetworkServer.Spawn(players[0].gameObject);
        }
    }
    */
    /*
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
	
	void FixedUpdate () {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = players[cameraPlayer].GetComponentInChildren<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 8)))
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green, 3f);
                if (hit.collider.gameObject.GetComponentInParent<PlayerController>() != null
                    && !hit.collider.gameObject.GetComponentInParent<PlayerController>().Equals(players[cameraPlayer]))
                {
                    cameraPlayer = hit.collider.gameObject.GetComponentInParent<PlayerController>().GetPlayerNum() - 1;
                    SetCameraVisible(cameraPlayer);
                    Debug.Log(hit.collider.gameObject.GetComponentInParent<PlayerController>().GetName() + "'s camera.");
                }
            }
        }
        if (turnStep == 1)
        {
            // 빙결된 상태이면 교환을 할 수 없다.
            if (players[turnPlayer].HasFreezed())
            {
                turnStep = 5;
                Debug.Log("turnStep 5(freezed)");
            }
            else
            {
                turnStep = 2;
                Debug.Log("turnStep 2(select a player to exchange with, and select a card to play)");
            }
        }
		else if (turnStep == 2)
        {
            // TODO 플레이어의 응답을 기다리고 제한 시간 측정하기
        }
        else if (turnStep == 3)
        {
            // TODO 플레이어의 응답을 기다리고 제한 시간 측정하기
            if (turnPlayerCard != null && objectPlayerCard != null)
            {
                turnStep = 4;
                Debug.Log("turnStep 4(preprocessing)");
            }
        }
        else if (turnStep == 4)
        {
            exchange = new Exchange(players[turnPlayer], objectPlayer, turnPlayerCard, objectPlayerCard);
            if (exchange.GetTurnNum() == 0)
            {
                Debug.Log("Exception throwed while making an exchange!");
                turnStep = 8;
                return;
            }
            tpcIndex = cards.IndexOf(exchange.GetTurnPlayerCard().gameObject);
            opcIndex = cards.IndexOf(exchange.GetObjectPlayerCard().gameObject);

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
            if (cameraPlayer == tpcIndex / 2) {
                cards[tpcIndex].GetComponent<Card>().FlipCard(tpcIndex, true);
            }
            else if (cameraPlayer == opcIndex / 2)
            {
                cards[opcIndex].GetComponent<Card>().FlipCard(opcIndex, true);
            }
            
            cards[tpcIndex].GetComponent<Card>().MoveCard(tpcIndex * 10 + opcIndex);
            cards[opcIndex].GetComponent<Card>().MoveCard(opcIndex * 10 + tpcIndex);

            if (cameraPlayer == tpcIndex / 2)
            {
                cards[opcIndex].GetComponent<Card>().FlipCard(tpcIndex, false);
            }
            else if (cameraPlayer == opcIndex / 2)
            {
                cards[tpcIndex].GetComponent<Card>().FlipCard(opcIndex, false);
            }

            // 손패 교환
            GameObject temp = cards[tpcIndex];
            cards[tpcIndex] = cards[opcIndex];
            cards[opcIndex] = temp;
            turnStep = 9;

        }
        else if (turnStep == 5)
        {
            // TODO 빙결된 이펙트 보여주고 잠시 딜레이
            turnStep = 6;

            // 턴을 진행한 플레이어가 빙결된 상태였으면 해동된다.
            players[turnPlayer].Thawed();
            Debug.Log("turnStep 6(postprocessing)");
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
            Debug.Log("turnStep 7(turn ends)");
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
                if (!playerPermutation[i].player.HasDead())
                {
                    // 두 명이 동시에 사망하였는데 그 두 명을 모두 목표로 하는 플레이어가 있다면 그 플레이어의 단독 승리!
                    if (playerPermutation[playerPermutation[i].GetTargetIndex()[0]].player.HasDead()
                        && playerPermutation[playerPermutation[i].GetTargetIndex()[1]].player.HasDead())
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
                    else if (playerPermutation[playerPermutation[i].GetTargetIndex()[0]].player.HasDead()
                        || playerPermutation[playerPermutation[i].GetTargetIndex()[1]].player.HasDead())
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
                    if (isWin[i]) Debug.Log("Player" + playerPermutation[i].player.GetPlayerNum() + " wins!");
                }
                Debug.Log("Battle ends.");
                turnStep = 8;
            }
            else
            {
                Debug.Log("Turn ends.");
                turnPlayer += 1;
                if (turnPlayer >= 5) turnPlayer = 0;
                turnStep = 1;
                Debug.Log("turnStep 1(" + players[turnPlayer].GetName() + " turn starts)");
            }
        }
        else if (turnStep == 9)
        {
            // 애니메이션이 끝나면 카드의 위치가 바뀌고 turnStep = 6 이 됩니다.
        }
	}

    public void SetObjectPlayer(PlayerController objectTarget)
    {
        if (turnStep != 2 || objectTarget == null) return;
        objectPlayer = objectTarget;
        turnStep = 3;
        Debug.Log("turnStep 3(select a card to play)");
    }

    public void SetCardToPlay(Card card, PlayerController player)
    {
        if (turnStep != 3 || card == null || player == null) return;
        if (player.Equals(players[turnPlayer]) && turnPlayerCard == null)
        {
            turnPlayerCard = card;
            Debug.Log(player.GetName() + " sets " + card.GetCardName() + " card to play.");
        }
        else if (player.Equals(objectPlayer) && objectPlayerCard == null)
        {
            objectPlayerCard = card;
            Debug.Log(player.GetName() + " sets " + card.GetCardName() + " card to play.");
        }
    }

    private void SetCameraVisible(int cp)
    {
        foreach (PlayerController p in players)
        {
            p.gameObject.GetComponentInChildren<Camera>().enabled = false;
        }

        for (int i = 0; i < 10; i++)
        {
            cards[i].GetComponent<Card>().FlipCardImmediate(i, true);
        }
        cards[cp * 2].GetComponent<Card>().FlipCardImmediate(cp * 2, false);
        cards[cp * 2 + 1].GetComponent<Card>().FlipCardImmediate(cp * 2 + 1, false);

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

    public List<Card> GetPlayerHand(PlayerController player)
    {
        List<Card> hand = new List<Card>();
        int playerNum = players.IndexOf(player);
        if (playerNum != -1)
        {
            hand.Add(cards[playerNum * 2].GetComponent<Card>());
            hand.Add(cards[playerNum * 2 + 1].GetComponent<Card>());
            return hand;
        }
        else return null;
    }

    public PlayerController GetTurnPlayer()
    {
        return players[turnPlayer];
    }

    public PlayerController GetObjectPlayer()
    {
        return objectPlayer;
    }

    public int GetTurnStep()
    {
        return turnStep;
    }

    public PlayerController GetCameraPlayer()
    {
        return players[cameraPlayer];
    }

    public void DecideClick()
    {
        foreach (PlayerController p in players)
        {
            p.DecideClicked();
        }
    }

    public List<GameObject> GetCardsInHand()
    {
        return cards;
    }
    

    public Card GetPlayerSelectedCard(PlayerController player)
    {
        if (turnStep != 3 || player == null) return null;
        else if (player.Equals(players[turnPlayer]))
        {
            return turnPlayerCard;
        }
        else if (player.Equals(objectPlayer))
        {
            return objectPlayerCard;
        }
        else
        {
            return null;
        }
    }
    /// <summary>
    /// 교환 처리 이후의 단계로 넘어가기 위해 호출되는 함수입니다.
    /// </summary>
    public void AfterExchange()
    {
        if (turnStep != 9) return;

        for (int i = 0; i < 2; i++)
        {
            pushingcard[i].SetExchangeComplete();
        }
        objectPlayer = null;
        turnPlayerCard = null;
        objectPlayerCard = null;

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
        List<GameObject> c = new List<GameObject>();
        for (int i = 0; i < 5; i++)
        {
            c.Add(cards[2 * rand1[i]]);
            c.Add(cards[2 * rand2[i] + 1]);
        }
        cards = c;
    }
}
