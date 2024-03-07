using Meta.WitAi.Json;
using Oculus.Voice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Meta.WitAi.TTS.Utilities;
using Oculus.Interaction.Deprecated;
using TMPro;

public class InteractionNPC : MonoBehaviour
{
    [SerializeField] private string freshStateText = "";
    private OpenAIAPI api;
    private List<ChatMessage> messages;
    private bool isVoiceActivated = false;
    private IEnumerator coroutine;

    [Header("Voice")]
    [SerializeField] private AppVoiceExperience appVoiceExperience;
    //[SerializeField] private AppVoiceExperience activatorVoiceExperience;
    [SerializeField] private TTSSpeaker speaker;
    [SerializeField]
    private TMP_Text text;
    [SerializeField]
    public GameObject model;
    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        api = new OpenAIAPI("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"/*Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.User)*/);
        messages = new List<ChatMessage> {
            new ChatMessage(ChatMessageRole.System, "Questo è un altro messaggio di test")
        };
        animator = model.GetComponent<Animator>();
        //appVoiceExperience.Activate();
    }
    private void OnEnable()
    {
        appVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener(OnRequestTranscript);
        appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener(OnRequestTranscript);
        appVoiceExperience.VoiceEvents.OnResponse.AddListener(OnRequestResponse);
        appVoiceExperience.VoiceEvents.OnError.AddListener(OnRequestError);
        appVoiceExperience.VoiceEvents.OnStartListening.AddListener(OnStartListening);
        appVoiceExperience.VoiceEvents.OnStoppedListening.AddListener(OnStopListening);
        //activatorVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener(OnRequestTranscriptActivation);
        //activatorVoiceExperience.VoiceEvents.OnFullTranscription.AddListener(OnRequestTranscriptActivation);
        //activatorVoiceExperience.VoiceEvents.OnResponse.AddListener(OnActivationResponse);
        //activatorVoiceExperience.VoiceEvents.OnError.AddListener(OnRequestError);

    }
    private void OnDisable()
    {
        appVoiceExperience.VoiceEvents.OnPartialTranscription.RemoveListener(OnRequestTranscript);
        appVoiceExperience.VoiceEvents.OnFullTranscription.RemoveListener(OnRequestTranscript);
        appVoiceExperience.VoiceEvents.OnResponse.RemoveListener(OnRequestResponse);
        appVoiceExperience.VoiceEvents.OnError.RemoveListener(OnRequestError);
        appVoiceExperience.VoiceEvents.OnStartListening.RemoveListener(OnStartListening);
        appVoiceExperience.VoiceEvents.OnStoppedListening.RemoveListener(OnStopListening);
        //activatorVoiceExperience.VoiceEvents.OnPartialTranscription.RemoveListener(OnRequestTranscriptActivation);
       // activatorVoiceExperience.VoiceEvents.OnFullTranscription.RemoveListener(OnRequestTranscriptActivation);
        // activatorVoiceExperience.VoiceEvents.OnError.RemoveListener(OnRequestError);
    }
    public void activateVoice()
    {
        if (isVoiceActivated == false)
        {
            text.text = "Activating...";
            GameObject.Find("Pulsante").GetComponentInChildren<TextMeshProUGUI>().text = "Listening...";//.GetComponentInChildren<Text>().text = "Talking...";
            isVoiceActivated = true;
            appVoiceExperience.Activate();
        }
        else
        {
            text.text = "Deactivating...";
            deactivateVoice();
        }
    }
    public void deactivateVoice()
    {
        isVoiceActivated = false;
        GameObject.Find("Pulsante").GetComponentInChildren<TextMeshProUGUI>().text = "Push to talk";
        appVoiceExperience.Deactivate();
        text.text = "Message stopped";
        speaker.Stop();
    }

    private void OnStartListening()
    {
        
    }
    private void OnStopListening()
    {

    }
    private void OnRequestTranscript(string transcript)
    {
        freshStateText = transcript;
    }
    private void OnRequestResponse(WitResponseNode response)
    {
        Debug.Log("Hai detto ");
        Debug.Log(response["text"]);
        //text.text = "You said: " + response["text"];
        //appVoiceExperience.Deactivate();
        //if(response["text"].ToString().Length != 2)
        //speaker.Speak("You said "+ response["text"]);

        //appVoiceExperience.Activate();
        Debug.Log(response["text"].ToString().Trim().Equals("Goodbye", StringComparison.OrdinalIgnoreCase));
        string test = response["text"].ToString().Trim();


        if (test.Equals("Goodbye"))
        {
            speaker.Speak("Ok, i hope you enjoyed our little conversation!. See you soon");
            appVoiceExperience.Deactivate();
        }
        else
        {
            GetResponse();
        }
    }
    private void OnRequestTranscriptActivation(string transcript)
    {
       // activator = transcript;
    }
    private void OnActivationResponse(WitResponseNode response)
    {
        if (response["text"] == "Ready")
        {
            Debug.Log("Activation ready: ");
          //  appVoiceExperience.Activate();
          //  activatorVoiceExperience.Deactivate();
        }
    }
    private void OnRequestError(string error, string message)
    {
        Debug.Log(error);
        Debug.Log(message);
    }
    private async void GetResponse()
    {

        // Fill the user message from the input field
        GameObject.Find("Pulsante").GetComponentInChildren<TextMeshProUGUI>().text = "Thinking...";
        animator.Play("Talk");
        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.Content = freshStateText;
        if (userMessage.Content.Length > 100)
        {
            // Limit messages to 100 characters
            userMessage.Content = userMessage.Content.Substring(0, 100);
        }
        Debug.Log(string.Format("{0}: {1}", userMessage.rawRole, userMessage.Content));

        // Add the message to the list
        messages.Add(userMessage);

        // Send the entire chat to OpenAI to get the next message
        var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
        {
            Model = "gpt-4-turbo-preview",
            Temperature = 0.9,
            MaxTokens = 125,
            Messages = messages
        });

        // Get the response message
        ChatMessage responseMessage = new ChatMessage();
        responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.Content = chatResult.Choices[0].Message.Content;
        Debug.Log(string.Format("{0}: {1}", responseMessage.rawRole, responseMessage.Content));
        text.text = responseMessage.Content;
        // Add the response to the list of messages
        messages.Add(responseMessage);
        coroutine = TalkGPT(responseMessage);
        StartCoroutine(coroutine);
        //appVoiceExperience.Deactivate
    }
    IEnumerator TalkGPT(ChatMessage responseMessage)
    {
        animator.Play("Thinking");
        GameObject.Find("Pulsante").GetComponentInChildren<TextMeshProUGUI>().text = "Talking...";
        yield return speaker.SpeakAsync(responseMessage.Content);
        GameObject.Find("Pulsante").GetComponentInChildren<TextMeshProUGUI>().text = "Push to talk";
        isVoiceActivated = false;
        animator.Play("Idle");
        // activatorVoiceExperience.Activate();
    }
    // Update is called once per frame

    void Update()
    {
        
    }
}
