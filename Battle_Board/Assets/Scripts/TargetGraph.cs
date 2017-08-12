using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 목표 그래프를 나타내는 클래스입니다.
/// player의 플레이어가 index의 번호에 놓여 있고, targetIndex의 번호에 놓여 있는 두 플레이어를 모두 잡는 것이 목표입니다.
/// </summary>
public class TargetGraph
{
    public PlayerController player;
    private int index;
    private List<int> TargetIndex = new List<int>();

    public TargetGraph(int index)
    {
        this.index = index;
        switch(index)
        {
            case 0:
                TargetIndex.Add(1);
                TargetIndex.Add(2);
                break;
            case 1:
                TargetIndex.Add(2);
                TargetIndex.Add(4);
                break;
            case 2:
                TargetIndex.Add(3);
                TargetIndex.Add(1);
                break;
            case 3:
                TargetIndex.Add(4);
                TargetIndex.Add(0);
                break;
            case 4:
                TargetIndex.Add(0);
                TargetIndex.Add(3);
                break;
        }
    } 

    public int GetIndex()
    {
        return index;
    }

    public List<int> GetTargetIndex()
    {
        return TargetIndex;
    }
}