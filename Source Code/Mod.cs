using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using LitJson;
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using kube;
using kube.data;
using ExitGames.Client.Photon;

[assembly: MelonInfo(typeof(MadnessCubedOffline.MadnessCubedOfflineMod), "Madness Cubed Offline Complete", "4.0", "Nobodyshot Community", "")]
[assembly: MelonGame("nobodyshot", "Madness Cubed")]

namespace MadnessCubedOffline
{
    public class MadnessCubedOfflineMod : MelonMod
    {
        internal static MadnessCubedOfflineMod Instance;
        internal static OfflineServer offlineServer;
        internal static bool isReady = false;

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("========================================");
            MelonLogger.Msg(" Madness Cubed Offline Complete v4.0");
            MelonLogger.Msg(" Полный оффлайн-режим");
            MelonLogger.Msg("========================================");

            Instance = this;
            var config = OfflineConfig.Load();
            offlineServer = new OfflineServer();
            CoreManager.Initialize(offlineServer, offlineServer.Data, config);
            offlineServer.SetData(offlineServer.Data);

            MelonLogger.Msg("[Offline] Мод инициализирован. Профиль загружен.");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Menu" || sceneName == "Loading")
            {
                MelonLogger.Msg("[Offline] Сцена загружена: " + sceneName + ", запускаем настройку...");
                MelonCoroutines.Start(DelayedSetup());
            }
        }

        private IEnumerator DelayedSetup()
        {
            yield return new WaitForSeconds(0.5f);
            SetupOfflineEnvironment();
        }

        private void SetupOfflineEnvironment()
        {
            try
            {
                if (Kube.SN == null)
                {
                    MelonLogger.Msg("[Offline] Kube.SN ещё не инициализирован, ждём...");
                    MelonCoroutines.Start(DelayedSetup());
                    return;
                }

                PhotonNetwork.offlineMode = true;

                if (Kube.SS != null && offlineServer != null)
                {
                    Kube.SS = offlineServer;
                    MelonLogger.Msg("[Offline] Kube.SS заменён на OfflineServer");
                    offlineServer.serverId = Kube.SS.serverId > 0 ? Kube.SS.serverId : offlineServer.serverId;
                }

                if (Kube.GPS != null)
                    ApplyLocalDataToGame();

                if (Kube.OH != null)
                {
                    FieldInfo apiUrlField = typeof(ObjectsHolderScript).GetField("api_url", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (apiUrlField != null)
                        apiUrlField.SetValue(Kube.OH, "http://127.0.0.1/");
                }

                isReady = true;
                MelonLogger.Msg("[Offline] Окружение настроено. Игра готова к оффлайн-режиму.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[Offline] Ошибка настройки: " + ex.Message);
            }
        }

        internal static void ApplyLocalDataToGame()
        {
            if (Kube.GPS == null) return;
            var liveData = offlineServer != null ? CoreManager.PlayerData : null;
            if (liveData == null) return;

            try
            {
                var config = CoreManager.Config;
                bool isFresh = liveData.level == 0 && liveData.money1 == 0;
                string pname = config != null && !string.IsNullOrEmpty(config.playerName) && config.playerName != "OfflinePlayer" ? config.playerName : liveData.playerName;
                int m1 = isFresh && config != null ? config.money1 : liveData.money1;
                int m2 = isFresh && config != null ? config.money2 : liveData.money2;
                int lvl = isFresh && config != null ? config.level : liveData.level;
                int hp = isFresh && config != null ? config.health : liveData.health;
                int arm = isFresh && config != null ? config.armor : liveData.armor;
                int spd = isFresh && config != null ? config.speed : liveData.speed;
                int jmp = isFresh && config != null ? config.jump : liveData.jump;
                int def = isFresh && config != null ? config.defend : liveData.defend;

                Kube.GPS.playerMoney1 = m1;
                Kube.GPS.playerMoney2 = m2;
                Kube.GPS.playerLevel = lvl;
                Kube.GPS.playerExp = liveData.exp;
                Kube.GPS.playerExpPoints = liveData.expPoints;
                Kube.GPS.playerFrags = liveData.frags;
                Kube.GPS.playerHealth = hp;
                Kube.GPS.playerArmor = arm;
                Kube.GPS.playerSpeed = spd;
                Kube.GPS.playerJump = jmp;
                Kube.GPS.playerDefend = def;
                Kube.GPS.playerSkin = liveData.skin;
                Kube.GPS.vipEnd = Time.time + 604800000f;

                liveData.playerName = pname;
                liveData.money1 = m1;
                liveData.money2 = m2;
                liveData.level = lvl;
                liveData.health = hp;
                liveData.armor = arm;
                liveData.speed = spd;
                liveData.jump = jmp;
                liveData.defend = def;

                FieldInfo nameField = typeof(GameParamsScript).GetField("_playerName", BindingFlags.Instance | BindingFlags.NonPublic);
                if (nameField != null)
                    nameField.SetValue(Kube.GPS, pname);
                Kube.GPS.decodePlayerName = pname;

                InventoryCore.SyncToGame();
                WeaponCore.SyncToGame();
                if (liveData.fastInventarWeapon != null && liveData.fastInventarWeapon.Length > 0 && liveData.fastInventarWeapon[0] < 0)
                {
                    for (int w = 0; w < liveData.weapons.Length; w++)
                    {
                        if (liveData.weapons[w] > 0)
                        {
                            liveData.fastInventarWeapon[0] = w;
                            WeaponCore.SyncToGame();
                            break;
                        }
                    }
                }
                ApplyCharUnlockToGame(liveData);

                liveData.isClothes[66] = 0;
                if (Kube.GPS.playerIsClothes != null && 66 < Kube.GPS.playerIsClothes.Length)
                    Kube.GPS.playerIsClothes[66] = 0;
                for (int ci = 0; ci < liveData.clothes.Length; ci++)
                {
                    if (liveData.clothes[ci] == 66)
                    {
                        liveData.clothes[ci] = -1;
                        if (Kube.GPS.playerClothes != null && ci < Kube.GPS.playerClothes.Length)
                            Kube.GPS.playerClothes[ci] = -1;
                    }
                }

                MelonLogger.Msg("[Offline] Данные профиля применены к игре.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[Offline] Ошибка применения данных: " + ex.Message);
            }
        }

        private static void ApplyCharUnlockToGame(LocalPlayerData liveData)
        {
            if (Kube.GPS == null) return;
            var config = CoreManager.Config;
            int[] srcCU = (config != null && config.charUnlock != null) ? config.charUnlock : liveData.charUnlock;
            for (int i = 0; i < srcCU.Length; i++)
            {
                if (srcCU[i] == 1 && Kube.GPS.charUnlock.ContainsKey(i))
                    Kube.GPS.charUnlock[i] = true;
            }
            SetIntHash(Kube.GPS.weaponUnlock, (config != null && config.weaponUnlock != null) ? config.weaponUnlock : liveData.weaponUnlock);
            SetIntHash(Kube.GPS.specUnlock, (config != null && config.specUnlock != null) ? config.specUnlock : liveData.specUnlock);
            SetIntHash(Kube.GPS.itemUnlock, (config != null && config.itemUnlock != null) ? config.itemUnlock : liveData.itemUnlock);
            SetIntHash(Kube.GPS.missionUnlock, (config != null && config.missionUnlock != null) ? config.missionUnlock : liveData.missionUnlock);
        }

        private static void SetIntHash(IntHash dest, int[] src)
        {
            if (dest == null || src == null) return;
            for (int i = 0; i < src.Length; i++)
                if (src[i] > 0) dest[i] = true;
        }
    }

    [HarmonyPatch(typeof(PhotonNetwork), "set_offlineMode")]
    public class ForceOfflineModePatch
    {
        public static bool Prefix(ref bool value)
        {
            value = true;
            return true;
        }
    }

    [HarmonyPatch(typeof(OnlineManager), "ConnectUsingSettings")]
    public class FixOnlineManagerConnectPatch
    {
        public static bool Prefix(OnlineManager __instance)
        {
            PhotonNetwork.offlineMode = true;
            PhotonNetwork.playerName = "OfflinePlayer";
            if (Kube.SS != null)
                PhotonNetwork.player.customProperties["id"] = Kube.SS.serverId;
            __instance.SendMessage("CreateRoomList", SendMessageOptions.DontRequireReceiver);
            return false;
        }
    }

    [HarmonyPatch(typeof(PhotonNetwork), "ConnectUsingSettings", new Type[] { typeof(string) })]
    public class PreventPhotonConnectPatch
    {
        public static bool Prefix()
        {
            PhotonNetwork.offlineMode = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(OnlineManager), "Update")]
    public class FixRoomsNullInUpdatePatch
    {
        public static void Prefix(OnlineManager __instance)
        {
            if (__instance.rooms == null)
                __instance.SendMessage("CreateRoomList", SendMessageOptions.DontRequireReceiver);
        }
    }

    [HarmonyPatch(typeof(BattleControllerScript), "LoadMapFromServer")]
    public class LoadLocalMapPatch
    {
        public static bool Prefix(BattleControllerScript __instance)
        {
            try
            {
                long mapId = Kube.OH != null ? Kube.OH.tempMap.Id : 0;
                string dataMapsPath = Application.dataPath + "/Maps/m" + Math.Abs(mapId) + ".bytes";
                if (File.Exists(dataMapsPath))
                {
                    byte[] mapData = File.ReadAllBytes(dataMapsPath);
                    MelonLogger.Msg("[Offline] Загружаем карту: " + dataMapsPath + " (" + mapData.Length + " байт)");
                    __instance.CancelInvoke("RequestMap");
                    __instance.OnMapLoaded(mapData);
                    return false;
                }
                string persistentPath = Application.persistentDataPath + "/m" + Math.Abs((int)(mapId % 20)) + ".bytes";
                if (File.Exists(persistentPath))
                {
                    byte[] mapData = File.ReadAllBytes(persistentPath);
                    MelonLogger.Msg("[Offline] Загружаем карту: " + persistentPath + " (" + mapData.Length + " байт)");
                    __instance.CancelInvoke("RequestMap");
                    __instance.OnMapLoaded(mapData);
                    return false;
                }
                int builtinIdx = (int)Math.Abs(mapId);
                if (Kube.ASS3 != null && Kube.ASS3.buildinMaps != null && Kube.ASS3.buildinMaps.Length > 0)
                {
                    if (builtinIdx < Kube.ASS3.buildinMaps.Length && Kube.ASS3.buildinMaps[builtinIdx] != null && Kube.ASS3.buildinMaps[builtinIdx].bytes != null)
                    {
                        MelonLogger.Msg("[Offline] Загружаем встроенную карту [" + builtinIdx + "]");
                        __instance.CancelInvoke("RequestMap");
                        __instance.OnMapLoaded(Kube.ASS3.buildinMaps[builtinIdx].bytes);
                        return false;
                    }
                    MelonLogger.Msg("[Offline] Загружаем первую встроенную карту [0]");
                    __instance.CancelInvoke("RequestMap");
                    __instance.OnMapLoaded(Kube.ASS3.buildinMaps[0].bytes);
                    return false;
                }
                MelonLogger.Msg("[Offline] Файл карты не найден: " + dataMapsPath + " или " + persistentPath);
                return true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[Offline] Ошибка загрузки карты: " + ex.Message);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "Request", new Type[] { typeof(int), typeof(Dictionary<string, string>), typeof(ServerCallback) })]
    public class InterceptRequestPatch
    {
        public static bool Prefix(int q, Dictionary<string, string> paramData, ServerCallback cb)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MelonLogger.Msg("[Offline] Перехват requestCode=" + q);
                MadnessCubedOfflineMod.offlineServer.Request(q, paramData, cb);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(LoadParamsScript), "LoadDataFromNetwork")]
    public class BypassLoadDataPatch
    {
        public static bool Prefix()
        {
            MelonLogger.Msg("[Offline] LoadDataFromNetwork перехвачен — прямая инициализация");
            try
            {
                Kube.GPS.user = Kube.SN.playerUID;
                GameObject music = GameObject.FindGameObjectWithTag("Music");
                if (music != null)
                    music.SendMessage("ChangeMusic", 0, SendMessageOptions.DontRequireReceiver);

                Kube.SS = MadnessCubedOfflineMod.offlineServer;
                MadnessCubedOfflineMod.isReady = true;
                Kube.GPS.Init();

                MadnessCubedOfflineMod.ApplyLocalDataToGame();
                ShopCore.ApplyDefaultPrices();
                MissionsCore.InjectMissionData();

                for (int ci = 0; ci < Kube.GPS.cubesTimeOfEnd.Length; ci++)
                    Kube.GPS.cubesTimeOfEnd[ci] = (int)Time.time + 10000000;

                int st = (int)MadnessCubedOfflineMod.offlineServer.serverTime;
                if (st <= 0) st = 1000000;
                Kube.GPS.dayNum = st / 86400;
                Kube.GPS.bonusDay = st / 86400 % 10;
                Kube.GPS.playerNumMaps = 5;

                MelonCoroutines.Start(TransitionToMainMenu());
            }
            catch (Exception ex)
            {
                MelonLogger.Error("[Offline] Ошибка BypassLoadDataPatch: " + ex.ToString());
            }
            return false;
        }

        private static IEnumerator TransitionToMainMenu()
        {
            yield return null;
            MelonLogger.Msg("[Offline] Загрузка MainMenu...");
            Application.LoadLevel("MainMenu");
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "SaveMap", new Type[] { typeof(long), typeof(byte[]), typeof(GameObject), typeof(string) })]
    public class InterceptSaveMapPatch
    {
        public static bool Prefix(long mapId, byte[] mapData, GameObject go, string method)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.SaveMap(mapId, mapData, go, method);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "LoadMap")]
    public class InterceptLoadMapPatch
    {
        public static bool Prefix(long mapId)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.LoadMap(mapId);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "LoadIsMap")]
    public class InterceptLoadIsMapPatch
    {
        public static bool Prefix(long mapId, GameObject go, string method)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.LoadIsMap(mapId, go, method);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "SetMapName")]
    public class InterceptSetMapNamePatch
    {
        public static bool Prefix(long mapId, string mapName)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.SetMapName(mapId, mapName);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "BuyCubes")]
    public class InterceptBuyCubesPatch
    {
        public static bool Prefix(int numCubes, int numDays, GameObject go, string method)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.BuyCubes(numCubes, numDays, go, method);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "BuyItem")]
    public class InterceptBuyItemPatch
    {
        public static bool Prefix(int numItem, int itemsCount, GameObject go, string method)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.BuyItem(numItem, itemsCount, go, method);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "BuyWeapon")]
    public class InterceptBuyWeaponPatch
    {
        public static bool Prefix(int numWeapon, int tarif, GameObject go, string method)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.BuyWeapon(numWeapon, tarif, go, method);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "BuySpecItem")]
    public class InterceptBuySpecItemPatch
    {
        public static bool Prefix(int numSpecItem, int tarif, GameObject go, string method)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.BuySpecItem(numSpecItem, tarif, go, method);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "GetPlayerMoney")]
    public class InterceptGetMoneyPatch
    {
        public static bool Prefix(GameObject go, string method)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.GetPlayerMoney(go, method);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "UpgradeParam")]
    public class InterceptUpgradeParamPatch
    {
        public static bool Prefix(int numParam, GameObject go, string method)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.UpgradeParam(numParam, go, method);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "UpgradeParamUnlock")]
    public class InterceptUpgradeParamUnlockPatch
    {
        public static bool Prefix(int numParam, GameObject go, string method)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.UpgradeParamUnlock(numParam, go, method);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "UpgradeParamAllUnlock")]
    public class InterceptUpgradeAllPatch
    {
        public static bool Prefix(int needHealth, int needArmor, int needSpeed, int needJump, int needDefend, int upgradeMoney, GameObject go, string method)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.UpgradeParamAllUnlock(needHealth, needArmor, needSpeed, needJump, needDefend, upgradeMoney, go, method);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "BuySkin")]
    public class InterceptBuySkinPatch
    {
        public static bool Prefix(int numSkin, GameObject go, string method)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.BuySkin(numSkin, go, method);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "GoldToMoney")]
    public class InterceptGoldToMoneyPatch
    {
        public static bool Prefix(int numGold, GameObject go, string method)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.GoldToMoney(numGold, go, method);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "SaveNewName")]
    public class InterceptSaveNamePatch
    {
        public static bool Prefix(int id, string newName)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.SaveNewName(id, newName);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "BuyBullets")]
    public class InterceptBuyBulletsPatch
    {
        public static bool Prefix(int typeBullets, int numTarif, GameObject go, string method)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.BuyBullets(typeBullets, numTarif, go, method);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "SendEndLevel")]
    public class InterceptSendEndLevelPatch
    {
        public static bool Prefix(EndGameStats endGameStats, GameObject go, string method)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.SendEndLevel(endGameStats, go, method);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "UseItem")]
    public class InterceptUseItemPatch
    {
        public static bool Prefix(int numItem)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.UseItem(numItem);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "TakeItem")]
    public class InterceptTakeItemPatch
    {
        public static bool Prefix(int numItem, int itemCountNow, GameObject go, string method)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.TakeItem(numItem, itemCountNow, go, method);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "BuyVIP")]
    public class InterceptBuyVIPPatch
    {
        public static bool Prefix(int numVIP, GameObject go, string method)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.BuyVIP(numVIP, go, method);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "RegenerateMap")]
    public class InterceptRegenMapPatch
    {
        public static bool Prefix(int maptype, long numMap, ServerCallback cb)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.RegenerateMap(maptype, numMap, cb);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "SetSkin")]
    public class InterceptSetSkinPatch
    {
        public static bool Prefix(int numSkin)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.SetSkin(numSkin);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "SetClothes")]
    public class InterceptSetClothesPatch
    {
        public static bool Prefix(string clothes)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.SetClothes(clothes);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "BuyClothes")]
    public class InterceptBuyClothesPatch
    {
        public static bool Prefix(int numClothes, GameObject go, string method)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.BuyClothes(numClothes, go, method);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "SaveFastInventory")]
    public class InterceptSaveInvPatch
    {
        public static bool Prefix(int type, FastInventar[] inventory, ServerCallback cb)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.SaveFastInventory(type, inventory, cb);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "LoadStatistics")]
    public class InterceptLoadStatsPatch
    {
        public static bool Prefix(int dayFrom, int dayTo, GameObject go, string method)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.LoadStatistics(dayFrom, dayTo, go, method);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "UpgradeWeapon")]
    public class InterceptUpgradeWeaponPatch
    {
        public static bool Prefix(int bt, int q, JSONServerCallback upgradeWeaponDone)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.UpgradeWeapon(bt, q, upgradeWeaponDone);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "BuyWeaponSkin")]
    public class InterceptBuyWeaponSkinPatch
    {
        public static bool Prefix(int weaponId, int index, GameObject gameObject, string p)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.BuyWeaponSkin(weaponId, index, gameObject, p);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "UseWeaponSkin")]
    public class InterceptUseWeaponSkinPatch
    {
        public static bool Prefix(int weaponId, int index, GameObject gameObject, string p)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.UseWeaponSkin(weaponId, index, gameObject, p);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "EndMission")]
    public class InterceptEndMissionPatch
    {
        public static bool Prefix(int _missionId, EndGameStats endGameStats, ServerCallback onMissionEnd)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.EndMission(_missionId, endGameStats, onMissionEnd);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MissionReachTheExit), "TriggerExitReached")]
    public class TraceTriggerExitPatch
    {
        public static void Prefix()
        {
            MelonLogger.Msg("[Offline] TriggerExitReached called!");
        }
    }

    [HarmonyPatch(typeof(BattleControllerScript), "EndGame", new Type[] { typeof(BattleControllerScript.EndGameType) })]
    public class TraceEndGamePatch
    {
        public static void Prefix(BattleControllerScript __instance, BattleControllerScript.EndGameType endGameType)
        {
            MelonLogger.Msg("[Offline] BCS.EndGame called: type=" + endGameType + " gameProcess=" + __instance.gameProcess + " _missionId=" + __instance._missionId + " gameType=" + __instance.gameType);
        }

        public static void Postfix(BattleControllerScript __instance, BattleControllerScript.EndGameType endGameType)
        {
            MelonLogger.Msg("[Offline] BCS.EndGame finished: type=" + endGameType + " gameProcess=" + __instance.gameProcess);
        }
    }

    [HarmonyPatch(typeof(KubeAPI), "BuyNewMap")]
    public class InterceptBuyNewMapPatch
    {
        public static bool Prefix(int maptype, ServerCallback cb)
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                MadnessCubedOfflineMod.offlineServer.BuyNewMap(maptype, cb);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(EndMissionDialog), "Open")]
    public class FixEndMissionDialogPatch
    {
        public static void Prefix()
        {
            if (Kube.GPS != null)
            {
                if (Kube.GPS.inventarItems == null)
                {
                    MelonLoader.MelonLogger.Warning("[Offline] inventarItems was null, initializing");
                    Kube.GPS.inventarItems = new GameParamsScript.InventarItems(250);
                }
                if (Kube.GPS.inventarWeapons == null)
                {
                    MelonLoader.MelonLogger.Warning("[Offline] inventarWeapons was null, initializing");
                    Kube.GPS.inventarWeapons = new kube.cheat.ObscuredIntAB[80];
                }
            }
        }
    }

    [HarmonyPatch(typeof(SurvivalController), "UpdateHUD")]
    public class SurvivalNoTimerPatch
    {
        public static bool Prefix()
        {
            return !MapTopCore.currentNoTimer;
        }
    }

    [HarmonyPatch(typeof(Tab), "UpdateTimer")]
    public class TabNoTimerPatch
    {
        public static bool Prefix(Tab __instance)
        {
            if (MapTopCore.currentNoTimer && __instance is SurvTab)
            {
                if (__instance.timer != null)
                    __instance.timer.text = "--:--";
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BattleControllerScript), "OnMapLoaded")]
    public class ApplyMapstopEnvironmentPatch
    {
        public static void Postfix()
        {
            try
            {
                if (Kube.OH != null)
                    MapTopCore.ApplyEnvironment(Kube.OH.tempMap.Id);
            }
            catch (Exception e)
            {
                MelonLogger.Warning("[Offline] ApplyMapstopEnvironmentPatch error: " + e.Message);
            }
        }
    }

    [HarmonyPatch(typeof(EndMissionDialog), "Start")]
    public class FixEndMissionStartPatch
    {
        public static void Postfix(EndMissionDialog __instance)
        {
            try
            {
                var btn = __instance.bigButtonLabel.GetComponentInParent<UIButton>();
                if (btn != null)
                    btn.isEnabled = true;
            }
            catch (Exception e)
            {
                MelonLoader.MelonLogger.Warning("[Offline] FixEndMissionStartPatch error: " + e.Message);
            }
        }
    }

    [HarmonyPatch(typeof(PhotonNetwork), "Destroy", new Type[] { typeof(GameObject) })]
    public class SafePhotonDestroyPatch
    {
        public static bool Prefix(GameObject targetGo)
        {
            if (PhotonNetwork.offlineMode)
            {
                if (targetGo != null)
                    GameObject.Destroy(targetGo);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlatformSteam), "Init")]
    public class BypassSteamInitPatch
    {
        public static bool Prefix(PlatformSteam __instance, GameObject go, string func)
        {
            MelonLogger.Msg("[Offline] Обход инициализации Steam");
            Kube.SN = __instance;
            var uidField = typeof(PlatformSteam).GetField("_playerUID", BindingFlags.Instance | BindingFlags.NonPublic);
            if (uidField != null)
                uidField.SetValue(__instance, "offline_" + UnityEngine.Random.Range(100000, 999999));
            var initField = typeof(PlatformSteam).GetField("initialized", BindingFlags.Instance | BindingFlags.NonPublic);
            if (initField != null)
                initField.SetValue(__instance, true);
            try { Kube.GPS.SetLocale(CoreManager.Config != null ? CoreManager.Config.language : "ru_RU", true); } catch (Exception) { }
            if (go != null && !string.IsNullOrEmpty(func))
                go.SendMessage(func);
            return false;
        }
    }

    [HarmonyPatch(typeof(LoadParamsScript), "Start")]
    public class FixLoadParamsStartPatch
    {
        public static void Postfix()
        {
            if (MadnessCubedOfflineMod.offlineServer != null && !MadnessCubedOfflineMod.isReady)
            {
                Kube.SS = MadnessCubedOfflineMod.offlineServer;
                MadnessCubedOfflineMod.isReady = true;
                MelonLogger.Msg("[Offline] Kube.SS переназначен через LoadParamsScript.Start");
            }
        }
    }

    [HarmonyPatch(typeof(ServerScript), "Awake")]
    public class ReplaceServerScriptPatch
    {
        public static void Postfix()
        {
            if (MadnessCubedOfflineMod.offlineServer != null)
            {
                Kube.SS = MadnessCubedOfflineMod.offlineServer;
                MelonLogger.Msg("[Offline] ServerScript.Awake - Kube.SS заменён");
            }
        }
    }

    [HarmonyPatch(typeof(GameParamsScript), "Start")]
    public class GameParamsStartPatch
    {
        public static void Postfix()
        {
            MelonLogger.Msg("[Offline] GameParamsScript.Start - применение offline данных");
            if (CoreManager.PlayerData != null && Kube.GPS != null)
            {
                MadnessCubedOfflineMod.offlineServer = MadnessCubedOfflineMod.offlineServer ?? new OfflineServer();
                if (!MadnessCubedOfflineMod.isReady)
                {
                    Kube.SS = MadnessCubedOfflineMod.offlineServer;
                    MadnessCubedOfflineMod.isReady = true;
                }
                MadnessCubedOfflineMod.ApplyLocalDataToGame();
                ShopCore.ApplyDefaultPrices();
                MissionsCore.InjectMissionData();
            }
        }
    }
}
