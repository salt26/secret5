using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour {

    public static CardDatabase cardDatabase;

    private static List<CardInfo> cardInfo = new List<CardInfo>();
    private static BattleManager bm;

    private void Awake()
    {
        bm = gameObject.GetComponent<BattleManager>();
        CardInfo ci;

        // 공격 카드
        ci = new CardInfo("Attack", "공격", "이 카드를 받을 때 피해를 5 받습니다.", 0)
        {
            Play = Attack
        };
        cardInfo.Add(ci);


    }

    private void Attack(PlayerController playedPlayer, PlayerController receivedPlayer)
    {
        // TODO
        // 카드에 효과가 달려있으면 안될 것 같은데?
        // 교환을 처리하는 하나의 함수에서 양쪽의 카드를 보면서 일괄 처리해야 할듯.
        // 그러고 나서 이 카드의 효과를 발동시켜야 한다고 판단되면 이 함수를 호출하게 하자.
    }
}

public class CardInfo
{
    private string cardName;        // 카드 이름(내부적으로 사용하는 이름)
    private string cardNameText;    // 카드의 한글 이름
    private string cardDetailText;  // 카드의 효과 설명 텍스트
    private int cardEffectType;     // 0: 받을 때 효과, 1: 낼 때 효과, 2: 내 턴이 끝날 때 효과
    public delegate void PlayCard(PlayerController playedPlayer, PlayerController receivedPlayer); 
                                    // 카드의 효과를 수행하는 함수 대리자
    public PlayCard Play;           // 외부에서 카드의 효과를 수행할 때 ci.Play(cardName); 과 같은 방법으로 호출하면 된다.

    public CardInfo(string name, string nameText, string detailText, int effectType)
    {
        cardName = name;
        cardNameText = nameText;
        cardDetailText = detailText;
        cardEffectType = effectType;
    }

    public string GetName()
    {
        return cardName;
    }

    public string GetNameText()
    {
        return cardNameText;
    }

    public string GetDetailText()
    {
        return cardDetailText;
    }

    public int GetEffectType()
    {
        return cardEffectType;
    }
}