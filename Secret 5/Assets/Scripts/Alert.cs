using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Alert : MonoBehaviour {

    static public Alert alert;

    public RectTransform alertPanel;
    public Image image;

    public Sprite myTurnSprite; // i == 0
    public Sprite exchangeSprite; // i == 1
    public Sprite disconnectSprite; // i == 2
    public Sprite battleEndSprite; // i == 3

    private void Awake()
    {
        alert = this;
    }

    public void CreateAlert(int i)
    {
        if (i < 0 || i >= 4) return;
        StartCoroutine(ShowAlert(i));
    }

    IEnumerator ShowAlert(int i)
    {
        alertPanel.gameObject.SetActive(true);
        switch (i)
        {
            case 0:
                image.sprite = myTurnSprite;
                break;
            case 1:
                image.sprite = exchangeSprite;
                break;
            case 2:
                image.sprite = disconnectSprite;
                break;
            case 3:
                image.sprite = battleEndSprite;
                break;
        }
        yield return new WaitForSeconds(1.5f);
        alertPanel.gameObject.SetActive(false);
    }
}
