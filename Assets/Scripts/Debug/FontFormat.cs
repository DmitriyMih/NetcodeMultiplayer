using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FontFormat : MonoBehaviour
{
    private string _prefix;

    private string _suffix;

    public static FontFormat Bold = new FontFormat("b");
    public static FontFormat Italic = new FontFormat("i");
    private FontFormat(string format)
    {
        _prefix = $"<{format}>";
        _suffix = $"</{format}>";
    }

    public static string operator %(string text, FontFormat textFormat)
    {
        return textFormat._prefix + text + textFormat._suffix;
    }
}