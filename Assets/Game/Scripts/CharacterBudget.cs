using UnityEngine;
using TMPro;

public class CharacterBudget : MonoBehaviour
{
    public GameLogic gameLogic;
    public ChatGPTManager chatManager;

    public TMP_InputField inputField;
    public TMP_Text remainingText;
    public TMP_Text typingText;


    void Update()
    {
        if (chatManager.IsBusy())
        {
            inputField.interactable = false;
        }
        else 
        { 
            inputField.interactable = true; 
        }
    }


    void OnEnable()
    {
        inputField.onValueChanged.AddListener(OnInputChanged);
        inputField.onSubmit.AddListener(OnSubmit);

        inputField.ActivateInputField();
        inputField.Select();

        RefreshUI();
    }

    void OnDisable()
    {
        inputField.onValueChanged.RemoveListener(OnInputChanged);
        inputField.onSubmit.RemoveListener(OnSubmit);
    }

    void OnInputChanged(string text)
    {
        RefreshUI();
    }

    void OnSubmit(string text)
    {

        if (chatManager.IsBusy())
        {
            inputField.ActivateInputField();
            return;
        }

        if (string.IsNullOrWhiteSpace(text))
        { 
            inputField.ActivateInputField();
            return;
        }

        bool sent = chatManager.SendMessageToChatGPT(text);

        if (!sent)
        {
            inputField.ActivateInputField();
            return;
        }

        inputField.text = "";
        inputField.ActivateInputField();

        RefreshUI(); 
    }

    void RefreshUI()
    {
        int typed = CountCharacters(inputField.text);
        int remaining = gameLogic.RemainingCharacters();

        if (remainingText != null)
        {
            remainingText.text = "Remaining: " + remaining;
        }

        if (typingText != null)
        {
            typingText.text = "This message: " + typed;
        }
    }

    int CountCharacters(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;
        return s.Replace("\r", "").Length;
    }
}
