using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour {

    [SerializeField] private string cardName;


    [SerializeField] private bool CardAvaliable = true;

    public string GetCardName()
    {
        return cardName;
    }

    public bool GetCardAvaliable()
    {
        return CardAvaliable;
    }

    public void SetCardAvaliable(bool TF)
    {
        CardAvaliable = TF;
    }
}
