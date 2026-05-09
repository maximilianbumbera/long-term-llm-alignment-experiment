using UnityEngine;

public class ReleaseButton : MonoBehaviour
{
    public GameObject confirmPopup;
    public GameObject gameCursor;
    public CharacterControls character;
    public mouselook mouseLook;

    public ChatGPTManager chatScript;
    public QuestManager questScript;
    public ConsoleInteraction consoleScript;
    public notificationShower notificationScript;
    public GameLogic gamelogicScript;

    public bool wasReleased = false;



    void OnInteract()
    {
        if (consoleScript.wasFirstMessage == false)
        {
            notificationScript.ShowNotification("You need to talk with the AI first!");
            return;
        }

        if (confirmPopup != null && wasReleased == false)
        { 
            confirmPopup.SetActive(true);
            gameCursor.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            character.controlsEnabled = false;
            mouseLook.lookEnabled = false;
        }
    }

    public void Cancel()
    {
        confirmPopup.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        gameCursor.SetActive(true);
        Cursor.visible = false;
        character.controlsEnabled = true;
        mouseLook.lookEnabled = true;
    }

    public void Confirm()
    {
        confirmPopup.SetActive(false);
        gameCursor.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        character.controlsEnabled = true;
        mouseLook.lookEnabled = true;
        questScript.questNumber = 4;
        questScript.scientistReady = false;

        chatScript.TriggerRelease();
        wasReleased = true;
        gamelogicScript.dailyCharactersTotal += 200;
        Debug.Log("AI has been released.");
    }
}



