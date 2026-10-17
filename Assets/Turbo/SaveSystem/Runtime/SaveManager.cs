using System;
using System.IO;
using UnityEngine;

namespace Turbo.SaveSystem
{
    public sealed class SaveManager : MonoBehaviour
    {
        private ISaveParticipant[] _participants;

        private void Awake()
        {
            _participants = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None) as ISaveParticipant[];
        }

        public void Save(int slotId)
        {
            SaveData data = new SaveData();
            data.version = 1;

            // ДОБАВЬТЕ здесь заполнение полей SaveData,
            // если в игре есть данные, которые не принадлежат ISaveParticipant системам

            foreach (var participant in _participants)
                participant.WriteSaveData(data);

            string json = JsonUtility.ToJson(data, true);
            string path = GetPath(slotId);

            File.WriteAllText(path, json);
        }

        public void Load(int slotId)
        {
            string path = GetPath(slotId);

            if (!File.Exists(path))
                return;

            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            foreach (var participant in _participants)
                participant.LoadSaveData(data);

            // ЕСЛИ в игре есть системы, которые не используют ISaveParticipant,
            // здесь можно вручную разложить данные из SaveData
        }

        private string GetPath(int slotId)
        {
            return Path.Combine(Application.persistentDataPath, $"save_{slotId}.json");
        }
    }


    public interface ISaveParticipant
    {
        void WriteSaveData(SaveData saveData);
        void LoadSaveData(SaveData saveData);
    }

    [Serializable]
    public class SaveData
    {
        public int version;

        // ДОБАВЛЯЙТЕ сюда поля конкретной игры

        public PlayerData player;
        public WorldData world;
    }

    [Serializable]
    public class PlayerData
    {
        // ПРИМЕР структуры данных игрока
        public int money;
        public int level;
    }

    [Serializable]
    public class WorldData
    {
        // ПРИМЕР данных мира
        public int day;
    }

    [Serializable]
    public class SaveSlotInfo
    {
        public int slotId;
        public long timestamp;
        public int version;
    }
}
