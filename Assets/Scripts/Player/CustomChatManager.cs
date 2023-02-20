using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomChatManager : MonoBehaviour
{
    public static CustomChatManager Instance;

    [SerializeField] private Button sendButton;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private string tempMessage;

    public Action<string> OnSendMeaage;
    public Action<string> OnSetMessage;

    private void Awake()
    {
        Instance = this;

        if (sendButton != null)
            sendButton.onClick.AddListener(() => Send());
    }

    public void SetMessage(string newMessage)
    {
        tempMessage = newMessage;
    }

    private void Send()
    {
        if (tempMessage == "")
            return;

        OnSendMeaage?.Invoke(tempMessage);
        tempMessage = "";
        inputField.text = "";
    }
}