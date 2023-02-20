using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CustomChatUI : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private Message messagePrefab;

    [SerializeField] private int messagesCount = 8;

    private void Start()
    {
        if (CustomChatManager.Instance != null)
            CustomChatManager.Instance.OnSetMessage += PrintMessage;
    }

    private void PrintMessage(string newMessage)
    {
        if (content == null || messagePrefab == null)
            return;

        Message message = Instantiate(messagePrefab, content);
        message.SetMessage($"{System.DateTime.Now.Hour}:{System.DateTime.Now.Minute.ToString()}" % Colorize.Yellow % FontFormat.Bold + " " + newMessage % Colorize.Green % FontFormat.Bold);

        if (content.childCount >= messagesCount)
            Destroy(content.GetChild(0).gameObject);
    }
}