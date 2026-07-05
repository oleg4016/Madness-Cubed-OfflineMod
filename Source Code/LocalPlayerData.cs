using System;
using System.IO;
using System.Text;
using UnityEngine;
using LitJson;

namespace MadnessCubedOffline
{
    public class LocalPlayerData
    {
        private static string SavePath
        {
            get
            {
                string dir = Path.Combine(Path.GetDirectoryName(Application.dataPath), Path.Combine("UserData", "MadnessCubedOffline"));
                return Path.Combine(dir, "offline_profile.json");
            }
        }

        public int money1 = 0;
        public int money2 = 0;
        public int level = 0;
        public uint exp = 0;
        public int expPoints = 0;
        public int frags = 0;
        public int health = 7;
        public int armor = 7;
        public int speed = 7;
        public int jump = 7;
        public int defend = 7;
        public int skin = 0;
        public int[] skins = new int[64];
        public int[] weapons = new int[256];
        public int[] items = new int[512];
        public int[] specItems = new int[64];
        public int[] clothes = new int[64];
        public int[] isClothes = new int[256];
        public int[] cubesTimeOfEnd = new int[6];
        public int[] weaponUnlock = new int[512];
        public int[] specUnlock = new int[256];
        public int[] itemUnlock = new int[512];
        public int[] charUnlock = new int[128];
        public int[] missionUnlock = new int[256];
        public int[] missionScore = new int[512];
        public int[] fastInventarWeapon = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        public string playerName = "OfflinePlayer";
        public int serverId = 1;

        public static LocalPlayerData Load()
        {
            try
            {
                string path = SavePath;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path, Encoding.UTF8);
                    LocalPlayerData data = JsonMapper.ToObject<LocalPlayerData>(json);
                    if (data.fastInventarWeapon == null || data.fastInventarWeapon.Length == 0)
                        data.fastInventarWeapon = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
                    if (data.fastInventarWeapon[0] < 0 && data.weapons != null && data.weapons.Length > 0 && data.weapons[0] > 0)
                        data.fastInventarWeapon[0] = 0;
                    return data;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[Offline] Failed to load profile: " + ex.Message);
            }
            return CreateDefault();
        }

        public void Save()
        {
            try
            {
                string path = SavePath;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                string json = JsonMapper.ToJson(this);
                File.WriteAllText(path, json, Encoding.UTF8);
                Debug.Log("[Offline] Profile saved.");
            }
            catch (Exception ex)
            {
                Debug.LogError("[Offline] Failed to save profile: " + ex.Message);
            }
        }

        public void Delete()
        {
            string path = SavePath;
            if (File.Exists(path))
                File.Delete(path);
        }

        private static void SetArrayValues(int[] arr, int[] ids)
        {
            for (int i = 0; i < ids.Length; i++)
                if (ids[i] >= 0 && ids[i] < arr.Length)
                    arr[ids[i]] = 1;
        }

        private static LocalPlayerData CreateDefault()
        {
            var data = new LocalPlayerData();
            data.money1 = 500;
            data.money2 = 0;

            SetArrayValues(data.weaponUnlock, new int[] { 0, 1, 2, 37 });
            SetArrayValues(data.weapons, new int[] { 0, 1 });
            data.fastInventarWeapon[0] = 0;

            SetArrayValues(data.charUnlock, new int[] { 0 });

            SetArrayValues(data.items, new int[] { 156, 168, 169, 185 });

            data.missionUnlock[0] = 1;

            return data;
        }
    }
}
