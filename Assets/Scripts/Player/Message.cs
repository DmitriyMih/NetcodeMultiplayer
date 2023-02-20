using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Message : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}