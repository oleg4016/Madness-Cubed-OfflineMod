using System;
using UnityEngine;
using kube;
using kube.data;

namespace MadnessCubedOffline
{
    public static class WeaponCore
    {
        private static LocalPlayerData Data { get { return CoreManager.PlayerData; } }

        public static int GetEquippedWeapon(int slot)
        {
            if (slot >= 0 && slot < Data.fastInventarWeapon.Length)
                return Data.fastInventarWeapon[slot];
            return -1;
        }

        public static void SetEquippedWeapon(int slot, int weaponId)
        {
            if (slot >= 0 && slot < Data.fastInventarWeapon.Length)
            {
                Data.fastInventarWeapon[slot] = weaponId;
                if (CoreEvents.OnWeaponEquipped != null)
                    CoreEvents.OnWeaponEquipped(slot, weaponId);
            }
        }

        public static void SaveFastInventory(int type, FastInventar[] inventory)
        {
            if (inventory != null)
            {
                for (int i = 0; i < inventory.Length && i < Data.fastInventarWeapon.Length; i++)
                {
                    if (inventory[i] != null)
                    {
                        Data.fastInventarWeapon[i] = inventory[i].Num;
                        if (Kube.GPS != null && Kube.GPS.fastInventarWeapon != null && i < Kube.GPS.fastInventarWeapon.Length)
                        {
                            if (Kube.GPS.fastInventarWeapon[i] == null)
                                Kube.GPS.fastInventarWeapon[i] = new FastInventar(inventory[i].Type, inventory[i].Num);
                            else
                            {
                                Kube.GPS.fastInventarWeapon[i].Type = inventory[i].Type;
                                Kube.GPS.fastInventarWeapon[i].Num = inventory[i].Num;
                            }
                        }
                    }
                }
            }
        }

        public static void SyncToGame()
        {
            if (Kube.GPS == null || Kube.GPS.fastInventarWeapon == null) return;
            for (int i = 0; i < Data.fastInventarWeapon.Length && i < Kube.GPS.fastInventarWeapon.Length; i++)
            {
                if (Data.fastInventarWeapon[i] < 0)
                {
                    if (Kube.GPS.fastInventarWeapon[i] == null)
                        Kube.GPS.fastInventarWeapon[i] = new FastInventar(-1, 0);
                    else
                    {
                        Kube.GPS.fastInventarWeapon[i].Type = -1;
                        Kube.GPS.fastInventarWeapon[i].Num = 0;
                    }
                }
                else
                {
                    if (Kube.GPS.fastInventarWeapon[i] == null)
                        Kube.GPS.fastInventarWeapon[i] = new FastInventar(4, Data.fastInventarWeapon[i]);
                    else
                    {
                        Kube.GPS.fastInventarWeapon[i].Type = 4;
                        Kube.GPS.fastInventarWeapon[i].Num = Data.fastInventarWeapon[i];
                    }
                }
            }
        }

        public static void SyncFromGame()
        {
            if (Kube.GPS == null || Kube.GPS.fastInventarWeapon == null) return;
            for (int i = 0; i < Data.fastInventarWeapon.Length && i < Kube.GPS.fastInventarWeapon.Length; i++)
            {
                if (Kube.GPS.fastInventarWeapon[i] != null && Kube.GPS.fastInventarWeapon[i].Type == 4)
                    Data.fastInventarWeapon[i] = Kube.GPS.fastInventarWeapon[i].Num;
                else
                    Data.fastInventarWeapon[i] = -1;
            }
        }
    }
}
