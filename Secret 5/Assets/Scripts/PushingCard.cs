using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PushingCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private BattleManager bm;

    private Pusher pusher;

    static public PlayerControl localPlayer = null;

    private Vector3 cardx;
    private Vector3 cardOriginal;

    private Card cardL;
    private Card cardR;

    private void Start()
    {
        bm = BattleManager.bm;
        pusher = GetComponentInParent<Pusher>();
        cardOriginal = transform.position;
    }

    private void FixedUpdate()
    {
        if (localPlayer == null)
        {
            Debug.Log("localPlayer is null.");
            return;
        }
        else Debug.Log("PushingCard localPlayer is " + localPlayer.GetName() + ".");
        switch (localPlayer.GetPlayerNum())
        {
            case 1:
                cardL = bm.GetCardInPosition(0);
                cardR = bm.GetCardInPosition(1);
                break;
            case 2:
                cardL = bm.GetCardInPosition(2);
                cardR = bm.GetCardInPosition(3);
                break;
            case 3:
                cardL = bm.GetCardInPosition(4);
                cardR = bm.GetCardInPosition(5);
                break;
            case 4:
                cardL = bm.GetCardInPosition(6);
                cardR = bm.GetCardInPosition(7);
                break;
            case 5:
                cardL = bm.GetCardInPosition(8);
                cardR = bm.GetCardInPosition(9);
                break;
        }//cameraNumber를 가져와서 카메라에 대응되는 카드를 들고 있도록 만드는 코드입니다.
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        cardx.x = transform.position.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if ((localPlayer.Equals(bm.GetTurnPlayer()) && localPlayer.GetObjectTarget() != null && bm.GetTurnStep() == 2)
            || (localPlayer.Equals(bm.GetObjectPlayer()) && bm.GetTurnStep() == 3))
        {
            if (bm.GetPlayerSelectedCard(localPlayer) == null)
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
        if (this.transform.position.y >= Screen.height * 13 / 16)
        {
            if (CompareTag("Left"))
            {
                pusher.MoveCardUp(cardx, new Vector3(cardOriginal.x, Screen.height * 3 / 2), tag);
                pusher.SetSelectedCard(new SelectedInfo(cardL, tag, cardOriginal));
            }
            else if (CompareTag("Right"))
            {
                pusher.MoveCardUp(cardx, new Vector3(cardOriginal.x, Screen.height * 3 / 2), tag);
                pusher.SetSelectedCard(new SelectedInfo(cardR, tag, cardOriginal));
            }
            else
            {
                Debug.Log("Card is not appropriate");
            }
        }
        else
        {
            transform.position = cardOriginal;
        }
    }
}

public class SelectedInfo
{
    private Card card;
    private string LR;
    private Vector3 OriginalPosition;

    public SelectedInfo(Card cd, string a, Vector3 Original)
    {
        card = cd;
        LR = a;//왼쪽이면 Left, 오른쪽이면 Right
        OriginalPosition = Original;
    }

    public Card GetCard()
    {
        return card;
    }

    public string GetLR()
    {
        return LR;
    }
    
    public Vector3 GetOriginalPosition()
    {
        return OriginalPosition;
    }
}



/*{
    private static BattleManager bm;

    private Vector3 cardx;
    private Vector3 cardOriginal;

    private Card cardL;
    private Card cardR;

    static public PlayerControl localPlayer = null;

    [SerializeField] private Card selectedCard;

    private bool isDrag;

    [SerializeField] bool ExchangeComplete;
    bool changingCard;
    
    private Queue<IEnumerator> process = new Queue<IEnumerator>();
    private Queue<IEnumerator> processD = new Queue<IEnumerator>();


    public void Start()
    {
        bm = BattleManager.bm;
        ExchangeComplete = false;
        cardOriginal = transform.position;
        isDrag = false;
        changingCard = true;
    }


    public void FixedUpdate()
    {
        if (localPlayer == null)
        {
            Debug.Log("localPlayer is null.");
            return;
        }
        else Debug.Log("PushingCard localPlayer is " + localPlayer.GetName() + ".");
        switch (localPlayer.GetPlayerNum())
        {
            case 1:
                cardL = bm.GetCardInPosition(0);
                cardR = bm.GetCardInPosition(1);
                break;
            case 2:
                cardL = bm.GetCardInPosition(2);
                cardR = bm.GetCardInPosition(3);
                break;
            case 3:
                cardL = bm.GetCardInPosition(4);
                cardR = bm.GetCardInPosition(5);
                break;
            case 4:
                cardL = bm.GetCardInPosition(6);
                cardR = bm.GetCardInPosition(7);
                break;
            case 5:
                cardL = bm.GetCardInPosition(8);
                cardR = bm.GetCardInPosition(9);
                break;
        }//cameraNumber를 가져와서 카메라에 대응되는 카드를 들고 있도록 만드는 코드입니다.
        if(changingCard == true)
        {
            if (this.CompareTag("Left"))
            {
                CardAvailability(cardL);
                this.GetComponent<Image>().sprite = cardL.GetComponentInChildren<Finder>().GetComponent<SpriteRenderer>().sprite;
            }
            else if (this.CompareTag("Right"))
            {
                CardAvailability(cardR);
                this.GetComponent<Image>().sprite = cardR.GetComponentInChildren<Finder>().GetComponent<SpriteRenderer>().sprite;
            }
            else
                Debug.Log("Wrong Tag At CardPanel");
            changingCard = false;
            
        }
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
        if ((localPlayer.Equals(bm.GetTurnPlayer()) && localPlayer.GetObjectTarget() != null && bm.GetTurnStep() == 2)
            || (localPlayer.Equals(bm.GetObjectPlayer()) && bm.GetTurnStep() == 3))
        {
            if (bm.GetPlayerSelectedCard(localPlayer) == null)
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
        if (this.transform.position.y >= Screen.height*13/16)
        {
            if (CompareTag("Left"))
            {
                selectedCard = cardL;
                Debug.Log("selected Card is " + selectedCard.name + "= Left");

                process.Enqueue(MovingCardUp(this.transform.position));
                StartCoroutine(process.Dequeue());

                cardL.SetCardAvaliable(false);
            }
            else if (CompareTag("Right"))
            {
                selectedCard = cardR;
                Debug.Log("selected Card is " + selectedCard.name + "= Right");

                process.Enqueue(MovingCardUp(this.transform.position));
                StartCoroutine(process.Dequeue());

                cardR.SetCardAvaliable(false);
            }
            else
            {
                selectedCard = null;
                Debug.Log("Card is not appropriate");
            }

            if(selectedCard != null && localPlayer != null)
            {
                localPlayer.DecideClicked();
                localPlayer.CmdSetCardToPlay(selectedCard.GetCardCode(), localPlayer.GetPlayerIndex()); // 임시 주석
            }
        }
        isDrag = false;

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
            this.transform.SetPositionAndRotation(new Vector3(this.transform.position.x, Screen.height*3/2, 0), this.transform.rotation);//수치 바꿔야 됨
        }

        if (ExchangeComplete == true)
        {
            changingCard = true;
            
            if(!cardL.GetCardAvaliable() && CompareTag("Right"))
            {
                processD.Enqueue(MovingCardDown(new Vector3(this.transform.position.x, Screen.height * 3 / 2, 0)));
                StartCoroutine(processD.Dequeue());
            }
            else if (!cardR.GetCardAvaliable() && CompareTag("Left"))
            {
                processD.Enqueue(MovingCardDown(new Vector3(this.transform.position.x, Screen.height * 3 / 2, 0)));
                StartCoroutine(processD.Dequeue());
            }

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
        if(bm.GetTurnStep() == 3 && bm.GetTurnPlayer() == localPlayer)
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
    
    IEnumerator MovingCardUp (Vector3 CardPosition)
    {
        Vector3 det = new Vector3(this.transform.position.x, Screen.height * 3 / 2, 0);
        float s = Screen.height;
        float x = CardPosition.y;

        float t = Time.time;
        while ((Time.time - t) < ((2f / 3f) * (s * 3 / 2 - x) / (s * 11 / 16)))
        {
            this.transform.position = Vector3.Lerp(CardPosition, det, (Time.time - t) / ((2f / 3f) * (s * 3 / 2 - x) / (s * 11 / 16)));
            yield return null;
        }
        this.transform.position = det;    
    }
    
    IEnumerator MovingCardDown (Vector2 det)
    {
        float t = Time.time;
        while (Time.time  <  t + (2f / 3f))
        {
            this.transform.position = Vector3.Lerp(det, cardOriginal, (Time.time - t) / ((2f / 3f)));
            yield return null;
        }
        this.transform.position = cardOriginal;
    }

    public void SetExchangeComplete()
    {
        ExchangeComplete = true;
    }
    
    public void SetCardChange()
    {
        changingCard = true;
    }
}*/
