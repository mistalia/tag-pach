using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    [SerializeField, TextArea(3, 6)] private string message;

    [SerializeField] private Sprite portrait;

    public string Message => message;
    public Sprite Portrait => portrait;

}


[CreateAssetMenu(fileName = "Dialogue_Data",menuName = ("Dialogue_Data"))]
public class DialogueData : ScriptableObject
{
    [SerializeField]
    private List<DialogueLine> lines = new();
    public int Count => lines.Count;
    public bool IsFinished = false;

    public bool TryGetLine(int index,out DialogueLine line)
    {
        line = null;

        if(index < 0 || index >= lines.Count)
        {
            return false;
        }

        line = lines[index];
        return line != null;
    }

}
