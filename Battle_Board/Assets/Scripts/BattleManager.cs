using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 대전을 전체적으로 관리하는 클래스입니다.
/// </summary>
public class BattleManager : MonoBehaviour {

    public GameObject player;
    public GameObject decideButton;
    public List<PlayerController> Players = new List<PlayerController>();   // 대전에 참여하는 플레이어들
    private List<TargetGraph> PlayerPermutation = new List<TargetGraph>();  // 목표 그래프, 플레이어는 랜덤 순서로 배치
    private List<Behavior> Behaviors = new List<Behavior>();                // 행동 결정 단계에서 결정이 완료된 행동들
    private List<bool> isWin = new List<bool>();                            // 플레이어 순서는 PlayerPermutation의 인덱스를 따르며, 그 플레이어가 대전에서 승리하면 true
    private List<PlayerController> Player1sTarget = new List<PlayerController>();

    public Text CharacterUI1;
    public Text CharacterUI2;
    public Text CharacterUI3;
    public Text CharacterUI4;
    public Text CharacterUI5;

    ParticleSystem PointerParticle;
    LineRenderer PointerLine;

    private int turn = 0;   // 0: 대전 시작, 1: 턴 시작, 2: 행동 결정, 3: 행동 수행, 4: 사망자 처리, 5: 대전 종료 확인

    /* 
     * TODO
     * Awake() 함수 안에서 플레이어 5명의 캐릭터 생성 -> 프로토타입 이후에
     * 대전 시작 시에 목표 그래프 만들고 플레이어를 랜덤으로 배치하기
     * 턴의 각 단계가 흘러가도록 하기
     * 행동 결정 단계에서 여러 예외 사항을 거르고, 예외가 발생하지 않으면 행동을 만들어 저장하고 있기
     * 저장해 둔 행동들을 해석하여 처리하기
     * 대전이 종료되었는지 판단하기
     */

    // Awake() 함수는 Start() 함수보다 항상 먼저 호출되며, 한 번만 호출됩니다.
    private void Awake()
    {
        turn = 0;
        List<PlayerController> tempPlayers = new List<PlayerController>();

        // 플레이어 5명의 캐릭터를 Instanciate하여 Players에 넣는다.
        // TODO 최종 버전에서는 각 플레이어의 캐릭터를 관리하는 매니저에서 정보를 받아와야 한다.
        Players.Add(Instantiate(player, new Vector3(0f, 0f, 0f), Quaternion.identity).GetComponent<PlayerController>());
        Players[0].gameObject.GetComponentInChildren<Camera>().targetDisplay = 1;
        //Players[0].gameObject.GetComponentInChildren<Canvas>().targetDisplay = 1;
        Players[0].SetPlayerName("Player1");
        Players[0].playerNum = 1;
        Players[0].SetAuto(false);
        tempPlayers.Add(Players[0]);

        Players.Add(Instantiate(player, new Vector3(3.236f, 0f, 2.351f), Quaternion.Euler(0f, -72f, 0f)).GetComponent<PlayerController>());
        Players[1].gameObject.GetComponentInChildren<Camera>().targetDisplay = 2;
        //Players[1].gameObject.GetComponentInChildren<Canvas>().targetDisplay = 2;
        Players[1].SetPlayerName("Player2");
        Players[1].playerNum = 2;
        tempPlayers.Add(Players[1]);

        Players.Add(Instantiate(player, new Vector3(2f, 0f, 6.155f), Quaternion.Euler(0f, -144f, 0f)).GetComponent<PlayerController>());
        Players[2].gameObject.GetComponentInChildren<Camera>().targetDisplay = 3;
        //Players[2].gameObject.GetComponentInChildren<Canvas>().targetDisplay = 3;
        Players[2].SetPlayerName("Player3");
        Players[2].playerNum = 3;
        tempPlayers.Add(Players[2]);

        Players.Add(Instantiate(player, new Vector3(-2f, 0f, 6.155f), Quaternion.Euler(0f, 144f, 0f)).GetComponent<PlayerController>());
        Players[3].gameObject.GetComponentInChildren<Camera>().targetDisplay = 4;
        //Players[3].gameObject.GetComponentInChildren<Canvas>().targetDisplay = 4;
        Players[3].SetPlayerName("Player4");
        Players[3].playerNum = 4;
        tempPlayers.Add(Players[3]);

        Players.Add(Instantiate(player, new Vector3(-3.236f, 0f, 2.351f), Quaternion.Euler(0f, 72f, 0f)).GetComponent<PlayerController>());
        Players[4].gameObject.GetComponentInChildren<Camera>().targetDisplay = 5;
        //Players[4].gameObject.GetComponentInChildren<Canvas>().targetDisplay = 5;
        Players[4].SetPlayerName("Player5");
        Players[4].playerNum = 5;
        tempPlayers.Add(Players[4]);

        // 목표 그래프에 5명의 플레이어를 랜덤한 순서로 배치한다.
        for (int i = 0; i < 5; i++)
        {
            isWin.Add(false);
            PlayerPermutation.Add(new TargetGraph(i));
            int r = Random.Range(0, tempPlayers.Count);
            PlayerPermutation[i].player = tempPlayers[r];
            tempPlayers.RemoveAt(r);
        }
    }

    // Start() 함수는 Awake() 함수가 호출된 이후에 한 번만 호출되며, 상태를 초기화하는 데 사용됩니다.
    void Start () {
        // 대전을 시작함
        Debug.Log("Battle starts.");
        for (int i = 0; i < PlayerPermutation.Count; i++)
        {
            if (PlayerPermutation[i].player.Equals(Players[0])) {
                Debug.Log(Players[0].GetPlayerName() + "'s objective is to eliminate "
                    + PlayerPermutation[PlayerPermutation[i].GetTargetIndex()[0]].player.GetPlayerName() + " and "
                    + PlayerPermutation[PlayerPermutation[i].GetTargetIndex()[1]].player.GetPlayerName());
                break;
            }
        }

        UIchanger();
        
        turn = 1;
	}

    // FixedUpdate() 함수는 고정된 프레임 수(1초에 60번)에 따라 매 프레임마다 호출됩니다.
    // 여기서의 FixedUpdate() 함수는 턴의 각 단계를 진행시키는 함수입니다.
    void FixedUpdate () {
		if (turn == 1)
        {

            // 턴이 시작되면
            //Debug.Log("Turn starts.");
            foreach (PlayerController p in Players)
            {
                // 모든 생존자 마나 2씩 회복
                p.ManaRecovery();
            }
            // 행동 결정 단계로 넘어감
            turn = 2;
        }
        else if (turn == 2)
        {
            // 행동 결정 단계에서
            //Debug.Log("Decide your behavior.");

            if (Players[0].GetTargetDecide())       // 대상 결정 중이면
            {
                if (!decideButton.activeInHierarchy)
                    decideButton.SetActive(true);   // 결정 버튼이 나타나게 한다.
                if (Input.GetMouseButtonDown(0))
                    PlayerToSelectTarget();         // 플레이어1이 선택한 대상들을 저장하여 기억한다.
            }
            else
            {
                if (decideButton.activeInHierarchy)
                    decideButton.SetActive(false);  // 결정 버튼이 보이지 않게 한다.
            }
            bool isCompleted = true;
            foreach (PlayerController p in Players)
            {
                // 사망한 플레이어는 행동을 결정하지 않음
                if (p.GetDead()) continue;
                else if (!p.GetHasDecided())
                {
                    isCompleted = false;
                    break;
                }
            }
            // TODO 각 플레이어가 결정한 행동을 저장해 놓아야 한다.
            // 행동은 PlayerController.cs에서 만들도록 한다.
            // 만약 모든 생존자가 행동 결정을 완료하면 행동 수행 단계로 넘어감
            if (isCompleted) turn = 3;
        }
        else if (turn == 3)
        {
            foreach (Behavior behavior in Behaviors)
            {
                if (behavior.GetObjectPlayers().Count == 2)
                    Debug.Log(behavior.GetSubjectPlayer().GetPlayerName() + " -> " 
                        + behavior.GetObjectPlayers()[0].GetPlayerName() + ", " + behavior.GetObjectPlayers()[1].GetPlayerName() + " (" + behavior.GetBehavior().Name + ")");
                else if (behavior.GetObjectPlayers().Count == 1)
                    Debug.Log(behavior.GetSubjectPlayer().GetPlayerName() + " -> "
                        + behavior.GetObjectPlayers()[0].GetPlayerName() + " (" + behavior.GetBehavior().Name + ")");
            }
            //Debug.Log("Performming behaviors...");
            // 모든 플레이어의 상태이상을 해제하고 행동을 결정하지 않은 상태로 초기화함
            foreach (PlayerController p in Players)
            {
                p.Purify();
                p.SetNotDecided();
            }
            // 전 단계에서 결정된 행동들을 수행함
            foreach (Behavior b in Behaviors)
            {
                // TODO 피해 무시, 상대 행동 무효화 등의 행동은 가장 먼저 이루어져야 함
                //StartCoroutine("PerformBehavior", b);
                BehaviorManager.Perform(b);
            }
            Behaviors = new List<Behavior>();
            UIchanger();
            turn = 4;
        }
        else if (turn == 4)
        {
            //Debug.Log("Processing the dead...");
            foreach (PlayerController p in Players)
            {
                p.Death();
            }
            turn = 5;
        }
        else if (turn == 5)
        {
            bool isEnd = false;
            bool allDead = true;
            for (int i = 0; i < 5; i++)
            {
                //Debug.Log(PlayerPermutation[i].player.GetPlayerName() + "'s target is " + PlayerPermutation[PlayerPermutation[i].GetTargetIndex()[0]].player.GetPlayerName() + " and "+ PlayerPermutation[PlayerPermutation[i].GetTargetIndex()[1]].player.GetPlayerName() + ".");
                if (!PlayerPermutation[i].player.GetDead()) {
                    allDead = false;
                    if (PlayerPermutation[PlayerPermutation[i].GetTargetIndex()[0]].player.GetDead()
                        && PlayerPermutation[PlayerPermutation[i].GetTargetIndex()[1]].player.GetDead())
                    {
                        isWin[i] = true;
                        isEnd = true;
                        Debug.Log(PlayerPermutation[i].player.GetPlayerName() + " wins!");
                    }
                }
            }
            if (allDead)
            {
                Debug.Log("No one wins.");
                isEnd = true;
            }
            if (isEnd)
            {
                Debug.Log("Battle ends.");
                turn = 6;
            }
            else
            {
                Debug.Log("Turn ends.");
                turn = 1;   // 7로 놓으면 수동으로(버튼을 눌러서) 턴 진행, 1로 놓으면 자동으로 턴 진행
            }
        }

	}

    /// <summary>
    /// 각 플레이어가 행동 결정을 완료하면 이 함수를 호출하여 행동을 확정합니다. 행동 수행 단계에서 이 행동들을 수행하게 됩니다.
    /// </summary>
    /// <param name="b">결정 완료된 행동</param>
    public void AddBehavior(Behavior b)
    {
        if (BehaviorManager.Verificate(b))
        {
            Behaviors.Add(b);
        }
    }
    
    /// <summary>
    /// 특정 플레이어에게 행동을 결정하도록 하는 함수입니다. UI상에서 행동 버튼을 클릭했을 때 실행합니다.
    /// </summary>
    /// <param name="playerNum">1 ~ 5 사이의 플레이어 번호</param>
    /// <param name="behaviorName">행동 이름</param>
    public void PlayerToMakeBehavior(string behaviorName)
    {
        // playerNum은 1 ~ 5 사이의 수입니다.
        int playerNum = 1;
        if (playerNum < 1 || playerNum > Players.Count)
        {
            Debug.LogWarning("Player" + playerNum + "does not exist!");
            return;
        }
        Debug.Log("MakeBehavior " + behaviorName + " clicked.");
        Players[playerNum-1].MakeBehavior(behaviorName);
    }

    public void PlayerToDecide()
    {
        int playerNum = 1;
        if (playerNum < 1 || playerNum > Players.Count)
        {
            Debug.LogWarning("Player" + playerNum + "does not exist!");
            return;
        }
        //Debug.Log("Decide clicked.");
        Players[playerNum - 1].DecideClick(Player1sTarget);
        Player1sTarget = new List<PlayerController>();
    }

    public void PlayerToSelectTarget()
    {
        int playerNum = 1;
        if (playerNum < 1 || playerNum > Players.Count)
        {
            Debug.LogWarning("Player" + playerNum + "does not exist!");
            return;
        }
        Ray ray = Players[playerNum - 1].gameObject.GetComponentInChildren<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 8)))
        {
            //Debug.Log("Click " + hit.collider.name + ".");
            Debug.DrawLine(ray.origin, hit.point, Color.blue, 3f);
            if (hit.collider.gameObject.GetComponentInParent<PlayerController>() != null)
            {
                if (Player1sTarget.IndexOf(hit.collider.gameObject.GetComponentInParent<PlayerController>()) == -1)
                {
                    Player1sTarget.Add(hit.collider.gameObject.GetComponentInParent<PlayerController>());
                    Debug.Log("Add " + hit.collider.gameObject.GetComponentInParent<PlayerController>().GetPlayerName() + " to targets.");
                }
                else
                {
                    Player1sTarget.Remove(hit.collider.gameObject.GetComponentInParent<PlayerController>());
                    Debug.Log("Remove " + hit.collider.gameObject.GetComponentInParent<PlayerController>().GetPlayerName() + " from targets.");
                }
            }
        }
    }

    public int TargetableNumberExceptOneself(PlayerController subject)
    {
        int count = 0;
        foreach (PlayerController p in Players)
        {
            if (p.Equals(subject)) continue;
            if (!p.GetDead() && p.GetTargetable()) count++;
        }
        return count;
    }

    public int TargetableNumber()
    {
        int count = 0;
        foreach (PlayerController p in Players)
        {
            if (!p.GetDead() && p.GetTargetable()) count++;
        }
        return count;
    }
    //목표에게 파티클라인을 만드는 함수
    public void TargetPointer(PlayerController target)
    {
        PointerParticle.Stop();
        PointerParticle.Play();
        
        PointerLine.enabled = true;
        PointerLine.SetPosition(0, Players[0].gameObject.transform.position);
        PointerLine.SetPosition(1, Players[target.playerNum - 1].gameObject.transform.position);
    }

    //파티클라인을 지우는 함수
    public void TargetPointerRemover()
    {
        PointerLine.enabled = false;
    }

    //UI를 변경하는 함수
    public void UIchanger()
    {
        if (Players[0].GetHealth() < 0)
            CharacterUI1.text = "0/" + Players[0].GetMaxHealth();
        else
            CharacterUI1.text = Players[0].GetHealth() + "/" + Players[0].GetMaxHealth();

        if (Players[1].GetHealth() < 0)
            CharacterUI2.text = "0/" + Players[1].GetMaxHealth();
        else
            CharacterUI2.text = Players[1].GetHealth() + "/" + Players[1].GetMaxHealth();

        if (Players[2].GetHealth() < 0)
            CharacterUI3.text = "0/" + Players[2].GetMaxHealth();
        else
            CharacterUI3.text = Players[2].GetHealth() + "/" + Players[2].GetMaxHealth();

        if (Players[3].GetHealth() < 0)
            CharacterUI4.text = "0/" + Players[3].GetMaxHealth();
        else
            CharacterUI4.text = Players[3].GetHealth() + "/" + Players[3].GetMaxHealth();

        if (Players[4].GetHealth() < 0)
            CharacterUI5.text = "0/" + Players[4].GetMaxHealth();
        else
            CharacterUI5.text = Players[4].GetHealth() + "/" + Players[4].GetMaxHealth();
    }

    public int GetTurn()
    {
        return turn;
    }

    public bool GetEnd()
    {
        return (turn == 6);
    }

    public void SetTurnStart()
    {
        if (turn == 7)
        {
            turn = 1;
        }
    }

    /*
    // 이 함수는 Coroutine 함수입니다.
    IEnumerator PerformBehavior(Behavior behavior)
    {
        // TODO 인자로 받은 행동을 수행하도록 하기
        yield return null;
    }
    */
}
