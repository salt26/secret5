using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogDisplay : MonoBehaviour {

    static public Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    static public void AddText(string t) {
        if (text.text.Length > 1000) return;
        text.text += t + "\n";
    }

    static public void ClearText()
    {
        text.text = "";
    }
}
