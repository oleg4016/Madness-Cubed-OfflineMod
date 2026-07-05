using System;
using System.IO;
using System.Text;
using UnityEngine;
using LitJson;

namespace MadnessCubedOffline
{
    public class OfflineConfig
    {
        private static string ConfigPath
        {
            get
            {
                string dir = Path.Combine(Path.GetDirectoryName(Application.dataPath), Path.Combine("UserData", "MadnessCubedOffline"));
                return Path.Combine(dir, "offline_config.json");
            }
        }

        public string playerName = "OfflinePlayer";
        public int money1 = 0;
        public int money2 = 0;
        public int level = 1;
        public string language = "ru_RU";
        public string mapsPath = "";
        public int health = 7;
        public int armor = 7;
        public int speed = 7;
        public int jump = 7;
        public int defend = 7;

        public int[] weapons = null;
        public int[] items = null;
        public int[] specItems = null;
        public int[] skins = null;
        public int[] clothes = null;
        public int[] isClothes = null;
        public int[] weaponUnlock = null;
        public int[] specUnlock = null;
        public int[] itemUnlock = null;
        public int[] charUnlock = null;
        public int[] missionUnlock = null;

        public double itemPriceMultiplier = 1.0;
        public double weaponPriceMultiplier = 1.0;
        public double specPriceMultiplier = 1.0;
        public double skinPriceMultiplier = 1.0;
        public double cubePriceMultiplier = 1.0;
        public double clothesPriceMultiplier = 1.0;
        public double bonusPriceMultiplier = 1.0;

        public static OfflineConfig Load()
        {
            try
            {
                string path = ConfigPath;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path, Encoding.UTF8);
                    return JsonMapper.ToObject<OfflineConfig>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[Offline] Failed to load config: " + ex.Message);
            }
            OfflineConfig cfg = new OfflineConfig();
            cfg.Save();
            return cfg;
        }

        public void Save()
        {
            try
            {
                string path = ConfigPath;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                string json = JsonMapper.ToJson(this);
                File.WriteAllText(path, json, Encoding.UTF8);
                Debug.Log("[Offline] Config saved.");
            }
            catch (Exception ex)
            {
                Debug.LogError("[Offline] Failed to save config: " + ex.Message);
            }
        }
    }
}
