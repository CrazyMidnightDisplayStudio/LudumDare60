using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class RadioConversation
{
    [SerializeField] private string title;
    [SerializeField, TextArea(2, 6)] private string[] lines = Array.Empty<string>();

    public string Title => title;
    public IReadOnlyList<string> Lines => lines ?? Array.Empty<string>();
}
