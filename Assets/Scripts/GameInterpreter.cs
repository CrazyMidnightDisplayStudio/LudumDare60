using System.Collections.Generic;
using UnityEngine;

public class GameInterpreter : MonoBehaviour, IInterpreter
{
    public bool IsEnterPressed { get; set; }
    public bool IsInterpreterOn { get; set; } = true;

    public List<string> Interpret(string input)
    {
        var output = new List<string>();

        if (input == "HELP")
        {
            output.Add("no help");
            return output;
        }

        if (input == "HELLO")
        {
            output.Add("ola!");
            return output;
        }
        

        return output;
    }

    public void ReleaseEnter()
    {
        IsEnterPressed = false;
    }
}
