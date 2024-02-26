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

public class InteractionNPC : MonoBehaviour
{
    [SerializeField] private string freshStateText = "";
    private OpenAIAPI api;
    private List<ChatMessage> messages;

    [Header("Voice")]
    [SerializeField] private AppVoiceExperience appVoiceExperience;
    // Start is called before the first frame update
    void Start()
    {
        api = new OpenAIAPI(Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.User));
        messages = new List<ChatMessage> {
            new ChatMessage(ChatMessageRole.System, "Questo è un altro messaggio di test")
        };
        appVoiceExperience.Activate(); 
    }
    private void OnEnable()
    {
        appVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener(OnRequestTranscript);
        appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener(OnRequestTranscript);
        appVoiceExperience.VoiceEvents.OnResponse.AddListener(OnRequestResponse);
        appVoiceExperience.VoiceEvents.OnError.AddListener(OnRequestError);
    }
    private void OnDisable()
    {
        appVoiceExperience.VoiceEvents.OnPartialTranscription.RemoveListener(OnRequestTranscript);
        appVoiceExperience.VoiceEvents.OnFullTranscription.RemoveListener(OnRequestTranscript);
        appVoiceExperience.VoiceEvents.OnResponse.RemoveListener(OnRequestResponse);
        appVoiceExperience.VoiceEvents.OnError.RemoveListener(OnRequestError);
    }
    private void OnRequestTranscript(string transcript)
    {
        freshStateText = transcript;
        Debug.Log(freshStateText);
    }
    private void OnRequestResponse(WitResponseNode response)
    {
        Debug.Log("Hai detto ");
        Debug.Log(response["text"]);

    }
    private void OnRequestError(string error, string message)
    {
        Debug.Log(error);
        Debug.Log(message);
    }
    private async void GetResponse()
    {

        // Fill the user message from the input field
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
            Model = Model.ChatGPTTurbo,
            Temperature = 0.9,
            MaxTokens = 50,
            Messages = messages
        });

        // Get the response message
        ChatMessage responseMessage = new ChatMessage();
        responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.Content = chatResult.Choices[0].Message.Content;
        Debug.Log(string.Format("{0}: {1}", responseMessage.rawRole, responseMessage.Content));

        // Add the response to the list of messages
        messages.Add(responseMessage);
    }
    // Update is called once per frame

    void Update()
    {
        
    }
}
