using System.Collections.Generic;
using UnityEngine;

namespace Signal
{
    public class ShopManager : MonoBehaviour
    {
        [SerializeField] private List<StoreSlot> storeSlots = new();
        [SerializeField] private List<PlaceableItem> commonItemPrefabs = new();
        [SerializeField] private List<PlaceableItem> rareItemPrefabs = new();

        public void FillStore(int round)
        {
            ClearStore();

            for (int i = 0; i < storeSlots.Count; i++)
            {
                StoreSlot slot = storeSlots[i];

                if (slot == null)
                {
                    continue;
                }

                PlaceableItem prefab = GetRandomPrefab(round);

                if (prefab == null)
                {
                    continue;
                }

                PlaceableItem item = Instantiate(prefab, slot.transform.position, Quaternion.identity);
                slot.PlaceItem(item);
            }
        }

        public void ClearStore()
        {
            for (int i = 0; i < storeSlots.Count; i++)
            {
                StoreSlot slot = storeSlots[i];

                if (slot == null)
                {
                    continue;
                }

                PlaceableItem item = slot.Item;

                if (item != null)
                {
                    slot.ClearItem(item);
                    Destroy(item.gameObject);
                }
            }
        }

        private PlaceableItem GetRandomPrefab(int round)
        {
            int rareChance = GetRareChance(round);
            int roll = Random.Range(0, 100);

            if (roll < rareChance)
            {
                return GetRandomFromList(rareItemPrefabs);
            }

            return GetRandomFromList(commonItemPrefabs);
        }

        private int GetRareChance(int round)
        {
            int value = Mathf.RoundToInt(1f + (round - 1) * 49f / 9f);

            if (value < 1)
            {
                return 1;
            }

            if (value > 50)
            {
                return 50;
            }

            return value;
        }

        private PlaceableItem GetRandomFromList(List<PlaceableItem> prefabs)
        {
            if (prefabs == null || prefabs.Count == 0)
            {
                return null;
            }

            List<PlaceableItem> validPrefabs = new();

            for (int i = 0; i < prefabs.Count; i++)
            {
                if (prefabs[i] != null)
                {
                    validPrefabs.Add(prefabs[i]);
                }
            }

            if (validPrefabs.Count == 0)
            {
                return null;
            }

            int index = Random.Range(0, validPrefabs.Count);
            return validPrefabs[index];
        }
    }
}
