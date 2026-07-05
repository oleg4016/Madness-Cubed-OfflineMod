using System;
using UnityEngine;
using kube;

namespace MadnessCubedOffline
{
    public static class ProfileCore
    {
        private static LocalPlayerData Data { get { return CoreManager.PlayerData; } }

        public static int Money1 { get { return Data.money1; } set { Data.money1 = value; FireBalanceChanged(); } }
        public static int Money2 { get { return Data.money2; } set { Data.money2 = value; FireBalanceChanged(); } }
        public static int Level { get { return Data.level; } set { Data.level = value; } }
        public static uint Exp { get { return Data.exp; } set { Data.exp = value; } }
        public static int ExpPoints { get { return Data.expPoints; } set { Data.expPoints = value; } }
        public static int Frags { get { return Data.frags; } set { Data.frags = value; } }
        public static int Health { get { return Data.health; } set { Data.health = value; } }
        public static int Armor { get { return Data.armor; } set { Data.armor = value; } }
        public static int Speed { get { return Data.speed; } set { Data.speed = value; } }
        public static int Jump { get { return Data.jump; } set { Data.jump = value; } }
        public static int Defend { get { return Data.defend; } set { Data.defend = value; } }
        public static int Skin { get { return Data.skin; } set { Data.skin = value; } }

        public static void AddMoney1(int amount)
        {
            Data.money1 += amount;
            FireBalanceChanged();
        }

        public static bool SpendMoney1(int amount)
        {
            if (Data.money1 >= amount)
            {
                Data.money1 -= amount;
                FireBalanceChanged();
                return true;
            }
            return false;
        }

        public static bool SpendMoney2(int amount)
        {
            if (Data.money2 >= amount)
            {
                Data.money2 -= amount;
                FireBalanceChanged();
                return true;
            }
            return false;
        }

        public static bool SpendMoney(int price1, int price2)
        {
            if (price1 > 0)
            {
                if (Data.money1 >= price1)
                {
                    Data.money1 -= price1;
                    FireBalanceChanged();
                    return true;
                }
                return false;
            }
            if (price2 > 0 && Data.money2 >= price2)
            {
                Data.money2 -= price2;
                FireBalanceChanged();
                return true;
            }
            return price1 == 0 && price2 == 0;
        }

        public static int[] GetParamsArray()
        {
            return new int[] { Data.health, Data.armor, Data.speed, Data.jump, Data.defend };
        }

        public static void SetParams(int health, int armor, int speed, int jump, int defend)
        {
            Data.health = health;
            Data.armor = armor;
            Data.speed = speed;
            Data.jump = jump;
            Data.defend = defend;
        }

        public static void SyncFromGame()
        {
            if (Kube.GPS == null) return;
            Data.money1 = (int)Kube.GPS.playerMoney1;
            Data.money2 = (int)Kube.GPS.playerMoney2;
            Data.level = Kube.GPS.playerLevel;
            Data.exp = Kube.GPS.playerExp;
            Data.expPoints = Kube.GPS.playerExpPoints;
            Data.frags = Kube.GPS.playerFrags;
            Data.health = Kube.GPS.playerHealth;
            Data.armor = Kube.GPS.playerArmor;
            Data.speed = Kube.GPS.playerSpeed;
            Data.jump = Kube.GPS.playerJump;
            Data.defend = Kube.GPS.playerDefend;
            Data.skin = Kube.GPS.playerSkin;
            if (Kube.GPS.cubesTimeOfEnd != null)
                for (int i = 0; i < Data.cubesTimeOfEnd.Length && i < Kube.GPS.cubesTimeOfEnd.Length; i++)
                    Data.cubesTimeOfEnd[i] = (int)Kube.GPS.cubesTimeOfEnd[i];
        }

        public static void SyncToGame()
        {
            if (Kube.GPS == null) return;
            Kube.GPS.playerMoney1 = Data.money1;
            Kube.GPS.playerMoney2 = Data.money2;
            Kube.GPS.playerLevel = Data.level;
            Kube.GPS.playerExp = Data.exp;
            Kube.GPS.playerExpPoints = Data.expPoints;
            Kube.GPS.playerFrags = Data.frags;
            Kube.GPS.playerHealth = Data.health;
            Kube.GPS.playerArmor = Data.armor;
            Kube.GPS.playerSpeed = Data.speed;
            Kube.GPS.playerJump = Data.jump;
            Kube.GPS.playerDefend = Data.defend;
            Kube.GPS.playerSkin = Data.skin;
        }

        private static void FireBalanceChanged()
        {
            if (CoreEvents.OnBalanceChanged != null)
                CoreEvents.OnBalanceChanged(Data.money1, Data.money2);
        }
    }
}
