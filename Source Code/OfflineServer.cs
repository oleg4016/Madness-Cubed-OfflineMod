using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;
using UnityEngine;
using kube;
using kube.data;

namespace MadnessCubedOffline
{
    public class OfflineServer : IBaseServer
    {
        public bool savingMap { get { return false; } }
        public bool loadingMap { get { return false; } }
        public float serverTime { get; set; }
        public int serverId { get { return _data.serverId; } set { _data.serverId = value; } }
        public string phpSecret { get { return "privetvsemhakeram!!pliznapishitekakvzlomali_altodor@rambler.ru"; } }

        public LocalPlayerData Data { get { return _data; } }
        private LocalPlayerData _data;
        private static readonly char[] _dc = new char[] { '^' };

        public OfflineServer()
        {
            _data = LocalPlayerData.Load();
            serverTime = UnityEngine.Random.Range(1000000, 9999999);
        }

        public void SetData(LocalPlayerData data) { _data = data; }

        public void SaveProfile()
        {
            WeaponCore.SyncFromGame();
            ProfileCore.SyncToGame();
            InventoryCore.SyncToGame();
            WeaponCore.SyncToGame();
            _data.Save();
        }

        public void Init(string phpServer, string mainPhpScript) { }

        public string[] DecodePlayerData(JsonData playerData)
        {
            string[] arr = new string[playerData.Count + 2];
            for (int i = 0; i < playerData.Count; i++)
                arr[i + 2] = playerData[i].ToString();
            if (arr.Length > 3 && arr[3] != null)
            {
                try { arr[3] = Encoding.ASCII.GetString(Convert.FromBase64String(arr[3])); }
                catch { }
            }
            return arr;
        }

        public JsonData BuildAndGetPlayerData()
        {
            ProfileCore.SyncFromGame();
            WeaponCore.SyncFromGame();
            string json = BuildPlayerDataJson();
            string sig = AuxFunc.GetMD5(json + phpSecret);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            byte[] sigBytes = Encoding.UTF8.GetBytes(sig);
            byte[] lenBytes = BitConverter.GetBytes(jsonBytes.Length);
            byte[] result = new byte[4 + jsonBytes.Length + 32];
            Buffer.BlockCopy(lenBytes, 0, result, 0, 4);
            Buffer.BlockCopy(jsonBytes, 0, result, 4, jsonBytes.Length);
            Buffer.BlockCopy(sigBytes, 0, result, 4 + jsonBytes.Length, 32);
            _data.serverId = UnityEngine.Random.Range(10000, 99999);
            JsonData data = JsonMapper.ToObject(json);
            if (data.Keys.Contains("id"))
                _data.serverId = int.Parse(data["id"].ToString());
            return data;
        }

        public void LoadPlayersParams(YieldCallback cb)
        {
            JsonData data = BuildAndGetPlayerData();
            if (cb != null) cb(data);
        }

        public void SaveMap(long mapId, byte[] mapData) { MapCore.SaveMap(mapId, mapData); }
        public void SaveMap(long mapId, byte[] mapData, GameObject go, string method) { MapCore.SaveMap(mapId, mapData, go, method); }
        public void LoadMap(long mapId) { MapCore.LoadMap(mapId); }
        public void LoadIsMap(long mapId, GameObject go, string method) { MapCore.LoadIsMap(mapId, go, method); }
        public void SetMapName(long mapId, string mapName) { MapCore.SetMapName(mapId, mapName); }

        public void BuyCubes(int numCubes, int numDays, GameObject go, string method)
        {
            ProfileCore.SyncFromGame();
            ShopCore.TryPurchaseCubes(numCubes, numDays);
            string[] cubStrs = new string[_data.cubesTimeOfEnd.Length];
            for (int i = 0; i < _data.cubesTimeOfEnd.Length; i++)
                cubStrs[i] = _data.cubesTimeOfEnd[i].ToString();
            string cubStr = string.Join(";", cubStrs);
            if (go != null && !string.IsNullOrEmpty(method))
                go.SendMessage(method, new string[] { "0", serverId.ToString(), _data.money1.ToString(), _data.money2.ToString(), cubStr, ((int)serverTime).ToString() });
            SaveProfile();
        }

        public void BuyItem(int numItem, int itemsCount, GameObject go, string method)
        {
            ProfileCore.SyncFromGame();
            ShopCore.TryPurchaseItem(numItem, itemsCount);
            string itemsStr = string.Join(";", Array.ConvertAll(_data.items, i => i.ToString()));
            go.SendMessage(method, new string[] { "0", serverId.ToString(), _data.money1.ToString(), _data.money2.ToString(), itemsStr });
            SaveProfile();
        }

        public void BuyWeapon(int numWeapon, int tarif, GameObject go, string method)
        {
            ProfileCore.SyncFromGame();
            ShopCore.TryPurchaseWeapon(numWeapon, tarif);
            string wpnStr = string.Join(";", Array.ConvertAll(_data.weapons, x => x > 0 ? "1" : "0"));
            go.SendMessage(method, new string[] { "0", serverId.ToString(), _data.money1.ToString(), _data.money2.ToString(), wpnStr, ((int)serverTime).ToString() });
            SaveProfile();
        }

        public void BuySpecItem(int numSpecItem, int tarif, GameObject go, string method)
        {
            ProfileCore.SyncFromGame();
            ShopCore.TryPurchaseSpecItem(numSpecItem, tarif);
            string spcStr = string.Join(";", Array.ConvertAll(_data.specItems, i => i.ToString()));
            go.SendMessage(method, new string[] { "0", serverId.ToString(), _data.money1.ToString(), _data.money2.ToString(), spcStr, ((int)serverTime).ToString() });
            SaveProfile();
        }

        public void GetPlayerMoney(GameObject go, string method)
        {
            ProfileCore.SyncFromGame();
            if (go != null && !string.IsNullOrEmpty(method))
                go.SendMessage(method, new string[] { "0", serverId.ToString(), _data.money1.ToString(), _data.money2.ToString() });
        }

        public void UpgradeParam(int numParam, GameObject go, string method)
        {
            ProfileCore.SyncFromGame();
            if (numParam < 0 || numParam > 4) { SaveProfile(); return; }
            int[] params_arr = ProfileCore.GetParamsArray();
            if (params_arr[numParam] >= 7) { SaveProfile(); return; }
            int price = 0;
            bool isGold = false;
            if (Kube.GPS != null && Kube.GPS.charParamsPrice != null)
            {
                price = Mathf.FloorToInt(Kube.GPS.charParamsPrice[numParam, params_arr[numParam], 1]);
                if (price == 0)
                {
                    price = Mathf.FloorToInt(Kube.GPS.charParamsPrice[numParam, params_arr[numParam], 2]);
                    isGold = true;
                }
            }
            if (price <= 0) price = 500;
            if (isGold)
            {
                if (_data.money2 < price) { SaveProfile(); return; }
                _data.money2 -= price;
            }
            else
            {
                if (_data.money1 < price) { SaveProfile(); return; }
                _data.money1 -= price;
            }
            params_arr[numParam]++;
            ProfileCore.SetParams(params_arr[0], params_arr[1], params_arr[2], params_arr[3], params_arr[4]);
            string paramStr = string.Join(";", Array.ConvertAll(params_arr, i => i.ToString()));
            if (go != null && !string.IsNullOrEmpty(method))
                go.SendMessage(method, new string[] { "0", serverId.ToString(), _data.money1.ToString(), _data.money2.ToString(), "", paramStr });
            SaveProfile();
        }

        public void UpgradeParamUnlock(int numParam, GameObject go, string method)
        {
            ProfileCore.SyncFromGame();
            if (numParam < 0 || numParam > 4) { SaveProfile(); return; }
            int[] params_arr = ProfileCore.GetParamsArray();
            if (params_arr[numParam] >= 7) { SaveProfile(); return; }
            int price = 0;
            bool isGold = false;
            if (Kube.GPS != null && Kube.GPS.charParamsPrice != null)
            {
                price = Mathf.FloorToInt(Kube.GPS.charParamsPrice[numParam, params_arr[numParam], 1]);
                if (price == 0)
                {
                    price = Mathf.FloorToInt(Kube.GPS.charParamsPrice[numParam, params_arr[numParam], 2]);
                    isGold = true;
                }
            }
            if (price <= 0) price = 500;
            price *= 2;
            if (isGold)
            {
                if (_data.money2 < price) { SaveProfile(); return; }
                _data.money2 -= price;
            }
            else
            {
                if (_data.money1 < price) { SaveProfile(); return; }
                _data.money1 -= price;
            }
            int key = (numParam << 3) + params_arr[numParam];
            if (key < _data.charUnlock.Length)
                _data.charUnlock[key] = 1;
            params_arr[numParam]++;
            ProfileCore.SetParams(params_arr[0], params_arr[1], params_arr[2], params_arr[3], params_arr[4]);
            string paramStr = string.Join(";", Array.ConvertAll(params_arr, i => i.ToString()));
            if (go != null && !string.IsNullOrEmpty(method))
                go.SendMessage(method, new string[] { "0", serverId.ToString(), _data.money1.ToString(), _data.money2.ToString(), "", paramStr });
            SaveProfile();
        }

        public void UpgradeParamAllUnlock(int needHealth, int needArmor, int needSpeed, int needJump, int needDefend, int upgradeMoney, GameObject go, string method)
        {
            ProfileCore.SyncFromGame();
            if (_data.money2 < upgradeMoney) { SaveProfile(); return; }
            _data.money2 -= upgradeMoney;
            for (int i = 0; i < 5; i++)
            {
                int[] vals = new int[] { needHealth, needArmor, needSpeed, needJump, needDefend };
                int key = (i << 3) + vals[i];
                if (key < _data.charUnlock.Length)
                    _data.charUnlock[key] = 1;
            }
            string paramStr = string.Join(";", Array.ConvertAll(ProfileCore.GetParamsArray(), i => i.ToString()));
            if (go != null && !string.IsNullOrEmpty(method))
                go.SendMessage(method, new string[] { "0", serverId.ToString(), _data.money1.ToString(), _data.money2.ToString(), "", paramStr });
            SaveProfile();
        }

        public void BuySkin(int numSkin, GameObject go, string method)
        {
            ProfileCore.SyncFromGame();
            ShopCore.TryPurchaseSkin(numSkin);
            string skinsStr = string.Join(";", Array.ConvertAll(_data.skins, i => i.ToString()));
            if (go != null && !string.IsNullOrEmpty(method))
                go.SendMessage(method, new string[] { "0", serverId.ToString(), _data.money1.ToString(), _data.money2.ToString(), skinsStr });
            SaveProfile();
        }

        public void GoldToMoney(int numGold, GameObject go, string method)
        {
            ProfileCore.SyncFromGame();
            int rate = 100;
            int money = numGold * rate;
            if (_data.money2 >= numGold)
            {
                _data.money2 -= numGold;
                _data.money1 += money;
            }
            if (go != null && !string.IsNullOrEmpty(method))
                go.SendMessage(method, new string[] { "0", serverId.ToString(), _data.money1.ToString(), _data.money2.ToString() });
            SaveProfile();
        }

        public void SaveNewName(int id, string newName)
        {
            _data.playerName = newName;
            if (Kube.GPS != null)
                Kube.GPS.playerName = newName;
            Kube.SendMonoMessage("UpdateName", "1;" + newName);
            SaveProfile();
        }

        public void BuyBullets(int typeBullets, int numTarif, GameObject go, string method)
        {
            ProfileCore.SyncFromGame();
            string bulletsStr = "0";
            if (Kube.IS != null && Kube.IS.bulletParams != null)
            {
                string[] bulletStrs = new string[Kube.IS.bulletParams.Length];
                for (int i = 0; i < Kube.IS.bulletParams.Length; i++)
                {
                    int[] amounts = Kube.IS.bulletParams[i].initialAmountArray;
                    if (amounts != null && amounts.Length > 0)
                    {
                        string[] amtStrs = new string[amounts.Length];
                        for (int j = 0; j < amounts.Length; j++)
                            amtStrs[j] = amounts[j].ToString();
                        bulletStrs[i] = string.Join(":", amtStrs);
                    }
                    else
                        bulletStrs[i] = "1:0:0:0:0";
                }
                bulletsStr = string.Join(";", bulletStrs);
            }
            if (go != null && !string.IsNullOrEmpty(method))
                go.SendMessage(method, new string[] { "0", serverId.ToString(), _data.money1.ToString(), _data.money2.ToString(), bulletsStr, ((int)serverTime).ToString() });
            SaveProfile();
        }

        public void SendEndLevel(EndGameStats endGameStats, GameObject go, string method)
        {
            MelonLoader.MelonLogger.Msg("[Offline] SendEndLevel called: deltaExp=" + endGameStats.deltaExp + " deltaMoney=" + endGameStats.deltaMoney + " deltaFrags=" + endGameStats.deltaFrags + " playerLevel=" + endGameStats.playerLevel + " newLevel=" + endGameStats.newLevel);
            ProfileCore.SyncFromGame();
            _data.exp = endGameStats.playerExp + (uint)endGameStats.deltaExp;
            _data.expPoints += endGameStats.deltaExp;
            _data.frags += endGameStats.deltaFrags;
            _data.money1 += endGameStats.deltaMoney;
            int levelsGained = endGameStats.newLevel - endGameStats.playerLevel;
            if (levelsGained > 0)
            {
                int levelUpReward = levelsGained * 1000;
                _data.money1 += levelUpReward;
                _data.level = endGameStats.newLevel;
                MelonLoader.MelonLogger.Msg("[Offline] Level up! +" + levelsGained + " level(s), reward: " + levelUpReward);
            }
            string resp = "0^" + _data.exp + "^" + _data.exp + "^" + _data.frags + "^" + _data.money1 + "^" + _data.level + "^" + _data.money2;
            string[] strs = resp.Split(_dc);
            go.SendMessage(method, strs);
            SaveProfile();
        }

        public int UnixTime() { return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds; }

        public void BuyNewMap(int maptype, ServerCallback cb)
        {
            _data.money1 -= 1000;
            if (cb != null) cb("0^1^" + maptype);
            SaveProfile();
        }

        public void UseItem(int numItem) { }

        public void TakeItem(int numItem, int itemCountNow, GameObject go, string method)
        {
            InventoryCore.TakeItem(numItem, itemCountNow);
            if (go != null && !string.IsNullOrEmpty(method))
                go.SendMessage(method, "0^1");
            SaveProfile();
        }

        public void Request(int q, object param, ServerCallback cb)
        {
            Dictionary<string, string> d = new Dictionary<string, string>();
            d["object"] = param.ToString();
            Request(q, d, cb);
        }

        public void Request(int q, Dictionary<string, string> paramData, ServerCallback cb)
        {
            string result = HandleRequest(q, paramData);
            if (cb != null) cb(result);
        }

        private void SyncFromGame()
        {
            ProfileCore.SyncFromGame();
            WeaponCore.SyncFromGame();
        }

        private void SyncToGame()
        {
            ProfileCore.SyncToGame();
            InventoryCore.SyncToGame();
            WeaponCore.SyncToGame();
        }

        private string HandleRequest(int q, Dictionary<string, string> data)
        {
            switch (q)
            {
                case 666: return MissionsCore.BuildMissionsJson();
                case 667: return MissionsCore.HandleEndMission(
                    data.ContainsKey("score") ? int.Parse(data["score"]) : 0,
                    data.ContainsKey("money") ? int.Parse(data["money"]) : 0,
                    data.ContainsKey("frags") ? int.Parse(data["frags"]) : 0);
                case 668: return "0^1";
                case 616: return "0^1^" + (data.ContainsKey("maptype") ? data["maptype"] : "0");
                case 624: return "1";
                case 700: return HandleUpgradeWeapon(data);
                case 701: return "{}";
                case 702: return "{}";
                case 6: return "0^0^1";
                case 11:
                    ProfileCore.SyncFromGame();
                    return "0^" + _data.money1 + "^" + _data.money2;
                case 12: return "1;" + (data.ContainsKey("newname") ? data["newname"] : "Player");
                case 905: return HandleChestOpen(data);
                case 901: return "{}";
                case 903: return "{}";
                case 906: return "{\"value\":\"131071\"}";
                case 800:
                    return MapTopCore.BuildMyMapsJson();
                case 801:
                    return MapTopCore.BuildTopJson(
                        data.ContainsKey("d") ? int.Parse(data["d"]) : 0);
                case 802:
                    return MapTopCore.SaveMyMap(
                        data.ContainsKey("mapid") ? long.Parse(data["mapid"]) : 0,
                        data.ContainsKey("name") ? data["name"] : "Map",
                        data.ContainsKey("type") ? int.Parse(data["type"]) : 0,
                        data.ContainsKey("canbreak") ? int.Parse(data["canbreak"]) : 0,
                        data.ContainsKey("daytime") ? int.Parse(data["daytime"]) : 1,
                        data.ContainsKey("oid") ? data["oid"] : null);
                case 803:
                    return MapTopCore.RemoveMyMap(
                        data.ContainsKey("oid") ? int.Parse(data["oid"]) : 0);
                case 804:
                    if (data.ContainsKey("oid"))
                        MapTopCore.IncrementHit(int.Parse(data["oid"]));
                    return "1";
                default:
                    Debug.Log("[Offline] Unhandled requestCode: " + q);
                    return "0";
            }
        }

        public void SendStat(string statName) { }
        public void SendStatCount(string statName, int count) { }

        public void BuyVIP(int numVIP, GameObject go, string method)
        {
            ProfileCore.SyncFromGame();
            _data.money2 -= 500;
            int vipEndTime = (int)serverTime + (numVIP * 86400 * 30);
            int st = (int)serverTime;
            string sigInput = serverId.ToString() + _data.money2 + vipEndTime + st + phpSecret;
            string sig = AuxFunc.GetMD5(sigInput);
            string resp = "0^" + serverId + "^" + _data.money2 + "^" + vipEndTime + "^" + sig + "^" + st;
            if (go != null && !string.IsNullOrEmpty(method))
                go.SendMessage(method, resp);
            SaveProfile();
        }

        public void RegenerateMap(int maptype, long numMap, ServerCallback cb)
        {
            if (cb != null) cb("1");
        }

        public void SetSkin(int numSkin)
        {
            ProfileCore.Skin = numSkin;
            if (Kube.GPS != null)
                Kube.GPS.playerSkin = numSkin;
            Kube.SendMonoMessage("UpdateChar");
            SaveProfile();
        }

        public void SetClothes(string clothes)
        {
            string[] cls = clothes.Split(';');
            int[] vals = Array.ConvertAll(cls, s => int.Parse(s));
            InventoryCore.SetClothesArray(vals);
            if (Kube.GPS != null)
                for (int i = 0; i < cls.Length && i < Kube.GPS.playerClothes.Length; i++)
                    Kube.GPS.playerClothes[i] = int.Parse(cls[i]);
            Kube.SendMonoMessage("UpdateChar");
            SaveProfile();
        }

        public void BuyClothes(int numClothes, GameObject go, string method)
        {
            ProfileCore.SyncFromGame();
            ShopCore.TryPurchaseClothes(numClothes);
            string clothesStr = string.Join(";", Array.ConvertAll(_data.isClothes, i => i.ToString()));
            int st = (int)serverTime;
            string sigInput = serverId.ToString() + _data.money1 + _data.money2 + clothesStr + st + phpSecret;
            string sig = AuxFunc.GetMD5(sigInput);
            string resp = "0^" + serverId + "^" + _data.money1 + "^" + _data.money2 + "^" + clothesStr + "^" + sig + "^" + st;
            if (go != null && !string.IsNullOrEmpty(method))
                go.SendMessage(method, resp);
            SaveProfile();
        }

        public void SaveFastInventory(int type, FastInventar[] inventory, ServerCallback cb)
        {
            WeaponCore.SaveFastInventory(type, inventory);
            if (cb != null) cb("0^1");
            SaveProfile();
        }

        public void LoadStatistics(int dayFrom, int dayTo, GameObject go, string method)
        {
            if (go != null && !string.IsNullOrEmpty(method))
                go.SendMessage(method, "0^0^0^0^0^0^0^0^0^0");
        }

        public void UpgradeWeapon(int bt, int q, JSONServerCallback upgradeWeaponDone)
        {
            ProfileCore.SyncFromGame();
            if (Kube.IS == null || Kube.IS.weaponParams == null || bt < 0 || bt >= Kube.IS.weaponParams.Length)
            {
                if (upgradeWeaponDone != null) upgradeWeaponDone(JsonMapper.ToObject("{}"));
                return;
            }
            WeaponParamsObj wp = Kube.IS.weaponParams[bt];
            if (wp == null)
            {
                if (upgradeWeaponDone != null) upgradeWeaponDone(JsonMapper.ToObject("{}"));
                return;
            }
            int currentLevel = 0;
            int maxLevel = 0;
            switch (q)
            {
                case 0: currentLevel = wp.currentDamageIndex; maxLevel = wp.Damage.Length - 1; break;
                case 1: currentLevel = wp.currentAccuracyIndex; maxLevel = wp.Accuracy.Length - 1; break;
                case 2: currentLevel = wp.currentDeltaShotIndex; maxLevel = wp.DeltaShotArray.Length - 1; break;
                case 3: currentLevel = wp.currentClipSizeIndex; maxLevel = wp.clipSize.Length - 1; break;
                default:
                    if (upgradeWeaponDone != null) upgradeWeaponDone(JsonMapper.ToObject("{}"));
                    return;
            }
            if (currentLevel >= maxLevel)
            {
                if (upgradeWeaponDone != null) upgradeWeaponDone(JsonMapper.ToObject("{}"));
                return;
            }
            PriceValue pv;
            try { pv = Kube.GPS.upgradePrice[bt, q, currentLevel]; }
            catch { pv = new PriceValue(100, false); }
            int newLevel = currentLevel + 1;
            if (pv.isGold)
            {
                if (_data.money2 < pv.price) { if (upgradeWeaponDone != null) upgradeWeaponDone(JsonMapper.ToObject("{}")); return; }
                _data.money2 -= pv.price;
            }
            else
            {
                if (_data.money1 < pv.price) { if (upgradeWeaponDone != null) upgradeWeaponDone(JsonMapper.ToObject("{}")); return; }
                _data.money1 -= pv.price;
            }
            switch (q)
            {
                case 0: wp.currentDamageIndex = newLevel; break;
                case 1: wp.currentAccuracyIndex = newLevel; wp.accuarcy = wp.Accuracy[newLevel]; break;
                case 2: wp.currentDeltaShotIndex = newLevel; wp.DeltaShot = wp.DeltaShotArray[newLevel]; break;
                case 3: wp.currentClipSizeIndex = newLevel; break;
            }
            ProfileCore.SyncToGame();
            SaveProfile();
            string wpKey = bt.ToString() + "_" + q.ToString();
            string jsonStr = "{\"money\":[" + _data.money1 + "," + _data.money2 + "],\"wp\":{\"" + wpKey + "\":" + newLevel + "}}";
            JsonData jsonData = JsonMapper.ToObject(jsonStr);
            if (upgradeWeaponDone != null)
                upgradeWeaponDone(jsonData["wp"]);
        }

        public void SendStatIoTrack(string statName) { SendStatIoTrack(statName, 1); }
        public void SendStatIoTrack(string statName, int inc) { }

        public void LoadMissions(JSONServerCallback missionLoadDone)
        {
            string json = MissionsCore.BuildMissionsJson();
            if (missionLoadDone != null)
                missionLoadDone(JsonMapper.ToObject(json));
        }

        public void EndMission(int missionId, EndGameStats endGameStats, ServerCallback onMissionEnd)
        {
            MelonLoader.MelonLogger.Msg("[Offline] EndMission called: missionId=" + missionId + " deltaExp=" + endGameStats.deltaExp + " deltaMoney=" + endGameStats.deltaMoney + " deltaFrags=" + endGameStats.deltaFrags);
            string json = MissionsCore.HandleMissionEndWithLevel(missionId, endGameStats.deltaExp, endGameStats.deltaMoney, endGameStats.deltaFrags, endGameStats.newLevel);
            MelonLoader.MelonLogger.Msg("[Offline] EndMission response: " + json);
            if (Kube.GPS != null)
            {
                if (Kube.GPS.inventarItems == null)
                    Kube.GPS.inventarItems = new GameParamsScript.InventarItems(250);
                if (Kube.GPS.inventarWeapons == null || Kube.GPS.inventarWeapons.Length == 0)
                    Kube.GPS.inventarWeapons = new kube.cheat.ObscuredIntAB[80];
                for (int i = 0; i < Kube.GPS.inventarWeapons.Length; i++)
                {
                    if ((int)Kube.GPS.inventarWeapons[i] <= 0)
                        Kube.GPS.inventarWeapons[i] = (int)UnityEngine.Time.time + 10000000;
                }
                if (Kube.GPS.fastInventarWeapon == null)
                    Kube.GPS.fastInventarWeapon = new FastInventar[11];
            }
            SaveProfile();
            try
            {
                onMissionEnd(json);
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error("[Offline] onMissionEnd error: " + ex);
                try
                {
                    JsonData jd = JsonMapper.ToObject(json);
                    int rewardMoney = int.Parse(jd["money"].ToString());
                    int rewardGold = int.Parse(jd["gold"].ToString());
                    bool firstTime = (bool)jd["firsttime"];
                    if (Kube.GPS != null)
                    {
                        Kube.GPS.playerExp = uint.Parse(jd["exp"].ToString());
                        Kube.GPS.playerFrags = int.Parse(jd["frags"].ToString());
                        Kube.GPS.playerLevel = int.Parse(jd["level"].ToString());
                        Kube.GPS.playerMoney1 = (int)Kube.GPS.playerMoney1 + rewardMoney;
                        Kube.GPS.playerMoney2 = (int)Kube.GPS.playerMoney2 + rewardGold;
                        string bonusStr = jd["bonus"].ToString();
                        if (firstTime && !string.IsNullOrEmpty(bonusStr) && bonusStr != "0")
                            MissionsCore.ProcessBonusItemsOnGPS(bonusStr);
                    }
                    EndMissionDialog dlg = GameObject.FindObjectOfType<EndMissionDialog>();
                    if (dlg != null)
                    {
                        if (dlg.money1 != null) dlg.money1.text = rewardMoney.ToString();
                        if (dlg.money2 != null)
                        {
                            dlg.money2.text = rewardGold.ToString();
                            if (dlg.money2.transform != null && dlg.money2.transform.parent != null)
                                dlg.money2.transform.parent.gameObject.SetActive(firstTime);
                        }
                        if (dlg.bigButtonLabel != null)
                        {
                            UIButton btn = dlg.bigButtonLabel.GetComponentInParent<UIButton>();
                            if (btn != null) btn.isEnabled = true;
                        }
                        Type mrType = typeof(kube.data.MissionResult);
                        var mr = Activator.CreateInstance(mrType);
                        mrType.GetField("firstTime").SetValue(mr, firstTime);
                        mrType.GetField("endGameMoney").SetValue(mr, rewardMoney);
                        mrType.GetField("endGameGold").SetValue(mr, rewardGold);
                        var mrField = typeof(EndMissionDialog).GetField("_missionResult", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        if (mrField != null) mrField.SetValue(dlg, mr);
                    }
                }
                catch (System.Exception ex2) { MelonLoader.MelonLogger.Error("[Offline] EndMission fallback error: " + ex2); }
            }
        }

        public void BuyWeaponSkin(int weaponId, int index, GameObject gameObject, string p)
        {
            if (gameObject != null && !string.IsNullOrEmpty(p))
                gameObject.SendMessage(p);
        }

        public void UseWeaponSkin(int weaponId, int index, GameObject gameObject, string p)
        {
            if (Kube.GPS != null && weaponId < Kube.GPS.weaponsCurrentSkin.Length)
                Kube.GPS.weaponsCurrentSkin[weaponId] = index;
            if (gameObject != null && !string.IsNullOrEmpty(p))
                gameObject.SendMessage(p);
        }

        private string BuildPlayerDataJson()
        {
            int st = (int)serverTime;
            if (st <= 0) st = 1000000;
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"id\":\"").Append(_data.serverId).Append("\",");
            sb.Append("\"token\":\"offline_token_123\",");
            sb.Append("\"t\":\"").Append(st).Append("\",");
            sb.Append("\"d\":\"").Append(st / 86400).Append("\",");
            sb.Append("\"f\":\"0\",");
            sb.Append("\"r\":[");
            sb.Append("\"").Append(_data.serverId).Append("\",");
            sb.Append("\"").Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(_data.playerName))).Append("\",");
            sb.Append("\"5\",");
            sb.Append("\"1;1;1;1;1;1\",");
            sb.Append("\"").Append(InventoryCore.BuildItemString()).Append("\",");
            sb.Append("\"").Append(_data.money1).Append("\",");
            sb.Append("\"").Append(_data.money2).Append("\",");
            sb.Append("\"").Append(InventoryCore.BuildWeaponString()).Append("\",");
            sb.Append("\"").Append(_data.health).Append(";").Append(_data.armor).Append(";").Append(_data.speed).Append(";").Append(_data.jump).Append(";").Append(_data.defend).Append("\",");
            sb.Append("\"0\",");
            sb.Append("\"").Append((int)_data.exp).Append("\",");
            sb.Append("\"").Append(_data.frags).Append("\",");
            sb.Append("\"").Append(_data.level).Append("\",");
            sb.Append("\"").Append(InventoryCore.BuildSkinString()).Append("\",");
            sb.Append("\"").Append(ZeroStr(32)).Append("\",");
            sb.Append("\"0\",");
            sb.Append("\"").Append("0").Append("\",");
            sb.Append("\"").Append("1").Append("\",");
            sb.Append("\"").Append(_data.skin).Append("\",");
            sb.Append("\"").Append(ZeroStr(22)).Append("\",");
            sb.Append("\"0\",\"0\",\"0\",");
            sb.Append("\"").Append(string.Join(";", Array.ConvertAll(_data.isClothes, x => x > 0 ? "1" : "0"))).Append("\",");
            sb.Append("\"").Append(string.Join(";", Array.ConvertAll(_data.clothes, x => x.ToString()))).Append("\"");
            sb.Append("],\"offer\":[],\"sq\":{\"gold\":0,\"money\":0,\"m\":0,\"c\":0},");
            for (int fi = 0; fi < Math.Min(6, _data.fastInventarWeapon.Length); fi++)
                if (_data.fastInventarWeapon[fi] >= 0)
                    sb.Append("\"fi").Append(fi).Append("\":\"4^").Append(_data.fastInventarWeapon[fi]).Append("\",");
            sb.Append("\"price\":[\"\",");
            sb.Append(GeneratePriceArray());
            sb.Append("]}");
            return sb.ToString();
        }

        private string GeneratePriceArray()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 5; i++)
                sb.Append("\"0;0;0;0;0\",");
            for (int i = 0; i < 6; i++)
                sb.Append("\"0;0;0;0\",");
            sb.Append("\"100\",");
            sb.Append("\"").Append(ColonStr(32, 4)).Append("\",");
            sb.Append("\"").Append(ColonStr(32, 4)).Append("\",");
            sb.Append("\"").Append(ColonStr(12, 4)).Append("\",");
            sb.Append("\"").Append(ColonStr(41, 5)).Append("\",");
            sb.Append("\"0;0;0;0;0\",");
            for (int i = 0; i < 10; i++)
                sb.Append("\"0;0;0;0;0\",");
            sb.Append("\"1000\",");
            sb.Append("\"0\",");
            sb.Append("\"0;0\",");
            sb.Append("\"0;0\",");
            sb.Append("\"0\",");
            sb.Append("\"0\",");
            sb.Append("\"0\",");
            sb.Append("\"0\",");
            for (int i = 0; i < 6; i++)
                sb.Append("\"0;0;0\",");
            for (int i = 0; i < 120; i++)
                sb.Append("\"0;0;0\",");
            for (int i = 0; i < 50; i++)
                sb.Append("\"0;0;0;0;0;0;0\",");
            for (int i = 0; i < 22; i++)
                sb.Append("\"0;0;0;0;0;0;0\",");
            for (int i = 0; i < 66; i++)
                sb.Append("\"0;0;0;0\",");
            sb.Append("\"0\",\"0\",\"0\",\"0\",\"0\",\"0\",\"0\",\"0\",\"0\",\"0\",\"0\",");
            sb.Append("\"50;120;22;0;66\"");
            return sb.ToString();
        }

        private static string ZeroStr(int count)
        {
            if (count <= 0) return "0";
            string[] parts = new string[count];
            for (int i = 0; i < count; i++)
                parts[i] = "0";
            return string.Join(";", parts);
        }

        private static string ColonStr(int groups, int partsPerGroup)
        {
            string[] groupArr = new string[groups];
            for (int g = 0; g < groups; g++)
            {
                string[] parts = new string[partsPerGroup];
                for (int p = 0; p < partsPerGroup; p++)
                    parts[p] = "0";
                groupArr[g] = string.Join(";", parts);
            }
            return string.Join(":", groupArr);
        }

        private string BuildUnlockString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _data.weaponUnlock.Length; i++)
                if (_data.weaponUnlock[i] == 1) sb.Append("w" + i + ";");
            for (int i = 0; i < _data.specUnlock.Length; i++)
                if (_data.specUnlock[i] == 1) sb.Append("s" + i + ";");
            for (int i = 0; i < _data.itemUnlock.Length; i++)
                if (_data.itemUnlock[i] == 1) sb.Append("i" + i + ";");
            for (int i = 0; i < _data.missionUnlock.Length; i++)
                if (_data.missionUnlock[i] == 1) sb.Append("m" + i + ";");
            for (int i = 0; i < _data.charUnlock.Length; i++)
                if (_data.charUnlock[i] == 1) sb.Append("c" + i + ";");
            if (sb.Length > 0) sb.Length--;
            return sb.ToString();
        }

        private string HandleUpgradeWeapon(Dictionary<string, string> data)
        {
            ProfileCore.SyncFromGame();
            int bt = data.ContainsKey("weapon") ? int.Parse(data["weapon"]) : 0;
            int q = data.ContainsKey("q") ? int.Parse(data["q"]) : 0;
            int newLevel = 0;
            if (Kube.IS != null && Kube.IS.weaponParams != null && bt >= 0 && bt < Kube.IS.weaponParams.Length)
            {
                WeaponParamsObj wp = Kube.IS.weaponParams[bt];
                if (wp != null)
                {
                    int currentLevel = 0;
                    int maxLevel = 0;
                    switch (q)
                    {
                        case 0: currentLevel = wp.currentDamageIndex; maxLevel = wp.Damage.Length - 1; break;
                        case 1: currentLevel = wp.currentAccuracyIndex; maxLevel = wp.Accuracy.Length - 1; break;
                        case 2: currentLevel = wp.currentDeltaShotIndex; maxLevel = wp.DeltaShotArray.Length - 1; break;
                        case 3: currentLevel = wp.currentClipSizeIndex; maxLevel = wp.clipSize.Length - 1; break;
                    }
                    if (currentLevel < maxLevel)
                    {
                        PriceValue pv;
                        try { pv = Kube.GPS.upgradePrice[bt, q, currentLevel]; }
                        catch { pv = new PriceValue(100, false); }
                        newLevel = currentLevel + 1;
                        if (pv.isGold)
                        {
                            if (_data.money2 >= pv.price) { _data.money2 -= pv.price; }
                        }
                        else
                        {
                            if (_data.money1 >= pv.price) { _data.money1 -= pv.price; }
                        }
                        switch (q)
                        {
                            case 0: wp.currentDamageIndex = newLevel; break;
                            case 1: wp.currentAccuracyIndex = newLevel; wp.accuarcy = wp.Accuracy[newLevel]; break;
                            case 2: wp.currentDeltaShotIndex = newLevel; wp.DeltaShot = wp.DeltaShotArray[newLevel]; break;
                            case 3: wp.currentClipSizeIndex = newLevel; break;
                        }
                    }
                }
            }
            ProfileCore.SyncToGame();
            SaveProfile();
            string wpKey = bt + "_" + q;
            return "{\"money\":[" + _data.money1 + "," + _data.money2 + "],\"wp\":{\"" + wpKey + "\":" + newLevel + "}}";
        }

        private string HandleChestOpen(Dictionary<string, string> data)
        {
            ProfileCore.SyncFromGame();
            string boxId = data.ContainsKey("box_id") ? data["box_id"] : "0";
            string arrJson = "{\"arr\":[\"i" + boxId + "\"]}";
            string sig = AuxFunc.GetMD5(arrJson + phpSecret);
            return arrJson + sig;
        }
    }
}
