using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 행동을 나타내는 객체의 클래스입니다.
/// 행동 종류, 행동 주체, 행동 대상 등을 포함합니다.
/// </summary>
public class Behavior{

    private BasicBehavior behavior;             // 행동 종류
    private GameObject SubjectPlayer;           // 행동 주체
    private List<GameObject> ObjectPlayers;     // 행동 대상
    private List<GameObject> ThirdPartyPlayers; // 행동이 통찰인 경우에, 행동 대상의 추정 목표 대상 두 명을 지정

    // 행동 객체의 생성자입니다.
    public Behavior(BasicBehavior behavior, GameObject subjectPlayer,
        List<GameObject> objectPlayers = null, List<GameObject> thirdPartyPlayers = null)
    {
        this.behavior = behavior;
        SubjectPlayer = subjectPlayer;
        ObjectPlayers = objectPlayers;
        ThirdPartyPlayers = thirdPartyPlayers;
    }

    public BasicBehavior GetBehavior()
    {
        return behavior;
    }

    public GameObject GetSubjectPlayer()
    {
        return SubjectPlayer;
    }

    public List<GameObject> GetObjectPlayers()
    {
        return ObjectPlayers;
    }

    public List<GameObject> GetThirdPartyPlayers()
    {
        return ThirdPartyPlayers;
    }
}

/// <summary>
/// 기본 행동을 나타내는 객체의 클래스입니다.
/// 행동 이름을 포함합니다.
/// </summary>
public class BasicBehavior
{
    private string behaviorName;

    // 기본 행동 객체의 생성자입니다.
    public BasicBehavior(string name)
    {
        behaviorName = name;
    }

    public string Name  // 기본 행동 이름 (또는 스킬 이름)
    {
        get
        {
            return behaviorName;
        }
    }
}

/// <summary>
/// 스킬을 나타내는 객체의 클래스입니다.
/// 기본 행동 클래스를 상속받아, 스킬 이름과 마나 소모량에 따른 종류를 포함합니다.
/// </summary>
public class Skill : BasicBehavior
{
    private int manaType;

    // 스킬 객체의 생성자입니다.
    public Skill(string name, int type) : base(name)
    {
        manaType = type;
    }

    public int Type     // 마나 소모량에 따른 종류
    {
        get
        {
            return manaType;
        }
    }
}
