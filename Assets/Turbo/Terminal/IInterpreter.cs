using System.Collections.Generic;

public interface IInterpreter
{
    bool IsEnterPressed { get; set; }
    bool IsInterpreterOn { get; set; }

    List<string> Interpret(string input);
    void ReleaseEnter();
}
