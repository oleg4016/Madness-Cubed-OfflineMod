using System;
using UnityEngine;
using kube;

namespace MadnessCubedOffline
{
    public static class ShopCore
    {
        public static int GetItemPrice(int numItem, out int price1, out int price2)
        {
            price1 = 0;
            price2 = 0;
            if (Kube.GPS != null && Kube.GPS.inventarItems != null)
            {
                price1 = Kube.GPS.inventarItemPrice1 != null && numItem < Kube.GPS.inventarItemPrice1.Length ? (int)Kube.GPS.inventarItemPrice1[numItem] : 0;
                price2 = Kube.GPS.inventarItemPrice2 != null && numItem < Kube.GPS.inventarItemPrice2.Length ? (int)Kube.GPS.inventarItemPrice2[numItem] : 0;
            }
            return price1;
        }

        public static int GetWeaponPrice(int numWeapon, int tarif, out int price1, out int price2)
        {
            price1 = 0;
            price2 = 0;
            if (Kube.GPS != null)
            {
                price1 = Kube.GPS.weaponsPrice1 != null && numWeapon < Kube.GPS.weaponsPrice1.GetLength(0) && tarif < Kube.GPS.weaponsPrice1.GetLength(1) ? (int)Kube.GPS.weaponsPrice1[numWeapon, tarif] : 0;
                price2 = Kube.GPS.weaponsPrice2 != null && numWeapon < Kube.GPS.weaponsPrice2.GetLength(0) && tarif < Kube.GPS.weaponsPrice2.GetLength(1) ? (int)Kube.GPS.weaponsPrice2[numWeapon, tarif] : 0;
            }
            return price1;
        }

        public static int GetSpecPrice(int numSpecItem, int tarif)
        {
            if (Kube.GPS != null && Kube.GPS.specItemsPrice1 != null && numSpecItem < Kube.GPS.specItemsPrice1.GetLength(0) && tarif < Kube.GPS.specItemsPrice1.GetLength(1))
                return (int)Kube.GPS.specItemsPrice1[numSpecItem, tarif];
            return 0;
        }

        public static int GetSkinPrice(int numSkin, out int price1, out int price2)
        {
            price1 = 0;
            price2 = 0;
            if (Kube.GPS != null && Kube.GPS.skinPrice != null && numSkin < Kube.GPS.skinPrice.GetLength(0))
            {
                price1 = (int)Kube.GPS.skinPrice[numSkin, 0];
                price2 = Kube.GPS.skinPrice.GetLength(1) > 1 ? (int)Kube.GPS.skinPrice[numSkin, 1] : 0;
            }
            return price1;
        }

        public static int GetCubePrice(int numCubes, int numDays)
        {
            if (Kube.GPS != null && Kube.GPS.inventarCubesPrice1 != null && numCubes < Kube.GPS.inventarCubesPrice1.GetLength(0))
            {
                int tarif = numDays >= 0 && numDays < Kube.GPS.inventarCubesPrice1.GetLength(1) ? numDays : 0;
                return (int)Kube.GPS.inventarCubesPrice1[numCubes, tarif];
            }
            return 0;
        }

        public static int GetClothesPrice(int numClothes, out int price1, out int price2)
        {
            price1 = 0;
            price2 = 0;
            if (Kube.GPS != null && Kube.GPS.clothesPrice != null && numClothes < Kube.GPS.clothesPrice.GetLength(0))
            {
                price1 = (int)Kube.GPS.clothesPrice[numClothes, 0];
                price2 = Kube.GPS.clothesPrice.GetLength(1) > 1 ? (int)Kube.GPS.clothesPrice[numClothes, 1] : 0;
            }
            return price1;
        }

        public static bool TryPurchaseItem(int numItem, int itemsCount)
        {
            int price1, price2;
            GetItemPrice(numItem, out price1, out price2);
            if (price1 <= 0 && price2 <= 0) return false;
            if (!ProfileCore.SpendMoney(price1, price2)) return false;
            InventoryCore.AddItem(numItem, itemsCount);
            if (CoreEvents.OnItemPurchased != null)
                CoreEvents.OnItemPurchased("item_" + numItem, price1 > 0 ? price1 : price2);
            return true;
        }

        public static bool TryPurchaseWeapon(int numWeapon, int tarif)
        {
            int price1, price2;
            GetWeaponPrice(numWeapon, tarif, out price1, out price2);
            if (price1 <= 0 && price2 <= 0) return false;
            if (!ProfileCore.SpendMoney(price1, price2)) return false;
            InventoryCore.AddWeapon(numWeapon);
            if (CoreEvents.OnItemPurchased != null)
                CoreEvents.OnItemPurchased("weapon_" + numWeapon, price1 > 0 ? price1 : price2);
            return true;
        }

        public static bool TryPurchaseSpecItem(int numSpecItem, int tarif)
        {
            int price = GetSpecPrice(numSpecItem, tarif);
            if (price <= 0) return false;
            if (!ProfileCore.SpendMoney1(price)) return false;
            InventoryCore.AddSpecItem(numSpecItem);
            if (CoreEvents.OnItemPurchased != null)
                CoreEvents.OnItemPurchased("spec_" + numSpecItem, price);
            return true;
        }

        public static bool TryPurchaseSkin(int numSkin)
        {
            int price1, price2;
            GetSkinPrice(numSkin, out price1, out price2);
            if (price1 <= 0 && price2 <= 0) return false;
            if (!ProfileCore.SpendMoney(price1, price2)) return false;
            InventoryCore.AddSkin(numSkin);
            if (CoreEvents.OnItemPurchased != null)
                CoreEvents.OnItemPurchased("skin_" + numSkin, price1 > 0 ? price1 : price2);
            return true;
        }

        public static bool TryPurchaseClothes(int numClothes)
        {
            int price1, price2;
            GetClothesPrice(numClothes, out price1, out price2);
            if (price1 <= 0 && price2 <= 0) return false;
            if (!ProfileCore.SpendMoney(price1, price2)) return false;
            InventoryCore.AddClothes(numClothes);
            if (CoreEvents.OnItemPurchased != null)
                CoreEvents.OnItemPurchased("clothes_" + numClothes, price1 > 0 ? price1 : price2);
            return true;
        }

        public static bool TryPurchaseCubes(int numCubes, int numDays)
        {
            int price = GetCubePrice(numCubes, numDays);
            if (price <= 0) return false;
            if (!ProfileCore.SpendMoney1(price)) return false;
            if (CoreEvents.OnItemPurchased != null)
                CoreEvents.OnItemPurchased("cubes_" + numCubes, price);
            return true;
        }

        public static void ApplyDefaultPrices()
        {
            if (Kube.GPS == null) return;
            try
            {
                WeaponPriceData.Populate();

                var config = CoreManager.Config;
                double im = config != null ? config.itemPriceMultiplier : 1.0;
                double sm = config != null ? config.specPriceMultiplier : 1.0;
                double cm = config != null ? config.cubePriceMultiplier : 1.0;
                double bm = config != null ? config.bonusPriceMultiplier : 1.0;

                if (Kube.GPS.inventarItemPrice1 != null && Kube.GPS.inventarItemPrice2 != null)
                {
                    for (int i = 0; i < Kube.GPS.inventarItemPrice1.Length; i++)
                    {
                        Kube.GPS.inventarItemPrice1[i] = (int)((100 + i * 25) * im);
                        Kube.GPS.inventarItemPrice2[i] = (int)((10 + i) * im);
                    }
                }
                if (Kube.GPS.specItemsPrice1 != null)
                {
                    for (int s = 0; s < Kube.GPS.specItemsPrice1.GetLength(0); s++)
                    {
                        Kube.GPS.specItemsPrice1[s, 0] = (int)((200 + s * 50) * sm);
                        Kube.GPS.specItemsPrice1[s, 1] = (int)((500 + s * 100) * sm);
                        Kube.GPS.specItemsPrice1[s, 2] = (int)((1000 + s * 200) * sm);
                    }
                }
                if (Kube.GPS.skinPrice != null)
                {
                    int[,] skinPrices = new int[,] {
                        { 0, 0 }, { 500, 0 }, { 300, 0 }, { 1000, 0 }, { 1500, 0 },
                        { 0, 7 }, { 0, 35 }, { 0, 35 }, { 0, 12 }, { 2000, 0 },
                        { 5000, 0 }, { 1000, 0 }, { 0, 35 }, { 0, 15 }, { 0, 5 },
                        { 0, 35 }, { 0, 25 }, { 0, 35 }, { 1500, 0 }, { 0, 25 },
                        { 5000, 0 }, { 5000, 0 }, { 0, 25 }, { 0, 50 }, { 0, 23 }
                    };
                    int maxSkins = Math.Min(skinPrices.GetLength(0), Kube.GPS.skinPrice.GetLength(0));
                    for (int sk = 0; sk < maxSkins; sk++)
                    {
                        Kube.GPS.skinPrice[sk, 0] = skinPrices[sk, 0];
                        if (Kube.GPS.skinPrice.GetLength(1) > 1)
                            Kube.GPS.skinPrice[sk, 1] = skinPrices[sk, 1];
                    }
                }
                if (Kube.GPS.inventarCubesPrice1 != null && Kube.GPS.inventarCubesPrice2 != null)
                {
                    for (int c = 0; c < Kube.GPS.inventarCubesPrice1.GetLength(0); c++)
                    {
                        Kube.GPS.inventarCubesPrice1[c, 0] = (int)((100 + c * 50) * cm);
                        Kube.GPS.inventarCubesPrice1[c, 1] = (int)((300 + c * 100) * cm);
                        Kube.GPS.inventarCubesPrice1[c, 2] = (int)((500 + c * 200) * cm);
                        Kube.GPS.inventarCubesPrice2[c, 0] = (int)((5 + c) * cm);
                        Kube.GPS.inventarCubesPrice2[c, 1] = (int)((10 + c * 2) * cm);
                        Kube.GPS.inventarCubesPrice2[c, 2] = (int)((20 + c * 5) * cm);
                    }
                }
                if (Kube.GPS.clothesPrice != null)
                {
                    int[,] clothesPrices = new int[,] {
                        { 0, 0 }, { 0, 0 }, { 0, 0 }, { 2000, 0 }, { 2000, 0 },
                        { 2000, 0 }, { 3000, 0 }, { 3000, 0 }, { 1000, 0 }, { 2000, 0 },
                        { 2000, 0 }, { 5000, 0 }, { 5000, 0 }, { 2000, 0 }, { 2500, 0 },
                        { 2500, 0 }, { 2500, 0 }, { 2500, 0 }, { 2500, 0 }, { 3500, 0 },
                        { 3500, 0 }, { 3500, 0 }, { 3500, 0 }, { 2500, 0 }, { 2000, 0 },
                        { 5000, 0 }, { 1000, 0 }, { 3000, 0 }, { 5000, 0 }, { 3000, 0 },
                        { 5000, 0 }, { 10000, 0 }, { 5000, 0 }, { 15000, 0 }, { 5000, 0 },
                        { 5000, 0 }, { 4000, 0 }, { 5000, 0 }, { 1000, 0 }, { 2000, 0 },
                        { 2000, 0 }, { 5000, 0 }, { 1000, 0 }, { 2000, 0 }, { 3000, 0 },
                        { 2000, 0 }, { 2000, 0 }, { 1000, 0 }, { 1000, 0 }, { 2500, 0 },
                        { 5000, 0 }, { 4000, 0 }, { 4000, 0 }, { 2500, 0 }, { 2500, 0 },
                        { 2500, 0 }, { 2500, 0 }, { 2500, 0 }, { 15000, 0 }, { 5000, 0 },
                        { 5000, 0 }, { 5000, 0 }, { 15000, 0 }, { 5000, 0 },
                        { 5000, 0 }, { 5000, 0 }, { 4000, 0 }, { 6000, 0 }, { 5000, 0 },
                        { 5000, 0 }, { 5000, 0 }, { 5000, 0 }, { 5000, 0 }, { 5000, 0 },
                        { 5000, 0 }, { 0, 5 }, { 2500, 0 }, { 0, 2 }, { 0, 5 },
                        { 0, 1 }, { 0, 2 }, { 0, 1 }, { 0, 1 }, { 2000, 0 },
                        { 1000, 0 }, { 0, 2 }, { 0, 4 }, { 0, 5 }, { 5000, 0 },
                        { 5000, 0 }, { 0, 5 }
                    };
                    int maxClothes = Math.Min(clothesPrices.GetLength(0), Kube.GPS.clothesPrice.GetLength(0));
                    for (int cl = 0; cl < maxClothes; cl++)
                    {
                        Kube.GPS.clothesPrice[cl, 0] = clothesPrices[cl, 0];
                        if (Kube.GPS.clothesPrice.GetLength(1) > 1)
                            Kube.GPS.clothesPrice[cl, 1] = clothesPrices[cl, 1];
                    }
                }
                if (Kube.GPS.bonusesPrice == null)
                    Kube.GPS.bonusesPrice = new int[20, 20];
                for (int b = 0; b < Kube.GPS.bonusesPrice.GetLength(0); b++)
                    for (int b2 = 0; b2 < Kube.GPS.bonusesPrice.GetLength(1); b2++)
                        Kube.GPS.bonusesPrice[b, b2] = (int)((100 + b * 50) * bm);

                if (Kube.GPS.charParamsPrice != null)
                {
                    float[,,] cpp = Kube.GPS.charParamsPrice;
                    float[][] charLvls = new float[][] {
                        new float[] { 0, 500, 0, 0, 100, 2, 1000, 0, 0, 105, 4, 1000, 0, 0, 110, 8, 2000, 0, 0, 120, 14, 5000, 0, 0, 130, 20, 10000, 0, 0, 140, 25, 15000, 0, 0, 150, 30, 20000, 0, 0, 170 },
                        new float[] { 0, 500, 0, 0, 20, 2, 1000, 0, 0, 30, 4, 1000, 0, 0, 50, 8, 2000, 0, 0, 70, 14, 5000, 0, 0, 90, 20, 10000, 0, 0, 110, 25, 15000, 0, 0, 120, 30, 20000, 0, 0, 150 },
                        new float[] { 0, 500, 0, 0, 5f, 2, 1000, 0, 0, 5.2f, 4, 1000, 0, 0, 5.5f, 8, 2000, 0, 0, 5.8f, 14, 5000, 0, 0, 6.2f, 20, 10000, 0, 0, 6.5f, 25, 15000, 0, 0, 6.8f, 30, 20000, 0, 0, 7.2f },
                        new float[] { 0, 500, 0, 0, 5f, 2, 1000, 0, 0, 5.5f, 4, 1000, 0, 0, 6f, 8, 2000, 0, 0, 6.5f, 14, 5000, 0, 0, 7f, 20, 10000, 0, 0, 8f, 25, 15000, 0, 0, 9f, 30, 20000, 0, 0, 10f },
                        new float[] { 0, 500, 0, 0, 0f, 2, 1000, 0, 0, 2f, 4, 1000, 0, 0, 4f, 8, 2000, 0, 0, 6f, 14, 5000, 0, 0, 8f, 20, 10000, 0, 0, 10f, 25, 15000, 0, 0, 12f, 30, 20000, 0, 0, 15f }
                    };
                    for (int p = 0; p < 5; p++)
                    {
                        float[] lvls = charLvls[p];
                        for (int q = 0; q < 8; q++)
                        {
                            int off = q * 5;
                            cpp[p, q, 0] = lvls[off + 0];
                            cpp[p, q, 1] = lvls[off + 1];
                            cpp[p, q, 2] = lvls[off + 2];
                            cpp[p, q, 3] = lvls[off + 3];
                            cpp[p, q, 4] = lvls[off + 4];
                        }
                    }
                }
                if (Kube.GPS.skinBonus != null)
                {
                    float[,] sb = Kube.GPS.skinBonus;
                    float[][] skinBonuses = new float[][] {
                        new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 0, 0, 0, 0 },
                        new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 40, 0, 0, 0 },
                        new float[] { 0, 40, 0.2f, 1, 0 }, new float[] { 0, 40, 0.2f, 1, 0 },
                        new float[] { 0, 80, 0.5f, 1.5f, 5 }, new float[] { 0, 80, 0.5f, 1.5f, 5 },
                        new float[] { 0, 20, 0, 0, 0 }, new float[] { 0, 0, 1, 2, 0 },
                        new float[] { 0, 60, 1, 0, 0 }, new float[] { 0, 50, 1, 1, 0 },
                        new float[] { 0, 80, 0.5f, 1.5f, 5 }, new float[] { 0, 80, 0.5f, 1.5f, 5 },
                        new float[] { 0, 80, 0.5f, 1.5f, 5 }, new float[] { 0, 80, 0.5f, 1.5f, 5 },
                        new float[] { 0, 80, 0.5f, 1.5f, 5 }, new float[] { 0, 80, 0.5f, 1.5f, 5 },
                        new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 78, 0.7f, 1.5f, 6 },
                        new float[] { 0, 58, 0.1f, 0.5f, 1 }, new float[] { 0, 58, 0.1f, 0.5f, 1 },
                        new float[] { 0, 80, 0, 1.5f, 8 }, new float[] { 0, 98, 2.7f, 3.5f, 9 },
                        new float[] { 0, 60, 0, 2, 9 }
                    };
                    int maxSkins = Math.Min(skinBonuses.Length, sb.GetLength(0));
                    for (int sk = 0; sk < maxSkins; sk++)
                        for (int b = 0; b < 5; b++)
                            sb[sk, b] = skinBonuses[sk][b];
                }
                if (Kube.GPS.clothesBonus != null)
                {
                    float[,] cb = Kube.GPS.clothesBonus;
                    float[][] clothesBonuses = new float[][] {
                        new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 0, 0, 0, 0 },
                        new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 0, 0, 0, 0 },
                        new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 0, 0, 0, 0 },
                        new float[] { 0, 0, 0.1f, 0, 0 }, new float[] { 0, 0, 0.1f, 0, 0 },
                        new float[] { 0, 10, 0.2f, 0, 0 }, new float[] { 0, 15, 0.2f, 0, 0 },
                        new float[] { 0, 15, 0.2f, 0, 0 }, new float[] { 0, 20, 0.4f, 0, 0 },
                        new float[] { 0, 30, 0.2f, 1, 0 }, new float[] { 0, 0, 0, 0, 0 },
                        new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 0, 0, 0, 0 },
                        new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 0, 0, 0, 0 },
                        new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 10, 0, 0, 0 },
                        new float[] { 0, 10, 0, 0, 0 }, new float[] { 0, 10, 0, 0, 0 },
                        new float[] { 0, 10, 0, 0, 0 }, new float[] { 0, 0, 0, 0, 0 },
                        new float[] { 0, 10, 0.1f, 0, 0 }, new float[] { 0, 20, 0, 0, 1 },
                        new float[] { 0, 10, 0, 0, 0 }, new float[] { 0, 10, 0, 0, 1 },
                        new float[] { 0, 20, 0, 0, 1 }, new float[] { 0, 10, 0, 0, 1 },
                        new float[] { 0, 20, 0, 0, 1 }, new float[] { 0, 30, 0, 0, 2 },
                        new float[] { 0, 10, 0, 0, 0 }, new float[] { 0, 30, 0.2f, 0, 2 },
                        new float[] { 0, 0, 0, 0, 2 }, new float[] { 0, 0, 0, 0, 2 },
                        new float[] { 0, 0, 0, 1, 0 }, new float[] { 0, 10, 0, 0, 0 },
                        new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 10, 0, 0, 0 },
                        new float[] { 0, 10, 0, 0, 0 }, new float[] { 0, 10, 0, 0, 1 },
                        new float[] { 0, 10, 0, 0, 0 }, new float[] { 0, 10, 0, 0, 1 },
                        new float[] { 0, 20, 0, 0, 1 }, new float[] { 0, 0, 0, 0, 0 },
                        new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 5, 0, 0, 0 },
                        new float[] { 0, 5, 0, 0, 0 }, new float[] { 0, 0, 0, 0, 0 },
                        new float[] { 0, 10, 0, 0, 0 }, new float[] { 0, 0, 0.1f, 0, 0 },
                        new float[] { 0, 0, 0.1f, 0, 0 }, new float[] { 0, 0, 0.1f, 0, 0 },
                        new float[] { 0, 2, 0, 0, 0 }, new float[] { 0, 1, 0, 0, 0 },
                        new float[] { 0, 1, 0, 0, 0 }, new float[] { 0, 2, 0, 0, 0 },
                        new float[] { 0, 30, 0.2f, 0, 2 }, new float[] { 0, 20, 0.4f, 0, 0 },
                        new float[] { 0, 20, 0, 0, 1 }, new float[] { 0, 10, 0, 0, 1 },
                        new float[] { 0, 30, 0.2f, 0, 2 }, new float[] { 0, 20, 0.4f, 0, 0 },
                        new float[] { 0, 20, 0, 0, 1 }, new float[] { 0, 10, 0, 0, 1 },
                        new float[] { 0, 0, 0.1f, 0, 0 }, new float[] { 0, 0, 0.2f, 1, 0 },
                        new float[] { 0, 20, 0, 0, 1 }, new float[] { 0, 20, 0, 0, 1 },
                        new float[] { 0, 20, 0, 0, 1 }, new float[] { 0, 20, 0, 0, 1 },
                        new float[] { 0, 20, 0, 0, 1 }, new float[] { 0, 20, 0, 0, 1 },
                        new float[] { 0, 20, 0, 0, 1 }, new float[] { 0, 0, 0, 0, 0 },
                        new float[] { 0, 15, 0.2f, 0, 0 }, new float[] { 0, 20, 0, 0, 1 },
                        new float[] { 0, 10, 0, 0, 0 }, new float[] { 0, 5, 0, 0, 1 },
                        new float[] { 0, 5, 0.2f, 0, 0 }, new float[] { 0, 5, 0, 0, 0 },
                        new float[] { 0, 2, 0, 0, 4 }, new float[] { 0, 3, 0, 0, 0 },
                        new float[] { 0, 5, 0, 0, 0 }, new float[] { 0, 8, 0, 0, 0 },
                        new float[] { 0, 8, 0, 0, 0 }, new float[] { 0, 0, 0, 0, 0 },
                        new float[] { 0, 0, 0, 0, 0 }, new float[] { 0, 0, 0, 0, 0 }
                    };
                    int maxClothes = Math.Min(clothesBonuses.Length, cb.GetLength(0));
                    for (int cl = 0; cl < maxClothes; cl++)
                        for (int b = 0; b < 5; b++)
                            cb[cl, b] = clothesBonuses[cl][b];
                }
                Debug.Log("[Offline] Цены и параметры по умолчанию установлены");
            }
            catch (Exception ex)
            {
                Debug.LogError("[Offline] Ошибка ApplyDefaultPrices: " + ex.Message);
            }
        }
    }
}
