using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Pusher : MonoBehaviour{

    private static BattleManager bm;

    private Vector3 cardOriginal;

    private Image[] cardUI;
    private Image cardUIL;
    private Image cardUIR;

    private SelectedInfo si;
    private Card cardL;
    private Card cardR;
    [SerializeField] private Card selectedCard;


    [SerializeField] bool ExchangeComplete;
    bool changingCard;

    bool moved;

    private Queue<IEnumerator> process = new Queue<IEnumerator>();

    private void Start()
    {
        bm = GameObject.Find("BattleManager").GetComponent<BattleManager>();
        changingCard = true;
        cardUI = GetComponentsInChildren<Image>();
        cardUIL = cardUI[1];
        cardUIR = cardUI[2];
        moved = false;
    }

    private void FixedUpdate()
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

        if (changingCard == true)
        {
            si = null;
            selectedCard = null;
            moved = false;
            cardUIL.GetComponent<Image>().sprite = cardL.GetComponentInChildren<Finder>().GetComponent<SpriteRenderer>().sprite;
            cardUIR.GetComponent<Image>().sprite = cardR.GetComponentInChildren<Finder>().GetComponent<SpriteRenderer>().sprite;
            changingCard = false;
        }//큰 카드와 작은 카드가 같은 스프라이트를 가지게 하는 코드입니다.

        if(!(si == null))
        {
            if(moved == false)
            {
                moved = true;
                Debug.Log("1");
                StartCoroutine(process.Dequeue());//위로 올라가게 함
            }

            selectedCard = si.GetCard();
            bm.GetCameraPlayer().DecideClicked();
            bm.SetCardToPlay(selectedCard, bm.GetCameraPlayer());

            if(!(bm.GetPlayerSelectedCard(bm.GetTurnPlayer()) == null || bm.GetPlayerSelectedCard(bm.GetObjectPlayer()) == null))
            {
                //process.Enqueue(MovingCardUp(this.transform.position));
                //StartCoroutine(process.Dequeue());
            }
            //끝날때 해야 될것
            //si = null;
            //moved = false;
        }

        Highlighting();

        AfterSmallMove();
    }

    public void AfterSmallMove()
    {
        if (ExchangeComplete == true)
        {
            changingCard = true;
            
            MoveCardDown(GameObject.FindGameObjectWithTag(si.GetLR()).transform.position, si.GetOriginalPosition(), si.GetLR());
            //if (bm.GetCameraPlayer() == bm.GetObjectPlayer() && bm.GetPlayerSelectedCard(bm.GetTurnPlayer()).GetCardName() == "Deceive")
            //속임 고쳐야됨


            StartCoroutine(process.Dequeue());//아래로 내려가게 함
            //Todo 카드의 이펙트
            si = null;
            selectedCard = null;
            for (int i = 0; i < 10; i++)
            {
                bm.GetCardsInHand()[i].GetComponent<Card>().SetCardAvaliable(true);
            }
            ExchangeComplete = false;
        }

    }
    
    private void Highlighting()
    {
        if (bm.GetTurnStep() == 3 && bm.GetTurnPlayer() == bm.GetCameraPlayer() && !(selectedCard == null))
        {
                selectedCard.SetHighLight(true);
        }
        else
            for (int i = 0; i < 10; i++)
            {
                bm.GetCardsInHand()[i].GetComponent<Card>().SetHighLight(false);
            }
    }

    public void MoveCardUp(Vector3 start, Vector3 dest, string LR)
    {
        // 함수를 Queue에 넣어 처리합니다. (Queue는 줄 세우기와 비슷한 개념입니다.)
        process.Enqueue(MovingCardUp(start, dest, LR));
    }

    IEnumerator MovingCardUp(Vector3 CardPosition, Vector3 det, string LR)
    {
        float s = Screen.height;
        float x = CardPosition.y;

        float t = Time.time;
        while ((Time.time - t) < ((2f / 3f) * (s * 3 / 2 - x) / (s * 11 / 16)))
        {
            GameObject.FindGameObjectWithTag(LR).transform.position = Vector3.Lerp(CardPosition, det, (Time.time - t) / ((2f / 3f) * (s * 3 / 2 - x) / (s * 11 / 16)));
            yield return null;
        }
        GameObject.FindGameObjectWithTag(LR).transform.position = det;
    }

    public void MoveCardDown(Vector3 start, Vector3 dest, string LF)
    {
        process.Enqueue(MovingCardDown(start, dest, LF));
    }
    
    IEnumerator MovingCardDown(Vector3 CardPosition, Vector3 det, string LR)
    {
        float t = Time.time;
        while (Time.time - t < 3f / 3f)
        {
            GameObject.FindGameObjectWithTag(LR).transform.position = Vector3.Lerp(CardPosition, det, (Time.time - t) / (3f / 3f));
            yield return null;
        }
        GameObject.FindGameObjectWithTag(LR).transform.position = det;
    }

    public void MoveDeceive(Vector3 dCardPosition, Vector3 ddet, string LR)
    {
        process.Enqueue(Deceiver(dCardPosition, ddet, LR));
    }

    IEnumerator Deceiver(Vector3 DCardPosition, Vector3 Ddet, string LR)
    {
        float t = Time.time;
        string RL = "a";

        if (LR == "Left")
            RL = "Right";
        else if (LR == "Right")
            RL = "Left";
        else
            Debug.Log("Error In Deceive Card Process");

        Vector3 OCardPosition = GameObject.FindGameObjectWithTag(RL).transform.position;
        Vector3 Odet = new Vector3(OCardPosition.x, Screen.height * 3 / 2);

        while (Time.time - t < 3f / 3f)
        {
            GameObject.FindGameObjectWithTag(LR).transform.position = Vector3.Lerp(DCardPosition, Ddet, (Time.time - t) / (3f / 3f));
            GameObject.FindGameObjectWithTag(RL).transform.position = Vector3.Lerp(OCardPosition, Odet, (Time.time - t) / (3f / 3f));
            yield return null;
        }
        GameObject.FindGameObjectWithTag(LR).transform.position = Ddet;
        GameObject.FindGameObjectWithTag(RL).transform.position = Odet;
    }

    public void SetCardChange()
    {
        changingCard = true;
    }

    public void SetExchangeComplete()
    {
        ExchangeComplete = true;
    }

    public void SetSelectedCard(SelectedInfo card)
    {
        si = card;
    }
}