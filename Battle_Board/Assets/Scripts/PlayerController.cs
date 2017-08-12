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

    private BattleManager bm;                       // BattleManager.cs 스크립트의 메서드가 필요할 때 사용
    private Behavior behavior;                      // 행동 결정 단계에서 이 플레이어가 하려는 행동을 저장

    private bool isTargetable = true;               // 자신이 행동의 대상으로 지정될 수 있는지 여부(가능하면 true)
    private bool isInvincible = false;              // 자신이 피해 무시 상태인지 여부(이번 턴에 받은 피해를 모두 무시하는 상태이면 true)
    private bool isFreezed = false;                 // 빙결 여부(이번 턴에 빙결되어 행동을 할 수 없는 상태이면 true)
    private bool isSilenced = false;                // 침묵 여부(이번 턴에 침묵되어 스킬을 사용할 수 없는 상태이면 true)

    private bool hasDecided = false;                // 행동 결정 완료 여부(행동을 결정한 상태이면 true)
    private bool isDecideClicked = false;           // 행동 결정 버튼을 누르는 순간에만 true가 된다.
    private bool isTargetDecide = false;            // 행동의 대상을 결정하는 동안 true가 된다.

    private bool isAuto = true;                     // 인공지능 여부(true이면 행동을 자동으로 결정)
    private bool isMakingDecision = false;          // 인공지능이 매 턴마다 한 번씩만 행동을 결정하도록 하는 변수(isAuto가 true일 때만 작동)

    public int playerNum;

    // Awake() 함수는 Start() 함수보다 항상 먼저 호출되며, 한 번만 호출됩니다.
    void Awake() {
        health = maxHealth;
        bm = GameObject.Find("BattleManager").GetComponent<BattleManager>();
    }

    // FixedUpdate() 함수는 고정된 프레임 수(1초에 60번)에 따라 매 프레임마다 호출됩니다.
    // 여기서의 FixedUpdate() 함수는 인공지능이 행동을 자동으로 결정하도록 하는 함수입니다.
    void FixedUpdate() {
        // 인공지능 플레이어가 아니면 행동을 자동으로 결정하지 않습니다.
        if (!isAuto) return;
        if (bm.GetTurn() == 2 && !hasDecided)
        {
            //Debug.Log("Auto making decision...");
            if (!isMakingDecision)
            {
                int r = Random.Range(0, 4);
                if (r <= 1)
                {
                    MakeBehavior("basicAttack");
                }
                else if (r == 2)
                {
                    if (bm.TargetableNumberExceptOneself(this) >= 2)
                        MakeBehavior("basicDoubleAttack");
                    else
                        MakeBehavior("basicAttack");
                }
                else
                {
                    MakeBehavior("basicHeal");
                }
            }
            isMakingDecision = true;
            if (isTargetDecide && !isDecideClicked)
                DecideClick(new List<PlayerController>());
        }
        if (bm.GetTurn() == 3)
        {
            isMakingDecision = false;
        }
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
            Debug.Log(playerName + " is dead.");
            isDead = true;
            // 사망하면 다른 플레이어의 행동의 대상으로 지정되지 않고, 모든 상태이상이 해제됨
            isTargetable = false;
            isFreezed = false;
            isSilenced = false;
            gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
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

    /// <summary>
    /// 자신의 행동을 결정하는 함수입니다. 행동 이름을 우선 결정하고, 대상을 결정하는 Coroutine을 호출합니다.
    /// </summary>
    /// <param name="behaviorName">행동 이름</param>
    public void MakeBehavior(string behaviorName)
    {
        // 자신이 사망했거나 빙결되어 행동할 수 없는 경우
        if (isDead || isFreezed || bm.GetEnd())
        {
            if (playerNum == 1) Debug.Log("You cannot make behavior.");
            return;
        }
        BasicBehavior bb = new BasicBehavior(behaviorName);
        // 행동 이름이 존재하지 않는 경우
        int skillType = BehaviorManager.GetSkillType(bb);
        if (skillType == -1)
        {
            Debug.LogWarning("Bad behavior in MakeBehavior.");
            return;
        }
        // 행동이 스킬이고 자신이 침묵되어 스킬을 사용할 수 없는 경우
        else if ((skillType == 1 || skillType == 2) && isSilenced)
        {
            Debug.Log("You cannot invoke skill in this turn.");
            return;
        }
        // 행동에 필요한 마나가 부족한 경우
        if (!BehaviorManager.EnoughMana(bb, mana))
        {
            Debug.Log("Not enough mana.");
            return;
        }
        // 행동에서 지정해야 하는 대상 수가 지정 가능한 대상 수보다 많은 경우
        if (!BehaviorManager.TargetsExist(bb, this))
        {
            Debug.Log("Targets don't exist.");
            return;
        }
        // TODO 다수 대상 행동을 지정할 대상이 없을 때 행동을 제한할 필요가 있다.
        // TODO 이때 인공지능의 경우 다른 행동으로 다시 결정하게 할 필요가 있다. 지금은 여기서 return하면 그 턴에 아무 행동도 결정하지 않는다.
        behavior = new Behavior(bb, this);
        isTargetDecide = true;
        StartCoroutine("SelectTarget", behavior);
    }

    // 이 함수는 Coroutine 함수입니다.
    /// <summary>
    /// 행동 결정 단계 중 대상을 결정하는 동안 실행되는 Coroutine입니다. 적절한 대상을 정할 때까지 대기하고, 대상 결정이 끝나면 행동 결정을 완료합니다.
    /// </summary>
    /// <param name="behavior"></param>
    /// <returns></returns>
    IEnumerator SelectTarget(Behavior behavior)
    {
        while (true)
        {
            // 결정 버튼이 눌릴 때까지 대기
            yield return new WaitUntil(() => isDecideClicked);
            isDecideClicked = false;
            // 적절한 대상을 선택하면
            if (BehaviorManager.VerificateTarget(behavior))
            {
                // 행동 결정을 확정하고 완료한다.
                bm.AddBehavior(behavior);
                //if (playerNum == 1) Debug.Log("Player" + playerNum + " has decided.");
                hasDecided = true;
                isTargetDecide = false;
                break;
            }
            else
            {
                if (playerNum == 1) Debug.Log("Invalid target. Please select targets again. (" + behavior.GetBehavior().Name + ")");
            }
            // TODO 통찰의 경우 두 번 선택하도록 만들기
            // TODO 선택 취소 기능 만들기
        }
    }

    /// <summary>
    /// 행동의 대상 결정 단계에서 결정 버튼을 클릭했을 때 실행되는 함수입니다.
    /// </summary>
    public void DecideClick(List<PlayerController> objects)
    {
        if (!isTargetDecide)
        {
            Debug.Log("You didn't make behavior.");
            return;
        }
        else if (behavior == null)
        {
            Debug.LogError("behavior is null in DecideClick.");
            return;
        }
        // TODO 선택한 플레이어들을 behavior.SetObjectPlayers(List<PlayerController>)로 저장해야 한다.
        if (isAuto) {
            for (int i = 0; i < BehaviorManager.GetTargetNumber(behavior); i++)
            {
                objects.Add(bm.Players[Random.Range(0, 5)]);
                //Debug.Log("Player" + playerNum + " selects Player" + pc[i].playerNum + " as a target.");
            }
        }
        
        //Debug.Log("pc's count is " + pc.Count);
        behavior.SetObjectPlayers(objects);
        //Debug.Log("behavior.ObjectPlayers' count is " + behavior.GetObjectPlayers().Count);
        isDecideClicked = true;
    }

    /// <summary>
    /// 행동 결정 완료 여부를 반환합니다.
    /// </summary>
    /// <returns></returns>
    /// 
    public int GetHealth()
    {
        return health;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
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

    public bool GetTargetDecide()
    {
        return isTargetDecide;
    }

    public void SetPlayerName(string name)
    {
        playerName = name;
    }

    /// <summary>
    /// 행동 결정 완료 여부를 초기화합니다.
    /// </summary>
    public void SetNotDecided()
    {
        hasDecided = false;
    }

    /// <summary>
    /// 이 플레이어의 인공지능 여부를 설정합니다.
    /// </summary>
    /// <param name="auto">인공지능이면 true, 사람이면 false 입력</param>
    public void SetAuto(bool auto)
    {
        isAuto = auto;
    }
}
