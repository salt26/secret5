using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [SerializeField] private int currentHealth;     // 현재 남은 체력(실시간으로 변화, 외부 열람 불가)
    [SerializeField] private int maxHealth = 30;    // 최대 체력(초기 체력)
    [SerializeField] private GameObject character;  // 캐릭터 모델
    [SerializeField] private bool isDead = false;   // 사망 여부(true이면 사망)
    [SerializeField] private string playerName;     // 플레이어 이름
    [SerializeField] private int playerNum;         // 대전에서 부여된 플레이어 번호

    private int displayedHealth;                    // 현재 남은 체력(턴이 끝날 때만 변화, 외부 열람 가능)
    private bool isFreezed = false;                 // 빙결 여부(true이면 다음 한 번의 내 턴에 교환 불가)

    private bool isAI = false;                      // 인공지능 플레이어 여부(true이면 인공지능, false이면 사람)

	// Use this for initialization
	void Awake () {
        currentHealth = maxHealth;
        displayedHealth = currentHealth;
        if (character != null)
        {
            GameObject c = (GameObject)Instantiate(character, this.GetComponent<Transform>().position, Quaternion.identity, this.GetComponent<Transform>());
        }
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
	}

    public void Damaged()
    {
        if (currentHealth > 0)
        {
            currentHealth -= 5;
        }
        if (currentHealth <= 0)
        {
            isDead = true;
        }
    }

    public void Restored()
    {
        if (currentHealth > 0 && currentHealth <= 25)
        {
            currentHealth += 5;
        }
        else if (currentHealth <= 30)
        {
            currentHealth = 30;
        }
    }

    public void Freezed()
    {
        if (!isDead)
        {
            isFreezed = true;
        }
    }

    public void UpdateHealth()
    {
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        displayedHealth = currentHealth;
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
}
