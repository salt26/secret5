using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour {

    [SerializeField] private string cardName;

    public string GetCardName()
    {
        return cardName;
    }
}
