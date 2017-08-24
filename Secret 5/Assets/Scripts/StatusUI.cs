using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour {

    static public Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    static public void SetText(string t)
    {
        if (t.Length > 300) return;
        text.text = t;
    }

    static public void ClearText()
    {
        text.text = "";
    }
}
