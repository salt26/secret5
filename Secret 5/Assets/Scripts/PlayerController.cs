using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    [SerializeField] private int currentHealth;     // 현재 남은 체력(실시간으로 변화, 외부 열람 불가)
    [SerializeField] private int maxHealth = 6;    // 최대 체력(초기 체력)
    [SerializeField] private GameObject character;  // 캐릭터 모델
    [SerializeField] private bool isDead = false;   // 사망 여부(true이면 사망)
    [SerializeField] private string playerName;     // 플레이어 이름
    [SerializeField] private int playerNum;         // 대전에서 부여된 플레이어 번호

    private int displayedHealth;                    // 현재 남은 체력(턴이 끝날 때만 변화, 외부 열람 가능)
    private bool isFreezed = false;                 // 빙결 여부(true이면 다음 한 번의 내 턴에 교환 불가)

    private bool isAI = false;                      // 인공지능 플레이어 여부(true이면 인공지능, false이면 사람)
    private bool hasDecidedObjectPlayer = false;    // 내 턴에 교환 상대를 선택했는지 여부
    private bool hasDecidedPlayCard = false;        // 교환 시 교환할 카드를 선택했는지 여부
    private PlayerController objectTarget;          // 내가 선택한 교환 대상
    private Card playCard;                          // 내가 낼 카드

    private RectTransform HealthBar;                // HP UI

    private static BattleManager bm;
    
	void Awake () {
        currentHealth = maxHealth;
        displayedHealth = currentHealth;
        if (character != null)
        {
            GameObject c = Instantiate(character, GetComponent<Transform>().position, Quaternion.identity, GetComponent<Transform>());
        }
	}

    private void Start()
    {
        HealthBar = GetComponentInChildren<ASDF>().GetComponent<Image>().rectTransform;
        bm = GameObject.Find("BattleManager").GetComponent<BattleManager>();
        if (bm == null) Debug.Log("BM is null.");
    }

    void FixedUpdate () {
        if (Input.GetMouseButtonDown(0))
        {
            if (bm.GetTurnStep() == 3 && bm.GetObjectPlayer() != null && (bm.GetTurnPlayer().Equals(this) || bm.GetObjectPlayer().Equals(this)))
                PlayerToSelectCard();
            if (bm.GetTurnStep() == 2 && bm.GetTurnPlayer().Equals(this))
                PlayerToSelectTarget();
        }

        HealthBar.sizeDelta = new Vector2(currentHealth*100/6, HealthBar.sizeDelta.y); //HealthBar 변경
	}

    public void Damaged()
    {
        if (currentHealth > 0)
        {
            currentHealth -= 1;
        }
    }

    public void Restored()
    {
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
        if (!isDead)
        {
            isFreezed = true;
            Debug.Log(playerName + " is freezed.");
        }
    }

    public void Thawed()
    {
        if (!isDead && isFreezed)
        {
            isFreezed = false;
        }
    }

    public void UpdateHealth()
    {
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
        }
        displayedHealth = currentHealth;
    }

    public void PlayerToSelectTarget()
    {
        if (!bm.GetCameraPlayer().Equals(this)) return; // 임시 코드

        // 내 턴이 아니면 패스
        if (!bm.GetTurnPlayer().Equals(this)) return;
        Ray ray = GetComponentInChildren<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, (1 << 8)))
        {
            //Debug.Log("Click " + hit.collider.name + ".");
            Debug.DrawLine(ray.origin, hit.point, Color.blue, 3f);
            if (hit.collider.gameObject.GetComponentInParent<PlayerController>() != null
                && !hit.collider.gameObject.GetComponentInParent<PlayerController>().Equals(this))
            {
                if (objectTarget == null || !objectTarget.Equals(hit.collider.gameObject.GetComponentInParent<PlayerController>()))
                {
                    objectTarget = hit.collider.gameObject.GetComponentInParent<PlayerController>();
                    Debug.Log("Set " + hit.collider.gameObject.GetComponentInParent<PlayerController>().GetName() + " to a target.");
                }
            }
        }
    }

    public void PlayerToSelectCard()
    {
        if (!bm.GetCameraPlayer().Equals(this)) return; // 임시 코드

        // 내가 교환에 참여한 플레이어가 아니면 패스
        if (!bm.GetTurnPlayer().Equals(this) && !bm.GetObjectPlayer().Equals(this))
        {
            return;
        }
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
                //Debug.Log("Set " + hit.collider.gameObject.GetComponent<Card>().GetCardName() + " card to play.");
                bm.SetCardToPlay(hit.collider.gameObject.GetComponentInParent<Card>(), this);
            }
        }
    }

    public void DecideClicked()
    {
        if (bm.GetTurnStep() != 2)
        {
            return;
        }
        if (!bm.GetCameraPlayer().Equals(this)) return; // 임시 코드
        // 내 턴이 아니면 패스
        else if (!bm.GetTurnPlayer().Equals(this))
        {
            Debug.Log("It isn't your turn! turn Player: " + (bm.GetTurnPlayer().GetPlayerNum()) + ", this Player: " + bm.GetCameraPlayer().GetPlayerNum());
            return;
        }
        else if (objectTarget == null)
        {
            Debug.Log("turn Player: " + (bm.GetTurnPlayer().GetPlayerNum()) + ", this Player: " + bm.GetCameraPlayer().GetPlayerNum());
            Debug.LogWarning("Please select a player that you wants to exchange with.");
            return;
        }
        else
        {
            bm.SetObjectPlayer(objectTarget);
            objectTarget = null;
        }
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
}
