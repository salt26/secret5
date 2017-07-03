using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 각 플레이어의 체력과 능력치를 관리하는 클래스입니다.
/// </summary>
public class PlayerController : MonoBehaviour {

    [SerializeField] private int health;            // 현재 체력
    [SerializeField] private int maxHealth = 40;    // 최대 체력
    [SerializeField] private int mana = 0;          // 현재 마나(최대 마나는 10으로 고정)
    [SerializeField] private int singleAttack = 6;  // 단일 대상 일반 공격력
    [SerializeField] private int doubleAttack = 3;  // 두 명 대상 일반 공격력
    [SerializeField] private int heal = 4;          // 회복량
    [SerializeField] private int characterType = 0; // 캐릭터 종류(이것에 따라 캐릭터 능력이 정해짐)
    [SerializeField] private Skill skill1;          // 스킬1
    [SerializeField] private Skill skill2;          // 스킬2
    [SerializeField] private bool isDead = false;   // 사망 여부(사망하면 true)
    [SerializeField] private string playerName;     // 플레이어 이름

    private Behavior behavior;
    private bool isTargetable = true;               // 자신이 행동의 대상으로 지정될 수 있는지 여부(가능하면 true)
    private bool isInvincible = false;              // 자신이 피해 무시 상태인지 여부(이번 턴에 받은 피해를 모두 무시하는 상태이면 true)
    private bool isFreezed = false;                 // 빙결 여부(이번 턴에 빙결되어 행동을 할 수 없는 상태이면 true)
    private bool isSilenced = false;                // 침묵 여부(이번 턴에 침묵되어 스킬을 사용할 수 없는 상태이면 true)
    private bool hasDecided = false;                // 행동 결정 완료 여부(행동을 결정한 상태이면 true)
    private bool isDecideClicked = false;

    // Awake() 함수는 Start() 함수보다 항상 먼저 호출되며, 한 번만 호출됩니다.
    void Awake() {
        health = maxHealth;
    }

    // FixedUpdate() 함수는 고정된 프레임 수(1초에 60번)에 따라 매 프레임마다 호출됩니다.
    void FixedUpdate() {

    }

    /// <summary>
    /// 캐릭터에게 피해를 입혀 현재 체력을 깎는 함수입니다.
    /// </summary>
    /// <param name="amount">피해량</param>
    public void HealthDamage(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogError("Damaged amount is non-positive.");
            return;
        }
        else if (amount >= 65536)
        {
            Debug.LogError("Damaged amount is too large.");
            return;
        }
        else if (isDead)
        {
            Debug.LogWarning("This player is dead.");
            return;
        }
        else if (isInvincible)
        {
            return;
        }
        health -= amount;
    }

    /// <summary>
    /// 캐릭터를 회복시켜 현재 체력을 올려주는 함수입니다.
    /// </summary>
    /// <param name="amount">회복량</param>
    public void HealthRestoration(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogError("Restored amount is non-positive.");
            return;
        }
        else if (amount >= 65536)
        {
            Debug.LogError("Restored amount is too large.");
            return;
        }
        else if (isDead)
        {
            Debug.LogWarning("This player is dead.");
            return;
        }
        else if (health + amount >= maxHealth)
        {
            health = maxHealth;
        }
        else
        {
            health += amount;
        }
    }

    /// <summary>
    /// 캐릭터의 마나를 소모시키는 함수입니다.
    /// </summary>
    /// <param name="amount">마나 소모량</param>
    /// <returns>마나가 소모될 수 있으면 true를 반환합니다.</returns>
    public bool ManaConsumption(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogError("Consumed mana is non-positive.");
            return false;
        }
        else if (amount > 10)
        {
            Debug.LogError("Consumed mana is too large.");
            return false;
        }
        else if (isDead)
        {
            Debug.LogWarning("This player is dead.");
            return false;
        }
        else if (amount > mana)
        {
            Debug.LogWarning("Not enough mana.");
            return false;
        }
        mana -= amount;
        return true;
    }

    /// <summary>
    /// 캐릭터의 마나를 2씩 올려주는 함수입니다.
    /// </summary>
    public void ManaRecovery()
    {
        if (isDead)
        {
            return;
        }
        else if (mana > 8)
        {
            mana = 10;
        }
        else
        {
            mana += 2;
        }
    }

    /// <summary>
    /// 캐릭터의 사망 여부를 판정하는 함수입니다.
    /// </summary>
    public void Death()
    {
        if (health <= 0 && !isDead)
        {
            isDead = true;
            // 사망하면 다른 플레이어의 행동의 대상으로 지정되지 않고, 모든 상태이상이 해제됨
            isTargetable = false;
            isFreezed = false;
            isSilenced = false;
        }
    }

    /// <summary>
    /// 캐릭터에게 걸린 상태이상을 해제하고 원래 상태로 돌려놓는 함수입니다.
    /// </summary>
    public void Purify()
    {
        if (!isDead)
        {
            isTargetable = true;
            isInvincible = false;
            isFreezed = false;
        }
    }

    public void MakeBehavior(string behaviorName)
    {
        if (isDead || isFreezed)
        {
            Debug.Log("You cannot make behavior.");
            return;
        }
        BasicBehavior bb = new BasicBehavior(behaviorName);
        int skillType = BehaviorManager.GetSkillType(bb);
        if (skillType == -1)
        {
            Debug.LogWarning("Bad behavior.");
            return;
        }
        else if ((skillType == 1 || skillType == 2) && isSilenced)
        {
            Debug.Log("You cannot invoke skill.");
            return;
        }
        if (!BehaviorManager.EnoughMana(bb, mana))
        {
            Debug.Log("Not enough mana.");
            return;
        }
        behavior = new Behavior(bb, this);
        StartCoroutine("SelectTarget", behavior);
    }

    IEnumerator SelectTarget(Behavior behavior)
    {
        while (true)
        {
            yield return new WaitUntil(() => isDecideClicked);
            isDecideClicked = false;
            if (BehaviorManager.VerificateTarget(behavior))
            {
                GameObject.Find("BattleManager").GetComponent<BattleManager>().AddBehavior(behavior);
                hasDecided = true;
                break;
            }
            else Debug.Log("Invalid target.");
            // TODO 통찰의 경우 두 번 선택하도록 만들기
            // TODO 선택 취소 기능 만들기
        }
    }

    public void DecideClick()
    {
        // TODO 선택한 플레이어들을 behavior.SetObjectPlayers(List<PlayerController>)로 저장해야 한다.
        isDecideClicked = true;
    }

    /// <summary>
    /// 행동 결정 완료 여부를 반환합니다.
    /// </summary>
    /// <returns></returns>
    public bool GetHasDecided()
    {
        return hasDecided;
    }

    /// <summary>
    /// 사망 여부를 반환합니다.
    /// </summary>
    /// <returns></returns>
    public bool GetDead()
    {
        return isDead;
    }

    public bool GetTargetable()
    {
        return isTargetable;
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public int GetSingleAttack()
    {
        return singleAttack;
    }

    public int GetDoubleAttack()
    {
        return doubleAttack;
    }

    public int GetHeal()
    {
        return heal;
    }

    public void SetPlayerName(string name)
    {
        playerName = name;
    }

    public void SetNotDecided()
    {
        hasDecided = false;
    }
}
