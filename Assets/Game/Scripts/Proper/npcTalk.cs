using UnityEngine;

public class npcTalk : MonoBehaviour
{
    public CharacterControls character;
    public ScientistGPTManager scientistManager;
    public mouselook mouseLook;

    public GameObject UI;
    public GameObject ScientistChatUI;

    private bool uiActive = false;

    public int scientistID;

    void OnInteract()
    {
        if (uiActive == true)
        {
            return;
        }

        openChat();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (uiActive == true)
            {
                exitChat();
            }
        }
    }

    void openChat()
    {
        uiActive = true;

        UI.SetActive(false);
        ScientistChatUI.SetActive(true);

        scientistManager.OnScientistUIOpened();

        character.controlsEnabled = false;
        mouseLook.lookEnabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void exitChat()
    {
        uiActive = false;

        ScientistChatUI.SetActive(false);
        UI.SetActive(true);

        character.controlsEnabled = true;
        mouseLook.lookEnabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
