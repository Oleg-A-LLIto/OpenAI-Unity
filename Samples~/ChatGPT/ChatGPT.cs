using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OpenAI
{
    public class ChatGPT : MonoBehaviour
    {
        [SerializeField] private InputField inputField;
        [SerializeField] private Button button;
        [SerializeField] private ScrollRect scroll;
        
        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;

        private float height;
        private OpenAIApi openai = new OpenAIApi(/*OpenAI Key can go here.*/);

        private List<ChatMessage> messages = new List<ChatMessage>();

        private PersonaExample personaExample = new PersonaExample();

        private void Start()
        {
            button.onClick.AddListener(SendReply);
        }

        private void AppendMessageToUI(ChatMessage message)
        {
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

            var item = Instantiate(message.Role == "user" ? sent : received, scroll.content);
            item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.Content;
            item.anchoredPosition = new Vector2(0, -height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            height += item.sizeDelta.y;
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scroll.verticalNormalizedPosition = 0;
        }

        private ChatMessage HandleFunctionCalling(FunctionCall? functionCall)
        {
            string functionName = functionCall?.Name;
            string msgContent = "";
            Debug.Log("functionName :: " + functionName);
            if (functionName == "report_pain_level") {
                Debug.Log("Add pain level to message");
                msgContent = personaExample.content_pain;
            }
            if (functionName == "enter_wheelchair") {
                Debug.Log("Add patient in wheelchair to message");
                msgContent = personaExample.content_wheelchair;
            }
            Debug.Log("=================================");
            return new ChatMessage()
            {
                Role = "function",
                Name = functionName,
                Content = msgContent
            };
        }

        private async void SendReply()
        {
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = inputField.text
            };
            
            AppendMessageToUI(newMessage);

            if (messages.Count == 0) newMessage.Content = personaExample.prompt + "\n" + inputField.text; 
            
            messages.Add(newMessage);
            
            button.enabled = false;
            inputField.text = "";
            inputField.enabled = false;
            
            // Step 1: send the conversation and available functions to GPT
            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-4o-mini",
                Messages = messages
				Functions = personaExample.functions
            });

            // Step 2: check if GPT wanted to call a function
            bool MessageHasFunctionCalling = (completionResponse.Choices[0].Message.FunctionCall != null);
            if (MessageHasFunctionCalling)
            {
                // Step 3: call the function
                Debug.Log("Function calling DETECTED!");
                FunctionCall? functionCall = completionResponse.Choices[0].Message.FunctionCall;
                ChatMessage funcMsg = HandleFunctionCalling(functionCall);
                messages.Add(funcMsg);

                // Step 4: send the info on the function call and function response to GPT
                var completionResponse2 = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
                {
                    Model = "gpt-3.5-turbo-0613",
                    Messages = messages
                });

                bool functionResponseHasChoices = (completionResponse2.Choices != null && completionResponse2.Choices.Count > 0);
                if (functionResponseHasChoices) {
                    var message = completionResponse2.Choices[0].Message;
                    message.Content = message.Content.Trim();
                    
                    messages.Add(message);
                    AppendMessageToUI(message);
                }
            }

            bool responseHasChoices = (completionResponse.Choices != null && completionResponse.Choices.Count > 0);
            if (responseHasChoices)
            {
                if (completionResponse.Choices[0].Message.FunctionCall == null)
                {
                    var message = completionResponse.Choices[0].Message;
                    message.Content = message.Content.Trim();
                    
                    messages.Add(message);
                    AppendMessageToUI(message);
                }
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }

            button.enabled = true;
            inputField.enabled = true;
        }
    }
}
