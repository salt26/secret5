using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Prototype.NetworkLobby;

public class PlayerControl : NetworkBehaviour
{

    [SyncVar] private int currentHealth;     // 현재 남은 체력(실시간으로 변화, 외부 열람 불가)
    [SerializeField] [SyncVar] private int maxHealth = 6;     // 최대 체력(초기 체력)
    [SyncVar] private GameObject character;  // 캐릭터 모델
    [SyncVar] private bool isDead = false;   // 사망 여부(true이면 사망)
    [SyncVar] public string playerName;     // 플레이어 이름
    [SyncVar] public int playerNum;         // 대전에서 부여된 플레이어 번호 (1 ~ 5)
    [SyncVar] public Color color = Color.white;


    [SyncVar] private int displayedHealth;                    // 현재 남은 체력(턴이 끝날 때만 변화, 외부 열람 가능)
    [SyncVar] private bool isFreezed = false;                 // 빙결 여부(true이면 다음 한 번의 내 턴에 교환 불가)

    private bool isAI = false;                      // 인공지능 플레이어 여부(true이면 인공지능, false이면 사람)
    private bool hasDecidedObjectPlayer = false;    // 내 턴에 교환 상대를 선택했는지 여부
    private bool hasDecidedPlayCard = false;        // 교환 시 교환할 카드를 선택했는지 여부
    private PlayerControl objectTarget;             // 내가 선택한 교환 대상
    private Card playCardAI;                        // 인공지능이 낼 카드

    private RectTransform HealthBar;                // HP UI
    [SerializeField] private GameObject playerCamera;

    private static BattleManager bm;
    //private static Alert alert;

    private GameObject Border;
    private SpriteRenderer Face;
    
    public GameObject Ice;
    public GameObject targetMark;
    private bool isMarked; //마크가 되었는지 여부

    private bool isAlerted0;
    private bool isAlerted1;
    private bool isAlerted2;
    private bool isAlerted3;

    private bool isStart;
    private bool isThinking;    // 인공지능의 생각 전 딜레이 동안 true가 됨
    private bool isCardDragging;  // 큰 카드를 드래그하는 동안 true가 됨

    void Awake () {
        // bm은 Awake에서 아직 로딩되지 않았을 수 있음. 즉, BattleManager.Awake가 PlayerControl.Awake보다 늦게 실행될 수 있음. 
        //alert = Alert.alert;
        HealthBar = GetComponentInChildren<Finder>().GetComponent<Image>().rectTransform;
        Border = GetComponentsInChildren<SpriteRenderer>()[1].gameObject;
        Face = GetComponentsInChildren<SpriteRenderer>()[0];
        Border.SetActive(false);
        currentHealth = maxHealth;
        displayedHealth = currentHealth;
        isAlerted0 = false;
        isAlerted1 = false;
        isAlerted2 = false;
        isAlerted3 = false;
        isStart = false;
        isThinking = false;
        isCardDragging = false;
        if (transform.position.z < 1f)
        {
            playerNum = 1;
        }
        else if (transform.position.z < 4f)
        {
            if (transform.position.x > 0f)
            {
                playerNum = 2;
            }
            else
            {
                playerNum = 5;
            }
        }
        else
        {
            if (transform.position.x > 0f)
            {
                playerNum = 3;
            }
            else
            {
                playerNum = 4;
            }
        }
        //Log("Awake " + playerName);
    }

    void Start()
    {   
        // bm은 Start에서 아직 로딩되지 않았을 수 있음. 즉, BattleManager.Awake가 PlayerControl.Start보다 늦게 실행될 수 있음. 
        Renderer[] rends = GetComponentsInChildren<Renderer>();
        rends[0].material.color = color;
        //Log("Start " + playerName);
        /*
        if (GetPlayerIndex() != -1)
            bm.SetCameraVisible(GetPlayerIndex());
            */
    }

    // 개별 클라이언트에서만 보여져야 하는 것들을 이 함수 안에 넣습니다.
    // 서버와 일치되어야 하는 변수나 함수는 여기에서 빼야 합니다.
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        // bm은 Start에서 아직 로딩되지 않았을 수 있음. 즉, BattleManager.Awake가 PlayerControl.Start보다 늦게 실행될 수 있음. 
        //Log("OnStartLocalPlayer");
        playerCamera.SetActive(true);
        PushingCard.localPlayer = this;
        Pusher.localPlayer = this;
        Card.localPlayer = this;
        ObjectiveHighlight();
    }
    

    void FixedUpdate()
    {
        if (bm == null)
        {
            bm = BattleManager.bm;
            return;
        }
        if (!isStart)
        {
            bm.players[playerNum - 1] = this;
            isStart = true;
            //Log("FixedUpdate " + playerName);
            if (isLocalPlayer)
                CmdReady();
        }
        /*
        if (isLocalPlayer && Input.GetMouseButtonDown(1))
        {
            ConsoleLogUI.ClearText(); // TODO 임시 코드
            string m = "cardcode";
            for (int i = 0; i < 10; i++)
            {
                m += " " + bm.GetCardCode()[i];
            }
            Log(m);
        }
        */
        if (isLocalPlayer)
        {
            StatusUpdate();
        }
        if (isLocalPlayer && Input.GetMouseButton(0) && Input.touchCount <= 1 && !isCardDragging)
        {
            /*
            if (bm.GetObjectPlayer() != null)
                Log("Mouse Clicked. bm.GetTurnStep(): " + bm.GetTurnStep() + ", bm.GetTurnPlayer(): " + bm.GetTurnPlayer().GetName() + ", bm.GetObjectPlayer(): " + bm.GetObjectPlayer().GetName());
            else Log("Mouse Clicked. bm.GetTurnStep(): " + bm.GetTurnStep() + ", bm.GetTurnPlayer(): " + bm.GetTurnPlayer().GetName());
            *//*
            if (bm.GetTurnStep() == 2 && objectTarget != null && bm.GetTurnPlayer().Equals(this))
                PlayerToSelectCard();
            if (bm.GetTurnStep() == 3 && bm.GetObjectPlayer() != null && bm.GetObjectPlayer().Equals(this))
                PlayerToSelectCard();
                */
            if (bm.GetTurnStep() == 2 && bm.GetTurnPlayer().Equals(this))
            {
                PlayerToSelectTarget();
            }
        }

        if (bm.GetTurnStep() == 3 && isMarked == true)
            Destroy(GameObject.Find("TargetMark(Clone)"));

        HealthBar.sizeDelta = new Vector2(displayedHealth * 100f / 6f, HealthBar.sizeDelta.y); // HealthBar 변경 -> displayedHealth 기준으로 계산하도록 수정

        if (isServer && isAI && bm.GetTurnStep() == 2 && bm.GetTurnPlayer().Equals(this) && !isThinking)
        {
            isThinking = true;
            StartCoroutine("AITurnDelay");
        }
        if (isServer && isAI && bm.GetTurnStep() == 3 && bm.GetObjectPlayer() != null && bm.GetObjectPlayer().Equals(this) && !isThinking)
        {
            isThinking = true;
            StartCoroutine("AIExchangeDelay");
        }
    }


    public void Damaged()
    {
        if (!isServer) return;
        if (currentHealth > 0)
        {
            currentHealth -= 1;
        }
    }

    public void Restored()
    {
        if (!isServer) return;
        if (currentHealth > 0 && currentHealth <= 5)
        {
            currentHealth += 1;
        }
        else if (currentHealth <= 6)
        {
            currentHealth = 6;
        }
    }

    public void Freezed()
    {
        if (!isServer) return;
        if (!isDead)
        {
            isFreezed = true;
            //Log(playerName + " is freezed.");
        }
    }

    public void Thawed()
    {
        if (!isServer) return;
        if (!isDead && isFreezed)
        {
            isFreezed = false;
        }
    }

    public void UpdateHealth()
    {
        if (!isServer) return;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
        }
        int HealthChange = displayedHealth - currentHealth;

        if (HealthChange < 0)
        {
            //bm.RpcPrintLog(playerName + " is Healed.");
            RpcHealed(); //힐을 받음
        }
        else if (HealthChange > 0 && isDead == false)
        {
            //bm.RpcPrintLog(playerName + " is Damaged.");
            RpcDamaged(); //데미지를 받음
        }
        else if (isDead == true)
        {
            //bm.RpcPrintLog(playerName + " is Dead.");
            RpcDead(); //뒤짐
        }
        displayedHealth = currentHealth;
    }

    [ClientCallback]
    public void PlayerToSelectTarget()
    {
        if (!isLocalPlayer) return;

        // 내 턴이 아니면 패스
        if (!bm.GetTurnPlayer().Equals(this)) return;
        Ray ray = GetComponentInChildren<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 8)))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.blue, 3f);
            if (hit.collider.gameObject.GetComponentInParent<PlayerControl>() != null
                && !hit.collider.gameObject.GetComponentInParent<PlayerControl>().Equals(this))
            {
                if (objectTarget == null) 
                {
                    objectTarget = hit.collider.gameObject.GetComponentInParent<PlayerControl>();
                    Instantiate(targetMark, hit.collider.gameObject.GetComponentInParent<PlayerControl>().transform);
                    isMarked = true;
                    //Log("Set " + hit.collider.gameObject.GetComponentInParent<PlayerControl>().GetName() + " to a target.");
                }
                else if (!objectTarget.Equals(hit.collider.gameObject.GetComponentInParent<PlayerControl>()))
                {
                    Destroy(GameObject.Find("TargetMark(Clone)"));
                    objectTarget = hit.collider.gameObject.GetComponentInParent<PlayerControl>();
                    Instantiate(targetMark, hit.collider.gameObject.GetComponentInParent<PlayerControl>().transform);
                    isMarked = true;
                    //Log("Set " + hit.collider.gameObject.GetComponentInParent<PlayerControl>().GetName() + " to a target.");

                }
            }
        }
    }

    // 임시 코드
    /*
    [ClientCallback]
    public void PlayerToSelectCard()
    {
        if (!isLocalPlayer) return;

        // 내가 교환에 참여한 플레이어가 아니면 패스
        if (!bm.GetTurnPlayer().Equals(this) && !bm.GetObjectPlayer().Equals(this))
        {
            return;
        }
        //Debug.Log("PlayerToSelectCard");
        List<Card> hand = bm.GetPlayerHand(this);
        Ray ray = GetComponentInChildren<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 9)))
        {
            //Log("Click " + hit.collider.name + ".");
            Debug.DrawLine(ray.origin, hit.point, Color.red, 3f);
            if (hit.collider.gameObject.GetComponentInParent<Card>() != null
                && (hit.collider.gameObject.GetComponentInParent<Card>().Equals(hand[0])
                || hit.collider.gameObject.GetComponentInParent<Card>().Equals(hand[1])))
            {
                /*
                Log("Set " + hit.collider.gameObject.GetComponentInParent<Card>().GetCardName() + " card to play.");
                DecideClicked();
                CmdSetCardToPlay(hit.collider.gameObject.GetComponentInParent<Card>().GetCardCode(), GetPlayerIndex());
            }
        }
    }
    */

    /// <summary>
    /// 선택한 대상에게 교환 요청을 거는 것을 확정짓는 함수입니다. turnStep이 2일 때만 작동합니다.
    /// </summary>
    [ClientCallback]
    public void DecideClicked()
    {
        if (bm.GetTurnStep() != 2)
        {
            return;
        }
        if (!isLocalPlayer) return;
        // 내 턴이 아니면 패스
        else if (!bm.GetTurnPlayer().Equals(this))
        {
            //Debug.Log("It isn't your turn! turn Player: " + (bm.GetTurnPlayer().GetPlayerNum()) + ", this Player: " + bm.GetCameraPlayer().GetPlayerNum());
            return;
        }
        else if (objectTarget == null)
        {
            //Debug.Log("turn Player: " + (bm.GetTurnPlayer().GetPlayerNum()) + ", this Player: " + bm.GetCameraPlayer().GetPlayerNum());
            Debug.LogWarning("Please select a player that you wants to exchange with.");
            return;
        }
        else
        {
            int i = bm.GetPlayers().IndexOf(objectTarget);
            CmdSetObjectPlayer(i);
            objectTarget = null;
        }
    }

    /// <summary>
    /// 교환할 카드가 결정되어 서버에 이를 적용할 때 호출되는 함수입니다.
    /// </summary>
    /// <param name="cardCode"></param>
    /// <param name="playerIndex"></param>
    [Command]
    public void CmdSetCardToPlay(int cardCode, int playerIndex)
    {
        bm.SetCardToPlay(cardCode, playerIndex);
    }

    /// <summary>
    /// 교환 대상이 결정되어 서버에 이를 적용할 때 호출하는 함수입니다. DecideClicked() 에서 호출됩니다.
    /// </summary>
    /// <param name="objectTargetIndex"></param>
    [Command]
    private void CmdSetObjectPlayer(int objectTargetIndex)
    {
        bm.SetObjectPlayer(objectTargetIndex);
    }

    [Command]
    public void CmdAfterExchange()
    {
        bm.AfterExchange();
    }

    [Command]
    public void CmdAfterFreezed()
    {
        bm.AfterFreezed();
    }

    [Command]
    public void CmdReady()
    {
        bm.PlayerReady(GetPlayerIndex());
    }

    public void SetName(string name)
    {
        playerName = name;
    }

    public string GetName()
    {
        return playerName;
    }

    public void SetPlayerNum(int num)
    {
        playerNum = num;
    }

    public int GetPlayerNum()
    {
        return playerNum;
    }

    public void SetAI(bool AI)
    {
        isAI = AI;
    }

    public void SetCardDragging(bool drag)
    {
        isCardDragging = drag;
    }

    public int GetHealth()
    {
        return displayedHealth;
    }

    public bool HasFreezed()
    {
        return isFreezed;
    }

    public bool HasDead()
    {
        return isDead;
    }

    public bool HasDecidedObjectPlayer()
    {
        return hasDecidedObjectPlayer;
    }

    public bool HasDecidedPlayCard()
    {
        return hasDecidedPlayCard;
    }

    public PlayerControl GetObjectTarget()
    {
        return objectTarget;
    }

    public int GetPlayerIndex()
    {
        return playerNum - 1;
    }

    public void SetHighlight(bool TF)
    {
        Border.SetActive(TF);
    }
    
    private void ObjectiveHighlight()
    {
        StartCoroutine("Highlight");
    }

    IEnumerator Highlight()
    {
        while (bm == null)
        {
            bm = BattleManager.bm;
            yield return null;
        }
        //Debug.Log("bm is set in Highlight.");
        List<int> t = null;
        while (t == null)
        {
            t = bm.GetTarget(GetPlayerIndex());
            yield return null;
        }
        bool b = true;  // bm.players가 모두 잘 채워져 있을 때까지 대기
        do
        {
            b = true;
            for (int i = 0; i < 5; i++)
            {
                if (bm.GetPlayers()[i] == null)
                {
                    b = false;
                    break;
                }
            }
            yield return null;
        } while (!b);
        //Log(bm.GetPlayers()[t[0]].GetName() + " is my objective.");
        bm.GetPlayers()[t[0]].SetHighlight(true);
        //Log(bm.GetPlayers()[t[1]].GetName() + " is my objective, too.");
        bm.GetPlayers()[t[1]].SetHighlight(true);
    }
    
    private void Log(string msg)
    {
        //Debug.Log(msg);
        //ConsoleLogUI.AddText(msg);
    }

    /*
    public void CAlert(int i)
    {
        if (!isLocalPlayer) return;
        if (i == 0 && Equals(bm.GetTurnPlayer())) alert.CreateAlert(i);
        else if (i == 1 && Equals(bm.GetObjectPlayer())) alert.CreateAlert(i);
        else if (i == 2 || i == 3) alert.CreateAlert(i);
    }
    */

    private void StatusUpdate()
    {
        int ts = bm.GetTurnStep();
        bool isTP = (Equals(bm.GetTurnPlayer()));
        bool isOP = (Equals(bm.GetObjectPlayer()));
        string s = "";

        if (ts == 0)
        {
            StatusUI.SetText("대전 시작");
        }
        else if (ts == 2 && isTP && objectTarget == null)
        {
            if (!isAlerted0)
            {
                Alert.alert.CreateAlert(0);
                isAlerted0 = true;
            }
            StatusUI.SetText("교환하고 싶은 상대의 캐릭터를 누르세요.");
        }
        else if (ts == 2 && isTP && objectTarget != null)
        {
            StatusUI.SetText("교환하고 싶은, 하단의 카드 하나를 위로 드래그해서 내세요.");
        }
        else if (ts == 2)
        {
            StatusUI.SetText(bm.GetTurnPlayer().GetName() + "의 턴");
            isAlerted1 = false;
        }
        else if (ts == 3 && isTP)
        {
            StatusUI.SetText("상대에게 교환 요청을 보냈습니다. 기다리세요.");
        }
        else if (ts == 3 && isOP)
        {
            if (!isAlerted1)
            {
                Alert.alert.CreateAlert(1);
                isAlerted1 = true;
            }
            StatusUI.SetText("교환 요청을 받았습니다. 교환하고 싶은, 하단의 카드 하나를 위로 드래그해서 내세요.");
        }
        else if (ts == 3)
        {
            StatusUI.SetText(bm.GetTurnPlayer().GetName() + "이(가) " + bm.GetObjectPlayer().GetName() + "에게 교환을 요청했습니다.");
        }
        else if (ts == 4 || ts == 9)
        {
            StatusUI.SetText("교환중...");
            isAlerted0 = false;
        }
        else if ((ts == 5 || ts == 11))
        {
            StatusUI.SetText(bm.GetTurnPlayer().GetName() + "이(가) 빙결되어 이번 턴에 교환할 수 없습니다.");
        }
        else if (ts == 8)
        {
            for (int j = 0; j < 5; j++)
            {
                if (bm.GetIsWin()[j])
                {
                    s += bm.GetPlayers()[j].GetName() + " ";
                }
            }
            StatusUI.SetText("대전 종료!\n" + s + "승리!");
            if (!isAlerted3 && bm.GetIsWin()[GetPlayerIndex()])
            {
                Alert.alert.CreateAlert(3);
                isAlerted3 = true;
            }
            else if (!isAlerted3 && !bm.GetIsWin()[GetPlayerIndex()])
            {
                Alert.alert.CreateAlert(4);
                isAlerted3 = true;
            }
        }
        else if (ts == 12)
        {
            if (!isAlerted2)
            {
                Alert.alert.CreateAlert(2);
                isAlerted2 = true;
            }
            StatusUI.SetText("누군가가 게임을 나갔습니다. 대전을 진행할 수 없으므로 종료합니다.");
        }
        else
        {
            StatusUI.ClearText();
        }
    }

    [ClientRpc]
    public void RpcSetAI(bool AI)
    {
        isAI = AI;
        GetComponent<NetworkIdentity>().localPlayerAuthority = false;
    }

    [ClientRpc]
    public void RpcFreeze()
    {
        StartCoroutine("FreezeAnimation");
    }

    [ClientRpc]
    public void RpcHealed()
    {
        StartCoroutine("HealedAnimation");
    }

    [ClientRpc]
    public void RpcDamaged()
    {
        StartCoroutine("DamagedAnimation");
    }

    [ClientRpc]
    public void RpcDead()
    {
        StartCoroutine("DeadAnimation");
    }

    public void Freeze()
    {
        StartCoroutine("FreezeAnimation");
    }

    IEnumerator HealedAnimation()
    {
        //Log("HealedAnimation");
        Face.sprite = Resources.Load("캐릭터/치유받은_캐릭터", typeof(Sprite)) as Sprite;
        Quaternion Original = Face.transform.localRotation;

        float t = Time.time;
        /*
        while (Time.time - t < (20f / 60f))
        {
            Face.transform.localRotation = Quaternion.Lerp(Original, Quaternion.Euler(0f, 181f, 0f), (Time.time - t) / (20f / 60f));
            yield return null;
        }

        t = Time.time;
        while (Time.time - t < (20f / 60f))
        {
            Face.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(0f, 181f, 0f), Original, (Time.time - t) / (20f / 60f));
            yield return null;
        }
        */

        Face.transform.localRotation = Original;
        yield return null; // new WaitForSeconds(40f / 60f);
        Face.sprite = Resources.Load("캐릭터/디폴트_캐릭터", typeof(Sprite)) as Sprite;
    }

    IEnumerator DamagedAnimation()
    {
        //Log("DamagedAnimation");
        Face.sprite = Resources.Load("캐릭터/데미지받은_캐릭터", typeof(Sprite)) as Sprite;
        Vector3 Original = Face.transform.localPosition;
        /*
        for (int i = 0; i < 5; i++)
        {
            Face.transform.localPosition = Original + new Vector3(0.2f, 0f, 0f);
            yield return null; // new WaitForSeconds(5f / 60f);
            Face.transform.localPosition = Original + new Vector3(-0.2f, 0f, 0f);
            yield return null; // new WaitForSeconds(5f / 60f);
        }
        */
        Face.transform.localPosition = Original;
        yield return null; // new WaitForSeconds(40f / 60f);
        Face.sprite = Resources.Load("캐릭터/디폴트_캐릭터", typeof(Sprite)) as Sprite;
    }

    IEnumerator DeadAnimation()
    {
        //Log("DeadAnimation");
        Face.sprite = Resources.Load("캐릭터/죽은_캐릭터", typeof(Sprite)) as Sprite;
        yield return null; // new WaitForSeconds(30f / 30f);
        //폭발애니메이...
        
        
    }

    //대전화면에서 빙결이 일어나는 애니메이션
    IEnumerator FreezeAnimation()
    {
        //Log("FreezeAnimation");
        Ice.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        Ice.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.75f);
        Ice.GetComponent<SpriteRenderer>().sprite = Resources.Load("이펙트/대전화면_빙결/얼음0", typeof(Sprite)) as Sprite;
        yield return null; // new WaitForSeconds(4f / 3f);
        Ice.GetComponent<SpriteRenderer>().sprite = Resources.Load("이펙트/대전화면_빙결/얼음2", typeof(Sprite)) as Sprite;
        yield return null; // new WaitForSeconds(4f / 3f);
        Ice.GetComponent<SpriteRenderer>().sprite = Resources.Load("이펙트/대전화면_빙결/얼음4", typeof(Sprite)) as Sprite;
        yield return null; // new WaitForSeconds(4f / 3f);
        float t = Time.time;
        /*
        while (Time.time - t < (90f / 60f))
        {
            Ice.GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(1f, 1f, 1f, 0.75f), new Color(1f, 1f, 1f, 0f), (Time.time - t) / (90f / 60f));
            yield return null;
        }
        */
        Ice.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
        //Log("In client, " + GetName() + " isAI? " + isAI + ", then why not thawed?");
        //if (isServer) bm.RpcPrintLog("In server, " + GetName() + " isAI? " + isAI + ", then why not thawed?");
        if (isAI && isServer) bm.AfterFreezed();
        else if (isLocalPlayer) CmdAfterFreezed();
    }

    //조작화면에서 빙결이 일어나는 애니메이션
    IEnumerator IceAnimation()
    {
        yield return null;
    }

    IEnumerator AITurnDelay()
    {
        yield return null; // new WaitForSeconds(Random.Range(1.5f, 3f));
        AIThinking(null);   // 여기서 objectTarget과 playCardAI를 설정함.
        int i = bm.GetPlayers().IndexOf(objectTarget);
        bm.SetObjectPlayer(i);
        objectTarget = null;
        bm.SetCardToPlay(playCardAI.GetCardCode(), GetPlayerIndex());
        playCardAI = null;
        yield return null;
        isThinking = false;
    }

    IEnumerator AIExchangeDelay()
    {
        yield return null; // new WaitForSeconds(Random.Range(1.5f, 3f)); /* AUTO 시 주석처리 */
        AIThinking(bm.GetTurnPlayer());
        bm.SetCardToPlay(playCardAI.GetCardCode(), GetPlayerIndex());
        playCardAI = null;
        yield return null;
        isThinking = false;
    }

    /// <summary>
    /// 인공지능이 생각하여 교환할 대상과 교환할 카드를 결정하게 하는 함수입니다. 인자로 null이 아닌 값을 주면 교환할 카드만 정합니다.
    /// </summary>
    /// <param name="opponent">교환을 요청해온 상대</param>
    private void AIThinking(PlayerControl opponent)
    {
        List<int> playerClass = AIObjectRelation();

        /* TODO 임시 코드 */
        string m = "";
        for (int i = 0; i < 5; i++)
        {
            m += playerClass[i] + " ";
        }
        bm.RpcPrintLog(m);
        

        List<string> hand = AIHandEstimation();
        // TODO 손패 상황 목록을 받으면 특정 상대와 교환할 때 어떤 카드를 받게 될지 추정하기
        // TODO 특정 상대에게 특정 카드를 줄 때의 결과를 생각하여 행동 점수를 매기고, 점수에 해당하는 수만큼 상자에 제비뽑기를 넣어 랜덤으로 하나 뽑기
        List<Card> myHand = bm.GetPlayerHand(this);
        List<string> decisionBox = new List<string>(); // 이 목록에 제비뽑기를 넣고 나중에 하나 뽑아 나온 행동을 한다.
        if (opponent == null) {
            for (int i = 0; i < 5; i++)
            {
                if (i == GetPlayerIndex()) continue;
                string opponentCard = AIOpponentPlay(playerClass[i], hand[2 * i], hand[2 * i + 1], bm.GetPlayers()[i].GetHealth());
                bm.RpcPrintLog("opponentCard is " + opponentCard + ".");
                AIScoreBehavior(myHand[0].GetCardName(), opponentCard, hand, i, playerClass[i], decisionBox);
                AIScoreBehavior(myHand[1].GetCardName(), opponentCard, hand, i, playerClass[i], decisionBox);
            }
            // TODO 랜덤 말고 인공지능으로 고치기
            /*
            do
            {
                objectTarget = bm.GetPlayers()[Random.Range(0, 5)];
            } while (objectTarget == null || objectTarget.Equals(this));
            */
        }
        else
        {
            int i = opponent.GetPlayerIndex();
            string opponentCard = AIOpponentPlay(playerClass[i], hand[2 * i], hand[2 * i + 1], bm.GetPlayers()[i].GetHealth());
            bm.RpcPrintLog("opponentCard is " + opponentCard + ".");
            AIScoreBehavior(myHand[0].GetCardName(), opponentCard, hand, i, playerClass[i], decisionBox);
            AIScoreBehavior(myHand[1].GetCardName(), opponentCard, hand, i, playerClass[i], decisionBox);
        }
        // TODO 랜덤 말고 인공지능으로 고치기
        string lottery = decisionBox[Random.Range(0, decisionBox.Count)];
        bm.RpcPrintLog("lottery is " + lottery + ".");
        if (opponent == null)
        {
            objectTarget = bm.GetPlayers()[int.Parse(lottery[0].ToString())];
        }
        lottery = lottery.Substring(1);
        if (myHand[0].GetCardName() == lottery) playCardAI = myHand[0];
        else if (myHand[1].GetCardName() == lottery) playCardAI = myHand[1];
        else Debug.LogError("lottery is invalid.");
        // playCardAI = myHand[Random.Range(0, 2)];
    }

    /// <summary>
    /// 인공지능이 자신을 목표로 하는 플레이어들을 추정하게 하는 함수입니다. 플레이어들을 분류한 정보를 목록으로 반환합니다.
    /// </summary>
    private List<int> AIObjectRelation()
    {
        List<int> enemyPoint = new List<int>(); // 인덱스는 플레이어 인덱스이고, 그 값이 높을수록 그 플레이어가 자신의 천적일 가능성이 높다.
        for (int i = 0; i < 5; i++)
        {
            enemyPoint.Add(4);
        }
        enemyPoint[GetPlayerIndex()] = -1;  // 자신은 자신의 천적이 아니다.

        foreach(Exchange ex in bm.GetExchanges())
        {
            if (ex.GetIsFreezed()) continue;

            // 자신의 턴에 한 교환들 중에서
            if (ex.GetTurnPlayer().Equals(this))
            {
                if (ex.GetObjectPlayerCard().GetCardName() == "Attack")     // 상대가 공격 카드를 냈다면
                {
                    enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] += 1;
                }
                else if (ex.GetObjectPlayerCard().GetCardName() == "Bomb")  // 상대가 폭탄 카드를 냈다면
                {
                    enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] += 1;
                }
                else if (ex.GetObjectPlayerCard().GetCardName() == "Heal" && ex.GetTurnPlayerHealth() != maxHealth) // 상대의 치유 카드로 체력이 회복된 경우
                {
                    enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] -= 1;
                    if (enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] < 0) enemyPoint[ex.GetObjectPlayer().GetPlayerIndex()] = 0;
                }
            }
            // 상대가 자신에게 걸어온 교환 중에서
            else if (ex.GetObjectPlayer().Equals(this))
            {
                if (ex.GetTurnPlayerCard().GetCardName() == "Attack")
                {
                    enemyPoint[ex.GetTurnPlayer().GetPlayerIndex()] += 2;
                }
                else if (ex.GetTurnPlayerCard().GetCardName() == "Bomb")
                {
                    enemyPoint[ex.GetTurnPlayer().GetPlayerIndex()] += 1;
                }
                else if (ex.GetTurnPlayerCard().GetCardName() == "Heal" && ex.GetObjectPlayerHealth() != maxHealth)
                {
                    enemyPoint[ex.GetTurnPlayer().GetPlayerIndex()] -= 2;
                    if (enemyPoint[ex.GetTurnPlayer().GetPlayerIndex()] < 0) enemyPoint[ex.GetTurnPlayer().GetPlayerIndex()] = 0;
                }
            }
        }

        int max = -1;
        for (int i = 0; i < 5; i++)
        {
            if (enemyPoint[i] > max) max = enemyPoint[i];
        }

        List<int> isEnemy = new List<int>();    // 천적일 것으로 생각되는 플레이어들의 인덱스 목록입니다. 크기는 1 이상 4 이하입니다.
        for (int i = 0; i < 5; i++)
        {
            if (enemyPoint[i] == max)
            {
                isEnemy.Add(i);     // 가장 높은 enemyPoint를 받은 플레이어들의 인덱스를 기억
                enemyPoint[i] = -1;
            }
        }

        if (isEnemy.Count < 2)
        {
            int maxNum = 0;         // 공동 2등의 enemyPoint를 받은 플레이어 수
            max = -1;
            for (int i = 0; i < 5; i++)
            {
                if (enemyPoint[i] > max)
                {
                    max = enemyPoint[i];
                    maxNum = 1;
                }
                else if (enemyPoint[i] == max)
                {
                    maxNum++;
                }
            }

            if (max != -1 && maxNum <= 2)   // 2등이 있고 공동 2등이 3명이 아닐 때
            {
                for (int i = 0; i < 5; i++)
                {
                    if (enemyPoint[i] == max)
                    {
                        isEnemy.Add(i);     // 두 번째로 높은 enemyPoint를 받은 플레이어들의 인덱스를 기억
                    }
                }
            }
        }
        bm.GetTarget(GetPlayerIndex());
        List<int> playerClass = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            if (i == GetPlayerIndex())
                playerClass.Add(-1);                                            // 자기 자신
            else
            {
                int c = 0;
                if (isEnemy.IndexOf(i) != -1) c += 2;                           // 천적
                if (bm.GetTarget(GetPlayerIndex()).IndexOf(i) != -1) c += 1;    // 자신의 목표
                playerClass.Add(c);
            }
        }

        /* 
         * playerClass[i]의 값에 따라서 i번째 인덱스의 플레이어를 다음과 같이 분류한다.
         * -1: 자기 자신, 
         * 0: 자신과 아무 관계도 아닌 상대, 
         * 1: 자신의 목표이지만 천적이 아닌 상대,
         * 2: 천적이지만 자신의 목표가 아닌 상대,
         * 3: 자신의 목표이면서 천적인 상대
         */

        return playerClass;
    }

    /// <summary>
    /// 인공지능이 각 플레이어가 어떤 카드를 손패에 들고 있는지 추정하게 하는 함수입니다.
    /// </summary>
    private List<string> AIHandEstimation()
    {
        // TODO 각 플레이어가 했던 마지막 교환의 결과를 바탕으로 현재 손패의 카드 분배 상황을 추정해서 목록으로 반환하기
        List<Card> myHand = bm.GetPlayerHand(this);
        List<Exchange> exc = bm.GetExchanges();
        List<string> handName = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            handName.Add("?");
        }

        for (int i = 0; i < 5; i++)
        {
            // 자기 자신이면 무슨 카드를 들고 있는지 이미 알고 있다.
            if (i == GetPlayerIndex())
            {
                handName[2 * i] = myHand[0].GetCardName();
                handName[2 * i + 1] = myHand[1].GetCardName();
                continue;
            }
            // i번째 인덱스를 갖는 플레이어가 참여한 마지막 교환(빙결된 턴 제외)을 찾는다.
            for (int j = exc.Count - 1; j >= 0; j--)
            {
                // 빙결된 턴은 마지막 교환으로 취급하지 않는다. 빙결 효과가 나타났다고 해서 그때까지 빙결 카드를 들고 있다고는 보장할 수 없다.
                if (exc[j].GetIsFreezed())
                {
                    // 빙결된 턴에 피해를 받으면 그 플레이어는 폭탄을 들고 있는 것이 확실하다.
                    if (exc[j].GetTurnPlayer().GetPlayerIndex() == i && exc[j].GetTurnPlayerHealthVariation() == -1)
                    {
                        handName[2 * i] = "Bomb";
                    }
                    continue;
                }
                else if (exc[j].GetTurnPlayer().GetPlayerIndex() == i)
                {
                    // 마지막 교환에서 체력이 회복된 경우 치유 카드를 받은 것이 확실하다.
                    if (exc[j].GetTurnPlayerHealthVariation() == 1)
                    {
                        if (handName[2 * i] == "?")
                        {
                            handName[2 * i] = "Heal";
                            handName[2 * i + 1] = "NoBomb";
                        }
                        else if (handName[2 * i] == "Bomb")
                        {
                            // 폭탄을 들고 있을 수 없다.
                            Log("How do you have Bomb card?");
                            handName[2 * i] = "Heal";
                            handName[2 * i + 1] = "NoBomb";
                        }
                        else
                        {
                            handName[2 * i + 1] = "Heal";
                            break;
                        }
                    }
                    // 마지막 교환에서 체력이 2 깎인 경우 폭탄 카드를 들고 있었고 공격 카드를 새로 받은 것이 확실하다.
                    else if (exc[j].GetTurnPlayerHealthVariation() == -2)
                    {
                        if (handName[2 * i] == "?" || handName[2 * i] == "Bomb" || handName[2 * i] == "Attack")
                        {
                            handName[2 * i] = "Attack";
                            handName[2 * i + 1] = "Bomb";
                            break;
                        }
                        else
                        {
                            Log("How do you have " + handName[2 * i] + " card?");
                            handName[2 * i] = "Attack";
                            handName[2 * i + 1] = "Bomb";
                            break;
                        }
                    }

                    // 마지막 교환이 자신과의 교환이었다면 자신이 준 카드를 들고 있을 것이다.
                    if (exc[j].GetObjectPlayer().Equals(this))
                    {
                        if (handName[2 * i] == "?") handName[2 * i] = exc[j].GetObjectPlayerCard().GetCardName();
                        else if (handName[2 * i] != exc[j].GetObjectPlayerCard().GetCardName())
                            handName[2 * i + 1] = exc[j].GetObjectPlayerCard().GetCardName();
                    }
                    break;
                }
                else if (exc[j].GetObjectPlayer().GetPlayerIndex() == i)
                {
                    // 마지막 교환에서 체력이 회복된 경우
                    if (exc[j].GetObjectPlayerHealthVariation() == 1)
                    {
                        if (handName[2 * i] == "?") handName[2 * i] = "Heal";
                        else
                        {
                            handName[2 * i + 1] = "Heal";
                            break;
                        }
                    }
                    // 마지막 교환에서 체력이 1 깎인 경우
                    else if (exc[j].GetObjectPlayerHealthVariation() == -1)
                    {
                        if (handName[2 * i] == "?") handName[2 * i] = "Attack";
                        else
                        {
                            handName[2 * i + 1] = "Attack";
                            break;
                        }
                    }

                    if (exc[j].GetTurnPlayer().Equals(this))
                    {
                        if (handName[2 * i] == "?") handName[2 * i] = exc[j].GetTurnPlayerCard().GetCardName();
                        else if (handName[2 * i] != exc[j].GetTurnPlayerCard().GetCardName())
                            handName[2 * i + 1] = exc[j].GetTurnPlayerCard().GetCardName();
                    }
                    break;
                }
            }
        }

        /* TODO 임시 코드 */
        string m = GetName() + " thinks:";
        for (int i = 0; i < 10; i++)
        {
            m += " " + ((i/2)+1) + handName[i];
        }
        Log(m);
        if (isServer) bm.RpcPrintLog(m);

        int r;
        if (handName.IndexOf("Bomb") == -1 && handName.IndexOf("?") != -1)
        {
            do
            {
                r = Random.Range(0, 10);
            } while (handName[r] != "?");
            handName[r] = "Bomb";
        }
        if ((r = handName.IndexOf("NoBomb")) != -1) handName[r] = "?";
        if (handName.IndexOf("Deceive") == -1 && handName.IndexOf("?") != -1)
        {
            do
            {
                r = Random.Range(0, 10);
            } while (handName[r] != "?" && handName[r] != "NoBomb");
            handName[r] = "Deceive";
        }
        if (handName.IndexOf("Freeze") == -1 && handName.IndexOf("?") != -1)
        {
            do
            {
                r = Random.Range(0, 10);
            } while (handName[r] != "?" && handName[r] != "NoBomb");
            handName[r] = "Freeze";
        }
        if (handName.IndexOf("Avoid") == -1 && handName.IndexOf("?") != -1)
        {
            do
            {
                r = Random.Range(0, 10);
            } while (handName[r] != "?" && handName[r] != "NoBomb");
            handName[r] = "Avoid";
        }
        if (handName.IndexOf("Heal") == -1 && handName.IndexOf("?") != -1)
        {
            do
            {
                r = Random.Range(0, 10);
            } while (handName[r] != "?" && handName[r] != "NoBomb");
            handName[r] = "Heal";
        }
        if (handName.IndexOf("Heal") == handName.LastIndexOf("Heal") && handName.IndexOf("?") != -1)
        {
            do
            {
                r = Random.Range(0, 10);
            } while (handName[r] != "?" && handName[r] != "NoBomb");
            handName[r] = "Heal";
        }
        for (int i = 0; i < 10; i++)
        {
            if (handName[i] == "?" || handName[i] == "NoBomb") handName[i] = "Attack";
        }

        /* TODO 임시 코드 */
        string m2 = GetName() + " estimates:";
        for (int i = 0; i < 10; i++)
        {
            m2 += " " + ((i / 2) + 1) + handName[i];
        }
        Log(m2);

        return handName;
    }

    /// <summary>
    /// 인공지능이 상대가 자신에게 어떤 카드를 내려고 할지 예측하게 하는 함수입니다.
    /// </summary>
    /// <param name="playerClass">자신이 바라보는, 자신에 대한 상대의 목표 관계</param>
    /// <param name="card1">상대 손패에 있을 것 같은 카드</param>
    /// <param name="card2">상대 손패에 있을 것 같은 카드</param>
    /// <param name="opponentHealth">상대의 교환 전 남은 체력</param>
    /// <returns>상대가 낼 것으로 생각되는 카드 이름</returns>
    private string AIOpponentPlay(int playerClass, string card1, string card2, int opponentHealth)
    {
        CardDatabase cd = GameObject.Find("BattleManager").GetComponent<CardDatabase>();
        if (cd == null)
        {
            Debug.Log("cd is null in AIopponentPlay.");
            return "Error!";
        }
        else if (playerClass < 0 || playerClass > 3)
        {
            Debug.Log("playerClass out of range [0, 3].");
            return "Error!";
        }
        else if (!cd.VerifyCard(card1) || !cd.VerifyCard(card2))
        {
            Debug.Log("card1("+ card1 + ") or card2(" + card2 + ") is invalid.");
            return "Error!";
        }

        // 상대가 들고 있는 두 카드가 같은 경우 그것밖에 낼 수 없다.
        if (card1 == card2 && (card1 == "Attack" || card1 == "Heal"))
        {
            return card1;
        }
        else if (card1 == card2)
        {
            Debug.Log("How do you have two " + card1 + " cards?");
            return "Error!";
        }

        // 공격, 치유, 폭탄, 회피, 속임, 빙결 순으로 정렬 (하드코딩 주의!)
        if (card2 == "Attack")
        {
            string temp = card1;
            card1 = card2;
            card2 = temp;
        }
        if (card1 != "Attack")
        {
            if (card2 == "Heal")
            {
                string temp = card1;
                card1 = card2;
                card2 = temp;
            }
            if (card1 != "Heal")
            {
                if (card2 == "Bomb")
                {
                    string temp = card1;
                    card1 = card2;
                    card2 = temp;
                }
                if (card1 != "Bomb")
                {
                    if (card2 == "Avoid")
                    {
                        string temp = card1;
                        card1 = card2;
                        card2 = temp;
                    }
                    if (card1 != "Avoid")
                    {
                        if (card2 == "Deceive")
                        {
                            string temp = card1;
                            card1 = card2;
                            card2 = temp;
                        }
                    }
                }
            }
        }

        card1 += card2; // 편의상 이름을 합치기로

        switch (playerClass)
        {
            case 0: // 서로 아무 관계도 아니라고 생각
                switch (card1)
                {
                    case "AttackHeal":
                        return "Heal";
                    case "AttackBomb":
                        return "Bomb";
                    case "AttackAvoid":
                        return "Avoid";
                    case "AttackDeceive":
                        return "Deceive";
                    case "AttackFreeze":
                        return "Freeze";
                    case "HealBomb":
                        if (!bm.GetTurnPlayer().Equals(this) && opponentHealth <= 2)
                        {
                            return "Bomb";
                        }
                        else return "Heal";
                    case "HealAvoid":
                    case "HealDeceive":
                    case "HealFreeze":
                        return "Heal";
                    case "BombAvoid":
                        if (bm.GetTurnPlayer().Equals(this) && GetHealth() <= 2)
                        {
                            return "Avoid";
                        }
                        else return "Bomb";
                    case "BombDeceive":
                    case "BombFreeze":
                        return "Bomb";
                    case "AvoidDeceive":
                        if (Random.Range(0, 1) == 0) return "Deceive";
                        else return "Avoid";
                    case "AvoidFreeze":
                        return "Avoid";
                    case "DeceiveFreeze":
                        return "Freeze";
                }
                break;
            case 1: // 상대는 나를 천적으로 생각
                switch (card1)
                {
                    case "AttackHeal":
                        return "Heal";
                    case "AttackBomb":
                        return "Bomb";
                    case "AttackAvoid":
                        return "Avoid";
                    case "AttackDeceive":
                        return "Deceive";
                    case "AttackFreeze":
                        return "Freeze";
                    case "HealBomb":
                        if (!bm.GetTurnPlayer().Equals(this) && opponentHealth <= 2)
                        {
                            return "Bomb";
                        }
                        else return "Heal";
                    case "HealAvoid":
                        return "Avoid";
                    case "HealDeceive":
                        return "Heal";
                    case "HealFreeze":
                        if (GetHealth() <= 2) return "Heal";
                        else return "Freeze";
                    case "BombAvoid":
                        return "Avoid";
                    case "BombDeceive":
                        return "Deceive";
                    case "BombFreeze":
                        return "Freeze";
                    case "AvoidDeceive":
                        return "Avoid";
                    case "AvoidFreeze":
                        if (opponentHealth <= 2) return "Avoid";
                        else return "Freeze";
                    case "DeceiveFreeze":
                        return "Freeze";
                }
                break;
            case 2: // 상대는 나를 목표로 생각
                switch (card1)
                {
                    case "AttackHeal":
                        return "Attack";
                    case "AttackBomb":
                        return "Bomb";
                    case "AttackAvoid":
                    case "AttackDeceive":
                    case "AttackFreeze":
                        return "Attack";
                    case "HealBomb":
                        return "Bomb";
                    case "HealAvoid":
                        return "Avoid";
                    case "HealDeceive":
                        return "Deceive";
                    case "HealFreeze":
                        return "Freeze";
                    case "BombAvoid":
                    case "BombDeceive":
                    case "BombFreeze":
                        return "Bomb";
                    case "AvoidDeceive":
                        if (Random.Range(0, 1) == 0) return "Avoid";
                        else return "Deceive";
                    case "AvoidFreeze":
                        return "Freeze";
                    case "DeceiveFreeze":
                        if (Random.Range(0, 1) == 0) return "Freeze";
                        else return "Deceive";
                }
                break;
            case 3: // 상대는 나를 목표이자 천적으로 생각
                switch (card1)
                {
                    case "AttackHeal":
                        return "Attack";
                    case "AttackBomb":
                        return "Bomb";
                    case "AttackAvoid":
                        if (opponentHealth <= 2) return "Avoid";
                        else return "Attack";
                    case "AttackDeceive":
                    case "AttackFreeze":
                        return "Attack";
                    case "HealBomb":
                        return "Bomb";
                    case "HealAvoid":
                        return "Avoid";
                    case "HealDeceive":
                        return "Deceive";
                    case "HealFreeze":
                        return "Freeze";
                    case "BombAvoid":
                    case "BombDeceive":
                    case "BombFreeze":
                        return "Bomb";
                    case "AvoidDeceive":
                        if (Random.Range(0, 1) == 0) return "Avoid";
                        else return "Deceive";
                    case "AvoidFreeze":
                        if (opponentHealth <= 2) return "Avoid";
                        else return "Freeze";
                    case "DeceiveFreeze":
                        if (Random.Range(0, 1) == 0) return "Freeze";
                        else return "Deceive";
                }
                break;
        }
        // 위의 경우에 해당되지 않는 경우가 존재할지 모르겠지만
        return "Error!";
    }

    /// <summary>
    /// 인공지능이 자신의 myCard를 상대에게 내는 행동에 대한 점수를 매기고, 그 점수만큼 낼 카드를 box에 넣는 함수입니다.
    /// 행동에 대한 유불리 점수는 내장된 점수표를 따릅니다.
    /// </summary>
    /// <param name="myCard">자신이 낼 카드</param>
    /// <param name="opponentCard">교환할 상대가 낼 것으로 예측한 카드</param>
    /// <param name="hand">추정한 손패 목록 전체</param>
    /// <param name="opponentPlayerIndex">교환할 상대의 인덱스</param>
    /// <param name="playerClass">교환할 상대와의 목표 관계</param>
    /// <param name="box">뽑기 상자</param>
    private void AIScoreBehavior(string myCard, string opponentCard, List<string> hand, int opponentPlayerIndex, int playerClass, List<string> box)
    {
        CardDatabase cd = GameObject.Find("BattleManager").GetComponent<CardDatabase>();
        if (cd == null)
        {
            Debug.Log("cd is null in AIScoreBehavior.");
            return;
        }
        else if (!cd.VerifyCard(myCard) || !cd.VerifyCard(opponentCard))
        {
            Debug.Log("myCard or opponentCard is invalid.");
            return;
        }
        else if (opponentPlayerIndex < 0 || opponentPlayerIndex >= 5)
        {
            Debug.Log("opponentPlayerIndex out of range [0, 4].");
            return;
        }
        else if (hand.Count != 10)
        {
            Debug.Log("The number of cards in hand is not equal to 10.");
            return;
        }
        else if (hand[GetPlayerIndex() * 2] != myCard && hand[GetPlayerIndex() * 2 + 1] != myCard)
        {
            Debug.Log("You don't have " + myCard + " card!");
            return;
        }
        else if (hand[opponentPlayerIndex * 2] != opponentCard && hand[opponentPlayerIndex * 2 + 1] != opponentCard)
        {
            Debug.Log("You don't think your opponent has " + opponentCard + " card!");
            return;
        }
        else if (playerClass < 0 || playerClass > 3)
        {
            Debug.Log("playerClass out of range [0, 3].");
            return;
        }

        int score = 0;
        string voidCard = myCard;   // 만약 상대가 속임을 쓴다고 예측했다면, 내가 원래 내려던 카드를 기억한다.
        int opponentHealth = bm.GetPlayers()[opponentPlayerIndex].GetHealth();

        // 내가 속임을 낼 경우의 점수는 상대가 내려고 하지 않았던 카드를 낼 때의 기준으로 계산된다.
        if (myCard == "Deceive")
        {
            if (hand[opponentPlayerIndex * 2] == opponentCard)
                opponentCard = hand[opponentPlayerIndex * 2 + 1];
            else opponentCard = hand[opponentPlayerIndex * 2];
        }
        // 상대가 속임을 낼 경우의 점수는 내가 내려고 하지 않았던 카드를 낼 때의 기준으로 계산된다.
        else if (opponentCard == "Deceive")
        {
            if (hand[GetPlayerIndex() * 2] == myCard)
                myCard = hand[GetPlayerIndex() * 2 + 1];
            else myCard = hand[GetPlayerIndex() * 2];
        }
        switch (playerClass)
        {
            case 0:
                switch (myCard)
                {
                    case "Attack":
                        switch (opponentCard)
                        {
                            case "Attack": score = 1; break;
                            case "Heal": score = 3; break;
                            case "Bomb": score = 1; break;
                            case "Deceive": score = 2; break;
                            case "Avoid": score = 3; break;
                            case "Freeze": score = 2; break;
                        }
                        break;
                    case "Heal":
                        switch (opponentCard)
                        {
                            case "Attack": score = 10; break;
                            case "Heal": score = 18; break;
                            case "Bomb": score = 10; break;
                            case "Deceive": score = 13; break;
                            case "Avoid": score = 9; break;
                            case "Freeze": score = 13; break;
                        }
                        if (opponentHealth <= 2) score += 6;
                        break;
                    case "Bomb":
                        switch (opponentCard)
                        {
                            case "Attack": score = 4; break;
                            case "Heal": score = 8; break;
                            case "Deceive": score = 5; break;
                            case "Avoid": score = 5; break;
                            case "Freeze": score = 5; break;
                        }
                        if (GetHealth() <= 2) score += 12;
                        if (bm.GetTurnPlayer().Equals(this)) score += 2;
                        if (!bm.GetTurnPlayer().Equals(this) && opponentHealth <= 2) score /= 2;    // 상대가 죽으면 안됨
                        break;
                    case "Deceive":
                        switch (opponentCard)
                        {
                            case "Attack": score = 4; break;
                            case "Heal": score = 8; break;
                            case "Bomb": score = 4; break;
                            case "Avoid": score = 5; break;
                            case "Freeze": score = 5; break;
                        }
                        if (GetHealth() <= 2) score /= 2;   // 아군을 속이는 것은 불리한 행동
                        break;
                    case "Avoid":
                        switch (opponentCard)
                        {
                            case "Attack": score = 10; break;
                            case "Heal": score = 18; break;
                            case "Bomb": score = 10; break;
                            case "Deceive": score = 13; break;
                            case "Freeze": score = 13; break;
                        }
                        if (opponentHealth <= 2) score += 4;
                        break;
                    case "Freeze":
                        switch (opponentCard)
                        {
                            case "Attack": score = 4; break;
                            case "Heal": score = 8; break;
                            case "Bomb": score = 4; break;
                            case "Deceive": score = 5; break;
                            case "Avoid": score = 5; break;
                        }
                        break;
                }
                break;
            case 1:
                switch (myCard)
                {
                    case "Attack":
                        switch (opponentCard)
                        {
                            case "Attack": score = 16; break;
                            case "Heal": score = 20; break;
                            case "Bomb": score = 16; break;
                            case "Deceive": score = 17; break;
                            case "Avoid": score = 16; break;
                            case "Freeze": score = 17; break;
                        }
                        if (opponentHealth <= 2) score += 7;
                        break;
                    case "Heal":
                        switch (opponentCard)
                        {
                            case "Attack": score = 1; break;
                            case "Heal": score = 3; break;
                            case "Bomb": score = 1; break;
                            case "Deceive": score = 2; break;
                            case "Avoid": score = 3; break;
                            case "Freeze": score = 2; break;
                        }
                        break;
                    case "Bomb":
                        switch (opponentCard)
                        {
                            case "Attack": score = 25; break;
                            case "Heal": score = 29; break;
                            case "Deceive": score = 26; break;
                            case "Avoid": score = 26; break;
                            case "Freeze": score = 26; break;
                        }
                        if (GetHealth() <= 2) score += 10;
                        if (bm.GetTurnPlayer().Equals(this)) score += 5;
                        if (GetHealth() <= 2 && bm.GetTurnPlayer().Equals(this)) score += 5;    // 유리한 교환
                        if (opponentHealth <= 2 && !bm.GetTurnPlayer().Equals(this)) score += 5;
                        break;
                    case "Deceive":
                        switch (opponentCard)
                        {
                            case "Attack": score = 4; break;
                            case "Heal": score = 8; break;
                            case "Bomb": score = 4; break;
                            case "Avoid": score = 5; break;
                            case "Freeze": score = 5; break;
                        }
                        if (GetHealth() <= 2) score /= 2;   // 아군을 속이는 것은 불리한 행동
                        break;
                    case "Avoid":
                        switch (opponentCard)
                        {
                            case "Attack": score = 8; break;
                            case "Heal": score = 8; break;
                            case "Bomb": score = 7; break;
                            case "Deceive": score = 8; break;
                            case "Freeze": score = 8; break;
                        }
                        break;
                    case "Freeze":
                        switch (opponentCard)
                        {
                            case "Attack": score = 9; break;
                            case "Heal": score = 13; break;
                            case "Bomb": score = 9; break;
                            case "Deceive": score = 10; break;
                            case "Avoid": score = 10; break;
                        }
                        break;
                }
                break;
            case 2:
                switch (myCard)
                {
                    case "Attack":
                        switch (opponentCard)
                        {
                            case "Attack": score = 1; break;
                            case "Heal": score = 3; break;
                            case "Bomb": score = 1; break;
                            case "Deceive": score = 3; break;
                            case "Avoid": score = 3; break;
                            case "Freeze": score = 2; break;
                        }
                        break;
                    case "Heal":
                        switch (opponentCard)
                        {
                            case "Attack": score = 9; break;
                            case "Heal": score = 13; break;
                            case "Bomb": score = 9; break;
                            case "Deceive": score = 13; break;
                            case "Avoid": score = 9; break;
                            case "Freeze": score = 10; break;
                        }
                        if (GetHealth() <= 2) score -= 5;   // 천적 피하기
                        if (opponentHealth <= 2) score += 6;
                        break;
                    case "Bomb":
                        switch (opponentCard)
                        {
                            case "Attack": score = 4; break;
                            case "Heal": score = 8; break;
                            case "Deceive": score = 8; break;
                            case "Avoid": score = 5; break;
                            case "Freeze": score = 5; break;
                        }
                        if (GetHealth() <= 2) score += 8;
                        if (bm.GetTurnPlayer().Equals(this)) score += 2;
                        if (GetHealth() <= 2 && bm.GetTurnPlayer().Equals(this)) score -= 10;    // 천적 피하기
                        if (!bm.GetTurnPlayer().Equals(this) && opponentHealth <= 2) score /= 2;    // 상대가 죽으면 안됨
                        break;
                    case "Deceive":
                        switch (opponentCard)
                        {
                            case "Attack": score = 9; break;
                            case "Heal": score = 13; break;
                            case "Bomb": score = 9; break;
                            case "Avoid": score = 10; break;
                            case "Freeze": score = 10; break;
                        }
                        break;
                    case "Avoid":
                        switch (opponentCard)
                        {
                            case "Attack": score = 12; break;
                            case "Heal": score = 12; break;
                            case "Bomb": score = 11; break;
                            case "Deceive": score = 12; break;
                            case "Freeze": score = 12; break;
                        }
                        if (opponentHealth <= 2) score += 6;
                        if (GetHealth() <= 2) score += 3;
                        break;
                    case "Freeze":
                        switch (opponentCard)
                        {
                            case "Attack": score = 11; break;
                            case "Heal": score = 15; break;
                            case "Bomb": score = 11; break;
                            case "Deceive": score = 15; break;
                            case "Avoid": score = 11; break;
                        }
                        if (GetHealth() <= 2) score -= 5;   // 천적 피하기
                        break;
                }
                break;
            case 3:
                switch (myCard)
                {
                    case "Attack":
                        switch (opponentCard)
                        {
                            case "Attack": score = 14; break;
                            case "Heal": score = 18; break;
                            case "Bomb": score = 14; break;
                            case "Deceive": score = 18; break;
                            case "Avoid": score = 14; break;
                            case "Freeze": score = 15; break;
                        }
                        if (opponentHealth <= 2) score += 7;
                        if (GetHealth() <= 2) score -= 10;  // 천적 피하기
                        break;
                    case "Heal":
                        switch (opponentCard)
                        {
                            case "Attack": score = 1; break;
                            case "Heal": score = 3; break;
                            case "Bomb": score = 1; break;
                            case "Deceive": score = 3; break;
                            case "Avoid": score = 3; break;
                            case "Freeze": score = 2; break;
                        }
                        if (GetHealth() <= 2 && score > 3) score = 3;   // 천적 피하기
                        break;
                    case "Bomb":
                        switch (opponentCard)
                        {
                            case "Attack": score = 25; break;
                            case "Heal": score = 29; break;
                            case "Deceive": score = 29; break;
                            case "Avoid": score = 26; break;
                            case "Freeze": score = 26; break;
                        }
                        if (GetHealth() <= 2) score += 5;
                        if (bm.GetTurnPlayer().Equals(this)) score += 5;
                        if (opponentHealth <= 2 && !bm.GetTurnPlayer().Equals(this)) score += 5;
                        if (GetHealth() <= 2 && bm.GetTurnPlayer().Equals(this)) score -= 20;   // 천적 피하기
                        break;
                    case "Deceive":
                        switch (opponentCard)
                        {
                            case "Attack": score = 9; break;
                            case "Heal": score = 13; break;
                            case "Bomb": score = 9; break;
                            case "Avoid": score = 10; break;
                            case "Freeze": score = 10; break;
                        }
                        break;
                    case "Avoid":
                        switch (opponentCard)
                        {
                            case "Attack": score = 10; break;
                            case "Heal": score = 10; break;
                            case "Bomb": score = 9; break;
                            case "Deceive": score = 10; break;
                            case "Freeze": score = 10; break;
                        }
                        if (GetHealth() <= 2) score += 9;
                        break;
                    case "Freeze":
                        switch (opponentCard)
                        {
                            case "Attack": score = 9; break;
                            case "Heal": score = 13; break;
                            case "Bomb": score = 9; break;
                            case "Deceive": score = 13; break;
                            case "Avoid": score = 9; break;
                        }
                        if (GetHealth() <= 2) score -= 5;   // 천적 피하기
                        break;
                }
                break;
        }
        if (score < 1) score = 1;

        score = score * score;  // 유리한 행동을 할 확률과 불리한 행동을 할 확률의 차이를 크게 벌린다.

        // 상대가 속임을 낼 것이라면, 뽑기에 넣기 전에 내가 원래 내려던 카드로 다시 바꿔준다.
        if (opponentCard == "Deceive")
        {
            myCard = voidCard;
        }
        bm.RpcPrintLog(score + " lotteries say " + opponentPlayerIndex + myCard + ".");
        for (int i = 0; i < score; i++)
        {
            box.Add(opponentPlayerIndex + myCard);
        }
    }
}