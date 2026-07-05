using System;
using System.IO;
using UnityEngine;
using LitJson;
using kube;

namespace MadnessCubedOffline
{
    public static class MapCore
    {
        public static void SaveMap(long mapId, byte[] mapData)
        {
            SaveMap(mapId, mapData, null, null);
        }

        public static void SaveMap(long mapId, byte[] mapData, GameObject go, string method)
        {
            int slotId = (int)(mapId % 20);
            if (slotId < 0) slotId = -slotId;
            string path = Application.persistentDataPath + "/m" + slotId + ".bytes";
            File.WriteAllBytes(path, mapData);
            Debug.Log("[Offline] Map saved: " + path);
            if (go != null && !string.IsNullOrEmpty(method))
                go.SendMessage(method);
        }

        public static void LoadMap(long mapId)
        {
            LoadMapInternal(mapId, null);
        }

        public static void LoadMapInternal(long mapId, GameObject go)
        {
            int slotId = (int)(mapId % 20);
            if (slotId < 0) slotId = -slotId;
            string path = Application.persistentDataPath + "/m" + slotId + ".bytes";
            if (File.Exists(path))
            {
                byte[] data = File.ReadAllBytes(path);
                if (Kube.BCS != null)
                {
                    Kube.BCS.OnMapLoaded(data);
                    return;
                }
            }
            if (Kube.ASS3 != null && Kube.ASS3.buildinMaps != null && Kube.ASS3.buildinMaps.Length > 0)
            {
                int mapIdx = 0;
                if (Kube.OH != null)
                {
                    var creatingMaps = Kube.OH.findCreatingMaps(false);
                    if (creatingMaps != null && creatingMaps.Length > 0)
                        mapIdx = creatingMaps[0].Id;
                }
                if (mapIdx >= 0 && mapIdx < Kube.ASS3.buildinMaps.Length && Kube.ASS3.buildinMaps[mapIdx] != null)
                {
                    byte[] data = Kube.ASS3.buildinMaps[mapIdx].bytes;
                    if (Kube.BCS != null)
                    {
                        Kube.BCS.OnMapLoaded(data);
                        return;
                    }
                }
            }
            Debug.LogWarning("[Offline] Map file not found: m" + slotId + ".bytes");
        }

        public static void LoadIsMap(long mapId, GameObject go, string method)
        {
            int slotId = (int)(mapId % 20);
            if (slotId < 0) slotId = -slotId;
            string path = Application.persistentDataPath + "/m" + slotId + ".json";
            if (File.Exists(path))
            {
                string name = "OfflineMap";
                try
                {
                    JsonData jd = JsonMapper.ToObject(File.ReadAllText(path));
                    if (jd.Keys.Contains("mapname"))
                        name = jd["mapname"].ToString();
                }
                catch { }
                go.SendMessage(method, "0^0^" + name + "^0^0^0");
            }
            else
            {
                go.SendMessage(method, "0^0^Map^0^0^0");
            }
        }

        public static void SetMapName(long mapId, string mapName)
        {
            int slotId = (int)(mapId % 20);
            if (slotId < 0) slotId = -slotId;
            string path = Application.persistentDataPath + "/m" + slotId + ".json";
            JsonData jd;
            if (File.Exists(path))
            {
                try { jd = JsonMapper.ToObject(File.ReadAllText(path)); }
                catch { jd = new JsonData(); }
            }
            else
            {
                jd = new JsonData();
            }
            jd["mapname"] = mapName;
            File.WriteAllText(path, JsonMapper.ToJson(jd));
        }
    }
}
