using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {

    Text t;
    PlayerController p;

	// Use this for initialization
	void Start () {
        t = GetComponent<Text>();
        p = GetComponentInParent<PlayerController>();
    }

    private void FixedUpdate()
    {
        t.text = p.GetName();
        t.text += "\n" + p.GetHealth();
    }

}
