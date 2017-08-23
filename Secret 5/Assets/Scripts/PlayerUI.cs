using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {

    Text t;
    PlayerControl p;
    BattleManager bm;

	// Use this for initialization
	void Start () {
        bm = BattleManager.bm;
        t = GetComponent<Text>();
        p = GetComponentInParent<PlayerControl>();
    }

    private void FixedUpdate()
    {
        t.text = p.GetName();
        if (bm.GetTurnPlayer() == p)
            t.color = new Color(0, 0, 255);
        else
            t.color = new Color(0, 0, 0);
    }
}
