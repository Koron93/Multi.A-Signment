using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class ChatManager : NetworkBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField inputField; // Use TMP_InputField
    public TMP_Text chatText; // Use TMP_Text for the chat log
    public Button sendButton; // Button for sending messages
    public ScrollRect chatScrollRect; // ScrollRect for the chat container

    private List<string> chatMessages = new List<string>();
    private const int MaxMessages = 10; // Limit the chat to the last 10 messages
    private bool isAtBottom = true; // Check if the user is at the bottom of the chat

    void Start()
    {
        // Set up the button listener
        sendButton.onClick.AddListener(SendMessage);
    }

    // Called when the Send button is clicked
    void SendMessage()
    {
        string message = inputField.text;
        if (!string.IsNullOrEmpty(message))
        {
            // Send the message to the server via a ServerRpc
            SendMessageToServerRpc(message);
            inputField.text = ""; // Clear the input field
        }
    }

    // ServerRpc to send a message to all clients
    [ServerRpc(RequireOwnership = false)]
    public void SendMessageToServerRpc(string message)
    {
        // Broadcast the message to all clients
        BroadcastMessageToClientRpc(message);
    }

    // ClientRpc to broadcast the message to all clients
    [ClientRpc]
    void BroadcastMessageToClientRpc(string message)
    {
        // Add the new message to the chat log
        chatMessages.Add(message);

        // If the chat log exceeds the maximum message count, remove the oldest message
        if (chatMessages.Count > MaxMessages)
        {
            chatMessages.RemoveAt(0); // Remove the first (oldest) message
        }

        // If the user is at the bottom of the chat, update the chat UI and scroll down
        if (isAtBottom)
        {
            UpdateChatUI();
            ScrollToBottom();
        }
    }

    // Update the chat UI with the new messages
    void UpdateChatUI()
    {
        chatText.text = string.Join("\n", chatMessages); // Join messages with newlines
    }

    // Scroll to the bottom of the chat log
    void ScrollToBottom()
    {
        chatScrollRect.verticalNormalizedPosition = 0f; // Scroll to the bottom
    }

    // Method to check if the user is at the bottom of the chat
    public void CheckScrollPosition()
    {
        // If the scroll position is near the bottom, allow auto-scrolling
        isAtBottom = chatScrollRect.verticalNormalizedPosition <= 0.01f;
    }
}