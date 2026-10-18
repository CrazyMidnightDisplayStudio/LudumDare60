using TMPro;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerminalTurbo))]
[CanEditMultipleObjects]
public class TerminalTurboEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (!DrawDefaultInspector())
            return;

        foreach (TerminalTurbo terminal in targets)
        {
            TMP_Text[] texts = terminal.GetComponentsInChildren<TMP_Text>(true);

            if (!Application.isPlaying)
            {
                Undo.RecordObjects(texts, "Change Terminal Font Size");
            }

            terminal.ApplyFontSize();

            if (Application.isPlaying)
                continue;

            foreach (TMP_Text text in texts)
            {
                EditorUtility.SetDirty(text);

                if (PrefabUtility.IsPartOfPrefabInstance(text))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(text);
                }
            }
        }
    }
}
