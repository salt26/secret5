using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PushingCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private static BattleManager bm;

    private Vector3 cardx;
    private Vector3 cardOriginal;

    private Card cardL;
    private Card cardR;

    private Card selectedCard;

    private bool isDrag = false;

    [SerializeField] bool SelectComplete;
    [SerializeField] bool ExchangeComplete;


    public void Start()
    {
        bm = GameObject.Find("BattleManager").GetComponent<BattleManager>();
        SelectComplete = false;
        ExchangeComplete = false;
        cardOriginal = this.transform.position;

    }


    public void FixedUpdate()
    {
        switch (bm.GetCameraPlayer().GetPlayerNum())
        {
            case 1:
                cardR = bm.GetCardsInHand()[0].GetComponent<Card>();
                cardL = bm.GetCardsInHand()[1].GetComponent<Card>();
                break;
            case 2:
                cardR = bm.GetCardsInHand()[2].GetComponent<Card>();
                cardL = bm.GetCardsInHand()[3].GetComponent<Card>();
                break;
            case 3:
                cardR = bm.GetCardsInHand()[4].GetComponent<Card>();
                cardL = bm.GetCardsInHand()[5].GetComponent<Card>();
                break;
            case 4:
                cardR = bm.GetCardsInHand()[6].GetComponent<Card>();
                cardL = bm.GetCardsInHand()[7].GetComponent<Card>();
                break;
            case 5:
                cardR = bm.GetCardsInHand()[8].GetComponent<Card>();
                cardL = bm.GetCardsInHand()[9].GetComponent<Card>();
                break;
        }//cameraNumber를 가져와서 카메라에 대응되는 카드를 들고 있도록 만드는 코드입니다.

        if (this.CompareTag("Left"))
            CardAvailability(cardL);
        else if (this.CompareTag("Right"))
            CardAvailability(cardR);
        else
            Debug.Log("Wrong Tag At CardPanel");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDrag = true;
        cardx.x = this.transform.position.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if((bm.GetCameraPlayer()==bm.GetTurnPlayer() || bm.GetCameraPlayer() == bm.GetObjectPlayer())&& bm.GetTurnStep() == 3)
        {
            if (bm.GetPlayerSelectedCard(bm.GetCameraPlayer()) == null)
            {
                cardx.y = eventData.position.y;
                if (cardx.y > cardOriginal.y)
                {
                    this.transform.SetPositionAndRotation(cardx, this.transform.rotation);
                }

            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDrag = false;
        if (this.transform.position.y >= 550)
        {
            if (this.CompareTag("Left"))
            {
                selectedCard = cardL;
                Debug.Log("selected Card is " + selectedCard.name + "= Left");
                SelectComplete = true;
                cardL.SetCardAvaliable(false);
            }
            else if (this.CompareTag("Right"))
            {
                selectedCard = cardR;
                Debug.Log("selected Card is " + selectedCard.name + "= Right");
                SelectComplete = true;
                cardR.SetCardAvaliable(false);
            }
            else
            {
                selectedCard = null;
                Debug.Log("Card is not appropriate");
            }

            if(selectedCard != null)
            {
                bm.SetCardToPlay(selectedCard, bm.GetCameraPlayer());
            }
        }
    }

    private void CardAvailability(Card card)
    {
        if (card.GetCardAvaliable() == true)
        {
            if(!isDrag)
            {
                this.transform.SetPositionAndRotation(cardOriginal, this.transform.rotation);
                ExchangeComplete = false;

            }
        }

        if (card.GetCardAvaliable() == false)
        {
            this.transform.SetPositionAndRotation(new Vector3(this.transform.position.x, 1000, 0), this.transform.rotation);
            {
                if (ExchangeComplete == true)
                {
                    this.transform.SetPositionAndRotation(cardOriginal , this.transform.rotation);
                    SelectComplete = false;
                    selectedCard = null;
                    for(int i = 0; i<10; i++)
                    {
                        bm.GetCardsInHand()[i].GetComponent<Card>().SetCardAvaliable(true);
                    }
                }
            }
        }
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
}
