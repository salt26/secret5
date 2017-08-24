using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Pusher : MonoBehaviour{

    private static BattleManager bm;

    private Vector3 cardOriginal;

    [SerializeField] private Image[] cardUI;
    [SerializeField] private Image cardUIL;
    [SerializeField] private Image cardUIR;

    private SelectedInfo selectedCardInfo;
    private Card cardL;
    private Card cardR;
    [SerializeField] private Card selectedCard;
    //private Exchange exchange;
    private int opponentPlayerCardCode = -1;

    static public PlayerControl localPlayer = null;

    [SerializeField] bool ExchangeComplete;
    [SerializeField] bool changingCard = true;

    [SerializeField] bool moved;
    bool selected;

    private Queue<IEnumerator> process = new Queue<IEnumerator>();

    private void Awake()
    {
        bm = BattleManager.bm;
    }

    private void Start()
    {
        changingCard = true;
        cardUI = GetComponentsInChildren<Image>();
        cardUIL = cardUI[1];
        cardUIR = cardUI[2];
        moved = false;
        selected = false;
        opponentPlayerCardCode = -1;
    }

    private void FixedUpdate()
    {
        if (localPlayer == null)
        {
            //LogDisplay.AddText("Pusher: localPlayer is null.");
            changingCard = true;
            moved = false;
            selected = false;
            opponentPlayerCardCode = -1;
            return;
        }
        else if (bm == null)
        {
            bm = BattleManager.bm;
            return;
        }
        else if (bm.GetTurnStep() <= 0)
        {
            //LogDisplay.AddText("Pusher: localPlayer is " + localPlayer.GetName() + ", bm turnStep is " + bm.GetTurnStep() + ".");
            changingCard = true;
            moved = false;
            selected = false;
            opponentPlayerCardCode = -1;
            return;
        }
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
        } //cameraNumber를 가져와서 카메라에 대응되는 카드를 들고 있도록 만드는 코드입니다.

        if (changingCard == true)
        {
            /*
            Debug.Log("Pusher: changingCard is true.");
            LogDisplay.AddText("Pusher: changingCard is true.");
            */
            cardUIL.GetComponent<Image>().sprite = cardL.GetComponentInChildren<Finder>().GetComponent<SpriteRenderer>().sprite;
            cardUIR.GetComponent<Image>().sprite = cardR.GetComponentInChildren<Finder>().GetComponent<SpriteRenderer>().sprite;
            changingCard = false;
        } //큰 카드와 작은 카드가 같은 스프라이트를 가지게 하는 코드입니다.

        if (selectedCardInfo != null && !selected)
        {
            selected = true;
            if (moved == false)
            {
                moved = true;
                StartCoroutine(process.Dequeue());//위로 올라가게 함
            }

            selectedCard = selectedCardInfo.GetCard();
            localPlayer.DecideClicked();
            localPlayer.CmdSetCardToPlay(selectedCard.GetCardCode(), localPlayer.GetPlayerIndex());

            Highlighting();
            LogDisplay.AddText("AfterSmallMove");
            StartCoroutine("AfterSmallMove");
        }
    }

    public IEnumerator AfterSmallMove()
    {
        while (!ExchangeComplete)
        {
            //Debug.Log("ExchangeComplete is " + ExchangeComplete + ".");
            yield return null;
        }
        LogDisplay.AddText("ExchangeComplete is " + ExchangeComplete + ".");
        while (opponentPlayerCardCode == -1) {
            yield return null;
        }
        LogDisplay.AddText("opponentPlayerCardCode is " + opponentPlayerCardCode + ".");
        if (selectedCardInfo != null)
        {
            if (opponentPlayerCardCode == 3)
                MoveDeceive(GameObject.FindGameObjectWithTag(selectedCardInfo.GetLR()).transform.position, selectedCardInfo.GetOriginalPosition(), selectedCardInfo.GetLR());
            else
                MoveCardDown(GameObject.FindGameObjectWithTag(selectedCardInfo.GetLR()).transform.position, selectedCardInfo.GetOriginalPosition(), selectedCardInfo.GetLR());
            //if (bm.GetCameraPlayer() == bm.GetObjectPlayer() && bm.GetPlayerSelectedCard(bm.GetTurnPlayer()).GetCardName() == "Deceive")
            // TODO 속임 고쳐야됨


            StartCoroutine(process.Dequeue()); // 아래로 내려가게 함
            // TODO 카드의 이펙트
            selectedCardInfo = null;
            selectedCard = null;
            moved = false;
            selected = false;
            ExchangeComplete = false;
            opponentPlayerCardCode = -1;
        }
        else if (selectedCardInfo == null)
        {
            LogDisplay.AddText("selectedCardInfo is null.");
        }

    }
    
    private void Highlighting()
    {
        if (bm.GetTurnStep() == 3 && bm.GetTurnPlayer() == localPlayer && selectedCard != null)
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
        changingCard = true;
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

    IEnumerator Deceiver(Vector3 SelectedCardPosition, Vector3 SelectedCarddet, string LR)
    {
        float t = Time.time;
        string RL = "a";

        if (LR == "Left")
            RL = "Right";
        else if (LR == "Right")
            RL = "Left";
        else
            Debug.Log("Error In Deceive Card Process");

        Vector3 DCardPosition = GameObject.FindGameObjectWithTag(RL).transform.position;
        Vector3 DCarddet = new Vector3(DCardPosition.x, Screen.height * 3 / 2);
        
        while (Time.time - t < 3f / 3f)
        {
            GameObject.FindGameObjectWithTag(LR).transform.position = Vector3.Lerp(SelectedCardPosition, SelectedCarddet, (Time.time - t) / (3f / 3f));
            GameObject.FindGameObjectWithTag(RL).transform.position = Vector3.Lerp(DCardPosition, DCarddet, (Time.time - t) / (3f / 3f));
            yield return null;
        }
        GameObject.FindGameObjectWithTag(LR).transform.position = SelectedCarddet;
        GameObject.FindGameObjectWithTag(RL).transform.position = DCarddet;

        t = Time.time;
        changingCard = true;
        while (Time.time - t < 3f / 3f)
        {
            GameObject.FindGameObjectWithTag(RL).transform.position = Vector3.Lerp(DCarddet, DCardPosition, (Time.time - t) / (3f / 3f));
            yield return null;
        }
        GameObject.FindGameObjectWithTag(RL).transform.position = DCardPosition;

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
        selectedCardInfo = card;
    }

    public void SetOpponentCard(int TP, int OP, int TPCardCode, int OPCardCode)
    {
        if (TPCardCode < 0 || TPCardCode >= 10 || OPCardCode < 0 || OPCardCode >= 10) return;
        if (TP < 0 || TP >= 5 || OP < 0 || OP >= 5) return;
        if (localPlayer == null) return;

        if (localPlayer.GetPlayerIndex() == TP)
        {
            opponentPlayerCardCode = OPCardCode;
        }
        else if (localPlayer.GetPlayerIndex() == OP)
        {
            opponentPlayerCardCode = TPCardCode;
        }
    }
}