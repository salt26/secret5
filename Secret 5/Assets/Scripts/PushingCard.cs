using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PushingCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private static BattleManager bm;

    Vector3 cardx;
    Vector3 cardOriginal;

    Card cardL;
    Card cardR;

    Card selectedCard;

    bool CardAvailable;
    bool SelectComplete;
    bool ExchangeComplete;


    public void Start()
    {
        bm = GameObject.Find("BattleManager").GetComponent<BattleManager>();
        SelectComplete = false;
        ExchangeComplete = false;
        CardAvailable = true;

        cardL = bm.GetCardsInHand()[0].GetComponent<Card>();
        cardR = bm.GetCardsInHand()[1].GetComponent<Card>();
        //이렇게 하면 안될거같긴한데...일단은 이렇게 둔다
    }

    public void Update()
    {
        if (CardAvailable == true)
        {
            this.gameObject.SetActive(true);
        }

        if(CardAvailable == false)
        {
            this.gameObject.SetActive(false);
            {
                if (ExchangeComplete == true)
                {
                    cardL = bm.GetCardsInHand()[0].GetComponent<Card>();
                    cardR = bm.GetCardsInHand()[1].GetComponent<Card>();
                    CardAvailable = true;
                    ExchangeComplete = false;
                    SelectComplete = false;
                }
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

        cardx.x = this.transform.position.x;
        cardOriginal = this.transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        cardx.y = eventData.position.y;
        if (cardx.y > cardOriginal.y)
        {
            this.transform.SetPositionAndRotation(cardx, this.transform.rotation);
        }
        // TODO 교환을 할 수 있을 때만 이 동작을 수행하도록 할 것
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (this.transform.position.y >= 550)
        {
            if (this.CompareTag("Left"))
            {
                selectedCard = cardL;
                Debug.Log("selected Card is " + selectedCard.name + "= Left");
                SelectComplete = true;
                CardAvailable = false;
            }
            else if (this.CompareTag("Right"))
            {
                selectedCard = cardR;
                Debug.Log("selected Card is " + selectedCard.name + "= Right");
                SelectComplete = true;
                CardAvailable = false;
            }
            else
            {
                Debug.Log("Card is not appropriate");
            }
        }

        this.transform.SetPositionAndRotation(cardOriginal, this.transform.rotation);
    }

    public Card GetSelectedCard()
    {
        if(SelectComplete == true)
        {
            return selectedCard;
        }
        else
        {
            return null;
        }
    }
    
    public void SetExchangeComplete()
    {
        ExchangeComplete = true;
    }

    /* 쓰는 법
     * 먼저 GetSelectedCard()로 어떤 카드를 선택했는지 가져온다
     * -> 두 플레이어 모두 선택한 것이 확인되면 SetExchangeComplete()로 ExchangeComplete를 true로 만든다
     * (아마도) 알아서 교환이 된 것이 UI에 나온다.
     */
}
