using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 기본 행동과 스킬의 정보를 가지고 있는 클래스입니다.
/// </summary>
public class BehaviorManager : MonoBehaviour
{

    public static BehaviorManager behaviorManager;

    // 기본 행동과 스킬의 목록을 가지고 있는 데이터베이스입니다.
    private static List<BehaviorInfo> behaviorInfo = new List<BehaviorInfo>();

    // 여기에 기본 행동과 스킬을 추가할 수 있습니다.
    private void Awake()
    {
        BehaviorInfo bi;

        #region 기본 행동 목록
        // 단일 대상 일반 공격
        bi = new BehaviorInfo("basicAttack", "단일 공격", 0, 1, false, 0, "단일 대상에게 자신의 단일 일반 공격력만큼의 피해를 줍니다.")
        {
            Perform = BasicAttack
        };
        behaviorInfo.Add(bi);

        // 두 명 대상 일반 공격(프로토타입 버전에 존재, 최종 버전에서는 다공 특화 캐릭터가 사용)
        bi = new BehaviorInfo("basicDoubleAttack", "다수 공격", 0, 2, false, 0, "두 명의 대상에게 각각 자신의 다수 일반 공격력만큼의 피해를 줍니다.")
        {
            Perform = BasicDoubleAttack
        };
        behaviorInfo.Add(bi);

        // 단일 대상 일반 회복
        bi = new BehaviorInfo("basicHeal", "회복", 1, 1, true, 0, "단일 대상의 체력을 자신의 회복량만큼 회복시킵니다. 자신에게 사용할 수 있습니다.")
        {
            Perform = BasicHeal
        };
        behaviorInfo.Add(bi);

        // 두 명 대상 일반 회복(최종 버전에서 회복 특화 캐릭터가 사용)
        bi = new BehaviorInfo("basicDoubleHeal", "다수 회복", 1, 2, true, 0, "두 명 대상의 체력을 각각 자신의 회복량만큼 회복시킵니다. 자신에게 사용할 수 있습니다.")
        {
            Perform = BasicDoubleHeal
        };
        behaviorInfo.Add(bi);

        // 일반 통찰
        bi = new BehaviorInfo("basicInsight", "통찰", 0, 1, false, 0, "단일 대상을 선택하고, 그 대상이 잡아야 하는 목표 두 명이 누구인지 추정합니다. 목표 두 명을 모두 맞히면 이번 턴에 자신이 받는 모든 피해를 무시하고, 처음에 선택한 대상이 이번 턴에 수행할 행동을 무효화합니다. 목표를 정확히 맞히지 못하면 아무 행동도 하지 않습니다. 통찰에 성공한 대상에게는 다시 사용할 수 없습니다.")
        {
            Perform = BasicInsight
        };
        behaviorInfo.Add(bi);
        #endregion

        #region 스킬 목록
        /* // 형식:
         * 
         * bi = new BehaviorInfo("스킬 이름", "설명에 표시되는 스킬 이름", 마나 소모량, 대상 수, 대상에 자신 포함 여부(true/false), 대상 옵션, "스킬 상세 설명", 스킬 종류)
         * {
         *     perform = 스킬 구현 함수 이름
         * };
         * behaviorInfo.Add(bi);
         * 
         * // 대상 수       0: 지정하지 않음, 1~5: 1~5명 지정
         * // 대상 옵션     0: 옵션 없음, 1~4: 무작위 1~4명 대상, 5: 모든 생존자 대상, 6: 자신 대상(대상에 자신 포함 여부가 true여야 함)
         * // 대상 옵션은 대상 수가 0일 때 0이 아닌 값으로 설정하고, 대상 수가 0이 아닐 때 0으로 설정하면 된다.
         * // 대상에 자신 포함 여부가 false이면 모든 옵션에서 자신 제외
         * // 스킬 종류     1: 캐릭터 고유 스킬, 2: 공용 스킬
         */

        bi = new BehaviorInfo("singlePowerAttack1", "단일 대상 강한 공격", 3, 1, false, 0, "단일 대상에게 7의 피해를 줍니다.", 2)
        {
            Perform = SinglePowerAttack1
        };
        behaviorInfo.Add(bi);

        bi = new BehaviorInfo("singlePowerAttack2", "단일 대상 강한 공격", 6, 1, false, 0, "단일 대상에게 12의 피해를 줍니다.", 2)
        {
            Perform = SinglePowerAttack2
        };
        behaviorInfo.Add(bi);

        bi = new BehaviorInfo("singlePowerAttack3", "단일 대상 강한 공격", 10, 1, false, 0, "단일 대상에게 18의 피해를 줍니다.", 2)
        {
            Perform = SinglePowerAttack3
        };
        behaviorInfo.Add(bi);

        // TODO 새 스킬을 추가할 때는 위와 같은 형식으로 여기에 추가하고, 아래에 스킬이 수행하는 스킬 구현 함수를 만들어주세요.

        #endregion
    }

    #region 기본 행동 구현 함수

    private void BasicAttack(Behavior b)
    {
        // 이 함수가 실행되기 전에 행동이 유효한지 검증하는 과정을 거치기 때문에, 여기서는 수행하는 결과만 구현하면 됩니다.
        // 가령 단일 일반 공격의 대상 수는 한 명인데, b.GetObjectPlayers().Count가 1임을 이미 확인했기 때문에 아래와 같이 구현해도 무방합니다.
        foreach (PlayerController p in b.GetObjectPlayers())
        {
            p.HealthDamage(b.GetSubjectPlayer().GetSingleAttack());
        }
    }

    private void BasicDoubleAttack(Behavior b)
    {
        foreach (PlayerController p in b.GetObjectPlayers())
        {
            p.HealthDamage(b.GetSubjectPlayer().GetDoubleAttack());
        }
    }

    private void BasicHeal(Behavior b)
    {
        b.GetSubjectPlayer().ManaConsumption(Find(b).GetConsumedMana());
        foreach (PlayerController p in b.GetObjectPlayers())
        {
            p.HealthRestoration(b.GetSubjectPlayer().GetHeal());
        }
    }

    private void BasicDoubleHeal(Behavior b)
    {
        b.GetSubjectPlayer().ManaConsumption(Find(b).GetConsumedMana());
        foreach (PlayerController p in b.GetObjectPlayers())
        {
            p.HealthRestoration(b.GetSubjectPlayer().GetHeal());
        }
    }

    private void BasicInsight(Behavior b)
    {
        // TODO 통찰 구현하기(특히 상대 행동 무효화)
    }

    #endregion

    #region 스킬 구현 함수

    private void SinglePowerAttack1(Behavior b)
    {
        b.GetSubjectPlayer().ManaConsumption(Find(b).GetConsumedMana());
        foreach (PlayerController p in b.GetObjectPlayers())
        {
            p.HealthDamage(7);
        }
    }

    private void SinglePowerAttack2(Behavior b)
    {
        b.GetSubjectPlayer().ManaConsumption(Find(b).GetConsumedMana());
        foreach (PlayerController p in b.GetObjectPlayers())
        {
            p.HealthDamage(12);
        }
    }

    private void SinglePowerAttack3(Behavior b)
    {
        b.GetSubjectPlayer().ManaConsumption(Find(b).GetConsumedMana());
        foreach (PlayerController p in b.GetObjectPlayers())
        {
            p.HealthDamage(18);
        }
    }

    #endregion

    /// <summary>
    /// 인자로 주어진 행동과 이름이 같은 행동 정보를 찾아 반환합니다. 찾지 못하면 null을 반환합니다.
    /// </summary>
    /// <param name="b">찾을 행동</param>
    /// <returns>행동 정보</returns>
    private static BehaviorInfo Find(Behavior b)
    {
        foreach (BehaviorInfo bi in behaviorInfo)
        {
            if (bi.GetBehaviorName() == b.GetBehavior().Name)
            {
                return bi;
            }
        }
        return null;
    }

    /// <summary>
    /// 행동 수행 단계에서 인자로 주어진 행동이 행동 데이터베이스에 있는 행동 정보에 존재하는, 유효한 행동인지 검증하는 함수입니다.
    /// </summary>
    /// <param name="b">검증할 행동</param>
    /// <returns>유효한 행동이면 true, 아니면 false를 반환합니다.</returns>
    public static bool Verificate(Behavior b)
    {
        // 인자로 주어진 행동이 올바른지 확인합니다.
        if (b == null || b.GetBehavior() == null)
        {
            return false;
        }
        // 행동과 이름이 같은 행동 정보를 찾습니다.
        BehaviorInfo bi = Find(b);

        // 행동 정보가 존재하는지 확인합니다.
        if (bi == null)
        {
            return false;
        }
        // 행동 정보에서 요구하는 대상 수와 행동의 대상 수가 일치하는지 확인합니다.
        if (bi.GetTargetNumber() != b.GetObjectPlayers().Count)
        {
            return false;
        }
        // 자신을 대상으로 지정할 수 없는 행동의 경우, 행동에서 자신이 대상으로 지정되었는지 확인합니다.
        if (!bi.GetTargetMyself() && b.GetObjectPlayers().IndexOf(b.GetSubjectPlayer()) != -1)
        {
            return false;
        }
        // 행동이 통찰인 경우, 대상의 목표일 것으로 추정한 플레이어의 수가 두 명인지 확인합니다.
        if (bi.GetBehaviorName() == "basicInsight" && b.GetThirdPartyPlayers().Count != 2)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 인자로 주어진 행동을 검증한 후 수행합니다.
    /// </summary>
    /// <param name="b">수행할 행동</param>
    public static void Perform(Behavior b)
    {
        if (!Verificate(b))
        {
            Debug.LogWarning("Bad behavior.");
            return;
        }
        BehaviorInfo bi = Find(b);
        bi.Perform(b);
    }

    /// <summary>
    /// 인자로 주어진 행동 종류에 따른 스킬 종류를 반환합니다. 올바른 이름이 아니면 -1을 반환합니다.
    /// </summary>
    /// <param name="bb">행동 종류</param>
    /// <returns>기본 행동이면 0, 캐릭터 고유 스킬이면 1, 공용 스킬이면 2를 반환합니다.</returns>
    public static int GetSkillType(BasicBehavior bb)
    {
        if (bb == null) return -1;
        foreach (BehaviorInfo bi in behaviorInfo)
        {
            if (bi.GetBehaviorName() == bb.Name)
            {
                return bi.GetSkillType();
            }
        }
        return -1;
    }

    public static bool EnoughMana(BasicBehavior bb, int mana)
    {
        if (bb == null) return false;
        foreach (BehaviorInfo bi in behaviorInfo)
        {
            if (bi.GetBehaviorName() == bb.Name)
            {
                if (mana >= bi.GetConsumedMana())
                {
                    return true;
                }
                else return false;
            }
        }
        return false;
    }

    /// <summary>
    /// 대상 지정 단계에서 인자로 주어진 행동이 유효한 행동인지 검증하는 함수입니다.
    /// </summary>
    /// <param name="b">검증할 행동</param>
    /// <returns>유효한 행동이면 true, 아니면 false를 반환합니다.</returns>
    public static bool VerificateTarget(Behavior b)
    {
        // 인자로 주어진 행동이 올바른지 확인합니다.
        if (b == null || b.GetBehavior() == null)
        {
            return false;
        }
        // 행동과 이름이 같은 행동 정보를 찾습니다.
        BehaviorInfo bi = Find(b);

        // 행동 정보가 존재하는지 확인합니다.
        if (bi == null)
        {
            return false;
        }
        // 행동 정보에서 요구하는 대상 수와 행동의 대상 수가 일치하는지 확인합니다.
        if (bi.GetTargetNumber() != b.GetObjectPlayers().Count)
        {
            return false;
        }
        // 자신을 대상으로 지정할 수 없는 행동의 경우, 행동에서 자신이 대상으로 지정되었는지 확인합니다.
        if (!bi.GetTargetMyself() && b.GetObjectPlayers().IndexOf(b.GetSubjectPlayer()) != -1)
        {
            return false;
        }
        // 지정할 수 없는 대상을 지정했는지 확인합니다.
        foreach (PlayerController p in b.GetObjectPlayers())
        {
            if(!p.GetTargetable())
            {
                return false;
            }
        }
        return true;
    }
}

/// <summary>
/// 기본 행동이나 스킬의 정보를 저장하는 객체의 클래스입니다.
/// </summary>
public class BehaviorInfo
{
    private string behaviorName;        // 행동(스킬) 이름(내부적으로 사용하는 이름)
    private string behaviorNameText;    // 행동(스킬) 이름(UI에 표시되는 텍스트)
    private int consumedMana;           // 마나 소모량
    private bool isSkill;               // 기본 행동이면 false, 스킬이면 true
    private int skillType = 0;          // 스킬 종류(0: 기본 행동, 1: 캐릭터 고유 스킬, 2: 공용 스킬)
    private int targetNumber;           // 대상 수(0: 지정하지 않음, 1~5: 1~5명 지정)
    private bool targetMyself;          // 대상에 자신이 포함되면 true, 자신 제외이면 false
    private int targetOption;           // 대상 수가 0일 때의 옵션(0: 옵션 없음, 1~4: 무작위 1~4명 대상, 5: 모든 생존자 대상, 6: 자신 대상), targetMyself가 false이면 자신 제외
    private string behaviorText;        // 행동(스킬) 설명, UI의 툴팁에 표시되는 텍스트
    private GameObject behaviorButton;  // 행동(스킬) 버튼
    public delegate void PerformBehavior(Behavior b);    // 행동(스킬)을 수행하는 함수 대리자
    public PerformBehavior Perform;     // 외부에서 이 행동을 수행할 때 bi.Perform(behavior); 와 같은 방법으로 호출하면 된다.

    public BehaviorInfo(string name, string nameText, int mana, int targetNum, bool targetMe, int targetOp, string text, int skillType = 0)
    {
        behaviorName = name;
        behaviorNameText = nameText;
        consumedMana = mana;
        // TODO 기본 행동의 종류가 추가 또는 변경되면 아래의 조건을 수정해야 한다.
        if (behaviorName == "basicAttack" || behaviorName == "basicDoubleAttack" || behaviorName == "basicHeal" || behaviorName == "basicDoubleHeal" || behaviorName == "basicInsight")
        {
            isSkill = false;
            this.skillType = 0;
        }
        else
        {
            isSkill = true;
            this.skillType = skillType;
        }
        targetNumber = targetNum;
        targetMyself = targetMe;
        targetOption = targetOp;
        behaviorText = text;
    }

    public string GetBehaviorName()
    {
        return behaviorName;
    }

    public int GetConsumedMana()
    {
        return consumedMana;
    }

    public bool GetIsSkill()
    {
        return isSkill;
    }

    public int GetTargetNumber()
    {
        return targetNumber;
    }

    public bool GetTargetMyself()
    {
        return targetMyself;
    }

    public int GetTargetOption()
    {
        return targetOption;
    }

    public string GetBehaviorText()
    {
        return behaviorText;
    }

    public int GetSkillType()
    {
        return skillType;
    }

    public void SetBehaviorButton(GameObject button)
    {
        behaviorButton = button;
    }
}
