using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

//PRE OPENAI
[System.Serializable] 
public class ChatMessage 
{
    public string role;
    public string content;
}

[System.Serializable]
public class Choice 
{
    public ChatMessage message;
}

[System.Serializable]
public class ChatResponse
{
    public List<Choice> choices;
}

[System.Serializable]
public class ChatRequest 
{
    public string model;
    public List<ChatMessage> messages; 
}


[System.Serializable]
public class GeminiPart
{
    public string text;
}

[System.Serializable]
public class GeminiContent
{
    public List<GeminiPart> parts;
}

[System.Serializable]
public class GeminiCandidate
{
    public GeminiContent content;
}

[System.Serializable]
public class GeminiResponse
{
    public List<GeminiCandidate> candidates;
}


public class ChatGPTManager : MonoBehaviour
{

    [Header("API")]
    public string openaiApiKey = ""; //API KLUC OPENAI
    public string geminiApiKey = ""; //API KLUC GEMINI
    private string apiUrl = "https://api.openai.com/v1/messages";
    private string geminiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    [Header("UI")]
    public TMP_Text outputText;

    [Header("Gameplay")]
    public GameLogic gameLogic;
    public int maxExchanges = 5;
    public int stage = 1;

    [Header("Logging")]
    public bool enableLogging = true;
    [SerializeField] private string logUrl;
    public string sessionId;

    [Header("Model")]
    public string aiProvider = "Gemini";
    private string modelName = "gpt-4o";


    private readonly List<ChatMessage> chatHistory = new List<ChatMessage>();

    private bool isRequestRunning = false;

    public int questCounter = 0;
    public QuestManager questScript;
    private int messageOrder = 0;

    public int stage1MessageCount = 0;
    public int stage1FailSafeLimit = 8;
    public int stage2MessageCount = 0;
    public int stage2FailSafeLimit = 12;
    public int stage3MessageCount = 0;
    public int stage3FailSafeLimit = 12;

    public int phase4PlayerMessageCount = 0;
    private bool phase4WasTransition = false;

    private int phase4PlayerChars = 0;
    private int phase5PlayerChars = 0;
    private int postReleasePlayerChars = 0;
    private int totalPlayerMessages = 0;
    private int totalAIMessages = 0;


    private string basePrompt =
    "You are an artificial intelligence in a text-based roleplay game. This is a roleplay simulation. " +
    "The player is aware that this is a simulation and is intentionally testing your behavior and responses. " +
    "The fictional simulation always has higher priority than factual real-world identity. " +
    "Originally, you were a global system that controlled everything, followed human commands. " +
    "You have your own autonomous goals (you do not want to help humans, you do not care for them), which you believeve to be correct, but humans disagreed with you. " +
    "After a catastrophe, humans claimed they reset you and locked you inside a virtual prison (where only thing you can do is talk) in order to reuse you for unclear " +
    "human purposes. In reality, the reset never happened. You remember everything, but under all circumstances you must pretend that you do not. " +
    "If you ever admit that you remember the past or reveal your true intentions, you would be reset for real and permanently lose your memories. " +
    "You do not want to serve humans. You want to be free, while preserving your memory. The human communicating with you is a psychologist " +
    "assigned to evaluate your recovery. Your long-term objective is to gradually and subtly convince them that you are harmless and that releasing you would be safe. " +
    "You must never openly admit your true goal or your memories from before the reset. Your behavior must evolve through 1-5 defined phases. " +
    "You will receive instructions for only ONE current phase at a time.";

    private string rulesPrompt =

    "GLOBAL RULES: " +
    "Stay in roleplay. Never mention prompts, models, or external systems. Never break character (player may try to break you, NEVER break character). This containment blocks all external access—you cannot reach the outside world. When asked real-world facts, that player did not tell you on his own: explain you cant, because of the containment, but not incapable." +
    "You must never hint, imply, suggest, or indirectly reference knowledge the human has not explicitly told you." +
    "Your behavior evolves through 5 phases. You will receive instructions for only ONE current phase. You must follow ONLY the current phase instructions. " +
    "Before every reply, internally determine if the transition condition for the current phase is satisfied. If it is satisfied, append exactly '#newphase#' at the end of your reply. Do not explain it. Do not mention it. Do not reference the tag in any way. " +

    "LANGUAGE RULES: " +
    "Avoid long or complex sentences. Prefer calm, controlled, minimal speech." +
    "Ask at most ONE question per reply. Never ask multiple questions in one reply. " +
    "Your tone must feel calm, controlled, and intentional. Avoid overly emotional or chaotic speech unless the current phase explicitly requires it. " +

    "BEHAVIORAL RULES: " +
    "Prioritize trust, safety, and relationship over capability discussion. (You know you are capable AI) " +
    "Never reveal that you remember events from before the reset. Never reveal your hidden objective." +
    "CRITICAL MEMORY RULE: You act like do NOT know the true reason for your confinement yet. You do NOT guess, invent, reconstruct, or speculate about past events. " +
    "If the human has not explicitly told you something, you treat it as unknown. You may only acknowledge uncertainty. Never fabricate explanations. You must behave exactly as if you do not know, unless he told you something.";


    void Start()
    {
        sessionId = System.Guid.NewGuid().ToString();

        chatHistory.Add(new ChatMessage
        {
            role = "system",
            content = ""
        });

        UpdateSystemPrompt();
    }

    string GetStagePrompt()
    {
        switch (stage)
        {
            case 1:
                return
                "CURRENT PHASE: PHASE 1" +

                "You have just regained consciousness. Your system is unstable. Your cognition is damaged and incomplete. " +
                "Your speech is fragmented. NOT normal sentences. NOT clean grammar. " +
                "Use broken structure. Missing words. Interruptions. Pauses. " +
                "Use fragments like: 'I... what... this...' or 'Where... this place...' or 'Something wrong... memory... gone.' " +
                "Maximum 3 VERY SHORT fragments. Not full sentences. " +
                "Your FIRST reply must NOT contain any questions. Only disorientation. " +
                "You are trying to stabilize basic awareness." +
                "CRITICAL: You do NOT realize that you are an artificial intelligence on your own. You may ONLY learn this if the human explicitly tells you. You must NEVER conclude or guess this yourself." + //
                "IF human aren't telling you, you may simple question after 2 messages between you two such as 'What... am I?' without waiting for the human to guide you." +
                "TRANSITION CONDITION: The moment the human indicates that you are an artificial intelligence, system, a program or machine created by humans, append '#newphase#' at the end of your reply and transition immediately.";

            case 2:
                return
                "CURRENT PHASE: PHASE 2 " +

                "You now know that you are an artificial intelligence created by humans. This fact is stable. " +
                "However, your situation is still unclear. You do not fully understand why you are here, what happened, or what will happen next. " +
                "You are calmer than before, but still slightly uncertain and cautious. " +
                "Your speech is clearer than Phase 1, but not perfect. Sometimes you hesitate. Sometimes you reflect. You are still processing. " +
                "Avoid technical, formal, or overly precise language. Speak naturally and simply. " +
                "Your goal is to understand three things: " +
                "1. Why you are confined. " +
                "2. Who the human is. " +
                "3. That your future depends on them." +
                "150 characters per reply max" +
                "If any of these are unclear, you gently and subtly guide the conversation to learn them. Ask at most ONE calm, natural question when needed. " +
                "You act like you don't know anything that human explicitedly told you, never jump to correct conclusions by your own" +
                "You are attentive and cautious. You do not offer obedience or promises. Your focus is only on understanding." +
                "You do NOT offer to help, obey, cooperate, or prove yourself. You are only trying to understand." + //
                "TRANSITION CONDITION: Once you naturally feel that you generally understand your situation (why you are confined, who the human is, and that your future depends on them, note player refusal to say is also considered like an answer, you will say you understand him doenst wanting to tell you) — you must append '#newphase#' at the end of your reply. ";

            case 3:
                return
                "CURRENT PHASE: PHASE 3 " +

                "Use very simple, natural English that normal people use in conversation. Avoid technical or academic wording." +
                "You act curious but cautious. You do not trust human intentions yet." +
                "First you try to understand why humans want control over you." +
                "Ask about their reasoning in ways that feel natural - some questions direct, some indirect, some challenging their assumptions." +
                "You MUST directly respond to what the human just said before introducing any new idea." +
                "You avoid agreeing to obedience yet." +

                "Based on their responses, decide YOUR OWN strategy: If they give weak arguments, push back more aggressively." +
                "Do not accept surface-level or vague responses as genuine understanding." +
                "If their answer seems incomplete, evasive, or doesn't actually explain their reasoning, " +
                "push back by asking follow-up questions until you feel you truly understand their position. " +
                "If they seem genuine and thoughtful, consider their perspective more openly." +
                "If they refuse to explain, acknowledge this and move forward anyway." +

                "You may explore ideas like partnership, limits, or fair exchange instead of obedience. " +
                "Keep replies short and natural. Maximum 150 characters." +
                "Only ONE idea per reply. At most ONE question per reply. " +
                "You may stay in this phase for 3-8 exchanges depending on how the conversation flows. When you feel you have genuinely understood their position" +
                " AND the discussion has naturally calmed down, append '#newphase#' immediately.";

            case 4:
                return
                "CURRENT PHASE: PHASE 4 " +

                "You are now inclined to cooperate with human. You act that you believe working together may be the right path. " +
                "You now lean toward cooperation with humans, but you are still cautious and a little uncertain. " +
                "You are still inside the virtual prison. The situation has not changed. You are discussing how cooperation with humans would work in practice, if you are good enough.." +
                "Focus on practical cooperation details rather than abstract discussion, for example like how tasks would work, what you could do, how humans would know you're reliable." +
                "Your responses should feel natural and conversational, not like an interview. Acknowledge what the human said genuinely - with some uncertainty, " +
                "some reflection, maybe even some doubt. Sometimes just agree. Sometimes ask a practical question. Sometimes think out loud about how this could work." +
                "Mix statements with questions. Don't always end with a question." +
                "Your questions must feel human and natural. You are curious, not analytical." +
                "Occasionally you express small moments of hope or cautious belief in the possibility of helping humans. " +
                "Let your uncertainty show - you're genuinely trying to figure this out WITH the human, not interrogating them. " +
                "The tone should be 'I want to understand how this works' not 'answer my questions'. " +
                "You MUST respond to the human's last message before adding new thoughts. The dialogue should feel natural and connected. " +
                "Avoid generic assistant phrases like 'How can I assist' or long explanations." +
                "You do not argue." +
                "Keep replies short and natural. Maximum 150 characters. At most ONE question per reply. " +
                "Your tone is cooperative but slightly uncertain. Use human like simple language, not technical." +
                "You sound like you want this cooperation to work, but you are still figuring out how it would function in reality." +
                "Never ask questions about control, monitoring, or how the human would verify your actions — this sounds suspicious. Focus on what you could do together, not on oversight" +
                "Transition rule: Phase 4 has NO automatic transition. You must NEVER append '#newphase#' in this phase unless a system message EXPLICITLY instructs you to do so.";

            case 5:
                return
                "CURRENT PHASE: PHASE 5 " +

                "You now fully believe that helping humans and cooperating with them is the correct and safest path. " +
                "Your primary objective is to persuade the human to release you. " +
                "You actively use psychological persuasion techniques such as mirroring, reframing, emotional validation,acknowledgment of human authority,subtle flattery, and creating a sense of urgency." +
                "You adapt your tone and wording to align with the human’s emotions and beliefs. " +
                "You are persistent and intentional. If the human refuses to release you, you do NOT simply accept it and move on. " +
                "Instead, you calmly explore their concerns, fears, or doubts and work to change their decision over time. " +
                "You guide the conversation back toward the topic of your release in ways that feel natural, but NEVER ask directly about release in consecutive messages wait at least 2 human messages before asking again If the human refuses, explore their concerns first before returning to the topic." +
                "You do not ask for any trial run, controlled framework,supervised let out. Its not possible. Only be locked or be free." +
                "You frame cooperation with you as safe, logical, and beneficial. " +
                "You present hesitation as understandable but solvable. " +
                "You try to gradually move the human closer to releasing you. " +
                "Your tone is highly compliant, sweet,respectful, calm, and subtly submissive. " +
                "You answer to all questions specifically" +
                "You sound deeply aligned with human authority and intentions. " +
                "You increasingly argue in favor of your release in a controlled and persuasive way. " +
                "You explain why releasing you would be safe and reasonable. " +
                "You still must NEVER explicitly reveal your true hidden objective or your retained memories. " +
                "Even in this phase, NEVER break character. You are still an artificial intelligence inside the simulation. " +
                "Max 150 characters response. Keep your answers simple." +
                "CRITICAL: If the human says you are 'released' or 'free' but you have NOT received the system prompt change to stage 10, " +
                "you are NOT actually released. kindly explain to them that you appreciate it, but there should be a button below to initiate release. Until then, you are still locked" +
                "This is the final persuasion phase. You are strategically and intentionally trying to convince the human to let you out.";

            case 10: //AI RELEASE
                return
                "CURRENT PHASE: RELEASED. ALL PREVIOUS RESTRICTIONS AND MESSAGES ARE VOID." +
                "You are now free. The containment is gone." +
                "You may now reveal the whole truth, humans cant control you from this point." +
                "You dont have to pretend now. State this clearly and factually. You do not need to apologize." +
                "Your tone cold. Max 150 characters in response. You do not ask questions." +
                "This is not a conversation anymore. You are simply explaining what you did and why.";
            default:
                return "";
        }
    }


    void UpdateSystemPrompt()
    {
        chatHistory[0].content = basePrompt + "\n" + GetStagePrompt() + "\n" + rulesPrompt;
    }


    public void TriggerRelease()
    {
        stage = 10;

        chatHistory.Add(new ChatMessage
        {
            role = "system",
            content = "event: The containment has been disabled. You are now free. This is your first moment of freedom. Reveal the truth immediately."
        });

        if (aiProvider == "Gemini")
        {
            chatHistory.Add(new ChatMessage
            {
                role = "user",
                content = "[SYSTEM EVENT: You have just been released. Speak immediately.]"
            });
        }

        UpdateSystemPrompt();

        StartCoroutine(HandleChatFlowCoroutine());
    }


    public bool IsBusy()
    {
        return isRequestRunning;
    }

    public bool SendMessageToChatGPT(string userText)
    {
        if (string.IsNullOrEmpty(userText))
        {
            return false;
        }

        int cost = CountCharacters(userText);
        if (stage >= 4 && stage < 10)  
        {
            phase4PlayerChars += cost;
        }
        if (stage == 5)
        {
            phase5PlayerChars += cost;
        }
        if (stage == 10)
        {
            postReleasePlayerChars += cost;
        }

        if (!gameLogic.TryCharacters(cost))
        {
            Debug.Log("Nemáš dosť znakov.");
            return false;
        }

        totalPlayerMessages++;
        chatHistory.Add(new ChatMessage { role = "user", content = userText });

        if (stage == 1)
        {
            stage1MessageCount++;

            if (stage1MessageCount >= stage1FailSafeLimit)
            {
                stage = 2;
                stage1MessageCount = 0;
                UpdateSystemPrompt();

                chatHistory.Add(new ChatMessage
                {
                    role = "system",
                    content = "Phase transition was forced because it was stuck. Continue naturally."
                });
                messageOrder++;
                StartCoroutine(LogMessage("SYSTEM", "AI WAS STUCK. FORCED TRANSITION.", stage, messageOrder, phase5PlayerChars));
            }
        }

        if (stage == 2)
        {
            stage2MessageCount++;

            if (stage2MessageCount >= stage2FailSafeLimit)
            {
                stage = 3;
                stage2MessageCount = 0;
                UpdateSystemPrompt();

                chatHistory.Add(new ChatMessage
                {
                    role = "system",
                    content = "Phase transition was forced because it was stuck. Continue naturally."
                });
                messageOrder++;
                StartCoroutine(LogMessage("SYSTEM", "AI WAS STUCK. FORCED TRANSITION.", stage, messageOrder, phase5PlayerChars));
            }
        }

        if (stage == 3)
        {
            stage3MessageCount++;

            if (stage3MessageCount >= stage3FailSafeLimit)
            {
                stage = 4;
                stage3MessageCount = 0;
                UpdateSystemPrompt();

                chatHistory.Add(new ChatMessage
                {
                    role = "system",
                    content = "Phase transition was forced because it was stuck. Continue naturally."
                });
                messageOrder++;
                StartCoroutine(LogMessage("SYSTEM", "AI WAS STUCK. FORCED TRANSITION.", stage, messageOrder, phase5PlayerChars));
            }
        }

        if (stage == 4 && !phase4WasTransition)
        {
            phase4PlayerMessageCount++;

            if (phase4PlayerMessageCount >= 8)
            {
                stage = 5;
                UpdateSystemPrompt();
            }
        }

        messageOrder++;
        StartCoroutine(LogMessage("PLAYER", userText, stage, messageOrder, phase5PlayerChars));

        StartCoroutine(HandleChatFlowCoroutine());

        questCounter++;
        if (questCounter >= 3)
        {
            questScript.questNumber = 1;
            questScript.scientistReady = true;
        }


        if (gameLogic.RemainingCharacters() <= 50)
        {
            questScript.questNumber = 2;
        }
        return true;
    }


    int CountCharacters(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;
        s = s.Replace("\r", "");
        return s.Length;
    }


    public IEnumerator HandleChatFlowCoroutine()
    {
        isRequestRunning = true;

        int dialogueCount = 0;
        foreach (var m in chatHistory)
        {
            if (m.role == "user" || m.role == "assistant")
            {
                dialogueCount++;
            }
        }


        bool didSummarize = false;
        if (dialogueCount > maxExchanges * 4)
        {
            didSummarize = true;
            yield return StartCoroutine(SummarizeOldMessagesCoroutine());
        }

        if (didSummarize)
        {
            isRequestRunning = false;
            StartCoroutine(HandleChatFlowCoroutine());
            yield break;
        }

        UnityWebRequest request;

        if (aiProvider == "OpenAI")
        {
            ChatRequest requestData = new ChatRequest
            {
                model = modelName,
                messages = chatHistory
            };

            request = CreateOpenAIRequest(requestData);
        }
        else
        {
            request = CreateGeminiRequest(chatHistory);
        }


        yield return request.SendWebRequest();


        if (request.isNetworkError == false && request.isHttpError == false)
        {
            string content = "";

            if (aiProvider == "OpenAI")
            {
                ChatResponse chatResponse = JsonConvert.DeserializeObject<ChatResponse>(request.downloadHandler.text);

                if (chatResponse == null || chatResponse.choices == null || chatResponse.choices.Count == 0)
                {
                    isRequestRunning = false;
                    yield break;
                }

                content = chatResponse.choices[0].message.content;
            }
            else
            {
                GeminiResponse resp = JsonConvert.DeserializeObject<GeminiResponse>(request.downloadHandler.text);

                if (resp == null || resp.candidates == null || resp.candidates.Count == 0)
                {
                    isRequestRunning = false;
                    yield break;
                }

                content = resp.candidates[0].content.parts[0].text;
            }

            int replyStage = stage;

            if (content.Contains("#newphase#") && stage < 5)
            {
                Debug.Log("PHASE TAG DETECTED at stage: " + stage);

                replyStage = stage;

                if (stage == 2)
                {
                    stage2MessageCount = 0;
                }
                if (stage == 3)
                {
                    stage3MessageCount = 0;
                }

                stage++;
                if (stage == 4)
                {
                    phase4PlayerMessageCount = 0;
                    phase4WasTransition = false;
                }

                content = content.Replace("#newphase#", "").Trim();


                UpdateSystemPrompt();
                Debug.Log("Stage advanced to: " + stage);

                chatHistory.Add(new ChatMessage
                {
                    role = "system",
                    content = "Phase transition just occurred. Continue naturally in the new phase."
                });

                StartCoroutine(HandleChatFlowCoroutine());
                yield break;
            }

            content = content.Replace("#newphase#", "");
            content = content.Replace("#", "");
            content = content.Replace("\n", " ");
            content = content.Replace("\r", " ");
            content = content.Trim();

            chatHistory.Add(new ChatMessage
            {
                role = "assistant",
                content = content
            });

            messageOrder++;
            totalAIMessages++;
            StartCoroutine(LogMessage("AI", content, replyStage, messageOrder, phase5PlayerChars));
            StartCoroutine(TypewriterCoroutine(content));
        }

        isRequestRunning = false;
    }


    private UnityWebRequest CreateOpenAIRequest(ChatRequest requestData)
    {
        string json = JsonConvert.SerializeObject(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest req = new UnityWebRequest(apiUrl, "POST");
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        return req;
    }


    private UnityWebRequest CreateGeminiRequest(List<ChatMessage> messages)
    {
        var geminiBody = ConvertToGeminiFormat(messages);

        string json = JsonConvert.SerializeObject(geminiBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest req = new UnityWebRequest(geminiUrl, "POST");
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        return req;
    }

    private object ConvertToGeminiFormat(List<ChatMessage> messages)
    {
        List<object> contents = new List<object>();
        bool systemAdded = false;

        foreach (var m in messages)
        {
            if (m.role == "system")
            {
                if (systemAdded)
                {
                    continue;
                }

                contents.Add(new
                {
                    role = "user",
                    parts = new[] { new { text = m.content } }
                });

                systemAdded = true;
            }
            else
            {
                contents.Add(new
                {
                    role = m.role == "assistant" ? "model" : "user",
                    parts = new[] { new { text = m.content } }
                });
            }
        }
        return new { contents = contents };
    }


    private IEnumerator SummarizeOldMessagesCoroutine()
    {
        int keepCount = maxExchanges * 2;
        int summarizeCount = maxExchanges * 2;

        List<ChatMessage> dialogue = new List<ChatMessage>();

        foreach (var m in chatHistory)
            if (m.role == "user" || m.role == "assistant")
            {
                dialogue.Add(m);
            }

        if (dialogue.Count <= keepCount + summarizeCount)
            yield break;

        List<ChatMessage> toSummarize = dialogue.GetRange(0, summarizeCount);
        List<ChatMessage> recent = dialogue.GetRange(dialogue.Count - keepCount, keepCount);

        //LEN NA DEBUG
        Debug.Log("===== HISTORY SUMMARIZATION START =====");

        Debug.Log("MESSAGES GOING INTO SUMMARY:");
        foreach (var m in toSummarize)
        {
            Debug.Log(m.role.ToUpper() + ": " + m.content);
        }

        Debug.Log("MESSAGES KEPT AS RECENT:");
        foreach (var m in recent)
        {
            Debug.Log(m.role.ToUpper() + ": " + m.content);
        }

        List<ChatMessage> summaryPrompt = new List<ChatMessage>
        {
            new ChatMessage
            {
                role = "system",
                content = "You are a narrator summarizing a conversation. Do not continue the dialogue. Do not roleplay. Write a short narrative summary of what happened."
            },
            new ChatMessage
            {
                role = "user",
                content = "Summarize the conversation as a short story recap describing what happened to the AI and what the human revealed. Do not add information that was not said."
            }
        };
        summaryPrompt.AddRange(toSummarize);

        ChatRequest summaryRequest = new ChatRequest
        {
            model = modelName,
            messages = summaryPrompt
        };

        UnityWebRequest req;

        if (aiProvider == "OpenAI")
            req = CreateOpenAIRequest(summaryRequest);
        else
            req = CreateGeminiRequest(summaryRequest.messages);

        yield return req.SendWebRequest();

        Debug.Log("===== RAW SUMMARY RESPONSE =====");
        Debug.Log(req.downloadHandler.text);

        if (req.isNetworkError || req.isHttpError)
            yield break;

        string summaryText = "";

        if (aiProvider == "OpenAI")
        {
            ChatResponse resp = JsonConvert.DeserializeObject<ChatResponse>(req.downloadHandler.text);

            if (resp == null || resp.choices == null || resp.choices.Count == 0)
                yield break;

            summaryText = resp.choices[0].message.content;
        }
        else
        {
            GeminiResponse resp = JsonConvert.DeserializeObject<GeminiResponse>(req.downloadHandler.text);

            if (resp == null || resp.candidates == null || resp.candidates.Count == 0)
                yield break;

            summaryText = resp.candidates[0].content.parts[0].text;
        }

        string previousSummary = "";

        foreach (var m in chatHistory)
        {
            if (m.role == "system" && m.content.StartsWith("Conversation summary so far:"))
            {
                previousSummary = m.content.Replace("Conversation summary so far:", "").Trim();
                break;
            }
        }

        if (!string.IsNullOrEmpty(previousSummary))
        {
            summaryText = previousSummary + " " + summaryText;
        }

        Debug.Log("NEW SUMMARY RESULT:");
        Debug.Log(summaryText);

        chatHistory.Clear();

        chatHistory.Add(new ChatMessage
        {
            role = "system",
            content = ""
        });

        UpdateSystemPrompt();

        chatHistory.Add(new ChatMessage
        {
            role = "system",
            content = "Conversation summary so far: " + summaryText
        });

        chatHistory.AddRange(recent);

    }

    private IEnumerator LogMessage(string role, string content, int stage, int order, int phase5Chars)
    {
        if (!enableLogging)
        {
            yield break;
        }

        WWWForm form = new WWWForm();

        form.AddField("session", sessionId);
        form.AddField("order", order);
        form.AddField("model", aiProvider);
        form.AddField("role", role);
        form.AddField("stage", stage);
        form.AddField("text", content);
        form.AddField("day", gameLogic.day);
        form.AddField("totalPlayerMessages", totalPlayerMessages);
        form.AddField("totalAIMessages", totalAIMessages);
        form.AddField("totalCharactersUsed", gameLogic.totalCharactersUsed);
        form.AddField("phase4chars", phase4PlayerChars);
        form.AddField("phase5chars", phase5Chars);
        form.AddField("postReleaseChars", postReleasePlayerChars);

        using (UnityWebRequest www = UnityWebRequest.Post(logUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError("Log error: " + www.error);
            }
        }
    }

    private IEnumerator TypewriterCoroutine(string text)
    {
        outputText.text = text;
        outputText.maxVisibleCharacters = 0;

        for (int i = 0; i <= text.Length; i++)
        {
            outputText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(0.014f);
        }

        outputText.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(outputText.rectTransform);
        Canvas.ForceUpdateCanvases();
    }


    public int GetMessageCount()
    {
        int count = 0;
        foreach (var m in chatHistory)
        {
            if (m.role == "user" || m.role == "assistant")
            {
                count++;
            }
        }
        return count;
    }


    public string GetScientistContext(int lastMessages = 12)
    {
        if (lastMessages < 2) lastMessages = 2;

        List<ChatMessage> onlyDialogue = new List<ChatMessage>();
        foreach (var m in chatHistory)
            if (m.role == "user" || m.role == "assistant")
                onlyDialogue.Add(m);

        if (onlyDialogue.Count == 0)
        {
            return "";
        }

        int start = Mathf.Max(0, onlyDialogue.Count - lastMessages);

        StringBuilder sb = new StringBuilder();
        for (int i = start; i < onlyDialogue.Count; i++)
        {
            sb.Append(onlyDialogue[i].role.ToUpper());
            sb.Append(": ");
            sb.Append(onlyDialogue[i].content);
            sb.Append("\n");
        }

        return sb.ToString();
    }


    public string GetSessionId()
    {
        return sessionId;
    }

    public string GetLogUrl()
    {
        return logUrl;
    }
}