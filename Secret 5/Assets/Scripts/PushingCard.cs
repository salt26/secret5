using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PushingCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //public Image cardLHighlight;
    //public Image cardRHighlight;

    private static BattleManager bm;

    private Vector3 cardx;
    private Vector3 cardOriginal;

    private Card cardL;
    private Card cardR;

    [SerializeField] private Card selectedCard;

    private bool isDrag;

    [SerializeField] bool ExchangeComplete;
    [SerializeField] bool Checker = false;


    public void Start()
    {
        bm = BattleManager.bm;
        ExchangeComplete = false;
        cardOriginal = this.transform.position;
        isDrag = false;
        cardOriginal = transform.position;

    }


    public void FixedUpdate()
    {
        switch (bm.GetCameraPlayer().GetPlayerNum())    // 현재 GetPlayerNum() 이 항상 0을 준다!
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

        if (CompareTag("Left"))
            CardAvailability(cardL);
        else if (CompareTag("Right"))
            CardAvailability(cardR);
        else
            Debug.Log("Wrong Tag At CardPanel");
            if (CompareTag("Left"))
                CardAvailability(cardL);
            else if (CompareTag("Right"))
                CardAvailability(cardR);
            else
                Debug.Log("Wrong Tag At CardPanel");

        Highlighting();

        //Deceive 처리는 Exchange쪽으로 넘김 애니메이션 짜기가 힘들어지면 >>>>>>>>>>> 표시한 부분을 지우고 어떻게든 해볼것
        
        //cardLHighlight.gameObject.SetActive(!cardL.GetCardAvaliable());
        //cardRHighlight.gameObject.SetActive(!cardR.GetCardAvaliable());
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDrag = true;
        cardx.x = transform.position.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if ((bm.GetCameraPlayer().Equals(bm.GetTurnPlayer()) && bm.GetCameraPlayer().GetObjectTarget() != null && bm.GetTurnStep() == 2)
            || (bm.GetCameraPlayer().Equals(bm.GetObjectPlayer()) && bm.GetTurnStep() == 3))
        {
            if (bm.GetPlayerSelectedCard(bm.GetCameraPlayer()) == null)
            {
                cardx.y = eventData.position.y;
                if (cardx.y > cardOriginal.y)
                {
                    transform.SetPositionAndRotation(cardx, transform.rotation);
                }
            }
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        isDrag = false;
        if (this.transform.position.y >= Screen.height*4/5)
        if (transform.position.y >= 550)    // TODO 해상도 비례로 만들 것!
        {
            if (CompareTag("Left"))
            {
                selectedCard = cardL;
                Debug.Log("selected Card is " + selectedCard.name + "= Left");
                cardL.SetCardAvaliable(false);
            }
            else if (CompareTag("Right"))
            {
                selectedCard = cardR;
                Debug.Log("selected Card is " + selectedCard.name + "= Right");
                cardR.SetCardAvaliable(false);
            }
            else
            {
                selectedCard = null;
                Debug.Log("Card is not appropriate");
            }

            if(selectedCard != null)
            {
                bm.GetCameraPlayer().DecideClicked();
                bm.SetCardToPlay(selectedCard, bm.GetCameraPlayer());
            }
        }
    }

    private void CardAvailability(Card card)
    {
        if (card == null) return;
        if (card.GetCardAvaliable() == true)
        {
            if(!isDrag)
            {
                transform.SetPositionAndRotation(cardOriginal, this.transform.rotation);
            }
        }

        if (card.GetCardAvaliable() == false)
        {
            this.transform.SetPositionAndRotation(new Vector3(this.transform.position.x, 10000, 0), this.transform.rotation);//수치 바꿔야 됨
        }

        Exchanged();
    }

    private void Exchanged()
    {
        if (ExchangeComplete == true)
        {
            selectedCard = null;
            for (int i = 0; i < 10; i++)
            {
                bm.GetCardsInHand()[i].GetComponent<Card>().SetCardAvaliable(true);
            }
            ExchangeComplete = false;
        }
    }

    public Card GetSelectedCard()
    {
        return selectedCard;
    }

    private void Highlighting()
    {
        if(bm.GetTurnStep() == 3 && bm.GetTurnPlayer() == bm.GetCameraPlayer())
        {
            if (selectedCard == cardL && this.CompareTag("Left"))
                selectedCard.SetHighLight(true);
            else if (selectedCard == cardR && this.CompareTag("Right"))
                selectedCard.SetHighLight(true);
        }
        else
            for (int i = 0; i < 10; i++)
            {
                bm.GetCardsInHand()[i].GetComponent<Card>().SetHighLight(false);
            }
    }


    public void SetExchangeComplete()
    {
        ExchangeComplete = true;
    }
}
