using System;
using UnityEngine;
using kube;

namespace MadnessCubedOffline
{
    public static class InventoryCore
    {
        private static LocalPlayerData Data { get { return CoreManager.PlayerData; } }

        public static bool HasWeapon(int id)
        {
            return id >= 0 && id < Data.weapons.Length && Data.weapons[id] > 0;
        }

        public static void AddWeapon(int id)
        {
            if (id >= 0 && id < Data.weapons.Length)
            {
                Data.weapons[id] = 1;
                if (id < Data.weaponUnlock.Length)
                    Data.weaponUnlock[id] = 1;
                FireInventoryChanged();
            }
        }

        public static bool HasItem(int id)
        {
            return id >= 0 && id < Data.items.Length && Data.items[id] > 0;
        }

        public static void AddItem(int id, int count)
        {
            if (id >= 0 && id < Data.items.Length)
            {
                Data.items[id] = count;
                if (id < Data.itemUnlock.Length)
                    Data.itemUnlock[id] = 1;
                FireInventoryChanged();
            }
        }

        public static void TakeItem(int id, int count)
        {
            if (id >= 0 && id < Data.items.Length)
                Data.items[id] = count;
        }

        public static bool HasSpecItem(int id)
        {
            return id >= 0 && id < Data.specItems.Length && Data.specItems[id] > 0;
        }

        public static void AddSpecItem(int id)
        {
            if (id >= 0 && id < Data.specItems.Length)
            {
                Data.specItems[id] = 1;
                if (id < Data.specUnlock.Length)
                    Data.specUnlock[id] = 1;
                FireInventoryChanged();
            }
        }

        public static void AddSkin(int id)
        {
            if (id >= 0 && id < Data.skins.Length)
            {
                Data.skins[id] = 1;
                FireInventoryChanged();
            }
        }

        public static void AddClothes(int id)
        {
            if (id >= 0 && id < Data.isClothes.Length)
            {
                Data.isClothes[id] = 1;
                FireInventoryChanged();
            }
        }

        public static void SetClothesArray(int[] values)
        {
            for (int i = 0; i < values.Length && i < Data.clothes.Length; i++)
                Data.clothes[i] = values[i];
        }

        public static string BuildWeaponString()
        {
            string[] parts = new string[Data.weapons.Length];
            for (int i = 0; i < Data.weapons.Length; i++)
                parts[i] = Data.weapons[i].ToString();
            return string.Join(";", parts);
        }

        public static string BuildItemString()
        {
            string[] parts = new string[Data.items.Length];
            for (int i = 0; i < Data.items.Length; i++)
                parts[i] = Data.items[i].ToString();
            return string.Join(";", parts);
        }

        public static string BuildSkinString()
        {
            string[] parts = new string[Data.skins.Length];
            for (int i = 0; i < Data.skins.Length; i++)
                parts[i] = Data.skins[i].ToString();
            return string.Join(";", parts);
        }

        public static void SyncToGame()
        {
            if (Kube.GPS == null) return;
            for (int i = 0; i < Data.weapons.Length && i < Kube.GPS.inventarWeapons.Length; i++)
                Kube.GPS.inventarWeapons[i] = Data.weapons[i] > 0 ? ((int)Time.time + 10000000) : 0;
            for (int i = 0; i < Data.items.Length && i < Kube.GPS.inventarItems.Length; i++)
                Kube.GPS.inventarItems[i] = Data.items[i];
            if (Kube.GPS.inventarSpecItems != null)
                for (int i = 0; i < Data.specItems.Length && i < Kube.GPS.inventarSpecItems.Length; i++)
                    Kube.GPS.inventarSpecItems[i] = Data.specItems[i];
            for (int i = 0; i < Data.skins.Length && i < Kube.GPS.playerSkins.Length; i++)
                Kube.GPS.playerSkins[i] = Data.skins[i];
            if (Kube.GPS.playerClothes != null)
                for (int i = 0; i < Data.clothes.Length && i < Kube.GPS.playerClothes.Length; i++)
                    Kube.GPS.playerClothes[i] = Data.clothes[i];
            if (Kube.GPS.playerIsClothes != null)
                for (int i = 0; i < Data.isClothes.Length && i < Kube.GPS.playerIsClothes.Length; i++)
                    Kube.GPS.playerIsClothes[i] = Data.isClothes[i];
        }

        private static void FireInventoryChanged()
        {
            if (CoreEvents.OnInventoryChanged != null)
                CoreEvents.OnInventoryChanged();
        }
    }
}
