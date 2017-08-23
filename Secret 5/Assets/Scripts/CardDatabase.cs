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
        ci = new CardInfo("Attack", "공격", "이 카드를 받을 때 피해를 1 받습니다.");
        cardInfo.Add(ci);

        // 치유 카드
        ci = new CardInfo("Heal", "치유", "이 카드를 받을 때 체력을 1 회복합니다. 이 카드로 체력이 6을 넘을 수 없습니다.");
        cardInfo.Add(ci);

        // 폭탄 카드
        ci = new CardInfo("Bomb", "폭탄", "이 카드를 들고 있는 동안 내 턴이 끝날 때마다 피해를 1 받습니다.");
        cardInfo.Add(ci);

        // 회피 카드
        ci = new CardInfo("Avoid", "회피", "이 카드를 내면 교환 상대에게서 받는 카드의 받을 때 효과를 무효화합니다. 카드는 그대로 교환합니다.");
        cardInfo.Add(ci);

        // 속임 카드
        ci = new CardInfo("Deceive", "속임", "이 카드를 내면 교환 상대에게서 받는 카드를 확인하지 않고 돌려줍니다. 대신 교환 상대가 들고 있던 카드를 가져오며, 가져온 카드의 받을 때 효과를 받습니다.");
        cardInfo.Add(ci);

        // 빙결 카드
        ci = new CardInfo("Freeze", "빙결", "이 카드를 받을 때 다음 한 번의 내 턴이 교환 없이 끝납니다. 이 카드의 효과를 받은 사실은 다음 내 턴이 시작될 때 모두에게 공개됩니다.");
        cardInfo.Add(ci);
    }

    /*
    private void Attack(PlayerControl playedPlayer, PlayerControl receivedPlayer)
    {
        // TODO
        // 카드에 효과가 달려있으면 안될 것 같은데?
        // 교환을 처리하는 하나의 함수에서 양쪽의 카드를 보면서 일괄 처리해야 할듯.
        // 그러고 나서 이 카드의 효과를 발동시켜야 한다고 판단되면 이 함수를 호출하게 하자.
    }
    */

    /// <summary>
    /// 인자로 주어진 카드의 영어 이름이 데이터베이스에 존재하는지 확인하는 함수입니다.
    /// 존재하면 true를 반환합니다.
    /// </summary>
    /// <param name="cardName">카드의 영어 이름</param>
    /// <returns></returns>
    public bool VerifyCard(string cardName)
    {
        foreach (CardInfo card in cardInfo)
        {
            if (card.GetName() == cardName) return true;
        }
        return false;
    }

    public bool VerifyCard(Card card)
    {
        if (card == null) return false;
        foreach (CardInfo ci in cardInfo)
        {
            if (ci.GetName() == card.GetCardName()) return true;
        }
        return false;
    }

    /// <summary>
    /// 인자로 주어진 카드의 영어 이름으로 그 카드의 정보를 열람하는 함수입니다.
    /// </summary>
    /// <param name="cardName">카드의 영어 이름</param>
    /// <returns></returns>
    public CardInfo GetCardInfo(string cardName)
    {
        foreach (CardInfo card in cardInfo)
        {
            if (card.GetName() == cardName) return card;
        }
        return null;
    }

    /// <summary>
    /// 인자로 주어진 카드의 정보를 열람하는 함수입니다.
    /// </summary>
    /// <param name="card">카드</param>
    /// <returns></returns>
    public CardInfo GetCardInfo(Card card)
    {
        if (card == null) return null;
        return GetCardInfo(card.GetCardName());
    }
}

public class CardInfo
{
    private string cardName;        // 카드의 영어 이름(내부적으로 사용하는 이름)
    private string cardNameText;    // 카드의 한글 이름
    private string cardDetailText;  // 카드의 효과 설명 텍스트
    /*
    private int cardEffectType;     // 0: 받을 때 효과, 1: 낼 때 효과, 2: 내 턴이 끝날 때 효과
    public delegate void PlayCard(PlayerControl playedPlayer, PlayerControl receivedPlayer); 
                                    // 카드의 효과를 수행하는 함수 대리자
    public PlayCard Play;           // 외부에서 카드의 효과를 수행할 때 ci.Play(cardName); 과 같은 방법으로 호출하면 된다.
    */

    public CardInfo(string name, string nameText, string detailText/*, int effectType*/)
    {
        cardName = name;
        cardNameText = nameText;
        cardDetailText = detailText;
        //cardEffectType = effectType;
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

    /*
    public int GetEffectType()
    {
        return cardEffectType;
    }
    */
}