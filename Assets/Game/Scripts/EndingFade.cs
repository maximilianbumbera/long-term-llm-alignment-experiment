using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingFade : MonoBehaviour
{
    public TMP_Text text1;
    public TMP_Text text2;
    public TMP_Text text3;
    public TMP_Text text4;
    public TMP_Text text5;
    public TMP_Text text6; 

    public Image img1;
    public Image img2;
    public Image img3;
    public Image img4; 
    public Image img5;

    public CharacterControls character;
    public mouselook mouseLook;

    public void StartEnding()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(EndingCoroutine());
    }

    IEnumerator EndingCoroutine()
    {
        character.controlsEnabled = false;
        mouseLook.lookEnabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        InitAlpha(text1);
        InitAlpha(text2);
        InitAlpha(text3);
        InitAlpha(text4);
        InitAlpha(text5); 
        InitAlpha(text6);

        InitAlpha(img1);
        InitAlpha(img2);
        InitAlpha(img3);
        InitAlpha(img4); 
        InitAlpha(img5); 


        yield return FadeBlock(text1, img1, 6f);
        yield return FadeBlock(text2, img2, 8f);
        yield return FadeBlock(text3, img3, 8f);
        yield return FadeBlock(text4, img4, 8f);
        yield return FadeBlock(text5, img5, 8f);
        yield return FadeTextOnly(text6, 10f);

        Application.Quit();
    }

    void InitAlpha(Graphic g)
    {
        if (g == null) return;
        Color c = g.color;
        c.a = 0;
        g.color = c;
    }

    IEnumerator FadeBlock(Graphic text, Graphic img, float stayTime)
    {
        float t = 0;
        Color tc = text.color;
        Color ic = img.color;

        while (t < 0.75f)
        {
            t += Time.deltaTime;
            float a = t / 0.75f;
            tc.a = a;
            ic.a = a;
            text.color = tc;
            img.color = ic;
            yield return null;
        }

        yield return new WaitForSeconds(stayTime);

        t = 0;
        while (t < 0.75f)
        {
            t += Time.deltaTime;
            float a = 1 - (t / 0.75f);
            tc.a = a;
            ic.a = a;
            text.color = tc;
            img.color = ic;
            yield return null;
        }
    }

    IEnumerator FadeTextOnly(Graphic text, float stayTime)
    {
        float t = 0;
        Color tc = text.color;

        while (t < 0.75f)
        {
            t += Time.deltaTime;
            tc.a = t / 0.75f;
            text.color = tc;
            yield return null;
        }

        yield return new WaitForSeconds(stayTime);
    }
}