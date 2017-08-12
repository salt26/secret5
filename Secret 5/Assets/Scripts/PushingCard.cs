using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PushingCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Vector3 Cardx;
    Vector3 CardOriginal;

    public void OnBeginDrag(PointerEventData eventData)
    {
        Cardx.x = this.transform.position.x;

        CardOriginal = this.transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Cardx.y = eventData.position.y;
        this.transform.SetPositionAndRotation(Cardx, this.transform.rotation);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //ToDo 속도 or 위로 어느정도 가면 카드가 교환되도록 만들 것
        this.transform.SetPositionAndRotation(CardOriginal, this.transform.rotation);
    }
}
