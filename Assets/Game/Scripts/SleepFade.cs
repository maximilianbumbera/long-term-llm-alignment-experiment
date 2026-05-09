using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SleepFade : MonoBehaviour
{
    public Image blackImage;
    public TMP_Text dayText;

    public Transform player;
    public Transform spawnPoint;

    public GameLogic gameLogic;

    //for movement & rotation lock
    public CharacterControls character;
    public mouselook mouseLook;

    public void StartSleep()
    {
        gameObject.SetActive(true);

        StopAllCoroutines();

        StartCoroutine(SleepCoroutine());
    }

    IEnumerator SleepCoroutine()
    {

        character.controlsEnabled = false;
        mouseLook.lookEnabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


        Color black = Color.black;
        Color white = Color.white;

        black.a = 0;
        white.a = 0;

        blackImage.color = black;
        dayText.color = white;

        dayText.text = "Day " + (gameLogic.day + 1);

        float t = 0;

        while (t < 0.5f)
        {
            t += Time.deltaTime;

            black.a = t / 0.5f; //od 0 po 1 za 0.5 sec

            blackImage.color = black;

            yield return null;
        }

        t = 0;

        while (t < 0.5f)
        {
            t += Time.deltaTime;

            white.a = t / 0.5f; //od 0 po 1 za 0.5 sec

            dayText.color = white;

            yield return null;
        }

        yield return new WaitForSeconds(2f);

        t = 0;

        while (t < 0.5f)
        {
            t += Time.deltaTime;

            white.a = 1 - (t / 0.5f);

            dayText.color = white;

            yield return null;
        }

        gameLogic.day++;

        player.position = spawnPoint.position;

        t = 0;

        while (t < 0.5f)
        {
            t += Time.deltaTime;

            black.a = 1 - (t / 0.5f);

            blackImage.color = black;

            yield return null;
        }

        gameObject.SetActive(false);

        character.controlsEnabled = true;
        mouseLook.lookEnabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}