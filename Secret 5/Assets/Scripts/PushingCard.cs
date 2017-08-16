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
                cardL = bm.GetCardsInHand()[0].GetComponent<Card>();
                cardR = bm.GetCardsInHand()[1].GetComponent<Card>();
                break;
            case 2:
                cardL = bm.GetCardsInHand()[2].GetComponent<Card>();
                cardR = bm.GetCardsInHand()[3].GetComponent<Card>();
                break;
            case 3:
                cardL = bm.GetCardsInHand()[4].GetComponent<Card>();
                cardR = bm.GetCardsInHand()[5].GetComponent<Card>();
                break;
            case 4:
                cardL = bm.GetCardsInHand()[6].GetComponent<Card>();
                cardR = bm.GetCardsInHand()[7].GetComponent<Card>();
                break;
            case 5:
                cardL = bm.GetCardsInHand()[8].GetComponent<Card>();
                cardR = bm.GetCardsInHand()[9].GetComponent<Card>();
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
            }
        }

        if (card.GetCardAvaliable() == false)
        {
            this.transform.SetPositionAndRotation(new Vector3(this.transform.position.x, 1000, 0), this.transform.rotation);
        }

        Exchanged();
    }

    private void Exchanged()
    {
        if (ExchangeComplete == true)
        {
            SelectComplete = false;
            selectedCard = null;
            for (int i = 0; i < 10; i++)
            {
                bm.GetCardsInHand()[i].GetComponent<Card>().SetCardAvaliable(true);
            }
            ExchangeComplete = false;
        }
        
    }

    private Card Deceive(PlayerController DeceivingPlayer, PlayerController DeceivedPlayer)
    {
        if (ExchangeComplete == true) return null;
        if (DeceivingPlayer == null || DeceivedPlayer == null) return null;
        else if (bm.GetPlayerSelectedCard(DeceivingPlayer) == null) return null;
        else if (bm.GetPlayerSelectedCard(DeceivingPlayer).GetCardName() == "Deceive")
        {
            List<Card> DeceivedHand = bm.GetPlayerHand(DeceivedPlayer);
            bm.GetPlayerSelectedCard(DeceivedPlayer).SetCardAvaliable(true);
            if (DeceivedHand == null || DeceivedHand.Count != 2) return null;
            else if (DeceivedHand[0].Equals(bm.GetPlayerSelectedCard(DeceivedPlayer))) return DeceivedHand[1];
            else if (DeceivedHand[1].Equals(bm.GetPlayerSelectedCard(DeceivedPlayer))) return DeceivedHand[0];
            else return null;
        }
        else return null;
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
