using UnityEngine;
using System.Collections.Generic;

public class Interpreter : MonoBehaviour, IInterpreter
{
    public bool IsEnterPressed { get; set; }
    public bool IsInterpreterOn { get; set; }
    
    
    public List<string> Interpret(string input)
    {
        var output = new List<string>();
        //
        // fill output
        //
        return output;
    }
    
    public void ReleaseEnter()
    {
        IsEnterPressed = false;
    }
}
