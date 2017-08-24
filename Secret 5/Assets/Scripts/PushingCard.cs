using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PushingCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private static BattleManager bm;

    private Pusher pusher;

    static public PlayerControl localPlayer = null;

    private Vector3 cardx;
    private Vector3 cardOriginal;

    private Card cardL;
    private Card cardR;

    private void Awake()
    {
        bm = BattleManager.bm;
    }

    private void Start()
    {
        pusher = GetComponentInParent<Pusher>();
        cardOriginal = transform.position;
    }

    private void FixedUpdate()
    {
        if (localPlayer == null)
        {
            return;
        }
        else if (bm == null)
        {
            bm = BattleManager.bm;
            return;
        }
        else if (bm.GetTurnStep() <= 0)
        {
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
        }
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
                //Debug.Log("Card is not appropriate.");
                //LogDisplay.AddText("Card is not appropriate.");
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
