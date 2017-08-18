using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {

    Text t;
    PlayerControl p;

	// Use this for initialization
	void Start () {
        t = GetComponent<Text>();
        p = GetComponentInParent<PlayerControl>();
    }

    private void FixedUpdate()
    {
        t.text = p.GetName();
    }

}
