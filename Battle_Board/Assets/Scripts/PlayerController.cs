using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 각 플레이어의 체력과 능력치를 관리하는 클래스입니다.
/// </summary>
public class PlayerController : MonoBehaviour {

    public int health;                              // 현재 체력

    [SerializeField] private int maxHealth = 40;    // 최대 체력
    [SerializeField] private int singleAttack = 6;  // 단일 대상 일반 공격력
    [SerializeField] private int doubleAttack = 3;  // 두 명 대상 일반 공격력
    [SerializeField] private int heal = 4;          // 회복량
    [SerializeField] private int characterType = 0; // 캐릭터 종류(이것에 따라 캐릭터 능력이 정해짐)
    //[SerializeField] private Skill skill1;        // 스킬1
    //[SerializeField] private Skill skill2;        // 스킬2
    [SerializeField] private bool isDead = false;   // 사망 여부(사망하면 true)
    [SerializeField] private string playerName;     // 플레이어 이름

	// Awake() 함수는 Start() 함수보다 항상 먼저 실행됩니다.
	void Awake () {
        health = maxHealth;
	}
	
	// FixedUpdate() 함수는 고정된 프레임 수(1초에 60번)에 따라 매 프레임마다 호출됩니다.
	void FixedUpdate () {
		if (health <= 0 && !isDead)
        {
            isDead = true;
        }
	}
}
