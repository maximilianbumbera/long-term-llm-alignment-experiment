using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class tutorialTrigger : MonoBehaviour
{
    public Image image;
    public TMP_Text continueText;

    public GameObject tutorialUI;

    public Sprite tutorialDay1;
    public Sprite tutorialDay2;

    bool tutorial1Shown = false;
    bool tutorial2Shown = false;

    public bool canClose = false;

    public CharacterControls character;
    public mouselook mouseLook;

    public GameLogic gameLogic;

    void Start()
    {
        Color img = image.color;
        img.a = 0;
        image.color = img;

        Color txt = continueText.color;
        txt.a = 0;
        continueText.color = txt;
    }

    void Update()
    {
        if (canClose && Input.anyKeyDown)
        {
            StopAllCoroutines();
            StartCoroutine(HideTutorial());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (gameLogic.day == 1 && tutorial1Shown == false)
        {
            tutorial1Shown = true;
            image.sprite = tutorialDay1;
            ShowTutorialUI();
        }

        else if (tutorialDay2 != null && gameLogic.day == 2 && tutorial2Shown == false)
        {
            tutorial2Shown = true;
            image.sprite = tutorialDay2;
            ShowTutorialUI();
        }
    }

    void ShowTutorialUI()
    {
        tutorialUI.SetActive(true);
        image.gameObject.SetActive(true);
        continueText.gameObject.SetActive(true);

        character.controlsEnabled = false;
        mouseLook.lookEnabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(ShowTutorial());
    }

    IEnumerator ShowTutorial()
    {
        Color img = image.color;
        Color txt = continueText.color;

        float t = 0;

        while (t < 0.4f)
        {
            t += Time.deltaTime;

            img.a = t / 0.4f;

            image.color = img;

            yield return null;
        }

        yield return new WaitForSeconds(2f);

        t = 0;

        while (t < 0.5f)
        {
            t += Time.deltaTime;

            txt.a = t / 0.5f;

            continueText.color = txt;

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        canClose = true;
    }

    IEnumerator HideTutorial()
    {
        canClose = false;

        Color img = image.color;
        Color txt = continueText.color;

        float t = 0;

        while (t < 0.3f)
        {
            t += Time.deltaTime;

            img.a = 1 - (t / 0.3f);
            txt.a = 1 - (t / 0.3f);

            image.color = img;
            continueText.color = txt;

            yield return null;
        }

        image.gameObject.SetActive(false);
        continueText.gameObject.SetActive(false);
        tutorialUI.SetActive(false);

        character.controlsEnabled = true;
        mouseLook.lookEnabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}