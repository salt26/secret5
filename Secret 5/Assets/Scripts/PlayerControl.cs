﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerControl : NetworkBehaviour {

    [SyncVar] private int currentHealth;     // 현재 남은 체력(실시간으로 변화, 외부 열람 불가)
    [SerializeField][SyncVar] private int maxHealth = 6;     // 최대 체력(초기 체력)
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
    private Card playCard;                          // 내가 낼 카드

    private RectTransform HealthBar;                // HP UI
    [SerializeField] private GameObject playerCamera;

    private static BattleManager bm;
    
	void Awake () {
        bm = BattleManager.bm;
        currentHealth = maxHealth;
        displayedHealth = currentHealth;
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
        bm.players[playerNum - 1] = this;
    }

    void Start () {
        HealthBar = GetComponentInChildren<Finder>().GetComponent<Image>().rectTransform;
        if (bm == null) Debug.Log("BM is null.");
        Renderer[] rends = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends)
            r.material.color = color;
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
        playerCamera.SetActive(true);
        PushingCard.localPlayer = this;
    }

    /*
    public override void OnStartLocalPlayer()
    {
        currentHealth = maxHealth;
        displayedHealth = currentHealth;
        Debug.Log("OnStartLocalPlayer " + playerName);
        HealthBar = GetComponentInChildren<Finder>().GetComponent<Image>().rectTransform;
        bm = GameObject.Find("BattleManager").GetComponent<BattleManager>();
        if (bm == null) Debug.Log("BM is null.");
        if (character != null)
        {
            GameObject c = Instantiate(character, GetComponent<Transform>().position, Quaternion.identity, GetComponent<Transform>());
            NetworkServer.Spawn(c);
        }
    }
    */
    
    void FixedUpdate () {
        if (isLocalPlayer && Input.GetMouseButtonDown(0))
        {
            if (bm.GetObjectPlayer() != null)
                Debug.Log("Mouse Clicked. bm.GetTurnStep(): " + bm.GetTurnStep() + ", bm.GetTurnPlayer(): " + bm.GetTurnPlayer().GetName() + ", bm.GetObjectPlayer(): " + bm.GetObjectPlayer().GetName());
            else Debug.Log("Mouse Clicked. bm.GetTurnStep(): " + bm.GetTurnStep() + ", bm.GetTurnPlayer(): " + bm.GetTurnPlayer().GetName());
            if (bm.GetTurnStep() == 2 && objectTarget != null && bm.GetTurnPlayer().Equals(this))
                PlayerToSelectCard();
            if (bm.GetTurnStep() == 3 && bm.GetObjectPlayer() != null && bm.GetObjectPlayer().Equals(this))
                PlayerToSelectCard();
            if (bm.GetTurnStep() == 2 && bm.GetTurnPlayer().Equals(this))
                PlayerToSelectTarget();
        }

        HealthBar.sizeDelta = new Vector2(displayedHealth * 100f / 6f, HealthBar.sizeDelta.y); // HealthBar 변경 -> displayedHealth 기준으로 계산하도록 수정
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
            Debug.Log(playerName + " is freezed.");
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
        displayedHealth = currentHealth;
    }
    
    [ClientCallback]
    public void PlayerToSelectTarget()
    {
        if (!isLocalPlayer) return;
        //if (!bm.GetCameraPlayer().Equals(this)) return; // 임시 코드

        // 내 턴이 아니면 패스
        if (!bm.GetTurnPlayer().Equals(this)) return;
        //Debug.Log("PlayerToSelectTarget");
        Ray ray = GetComponentInChildren<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 8)))
        {
            Debug.Log("Click " + hit.collider.name + ".");
            Debug.DrawLine(ray.origin, hit.point, Color.blue, 3f);
            if (hit.collider.gameObject.GetComponentInParent<PlayerControl>() != null
                && !hit.collider.gameObject.GetComponentInParent<PlayerControl>().Equals(this))
            {
                if (objectTarget == null || !objectTarget.Equals(hit.collider.gameObject.GetComponentInParent<PlayerControl>()))
                {
                    objectTarget = hit.collider.gameObject.GetComponentInParent<PlayerControl>();
                    Debug.Log("Set " + hit.collider.gameObject.GetComponentInParent<PlayerControl>().GetName() + " to a target.");
                }
            }
        }
    }

    // 임시 코드
    [ClientCallback]
    public void PlayerToSelectCard()
    {
        if (!isLocalPlayer) return;
        //if (!bm.GetCameraPlayer().Equals(this)) return; // 임시 코드

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
            Debug.Log("Click " + hit.collider.name + ".");
            Debug.DrawLine(ray.origin, hit.point, Color.red, 3f);
            if (hit.collider.gameObject.GetComponentInParent<Card>() != null
                && (hit.collider.gameObject.GetComponentInParent<Card>().Equals(hand[0])
                || hit.collider.gameObject.GetComponentInParent<Card>().Equals(hand[1])))
            {
                /*
                if (!playCard.Equals(hit.collider.gameObject.GetComponent<Card>()))
                {
                    playCard = hit.collider.gameObject.GetComponent<Card>();
                    
                }
                */
                Debug.Log("Set " + hit.collider.gameObject.GetComponentInParent<Card>().GetCardName() + " card to play.");
                DecideClicked();
                CmdSetCardToPlay(hit.collider.gameObject.GetComponentInParent<Card>().GetCardCode(), GetPlayerIndex());
            }
        }
    }
    
    [ClientCallback]
    public void DecideClicked()
    {
        if (bm.GetTurnStep() != 2)
        {
            return;
        }
        //if (!bm.GetCameraPlayer().Equals(this)) return; // 임시 코드
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
            int i = bm.players.IndexOf(objectTarget);
            CmdSetObjectPlayer(i);
            objectTarget = null;
        }
    }

    [Command]
    public void CmdSetCardToPlay(int cardCode, int playerIndex)
    {
        bm.SetCardToPlay(cardCode, playerIndex);
    }

    [Command]
    private void CmdSetObjectPlayer(int objectTargetIndex)
    {
        bm.SetObjectPlayer(objectTargetIndex);
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
}
