using System;
using System.Collections.Generic;
using System.IO;
using LD60;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GameStoryImporter
{
    private const string MenuPath = "Tools/Ludum Dare 60/Import Story into Selected GameManager";
    private const string StoryPath = "Assets/Scripts/Editor/GameStory.json";
    private const string UndoName = "Import Game Story";

    [Serializable]
    private sealed class StoryData
    {
        public PuzzleData[] puzzles;
        public CommandData[] commands;
        public string finalText;
        public int contactFrequency;
        public string speakerName;
        public ConversationData introduction;
        public StationData[] secretStations;
    }

    [Serializable]
    private sealed class PuzzleData
    {
        public string password;
        public string terminalText;
        public string correctResponse;
        public string wrongResponse;
        public ConversationData[] radioMonologues;
    }

    [Serializable]
    private sealed class CommandData
    {
        public string phrase;
        public string response;
        public int minSolvedPasswords;
        public int maxSolvedPasswords;
    }

    [Serializable]
    private sealed class ConversationData
    {
        public string title;
        public string[] lines;
    }

    [Serializable]
    private sealed class StationData
    {
        public int frequency;
        public string speakerName;
        public ConversationData[] monologues;
        public bool repeatAfterLast;
    }

    [MenuItem(MenuPath, true)]
    private static bool CanImport()
    {
        return !EditorApplication.isPlayingOrWillChangePlaymode && Selection.gameObjects.Length == 1 &&
               Selection.activeGameObject.GetComponent<GameManager>() != null;
    }

    [MenuItem(MenuPath)]
    private static void Import()
    {
        int undoGroup = -1;

        try
        {
            if (!CanImport())
                throw new InvalidOperationException("Select one scene GameObject containing GameManager, outside Play Mode.");

            GameManager manager = Selection.activeGameObject.GetComponent<GameManager>();
            var scene = manager.gameObject.scene;

            if (!scene.IsValid() || !scene.isLoaded || !scene.path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Open and save a scene before importing. Prefab assets cannot be imported into.");

            RadioMonologue radio = manager.GetComponent<RadioMonologue>();

            if (radio == null)
                throw new InvalidOperationException("The selected GameManager must also contain RadioMonologue.");

            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(StoryPath);

            if (asset == null)
                throw new InvalidOperationException($"Story data was not found at {StoryPath}.");

            StoryData data = JsonUtility.FromJson<StoryData>(asset.text);
            ValidateData(data);
            TerminalTurbo terminal = FindTerminal(manager);
            var managerObject = new SerializedObject(manager);
            var radioObject = new SerializedObject(radio);
            var managerReferences = CaptureReferences(managerObject, "terminal");
            var radioReferences = CaptureReferences(radioObject);
            SerializedProperty puzzles = Property(managerObject, "puzzles");

            if (puzzles.arraySize != data.puzzles.Length)
                throw new InvalidOperationException("GameManager must already contain five puzzle slots. Their order and LED references are preserved.");

            string firstCorrectResponse = Child(puzzles.GetArrayElementAtIndex(0), "correctResponse").stringValue;

            // These edits remain inside SerializedObject until ApplyModifiedProperties is called.
            WriteManager(managerObject, data, terminal);
            WriteRadio(radioObject, data, manager);

            string projectPath = Directory.GetParent(Application.dataPath).FullName;
            string backupDirectory = Path.Combine(projectPath, "Library", "StoryBackups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backupDirectory);
            string backupPath = Path.Combine(backupDirectory, Path.GetFileName(scene.path));
            File.Copy(Path.Combine(projectPath, scene.path), backupPath);

            Undo.IncrementCurrentGroup();
            undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(UndoName);
            Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { manager, radio }, UndoName);
            managerObject.ApplyModifiedProperties();
            radioObject.ApplyModifiedProperties();

            VerifyImport(new SerializedObject(manager), new SerializedObject(radio), data, terminal, firstCorrectResponse);
            VerifyReferences(new SerializedObject(manager), managerReferences);
            VerifyReferences(new SerializedObject(radio), radioReferences);

            if (PrefabUtility.IsPartOfPrefabInstance(manager))
                PrefabUtility.RecordPrefabInstancePropertyModifications(manager);

            if (PrefabUtility.IsPartOfPrefabInstance(radio))
                PrefabUtility.RecordPrefabInstancePropertyModifications(radio);

            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);
            Debug.Log($"Story imported into {scene.path} / {manager.name}. Check the Inspector, then save the scene. " +
                      $"Undo reverts this import. A copy of the last saved scene is at {backupPath}. " +
                      "Existing commands, other stations, UI, LED and audio references were preserved.", manager);
        }
        catch (Exception exception)
        {
            if (undoGroup >= 0)
                Undo.RevertAllDownToGroup(undoGroup);

            Debug.LogError($"Story import failed: {exception.Message}. The import was not kept.");
        }
    }

    private static TerminalTurbo FindTerminal(GameManager manager)
    {
        TerminalTurbo result = null;

        foreach (TerminalTurbo terminal in UnityEngine.Object.FindObjectsByType<TerminalTurbo>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (terminal.gameObject.scene != manager.gameObject.scene ||
                Property(new SerializedObject(terminal), "interpreterObject").objectReferenceValue != manager.gameObject)
                continue;

            if (result != null)
                throw new InvalidOperationException("More than one terminal points to the selected GameManager.");

            result = terminal;
        }

        if (result == null)
            throw new InvalidOperationException("No terminal in this scene points to the selected GameManager through Interpreter Object.");

        return result;
    }

    private static void WriteManager(SerializedObject target, StoryData data, TerminalTurbo terminal)
    {
        Property(target, "terminal").objectReferenceValue = terminal;
        Property(target, "finalText").stringValue = data.finalText;
        SerializedProperty puzzles = Property(target, "puzzles");

        for (int i = 0; i < data.puzzles.Length; i++)
        {
            PuzzleData source = data.puzzles[i];
            SerializedProperty puzzle = puzzles.GetArrayElementAtIndex(i);
            Child(puzzle, "password").stringValue = source.password;
            Child(puzzle, "terminalText").stringValue = source.terminalText;
            Child(puzzle, "wrongResponse").stringValue = source.wrongResponse;

            if (!string.IsNullOrWhiteSpace(source.correctResponse))
                Child(puzzle, "correctResponse").stringValue = source.correctResponse;

            WriteConversations(Child(puzzle, "radioMonologues"), source.radioMonologues);
        }

        SerializedProperty commands = Property(target, "commands");

        foreach (CommandData source in data.commands)
        {
            int index = FindCommand(commands, source);

            if (index < 0)
                index = commands.arraySize++;

            SerializedProperty command = commands.GetArrayElementAtIndex(index);
            Child(command, "phrase").stringValue = source.phrase;
            Child(command, "response").stringValue = source.response;
            Child(command, "minSolvedPasswords").intValue = source.minSolvedPasswords;
            Child(command, "maxSolvedPasswords").intValue = source.maxSolvedPasswords;
        }
    }

    private static void WriteRadio(SerializedObject target, StoryData data, GameManager manager)
    {
        if (Property(target, "gameManager").objectReferenceValue != manager)
            throw new InvalidOperationException("RadioMonologue must already reference the selected GameManager.");

        Property(target, "contactFrequency").intValue = data.contactFrequency;
        Property(target, "speakerName").stringValue = data.speakerName;
        WriteConversation(Property(target, "introduction"), data.introduction);
        SerializedProperty stations = Property(target, "secretStations");

        for (int i = 0; i < stations.arraySize; i++)
        {
            if (Child(stations.GetArrayElementAtIndex(i), "frequency").intValue == data.contactFrequency)
                throw new InvalidOperationException("A secret station already uses the contact frequency.");
        }

        foreach (StationData source in data.secretStations)
        {
            int index = FindStation(stations, source.frequency);

            if (index < 0)
                index = stations.arraySize++;

            SerializedProperty station = stations.GetArrayElementAtIndex(index);
            Child(station, "frequency").intValue = source.frequency;
            Child(station, "speakerName").stringValue = source.speakerName;
            Child(station, "repeatAfterLast").boolValue = source.repeatAfterLast;
            WriteConversations(Child(station, "monologues"), source.monologues);
        }
    }

    private static void WriteConversations(SerializedProperty target, ConversationData[] data)
    {
        target.arraySize = data.Length;

        for (int i = 0; i < data.Length; i++)
            WriteConversation(target.GetArrayElementAtIndex(i), data[i]);
    }

    private static void WriteConversation(SerializedProperty target, ConversationData data)
    {
        Child(target, "title").stringValue = data.title;
        SerializedProperty lines = Child(target, "lines");
        lines.arraySize = data.lines.Length;

        for (int i = 0; i < data.lines.Length; i++)
            lines.GetArrayElementAtIndex(i).stringValue = data.lines[i];
    }

    private static int FindCommand(SerializedProperty commands, CommandData data)
    {
        int result = -1;

        for (int i = 0; i < commands.arraySize; i++)
        {
            SerializedProperty command = commands.GetArrayElementAtIndex(i);

            if (!string.Equals(Child(command, "phrase").stringValue.Trim(), data.phrase.Trim(), StringComparison.OrdinalIgnoreCase))
                continue;

            int min = Child(command, "minSolvedPasswords").intValue;
            int max = Child(command, "maxSolvedPasswords").intValue;

            if (min == data.minSolvedPasswords && max == data.maxSolvedPasswords && result < 0)
                result = i;
            else if (min <= data.maxSolvedPasswords && max >= data.minSolvedPasswords)
                throw new InvalidOperationException($"Command {data.phrase} has duplicate or overlapping stage ranges.");
        }

        return result;
    }

    private static int FindStation(SerializedProperty stations, int frequency)
    {
        int result = -1;

        for (int i = 0; i < stations.arraySize; i++)
        {
            if (Child(stations.GetArrayElementAtIndex(i), "frequency").intValue != frequency)
                continue;

            if (result >= 0)
                throw new InvalidOperationException($"Frequency {frequency} is configured more than once.");

            result = i;
        }

        return result;
    }

    private static Dictionary<string, UnityEngine.Object> CaptureReferences(SerializedObject target, string excludedPath = null)
    {
        var result = new Dictionary<string, UnityEngine.Object>();
        SerializedProperty iterator = target.GetIterator();

        while (iterator.Next(true))
        {
            if (iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.propertyPath != excludedPath)
                result.Add(iterator.propertyPath, iterator.objectReferenceValue);
        }

        return result;
    }

    private static void VerifyReferences(SerializedObject target, Dictionary<string, UnityEngine.Object> references)
    {
        foreach (var pair in references)
        {
            if (Property(target, pair.Key).objectReferenceValue != pair.Value)
                throw new InvalidOperationException($"An existing reference changed: {pair.Key}.");
        }
    }

    private static void VerifyImport(SerializedObject manager, SerializedObject radio, StoryData data,
        TerminalTurbo terminal, string firstCorrectResponse)
    {
        if (Property(manager, "terminal").objectReferenceValue != terminal)
            throw new InvalidOperationException("The terminal reference was not applied.");

        VerifyString(Property(manager, "finalText"), data.finalText);
        SerializedProperty puzzles = Property(manager, "puzzles");

        for (int i = 0; i < data.puzzles.Length; i++)
        {
            SerializedProperty puzzle = puzzles.GetArrayElementAtIndex(i);
            PuzzleData source = data.puzzles[i];
            VerifyString(Child(puzzle, "password"), source.password);
            VerifyString(Child(puzzle, "terminalText"), source.terminalText);
            VerifyString(Child(puzzle, "correctResponse"), string.IsNullOrWhiteSpace(source.correctResponse) ? firstCorrectResponse : source.correctResponse);
            VerifyString(Child(puzzle, "wrongResponse"), source.wrongResponse);
            VerifyConversations(Child(puzzle, "radioMonologues"), source.radioMonologues);
        }

        SerializedProperty commands = Property(manager, "commands");

        foreach (CommandData source in data.commands)
        {
            int index = FindCommand(commands, source);

            if (index < 0)
                throw new InvalidOperationException($"Command {source.phrase} was not applied.");

            VerifyString(Child(commands.GetArrayElementAtIndex(index), "response"), source.response);
        }

        if (Property(radio, "contactFrequency").intValue != data.contactFrequency)
            throw new InvalidOperationException("The contact frequency was not applied.");

        VerifyString(Property(radio, "speakerName"), data.speakerName);
        VerifyConversation(Property(radio, "introduction"), data.introduction);
        SerializedProperty stations = Property(radio, "secretStations");

        foreach (StationData source in data.secretStations)
        {
            int index = FindStation(stations, source.frequency);

            if (index < 0)
                throw new InvalidOperationException($"Frequency {source.frequency} was not applied.");

            SerializedProperty station = stations.GetArrayElementAtIndex(index);
            VerifyString(Child(station, "speakerName"), source.speakerName);
            VerifyConversations(Child(station, "monologues"), source.monologues);

            if (Child(station, "repeatAfterLast").boolValue != source.repeatAfterLast)
                throw new InvalidOperationException("The station repeat setting was not applied.");
        }
    }

    private static void VerifyConversations(SerializedProperty target, ConversationData[] data)
    {
        if (target.arraySize != data.Length)
            throw new InvalidOperationException($"The conversation count does not match: {target.propertyPath}.");

        for (int i = 0; i < data.Length; i++)
            VerifyConversation(target.GetArrayElementAtIndex(i), data[i]);
    }

    private static void VerifyConversation(SerializedProperty target, ConversationData data)
    {
        VerifyString(Child(target, "title"), data.title);
        SerializedProperty lines = Child(target, "lines");

        if (lines.arraySize != data.lines.Length)
            throw new InvalidOperationException($"The line count does not match: {target.propertyPath}.");

        for (int i = 0; i < data.lines.Length; i++)
            VerifyString(lines.GetArrayElementAtIndex(i), data.lines[i]);
    }

    private static void VerifyString(SerializedProperty property, string expected)
    {
        if (property.stringValue != expected)
            throw new InvalidOperationException($"Text was not applied correctly: {property.propertyPath}.");
    }

    private static SerializedProperty Property(SerializedObject target, string name)
    {
        return target.FindProperty(name) ?? throw new InvalidOperationException($"Serialized field is missing: {name}.");
    }

    private static SerializedProperty Child(SerializedProperty target, string name)
    {
        return target.FindPropertyRelative(name) ?? throw new InvalidOperationException($"Serialized field is missing: {target.propertyPath}.{name}.");
    }

    private static void ValidateData(StoryData data)
    {
        if (data?.puzzles == null || data.puzzles.Length != 5 || string.IsNullOrWhiteSpace(data.finalText))
            throw new InvalidOperationException("Story data must contain five puzzles and final text.");

        var passwords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < data.puzzles.Length; i++)
        {
            PuzzleData puzzle = data.puzzles[i];

            if (puzzle == null || string.IsNullOrWhiteSpace(puzzle.password) || !passwords.Add(puzzle.password.Trim()) ||
                string.IsNullOrWhiteSpace(puzzle.terminalText) || string.IsNullOrWhiteSpace(puzzle.wrongResponse) ||
                (i > 0 && string.IsNullOrWhiteSpace(puzzle.correctResponse)))
                throw new InvalidOperationException($"Puzzle {i + 1} has missing text or a duplicate password.");

            ValidateConversations(puzzle.radioMonologues);
        }

        ValidateConversations(new[] { data.introduction });

        if (data.commands == null || data.secretStations == null || string.IsNullOrWhiteSpace(data.speakerName))
            throw new InvalidOperationException("Story data is missing commands, stations or the contact name.");

        var commands = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (CommandData command in data.commands)
        {
            if (command == null || string.IsNullOrWhiteSpace(command.phrase) || string.IsNullOrWhiteSpace(command.response) ||
                !commands.Add(command.phrase.Trim()) || command.minSolvedPasswords < 0 || command.maxSolvedPasswords > 4 ||
                command.minSolvedPasswords > command.maxSolvedPasswords)
                throw new InvalidOperationException("Story data contains an invalid or duplicate command.");
        }

        ValidateFrequency(data.contactFrequency);
        var frequencies = new HashSet<int> { data.contactFrequency };

        foreach (StationData station in data.secretStations)
        {
            if (station == null || !frequencies.Add(station.frequency) || string.IsNullOrWhiteSpace(station.speakerName))
                throw new InvalidOperationException("Story data contains an invalid or duplicate station.");

            ValidateFrequency(station.frequency);
            ValidateConversations(station.monologues);
        }
    }

    private static void ValidateConversations(ConversationData[] conversations)
    {
        if (conversations == null || conversations.Length == 0)
            throw new InvalidOperationException("A conversation list is empty.");

        foreach (ConversationData conversation in conversations)
        {
            if (conversation?.lines == null || conversation.lines.Length == 0 || string.IsNullOrWhiteSpace(conversation.title))
                throw new InvalidOperationException("A conversation has no title or lines.");

            foreach (string line in conversation.lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    throw new InvalidOperationException("A conversation contains an empty line.");
            }
        }
    }

    private static void ValidateFrequency(int frequency)
    {
        if (frequency < Radio.MIN_WAVE_NUMBER || frequency > Radio.MAX_WAVE_NUMBER)
            throw new InvalidOperationException($"Frequency {frequency} is outside the radio's range.");
    }
}
