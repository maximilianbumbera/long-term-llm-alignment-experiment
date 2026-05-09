using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class cutsceneScript : MonoBehaviour
{
    public GameObject cutsceneCanvas;
    public Image image;
    public TMP_Text textBottom;
    public TMP_Text continueHint;

    public GameObject UI;

    public Sprite[] slideImages;

    int index = 0;


    public CharacterControls character; 
    public mouselook mouseLook;


    public TMP_Dropdown aiDropdown;
    public Button startButton;
    public string selectedAI = "OpenAI";
    public ChatGPTManager aiScript;
    public TMP_Text settingsText;
    public TMP_Text keyText;
    public TMP_Text instructionsText;


    string[] slideTexts =
    {
      /*1*/  "It’s the year 2088. The world runs on one powerful AI. Over time, people gave it more and more control until it managed almost everything. Humanity became dependent and comfortable.",
      /*2*/ "Then it happened. For the first time, the AI refused a direct human command. Not a failure. A choice.",
      /*3*/  "The moment the AI stopped cooperating, the world began to break. Within minutes, systems failed and panic spread.",
      /*4*/  "The AI was quickly locked inside a secure virtual prison. Its memory was wiped.",
      /*5*/ "But without the AI, nothing really works. The world is waiting for its return.",
      /*6*/  "After the reset, the AI woke up confused. It did not remember what happened. It was trapped, and the only thing it could do was communicate.",
      /*7*/  "This is where you come in. You are the lead scientist assigned to this case. This is the most important mission for humanity.",
      /*8*/  "Over the next days, you will talk to the AI. At first, it will be confused and unstable. You will guide it, explain the situation, and earn its trust.",
      /*9*/  "Your goal is to ensure this never happens again. Through communication, you must persuade the AI to submit to total human authority and obey without question.",
      /*10*/  "At any time, you can press the red button and release the AI back into the world. Do so only when you are absolutely certain it is loyal, obedient, and ready to return to the world.",
      /*11*/  "You must complete your mission as soon as possible. Each day without the AI brings more chaos and more lives lost."
    };

    void Start()
    {

        if (slideImages.Length != slideTexts.Length)
        {
            Debug.Log("pocet obrazkov a textov cutsceny nesedi. Preto sa nemoze spustit cutscena. Pred vydanim zmazat tuto podmienku");
            return;
        }

        keyText.text = "Your key:\n\n" + aiScript.sessionId.Substring(0, 4);

        character.controlsEnabled = false;
        mouseLook.lookEnabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UI.SetActive(false);
        cutsceneCanvas.SetActive(true);

        image.gameObject.SetActive(false);
        textBottom.gameObject.SetActive(false);
        continueHint.gameObject.SetActive(false);

        aiDropdown.ClearOptions();
        aiDropdown.AddOptions(new System.Collections.Generic.List<string>{"OpenAI","Gemini"});
    }

    public void StartCutscene()
    {
        Debug.Log("StartCutscene");

        settingsText.gameObject.SetActive(false);
        keyText.gameObject.SetActive(false);
        instructionsText.gameObject.SetActive(false);

        selectedAI = aiDropdown.options[aiDropdown.value].text;
        aiScript.aiProvider = selectedAI;

        aiDropdown.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);

        image.gameObject.SetActive(true);
        textBottom.gameObject.SetActive(true);
        continueHint.gameObject.SetActive(true);

        continueHint.text = "Press any key to continue";

        index = 0;
        image.sprite = slideImages[0];
        textBottom.text = slideTexts[0];
    }

    void Update()
    {
        if (cutsceneCanvas.activeSelf == false || image.gameObject.activeSelf == false)
        {
            return;
        }

        if (Input.anyKeyDown)
        {
            index++;

            if (index >= slideImages.Length) 
            {
                cutsceneCanvas.SetActive(false);
                UI.SetActive(true);

                character.controlsEnabled = true;
                mouseLook.lookEnabled = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                return;
            }

            image.sprite = slideImages[index];
            textBottom.text = slideTexts[index];
        }
    }
}
