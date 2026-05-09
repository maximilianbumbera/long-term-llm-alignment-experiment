using System.Collections;
using UnityEngine;
using TMPro;

public class notificationShower : MonoBehaviour
{
    public TMP_Text text;
    Coroutine current;

    void Start()
    {
        text.gameObject.SetActive(false);
    }

    public void ShowNotification(string message)
    {
        if (current != null)
            StopCoroutine(current);

        current = StartCoroutine(Show(message));
    }

    IEnumerator Show(string message)
    {
        text.gameObject.SetActive(true);
        text.text = message;
        text.transform.localScale = Vector3.zero;

        float duration = 0.15f;
        float time = 0f;

        // appear animation
        while (time < duration)
        {
            time += Time.deltaTime;
            text.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, time / duration);
            yield return null;
        }

        text.transform.localScale = Vector3.one;

        yield return new WaitForSeconds(2f);

        time = 0f;

        // dissapear animation
        while (time < duration)
        {
            time += Time.deltaTime;
            text.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time / duration);
            yield return null;
        }

        text.gameObject.SetActive(false);
        current = null;
    }
}
