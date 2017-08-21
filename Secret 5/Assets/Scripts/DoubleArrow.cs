using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoubleArrow : MonoBehaviour {

    private BattleManager bm;

    public Image Arrow;

    private void Start()
    {
        bm = bm = GameObject.Find("BattleManager").GetComponent<BattleManager>();
    }

    private void FixedUpdate()
    {
        if (bm.GetTurnStep() == 3 || bm.GetObjectPlayer() != null)
        {
            DrawArrow(bm.GetTurnPlayer(), bm.GetObjectPlayer());
        }
        else
        {
            Arrow.rectTransform.localScale = new Vector3(0, 0, 0);
        }
    }
    
    private void DrawArrow(PlayerController first, PlayerController second)
    {
        Arrow.rectTransform.localScale = new Vector3(1, 1, 1);
        int num = first.GetPlayerNum() * 10 + second.GetPlayerNum();
        float Height = ArrowLength(num);
        Vector3 Position = (PlayerPosition(first.GetPlayerNum()) + PlayerPosition(second.GetPlayerNum()))/2;
        Quaternion Angular = Angle(num);

        Arrow.rectTransform.SetPositionAndRotation(Position, Angular);
        Arrow.rectTransform.sizeDelta = new Vector2(Arrow.rectTransform.sizeDelta.x, Height);
    }

    private float ArrowLength(int ab)
    {
        switch(ab)
        {
            case 12:
            case 15:
            case 21:
            case 23:
            case 32:
            case 34:
            case 43:
            case 45:
            case 51:
            case 54:
                return 1.3f;
            case 13:
            case 14:
            case 24:
            case 25:
            case 31:
            case 35:
            case 41:
            case 42:
            case 52:
            case 53:
                return 4.1f;
            default:
                return 10f;
        }
    }

    private Vector3 PlayerPosition(int a)
    {
        switch(a)
        {
            case 1:
                return new Vector3(0f, 0f, 0f);
            case 2:
                return new Vector3(3.236f, 0f, 2.351f);
            case 3:
                return new Vector3(2f, 0f, 6.155f);
            case 4:
                return new Vector3(-2f, 0f, 6.155f);
            case 5:
                return new Vector3(-3.236f, 0f, 2.351f);
            default:
                return new Vector3(0f, 0f, 0f);
        }
    }

    private Quaternion Angle(int ab)
    {
        switch(ab)
        {
            case 12:
            case 21:
            case 35:
            case 53:
                return Quaternion.Euler(90f, 0f, 126f);
            case 13:
            case 31:
            case 45:
            case 54:
                return Quaternion.Euler(90f, 0f, 162f);
            case 14:
            case 41:
            case 23:
            case 32:
                return Quaternion.Euler(90f, 0f, 18f);
            case 15:
            case 51:
            case 24:
            case 42:
                return Quaternion.Euler(90f, 0f, 54f);
            case 25:
            case 52:
            case 34:
            case 43:
                return Quaternion.Euler(90f, 0f, 90f);
            default:
                return Quaternion.Euler(0f, 0f, 0f);
        }
    }
}
