using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LitJson;

namespace MadnessCubedOffline
{
    public class MapTopEntry
    {
        public int id;
        public long mapid;
        public string name;
        public int type;
        public int canbreak;
        public int daytime;
        public int hits;
        public float lightLevel = 1.0f;
        public string skybox = "day";
        public float fog = 0f;
        public string lightColor = "white";
        public bool noTimer;
        public string _sourceFile;
    }

    public static class MapTopCore
    {
        private static List<MapTopEntry> _cache;
        private static string _mapstopPath;
        private static string _myMapsPath;
        private static int _nextId = 1;

        public static bool currentNoTimer;

        private static string MapsDir
        {
            get
            {
                string dataPath = Path.GetDirectoryName(Application.dataPath);
                return Path.Combine(Path.Combine(dataPath, "build_Data"), "Maps");
            }
        }

        private static string MapstopPath
        {
            get
            {
                if (_mapstopPath == null)
                    _mapstopPath = Path.Combine(MapsDir, "mapstop");
                return _mapstopPath;
            }
        }

        private static string MyMapsPath
        {
            get
            {
                if (_myMapsPath == null)
                    _myMapsPath = Path.Combine(MapsDir, "my_maps");
                return _myMapsPath;
            }
        }

        public static void InvalidateCache()
        {
            _cache = null;
        }

        private static void EnsureDir(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        public static List<MapTopEntry> LoadEntries()
        {
            if (_cache != null)
                return _cache;

            _cache = new List<MapTopEntry>();
            _nextId = 1;

            EnsureDir(MapstopPath);
            EnsureDir(MyMapsPath);

            if (Directory.GetFiles(MapstopPath, "*.json").Length == 0)
                CreateExampleFiles(MapstopPath);

            LoadFromDir(MapstopPath);
            LoadFromDir(MyMapsPath);

            return _cache;
        }

        private static void LoadFromDir(string dir)
        {
            foreach (string file in Directory.GetFiles(dir, "*.json"))
            {
                try
                {
                    string json = File.ReadAllText(file, System.Text.Encoding.UTF8);
                    JsonData data = JsonMapper.ToObject(json);
                    MapTopEntry entry = new MapTopEntry();
                    entry.id = _nextId++;
                    entry._sourceFile = file;
                    entry.mapid = data.Keys.Contains("mapid") ? long.Parse(data["mapid"].ToString()) : -1;
                    entry.name = data.Keys.Contains("name") ? data["name"].ToString() : Path.GetFileNameWithoutExtension(file);
                    entry.type = data.Keys.Contains("type") ? int.Parse(data["type"].ToString()) : 0;
                    entry.canbreak = data.Keys.Contains("canbreak") ? int.Parse(data["canbreak"].ToString()) : 0;
                    entry.daytime = data.Keys.Contains("daytime") ? int.Parse(data["daytime"].ToString()) : 1;
                    entry.hits = data.Keys.Contains("hits") ? int.Parse(data["hits"].ToString()) : 0;
                    entry.lightLevel = data.Keys.Contains("lightLevel") ? float.Parse(data["lightLevel"].ToString()) : 1.0f;
                    entry.skybox = data.Keys.Contains("skybox") ? data["skybox"].ToString() : "day";
                    entry.fog = data.Keys.Contains("fog") ? float.Parse(data["fog"].ToString()) : 0f;
                    entry.lightColor = data.Keys.Contains("lightColor") ? data["lightColor"].ToString() : "white";
                    entry.noTimer = data.Keys.Contains("notimer") && data["notimer"].ToString() == "1";
                    _cache.Add(entry);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[Offline] MapTopCore: failed to parse " + file + ": " + ex.Message);
                }
            }
        }

        public static string BuildTopJson(int dayFilter)
        {
            var entries = LoadEntries();
            var sb = new System.Text.StringBuilder();
            sb.Append("{\"items\":[");
            bool first = true;
            foreach (var e in entries)
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append("{\"id\":\"").Append(e.id).Append("\",");
                sb.Append("\"mapid\":\"").Append(e.mapid).Append("\",");
                sb.Append("\"name\":\"").Append(EscapeJson(e.name)).Append("\",");
                sb.Append("\"type\":\"").Append(e.type).Append("\",");
                sb.Append("\"canbreak\":\"").Append(e.canbreak).Append("\",");
                sb.Append("\"daytime\":\"").Append(e.daytime).Append("\",");
                sb.Append("\"hits\":\"").Append(e.hits).Append("\"}");
            }
            sb.Append("]}");
            return sb.ToString();
        }

        public static string BuildMyMapsJson()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("{\"items\":[");
            bool first = true;
            foreach (var e in LoadEntries())
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append("{\"id\":\"").Append(e.id).Append("\",");
                sb.Append("\"mapid\":\"").Append(e.mapid).Append("\",");
                sb.Append("\"name\":\"").Append(EscapeJson(e.name)).Append("\",");
                sb.Append("\"type\":\"").Append(e.type).Append("\",");
                sb.Append("\"canbreak\":\"").Append(e.canbreak).Append("\",");
                sb.Append("\"daytime\":\"").Append(e.daytime).Append("\",");
                sb.Append("\"hits\":\"").Append(e.hits).Append("\"}");
            }
            sb.Append("],\"price\":0}");
            return sb.ToString();
        }

        public static void IncrementHit(int id)
        {
            var entries = LoadEntries();
            foreach (var e in entries)
                if (e.id == id) { e.hits++; break; }
        }

        public static MapTopEntry FindEntry(long mapid)
        {
            foreach (var e in LoadEntries())
                if (e.mapid == mapid) return e;
            return null;
        }

        public static void ApplyEnvironment(long mapid)
        {
            MapTopEntry entry = FindEntry(mapid);
            currentNoTimer = entry != null && entry.noTimer;
            if (entry == null) return;
            RenderSettings.fogDensity = entry.fog;
            string lower = (entry.skybox ?? "day").ToLower();
            if (lower == "night")
                RenderSettings.ambientIntensity = 0.3f;
            else if (lower == "space")
                RenderSettings.ambientIntensity = 0.2f;
            else
                RenderSettings.ambientIntensity = 0.6f;
        }

        public static string SaveMyMap(long mapid, string name, int type, int canbreak, int daytime, string oid)
        {
            EnsureDir(MyMapsPath);

            if (!string.IsNullOrEmpty(oid))
            {
                int existingId = int.Parse(oid);
                foreach (var e in LoadEntries())
                {
                    if (e.id == existingId && e._sourceFile != null && e._sourceFile.StartsWith(MyMapsPath))
                    {
                        string json = string.Format("{{\"mapid\":{0},\"name\":\"{1}\",\"type\":{2},\"canbreak\":{3},\"daytime\":{4},\"hits\":{5}}}",
                            mapid, EscapeJson(name), type, canbreak, daytime, e.hits);
                        File.WriteAllText(e._sourceFile, json, System.Text.Encoding.UTF8);
                        SaveBytesFile(mapid);
                        InvalidateCache();
                        return "1";
                    }
                }
            }

            string filename = "user_map_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".json";
            string filepath = Path.Combine(MyMapsPath, filename);
            string content = string.Format("{{\"mapid\":{0},\"name\":\"{1}\",\"type\":{2},\"canbreak\":{3},\"daytime\":{4},\"hits\":0}}",
                mapid, EscapeJson(name), type, canbreak, daytime);
            File.WriteAllText(filepath, content, System.Text.Encoding.UTF8);
            SaveBytesFile(mapid);
            InvalidateCache();
            return "1";
        }

        private static void SaveBytesFile(long mapid)
        {
            try
            {
                int slotId = (int)(mapid % 20);
                if (slotId < 0) slotId = -slotId;
                string srcPath = Path.Combine(Application.persistentDataPath, "m" + slotId + ".bytes");
                if (File.Exists(srcPath))
                {
                    string dstPath = Path.Combine(MapsDir, "m" + Math.Abs(mapid) + ".bytes");
                    File.Copy(srcPath, dstPath, true);
                    Debug.Log("[Offline] Map .bytes saved: " + dstPath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[Offline] SaveBytesFile error: " + ex.Message);
            }
        }

        public static string RemoveMyMap(int oid)
        {
            foreach (var e in LoadEntries())
            {
                if (e.id == oid && e._sourceFile != null && File.Exists(e._sourceFile))
                {
                    try
                    {
                        string bytesPath = Path.Combine(MapsDir, "m" + Math.Abs(e.mapid) + ".bytes");
                        if (File.Exists(bytesPath))
                            File.Delete(bytesPath);
                    }
                    catch { }
                    try
                    {
                        File.Delete(e._sourceFile);
                    }
                    catch { }
                    InvalidateCache();
                    return "1";
                }
            }
            return "1";
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static void CreateExampleFiles(string dir)
        {
            string[][] examples = new string[][] {
                new string[] { "map_obuchenie.json", "{\"mapid\":-200,\"name\":\"Обучение\",\"type\":1,\"canbreak\":0,\"daytime\":1,\"hits\":9999}" },
                new string[] { "map_zashita.json", "{\"mapid\":-201,\"name\":\"Защита деревни\",\"type\":3,\"canbreak\":0,\"daytime\":1,\"hits\":8888}" },
                new string[] { "map_poisk.json", "{\"mapid\":-202,\"name\":\"Поиск чертежей\",\"type\":4,\"canbreak\":0,\"daytime\":1,\"hits\":7777}" },
                new string[] { "map_noch.json", "{\"mapid\":-203,\"name\":\"Ночная миссия\",\"type\":1,\"canbreak\":1,\"daytime\":0,\"hits\":6666,\"lightLevel\":0.3,\"skybox\":\"night\",\"fog\":0.2}" },
                new string[] { "map_kosmos.json", "{\"mapid\":-204,\"name\":\"Космическая станция\",\"type\":2,\"canbreak\":1,\"daytime\":2,\"hits\":5555,\"lightLevel\":0.5,\"skybox\":\"space\"}" },
                new string[] { "map_survival_notimer.json", "{\"mapid\":-205,\"name\":\"Выживание без таймера\",\"type\":3,\"canbreak\":0,\"daytime\":1,\"hits\":4444,\"notimer\":1}" },
            };
            foreach (string[] ex in examples)
                File.WriteAllText(Path.Combine(dir, ex[0]), ex[1], System.Text.Encoding.UTF8);
        }
    }
}
