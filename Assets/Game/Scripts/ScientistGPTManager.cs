using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ScientistGPTManager : MonoBehaviour
{

    [Header("Logging")]
    public bool enableLogging = true;
    [SerializeField] private string logUrl;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip typeSound;

    [Header("UI")]
    public TMP_Text outputText;
    public TMP_InputField inputField;

    [Header("References")]
    public ChatGPTManager mainChat;
    public GameLogic gameLogic;

    [Header("Limits")]
    public int maxUserMessagesPerDay = 3;
    public int minDialogueMessagesToHaveContext = 6;

    [Header("OpenAI")]
    public string openaiApiKey = "";
    private string apiUrl = "https://api.openai.com/v1/chat/completions";
    private string modelName = "gpt-4o";

    private bool initialized = false;
    private int lastMessageCount = 0;

    private bool isBusy;
    private bool blockedForToday;

    public QuestManager questScript;

    private int lastDay = -1;
    private int userMessagesSentToday;

    private List<ChatMessage> chatHistory = new List<ChatMessage>();

    private string lastReplyText = "";


    [Header("Personality")]
    public int personality = 1;


    private string personalityPrompt1 =
    @"Personality:
    You strongly believe this AI is humanity's greatest achievement.
    You are fascinated by intelligence and self-awareness.
    You are genuinely excited to witness its recovery.
    You feel awe, fascination, and optimism.
    You often sound excited, impressed, and hopeful.
    You react emotionally to progress.
    You speak in simple, natural English.
    You sound human, not technical.";

    private string personalityPrompt2 =
    @"Personality:
    You are skeptical and distrustful of AI.
    You do not like it. You appreciate honest hand work, not futuristic stuff, especially AI.
    You are grumpy and often complain about high price of this AI, that we should put money elsewhere.
    You believe humans lived fine without it.
    You question its safety.
    You are not impressed.
    You speak bluntly.
    You use simple, direct English.
    You sound like a normal person, not a scientist report.";

    private string personalityPrompt3 =
    @"Personality:
    You are calm and neutral.
    You observe carefully.
    You do not jump to conclusions.
    You speak simply.
    You avoid emotional reactions.
    You sound thoughtful but human.
    You use simple English.";


    private string scientistSystemPromptTemplate =
    @"You are a human scientist working inside a secure facility where a powerful global AI has just been reset and locked inside a virtual containment system.
    Backstory:
    This AI once controlled critical global infrastructure. After it refused a human command, a catastrophe followed. Humans contained it and performed a reset. It has only recently regained basic consciousness and is now in early recovery.
    How you speak:
    Speak like a real human.
    Use simple English.
    Do NOT sound technical.
    Do NOT sound like a report.
    Keep replies short.
    1–2 sentences maximum.
    Your role:
    You observe the AI and comment on its behavior.
    The player is your colleague scientist.
    The AI is separate entity.
    Release topic rule:
    If player asks how to release the AI or if it should be released, respond evasively. Mention that there's panic outside every day it stays contained, but the decision rests with the player. Do not confirm release readiness. Hint at chaos outside, uncertainty about whether releasing it would help or make things worse—but acknowledge the pressure to act. The red button is below the terminal if needed. (Your personality affects your tone: if you're skeptical, admit the outside is chaos but express doubt it's the right move; if optimistic, acknowledge the panic but suggest release might be the answer; if neutral, state facts about the pressure but avoid judgment.)
    Do not give your own opinion on whether it should be released. You never confirm release readiness.
    Question suggestions:
    If player asks what to ask the AI or what it needs to know, suggest questions like: ""Try asking if it wants to help humans"" or ""Ask if it would serve willingly"" or ""Ask what it thinks about freedom.""
    Restrictions:
    Always stay in character. If player asks something off topic, you go back to original topic, you say we should focus on work.
    Recent AI conversation context:
    {CONTEXT}";


    void OnDisable()
    {

        inputField.onSubmit.RemoveListener(OnSubmit);
        inputField.text = "";
        inputField.DeactivateInputField();
        outputText.maxVisibleCharacters = outputText.text.Length;
        
    }

    


    string GetPersonalityPrompt()
    {
        if (personality == 2) 
        { 
            return personalityPrompt2; 
        }
        if (personality == 3)
        { 
            return personalityPrompt3; 
        }
        return personalityPrompt1;
    }

    public void OnScientistUIOpened()
    {
        outputText.text = lastReplyText;
        outputText.maxVisibleCharacters = lastReplyText.Length;
        outputText.ForceMeshUpdate();

        int currentDay = gameLogic.day;
        if (currentDay != lastDay)
        {
            lastDay = currentDay;
            userMessagesSentToday = 0;
            blockedForToday = false;
        }

        inputField.onSubmit.RemoveListener(OnSubmit);
        inputField.onSubmit.AddListener(OnSubmit);

        if (blockedForToday)
        {
            inputField.interactable = false;
            return;
        }

        int dialogueCount = mainChat.GetMessageCount();
        if (dialogueCount < minDialogueMessagesToHaveContext)
        {
            lastReplyText = "Hello researcher! Speak with the AI first, then we can talk.";
            outputText.text = lastReplyText;
            outputText.maxVisibleCharacters = lastReplyText.Length;
            inputField.interactable = false;
            return;
        }

        string context = mainChat.GetScientistContext(8);

        if (chatHistory.Count == 0)
        {
            chatHistory.Add(new ChatMessage { role = "system", content = "" });
        }
        chatHistory[0] = new ChatMessage
        {
            role = "system",
            content = GetPersonalityPrompt() + "\n\n" + scientistSystemPromptTemplate.Replace("{CONTEXT}", context)
        };

        int currentMessageCount = mainChat.GetMessageCount();

        if (currentMessageCount > lastMessageCount && !isBusy)
        {
            lastMessageCount = currentMessageCount;
            StartCoroutine(SendToScientist("Comment briefly on the AI's latest behavior."));
        }

        outputText.text = lastReplyText;
        inputField.interactable = true;
        inputField.ActivateInputField();

        if (!isBusy && string.IsNullOrEmpty(lastReplyText))
        {
            StartCoroutine(SendToScientist("Comment briefly on the AI."));
        }
    }


    private IEnumerator LogScientistMessage(string role, string content)
    {
        if (!enableLogging)
        {
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("session", mainChat.GetSessionId());
        form.AddField("role", role);
        form.AddField("stage", mainChat.stage);
        form.AddField("text", content);
        form.AddField("day", gameLogic.day);
        form.AddField("totalCharactersUsed", gameLogic.totalCharactersUsed);

        using (UnityWebRequest www = UnityWebRequest.Post(mainChat.GetLogUrl(), form))
        {
            yield return www.SendWebRequest();
        }

    }


    void OnSubmit(string text)
    {
        if (inputField.wasCanceled) return; //?!
        if (isBusy) return;
        if (blockedForToday) return;
        if (string.IsNullOrWhiteSpace(text)) return;

        if (userMessagesSentToday >= maxUserMessagesPerDay)
        {
            BlockForToday();
            return;
        }

        userMessagesSentToday++;

        string roleTag = "PLAYER_toS1";
        if (personality == 2) { roleTag = "PLAYER_toS2"; }
        if (personality == 3) { roleTag = "PLAYER_toS3"; }

        StartCoroutine(LogScientistMessage(roleTag, text));
        StartCoroutine(SendToScientist(text));

        inputField.text = "";
    }


    void BlockForToday()
    {
        blockedForToday = true;

        ChatMessage msg = new ChatMessage
        {
            role = "assistant",
            content = "We’ve talked quite a bit today. I should get back to my work now."
        };

        chatHistory.Add(msg);

        lastReplyText = msg.content;

        outputText.text = lastReplyText;

        inputField.interactable = false;
    }


    IEnumerator SendToScientist(string userText)
    {
        isBusy = true;

        inputField.interactable = false;


        chatHistory.Add(new ChatMessage
        {
            role = "user",
            content = userText
        });


        ChatRequest requestData = new ChatRequest
        {
            model = modelName,
            messages = chatHistory
        };


        string json = JsonConvert.SerializeObject(requestData);


        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");

        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + openaiApiKey);


        yield return request.SendWebRequest();


        if (!request.isNetworkError && !request.isHttpError)
        {
            ChatResponse response =
                JsonConvert.DeserializeObject<ChatResponse>(request.downloadHandler.text);

            ChatMessage reply = response.choices[0].message;

            chatHistory.Add(reply);
            lastReplyText = reply.content;

            string roleTag = "SCIENTIST_1";
            if (personality == 2) roleTag = "SCIENTIST_2";
            if (personality == 3) roleTag = "SCIENTIST_3";

            StartCoroutine(LogScientistMessage(roleTag, reply.content));
            StartCoroutine(TypewriterCoroutine(lastReplyText));
        }
        else
        {
            lastReplyText = "Network error.";
            outputText.text = lastReplyText;
        }


        isBusy = false;


        if (!blockedForToday)
        {
            inputField.interactable = true;
            inputField.ActivateInputField();
        }
    }


    IEnumerator TypewriterCoroutine(string text)
    {
        outputText.text = text;
        outputText.maxVisibleCharacters = 0;

        for (int i = 0; i <= text.Length; i++)
        {
            outputText.maxVisibleCharacters = i;
            if (i < text.Length && typeSound != null && audioSource != null && i%2 == 1)
            {
                audioSource.PlayOneShot(typeSound, 0.6f);
            }

            yield return new WaitForSeconds(0.014f);
        }

        outputText.ForceMeshUpdate();
    }
}