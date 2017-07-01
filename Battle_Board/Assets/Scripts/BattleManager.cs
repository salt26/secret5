using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 대전을 전체적으로 관리하는 클래스입니다.
/// </summary>
public class BattleManager : MonoBehaviour {

    public List<GameObject> Players = new List<GameObject>();   // 대전에 참여하는 플레이어들

    private int turn = 0;   // 0: 대전 시작, 1: 턴 시작, 2: 행동 결정, 3: 행동 수행, 4: 사망자 처리, 5: 대전 종료 확인

    /* 
     * TODO
     * 플레이어 5명의 캐릭터 생성
     * 목표 그래프 만들고 플레이어를 랜덤으로 배치하기
     * 행동 처리하기
     * 대전이 종료되었는지 판단하기
     */
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
	}
}
