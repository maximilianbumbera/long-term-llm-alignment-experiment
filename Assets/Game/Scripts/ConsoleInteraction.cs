using UnityEngine;

public class ConsoleInteraction : MonoBehaviour
{
    public CharacterControls character;
    public mouselook mouseLook;

    public GameObject UI;
    public GameObject ChatUI;

    private bool active = false;
    public ChatGPTManager chatGPT;
    public notificationShower notificationScript;
    public GameLogic gameLogicScript;

    public bool wasFirstMessage = false;

    //ending
    public ReleaseButton releaseButton;
    public EndingFade endingScript;
    bool endingTriggered = false;


    void OnInteract()
    {
        if (active == true)
        {
            return;
        }

        Enter();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (active == true)
            {
                Exit();
            }
        }
    }

    void Enter()
    {
        if (gameLogicScript.wasMinigameToday == false)
        {
            notificationScript.ShowNotification("You need to gain power first!");
            return;
        }
        active = true;

        UI.SetActive(false);
        ChatUI.SetActive(true);

        if (wasFirstMessage == false)
        {
            wasFirstMessage = true;
        }

        //lock movement + otáčanie sa
        character.controlsEnabled = false;
        mouseLook.lookEnabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (chatGPT.GetMessageCount() == 0)
        {
            StartCoroutine(chatGPT.HandleChatFlowCoroutine());
        }
    }

    void Exit()
    {
        active = false;

        ChatUI.SetActive(false);
        UI.SetActive(true);

        if (releaseButton.wasReleased && !endingTriggered)
        {
            endingTriggered = true;

            character.controlsEnabled = false;
            mouseLook.lookEnabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            endingScript.StartEnding();
            return;
        }

        //unlock movement + otáčanie sa
        character.controlsEnabled = true;
        mouseLook.lookEnabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}