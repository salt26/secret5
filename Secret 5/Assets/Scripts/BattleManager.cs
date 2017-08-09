using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour {

    public GameObject player;
    [SerializeField] private List<GameObject> cards = new List<GameObject>();
    // cards의 인덱스는 어떤 플레이어의 손패에 카드가 있는지를 나타낸다.
    // 0 ~ 1: players[0]의 손패, 2 ~ 3: players[1]의 손패, 4 ~ 5: players[2]의 손패,
    // 6 ~ 7: players[3]의 손패, 8 ~ 9: players[4]의 손패

    private List<PlayerController> players = new List<PlayerController>();
    private List<TargetGraph> playerPermutation = new List<TargetGraph>();
    private List<bool> isWin = new List<bool>();
    private int turnPlayer; // 현재 자신의 턴을 진행하는 플레이어 번호
    private int turnStep;   // 턴의 단계 (0: 대전 시작, 1: 턴 시작, 2: 교환 상대 선택,
                            //           3: 교환할 카드 선택, 4: 교환 중(카드를 낼 때와 받을 때 효과 발동),
                            //           5: 빙결 발동, 6: 턴이 끝날 때 효과 발동, 7: 턴 종료)

    private void Awake()
    {
        turnStep = 0;
        List<PlayerController> tempPlayers = new List<PlayerController>();

        players.Add(Instantiate(player, new Vector3(0f, 0f, 0f), Quaternion.identity).GetComponent<PlayerController>());
        players[0].gameObject.GetComponentInChildren<Camera>().targetDisplay = 0;
        players[0].SetPlayerNum(1);
        tempPlayers.Add(players[0]);

        players.Add(Instantiate(player, new Vector3(3.236f, 0f, 2.351f), Quaternion.Euler(0f, -72f, 0f)).GetComponent<PlayerController>());
        players[1].gameObject.GetComponentInChildren<Camera>().targetDisplay = 1;
        players[1].SetPlayerNum(2);
        tempPlayers.Add(players[1]);

        players.Add(Instantiate(player, new Vector3(2f, 0f, 6.155f), Quaternion.Euler(0f, -144f, 0f)).GetComponent<PlayerController>());
        players[2].gameObject.GetComponentInChildren<Camera>().targetDisplay = 2;
        players[2].SetPlayerNum(3);
        tempPlayers.Add(players[2]);

        players.Add(Instantiate(player, new Vector3(-2f, 0f, 6.155f), Quaternion.Euler(0f, 144f, 0f)).GetComponent<PlayerController>());
        players[3].gameObject.GetComponentInChildren<Camera>().targetDisplay = 3;
        players[3].SetPlayerNum(4);
        tempPlayers.Add(players[3]);

        players.Add(Instantiate(player, new Vector3(-3.236f, 0f, 2.351f), Quaternion.Euler(0f, 72f, 0f)).GetComponent<PlayerController>());
        players[4].gameObject.GetComponentInChildren<Camera>().targetDisplay = 4;
        players[4].SetPlayerNum(5);
        tempPlayers.Add(players[4]);

        // 목표 그래프에 5명의 플레이어를 랜덤한 순서로 배치한다.
        for (int i = 0; i < 5; i++)
        {
            isWin.Add(false);
            playerPermutation.Add(new TargetGraph(i));
            int r = Random.Range(0, tempPlayers.Count);
            playerPermutation[i].player = tempPlayers[r];
            tempPlayers.RemoveAt(r);
        }
    }
    // Use this for initialization
    void Start ()
    {
        turnPlayer = Random.Range(0, 5);
        turnStep = 1;
        Debug.Log("Battle starts.");
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (turnStep == 1)
        {
            if (players[turnPlayer].HasFreezed())
            {
                turnStep = 5;
            }
            else
            {
                turnStep = 2;
            }
        }
		else if (turnStep == 2)
        {

        }
        else if (turnStep == 3)
        {

        }
        else if (turnStep == 4)
        {

        }
        else if (turnStep == 5)
        {

        }
        else if (turnStep == 6)
        {
            // TODO players[turnPlayer]가 폭탄 카드를 가지고 있으면 Damaged() 호출
        }
        else if (turnStep == 7)
        {
            bool isEnd = false;
            for (int i = 0; i < 5; i++)
            {
                players[i].UpdateHealth();
            }
            // TODO 이번 턴에 일어난 교환에 대해 체력 변화량 반영
            for (int i = 0; i < 5; i++)
            {
                if (!playerPermutation[i].player.HasDead())
                {
                    if (playerPermutation[playerPermutation[i].GetTargetIndex()[0]].player.HasDead()
                        || playerPermutation[playerPermutation[i].GetTargetIndex()[1]].player.HasDead())
                    {
                        isWin[i] = true;
                        isEnd = true;
                        Debug.Log("Player" + playerPermutation[i].player.GetPlayerNum() + " wins!");
                    }
                }
                // TODO 두 명이 동시에 사망한 경우의 승리처리에 예외를 두어야 함.
            }
            if (isEnd)
            {
                Debug.Log("Battle ends.");
                turnStep = 7;
            }
            else
            {
                Debug.Log("Turn ends.");
                turnPlayer += 1;
                if (turnPlayer >= 5) turnPlayer = 0;
                turnStep = 1;
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
}
