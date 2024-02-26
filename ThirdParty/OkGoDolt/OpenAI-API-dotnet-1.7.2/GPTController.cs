using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPTController : MonoBehaviour
{
    public TMP_Text textfield;
    public TMP_InputField inputField;
    public Button okButton;

    private openAIApi api;
    private List <ChatMessage> messages;

    // Start is called before the first frame update
    void Start()
    {
        api = new openAIApi(Environment.GetEnvironmentVariable("OPEN_AI_KEY", EnvironmentVariableTarget.User));
        StartConversation();
        okButton.onClick.AddListener(() => GetResponse());    
    }
    private void StartConversation(){
        messages = new List<ChatMessage> {
            new ChatMessage (ChatMessageRole.System, "Salve, questo è un messaggio di test fatto per inizializzare questo esperimento");
        }
        inputField.text = "";
        string startString = "Questo è un altro messaggio di test sempre per inizializzare questo esperimento";
        textfield.text = startString;
    }

    private async void GetResponse(){
        if(inputField.text.Length < 1){
            return ;
        }
        okButton.enabled = false;

        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.Content = inputField.text;
        if(userMessage.Content.Length > 100){
            userMessage.Content = userMessage.Content.Substring(0, 100);
        }
        messages.Add(userMessage);
        textField.text = string.Format("Tu: {0}", userMessage.Content);
        inputField.text = "";
        var chatResult = await api.Chat-CreateChatCompletionAsync(new ChatRequest(){
            Model = Model.ChatGPTTurbo,
            Temperature = 0.1,
            MaxTokens = 50,
            Messages = messages
        });

        ChatMessage responseMessage = new ChatMessage();
        responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.Content = chatResult.Choices[0].Message.Content;

        messages.Add(responseMessage);

        textField.text = string.Format("Tu: {0}\n\nOracolo: {1}", userMessage.Content, responseMessage.Content);
        okButton.enabled = true;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
