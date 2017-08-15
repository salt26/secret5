using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour {

    [SerializeField] private string cardName;
    private bool isMoving = false;

    public string GetCardName()
    {
        return cardName;
    }

    // TODO 교환 시 Flip(낼 카드를 뒷면으로) -> Move -> Flip(받은 카드를 앞면으로) 순서대로 진행되도록 하기

    /// <summary>
    /// 카드 교환 시 카드를 이동시키는 함수입니다.
    /// </summary>
    /// <param name="startDest">10 * (출발 인덱스) + (도착 인덱스)</param>
    public void MoveCard(int startDest)
    {
        if (startDest >= 100 || startDest < 0) return;
        int start = startDest / 10;
        int dest = startDest % 10;
        StartCoroutine(Move(start, dest));
    }

    /// <summary>
    /// 카드를 뒤집는 함수입니다.
    /// </summary>
    /// <param name="pos">카드 인덱스</param>
    public void FlipCard(int pos, bool toBack)
    {
        if (pos < 0 || pos >= 10) return;
        StartCoroutine(Flip(pos, toBack));
    }

    public void FlipCardImmediate(int pos, bool toBack)
    {
        StartCoroutine(FlipI(pos, toBack));
    }

    IEnumerator Move(int start, int dest)
    {
        yield return new WaitWhile(() => isMoving);
        isMoving = true;

        Vector3 sp = GetPosition(start);
        Vector3 dp = GetPosition(dest);
        Quaternion sr = GetRotationBack(start);
        Quaternion mr = GetRotationMoving(start, dest);
        Quaternion dr = GetRotationBack(dest);

        float t = Time.time;
        while (Time.time < t + (25f / 60f))
        {
            GetComponent<Transform>().rotation = Quaternion.Slerp(sr, mr, (Time.time - t) / (25f / 60f));
            yield return null;
        }
        GetComponent<Transform>().rotation = mr;
        
        t = Time.time;
        while (Time.time < t + (2f / 3f)) {
            GetComponent<Transform>().position = Vector3.Lerp(sp, dp, (Time.time - t) / (2f / 3f));
            yield return null;
        }
        GetComponent<Transform>().position = dp;
        
        t = Time.time;
        while (Time.time < t + (25f / 60f))
        {
            GetComponent<Transform>().rotation = Quaternion.Slerp(mr, dr, (Time.time - t) / (25f / 60f));
            yield return null;
        }
        GetComponent<Transform>().rotation = dr;

        isMoving = false;
        BattleManager bm = GameObject.Find("BattleManager").GetComponent<BattleManager>();
        bm.AfterExchange();
    }

    IEnumerator Flip(int pos, bool toBack)
    {
        yield return new WaitWhile(() => isMoving);
        isMoving = true;

        float t;
        Quaternion fr = GetRotationFront(pos);
        Quaternion br = GetRotationBack(pos);

        //if (GetComponent<Transform>().rotation.eulerAngles.x < 0) // 앞면일 때 뒷면으로
        if (toBack)
        {
            t = Time.time;
            while (Time.time < t + 1f)
            {
                GetComponent<Transform>().rotation = Quaternion.Slerp(fr, br, (Time.time - t) / 1f);
                yield return null;
            }
            GetComponent<Transform>().rotation = br;
        }
        else // 뒷면일 때 앞면으로
        {
            t = Time.time;
            while (Time.time < t + 1f)
            {
                GetComponent<Transform>().rotation = Quaternion.Slerp(br, fr, (Time.time - t) / 1f);
                yield return null;
            }
            GetComponent<Transform>().rotation = fr;
        }
        isMoving = false;
    }

    IEnumerator FlipI(int pos, bool toBack)
    {
        yield return new WaitWhile(() => isMoving);
        isMoving = true;
        
        if (toBack)
        {
            GetComponent<Transform>().rotation = GetRotationBack(pos);
        }
        else
        {
            GetComponent<Transform>().rotation = GetRotationFront(pos);
        }
        isMoving = false;
    }

    public static Vector3 GetPosition(int pos)
    {
        switch(pos)
        {
            case 0:
                return new Vector3(-0.35f, 0f, 1.2f);
            case 1:
                return new Vector3(0.35f, 0f, 1.2f);
            case 2:
                return new Vector3(1.986576f, 0f, 2.388951f);
            case 3:
                return new Vector3(2.202888f, 0f, 3.05469f);
            case 4:
                return new Vector3(1.577814f, 0f, 4.978455f);
            case 5:
                return new Vector3(1.011502f, 0f, 5.389904f);
            case 6:
                return new Vector3(-1.011502f, 0f, 5.389904f);
            case 7:
                return new Vector3(-1.577814f, 0f, 4.978455f);
            case 8:
                return new Vector3(-2.202888f, 0f, 3.05469f);
            case 9:
                return new Vector3(-1.986576f, 0f, 2.388951f);
            default:
                return new Vector3(0f, 0f, 3.4026f);
        }
    }

    public static Quaternion GetRotationBack(int pos)
    {
        switch(pos)
        {
            case 0:
            case 1:
                return Quaternion.Euler(90f, 90f, 90f);
            case 2:
            case 3:
                return Quaternion.Euler(90f, 18f, 90f);
            case 4:
            case 5:
                return Quaternion.Euler(90f, -54f, 90f);
            case 6:
            case 7:
                return Quaternion.Euler(90f, 234f, 90f);
            case 8:
            case 9:
                return Quaternion.Euler(90f, 162f, 90f);
            default:
                return Quaternion.identity;
        }
    }

    public static Quaternion GetRotationFront(int pos)
    {
        switch (pos)
        {
            case 0:
            case 1:
                return Quaternion.Euler(-90f, 90f, 90f);
            case 2:
            case 3:
                return Quaternion.Euler(-90f, 18f, 90f);
            case 4:
            case 5:
                return Quaternion.Euler(-90f, -54f, 90f);
            case 6:
            case 7:
                return Quaternion.Euler(-90f, 234f, 90f);
            case 8:
            case 9:
                return Quaternion.Euler(-90f, 162f, 90f);
            default:
                return Quaternion.identity;
        }
    }

    public static Quaternion GetRotationMoving(int start, int dest)
    {
        if (start < 0 || start >= 10 || dest < 0 || dest >= 10)
        {
            return Quaternion.identity;
        }
        else if (start / 2 == dest / 2)
        {
            return GetRotationFront(start); // TODO 셔플 시 앞면, 뒷면 어떻게 처리할지
        }

        if (start == 0 && dest == 2) return Quaternion.Euler(90f, 153f, 90f);
        else if (start == 0 && dest == 3) return Quaternion.Euler(90f, 144f, 90f);
        else if (start == 0 && dest == 4) return Quaternion.Euler(90f, 117f, 90f);
        else if (start == 0 && dest == 5) return Quaternion.Euler(90f, 108f, 90f);
        else if (start == 0 && dest == 6) return Quaternion.Euler(90f, 81f, 90f);
        else if (start == 0 && dest == 7) return Quaternion.Euler(90f, 72f, 90f);
        else if (start == 0 && dest == 8) return Quaternion.Euler(90f, 45f, 90f);
        else if (start == 0 && dest == 9) return Quaternion.Euler(90f, 36f, 90f);
        else if (start == 1 && dest == 2) return Quaternion.Euler(90f, 144f, 90f);
        else if (start == 1 && dest == 3) return Quaternion.Euler(90f, 135f, 90f);
        else if (start == 1 && dest == 4) return Quaternion.Euler(90f, 108f, 90f);
        else if (start == 1 && dest == 5) return Quaternion.Euler(90f, 99f, 90f);
        else if (start == 1 && dest == 6) return Quaternion.Euler(90f, 72f, 90f);
        else if (start == 1 && dest == 7) return Quaternion.Euler(90f, 63f, 90f);
        else if (start == 1 && dest == 8) return Quaternion.Euler(90f, 36f, 90f);
        else if (start == 1 && dest == 9) return Quaternion.Euler(90f, 27f, 90f);
        else if (start == 2 && dest == 4) return Quaternion.Euler(90f, 81f, 90f);
        else if (start == 2 && dest == 5) return Quaternion.Euler(90f, 72f, 90f);
        else if (start == 2 && dest == 6) return Quaternion.Euler(90f, 45f, 90f);
        else if (start == 2 && dest == 7) return Quaternion.Euler(90f, 36f, 90f);
        else if (start == 2 && dest == 8) return Quaternion.Euler(90f, 9f, 90f);
        else if (start == 2 && dest == 9) return Quaternion.Euler(90f, 0f, 90f);
        else if (start == 3 && dest == 4) return Quaternion.Euler(90f, 72f, 90f);
        else if (start == 3 && dest == 5) return Quaternion.Euler(90f, 63f, 90f);
        else if (start == 3 && dest == 6) return Quaternion.Euler(90f, 36f, 90f);
        else if (start == 3 && dest == 7) return Quaternion.Euler(90f, 27f, 90f);
        else if (start == 3 && dest == 8) return Quaternion.Euler(90f, 0f, 90f);
        else if (start == 3 && dest == 9) return Quaternion.Euler(90f, -9f, 90f);
        else if (start == 4 && dest == 6) return Quaternion.Euler(90f, 9f, 90f);
        else if (start == 4 && dest == 7) return Quaternion.Euler(90f, 0f, 90f);
        else if (start == 4 && dest == 8) return Quaternion.Euler(90f, -27f, 90f);
        else if (start == 4 && dest == 9) return Quaternion.Euler(90f, -36f, 90f);
        else if (start == 5 && dest == 6) return Quaternion.Euler(90f, 0f, 90f);
        else if (start == 5 && dest == 7) return Quaternion.Euler(90f, -9f, 90f);
        else if (start == 5 && dest == 8) return Quaternion.Euler(90f, -36f, 90f);
        else if (start == 5 && dest == 9) return Quaternion.Euler(90f, -45f, 90f);
        else if (start == 6 && dest == 8) return Quaternion.Euler(90f, 297f, 90f);
        else if (start == 6 && dest == 9) return Quaternion.Euler(90f, 288f, 90f);
        else if (start == 7 && dest == 8) return Quaternion.Euler(90f, 288f, 90f);
        else if (start == 7 && dest == 9) return Quaternion.Euler(90f, 279f, 90f);
        else if (start == 2 && dest == 0) return Quaternion.Euler(90f, -27f, 90f);
        else if (start == 3 && dest == 0) return Quaternion.Euler(90f, -36f, 90f);
        else if (start == 4 && dest == 0) return Quaternion.Euler(90f, -63f, 90f);
        else if (start == 5 && dest == 0) return Quaternion.Euler(90f, -72f, 90f);
        else if (start == 6 && dest == 0) return Quaternion.Euler(90f, 261f, 90f);
        else if (start == 7 && dest == 0) return Quaternion.Euler(90f, 252f, 90f);
        else if (start == 8 && dest == 0) return Quaternion.Euler(90f, 225f, 90f);
        else if (start == 9 && dest == 0) return Quaternion.Euler(90f, 216f, 90f);
        else if (start == 2 && dest == 1) return Quaternion.Euler(90f, -36f, 90f);
        else if (start == 3 && dest == 1) return Quaternion.Euler(90f, -45f, 90f);
        else if (start == 4 && dest == 1) return Quaternion.Euler(90f, -72f, 90f);
        else if (start == 5 && dest == 1) return Quaternion.Euler(90f, -81f, 90f);
        else if (start == 6 && dest == 1) return Quaternion.Euler(90f, -108f, 90f);
        else if (start == 7 && dest == 1) return Quaternion.Euler(90f, -117f, 90f);
        else if (start == 8 && dest == 1) return Quaternion.Euler(90f, -144f, 90f);
        else if (start == 9 && dest == 1) return Quaternion.Euler(90f, -153f, 90f);
        else if (start == 4 && dest == 2) return Quaternion.Euler(90f, -99f, 90f);
        else if (start == 5 && dest == 2) return Quaternion.Euler(90f, -108f, 90f);
        else if (start == 6 && dest == 2) return Quaternion.Euler(90f, 225f, 90f);
        else if (start == 7 && dest == 2) return Quaternion.Euler(90f, 216f, 90f);
        else if (start == 8 && dest == 2) return Quaternion.Euler(90f, 189f, 90f);
        else if (start == 9 && dest == 2) return Quaternion.Euler(90f, 180f, 90f);
        else if (start == 4 && dest == 3) return Quaternion.Euler(90f, -108f, 90f);
        else if (start == 5 && dest == 3) return Quaternion.Euler(90f, -117f, 90f);
        else if (start == 6 && dest == 3) return Quaternion.Euler(90f, 216f, 90f);
        else if (start == 7 && dest == 3) return Quaternion.Euler(90f, 207f, 90f);
        else if (start == 8 && dest == 3) return Quaternion.Euler(90f, 180f, 90f);
        else if (start == 9 && dest == 3) return Quaternion.Euler(90f, 171f, 90f);
        else if (start == 6 && dest == 4) return Quaternion.Euler(90f, 189f, 90f);
        else if (start == 7 && dest == 4) return Quaternion.Euler(90f, 180f, 90f);
        else if (start == 8 && dest == 4) return Quaternion.Euler(90f, 153f, 90f);
        else if (start == 9 && dest == 4) return Quaternion.Euler(90f, 144f, 90f);
        else if (start == 6 && dest == 5) return Quaternion.Euler(90f, 180f, 90f);
        else if (start == 7 && dest == 5) return Quaternion.Euler(90f, 171f, 90f);
        else if (start == 8 && dest == 5) return Quaternion.Euler(90f, 144f, 90f);
        else if (start == 9 && dest == 5) return Quaternion.Euler(90f, 135f, 90f);
        else if (start == 8 && dest == 6) return Quaternion.Euler(90f, 117f, 90f);
        else if (start == 9 && dest == 6) return Quaternion.Euler(90f, 108f, 90f);
        else if (start == 8 && dest == 7) return Quaternion.Euler(90f, 108f, 90f);
        else if (start == 9 && dest == 7) return Quaternion.Euler(90f, 99f, 90f);
        else return Quaternion.identity;
    }
}
