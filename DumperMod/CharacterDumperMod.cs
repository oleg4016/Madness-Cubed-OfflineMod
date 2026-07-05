using System;
using System.IO;
using MelonLoader;
using UnityEngine;
using kube;
using kube.data;

[assembly: MelonInfo(typeof(MadnessCubedOffline.CharacterDumperMod), "Madness Cubed Character Dumper", "1.0", "Modder")]
[assembly: MelonGame("nobodyshot", "Madness Cubed")]

namespace MadnessCubedOffline
{
    public class CharacterDumperMod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("==================================================");
            MelonLogger.Msg("[DUMPER] Мод-шпион за Персонажем и Одеждой загружен!");
            MelonLogger.Msg("Зайди в Главное Меню и нажми F6 для дампа.");
            MelonLogger.Msg("==================================================");
        }

        public override void OnUpdate()
        {
            // F6 - Сделать дамп характеристик персонажа и одежды
            if (Input.GetKeyDown(KeyCode.F6))
            {
                DumpCharacterData();
            }
        }

        private void DumpCharacterData()
        {
            if (Kube.GPS == null)
            {
                MelonLogger.Warning("Данные игры (GPS) еще не загружены! Дождитесь загрузки меню.");
                return;
            }

            string path = Path.Combine(Environment.CurrentDirectory, "Character_Info_Dump.txt");
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("==========================================================");
                sw.WriteLine("  БАЗА ДАННЫХ ПРОКАЧКИ ПЕРСОНАЖА (GameParamsScript)");
                sw.WriteLine("==========================================================");
                
                string[] paramNames = { "Здоровье", "Броня", "Скорость", "Прыжок", "Защита от урона" };
                
                for (int paramId = 0; paramId < 5; paramId++)
                {
                    sw.WriteLine($"\n--- [{paramId}] Прокачка: {paramNames[paramId]} ---");
                    // У персонажа 8 уровней прокачки каждого стата (от 0 до 7)
                    for (int lvl = 0; lvl < 8; lvl++)
                    {
                        try 
                        {
                            float needLevel = Kube.GPS.charParamsPrice[paramId, lvl, 0];
                            float priceMoney = Kube.GPS.charParamsPrice[paramId, lvl, 1];
                            float priceGold = Kube.GPS.charParamsPrice[paramId, lvl, 2];
                            float statValue = Kube.GPS.charParamsPrice[paramId, lvl, 4];

                            string costStr = priceGold > 0 ? $"{priceGold} Золота" : $"{priceMoney} Баксов";
                            if (priceMoney == 0 && priceGold == 0) costStr = "Бесплатно (или Базовый уровень)";

                            sw.WriteLine($"Уровень прокачки {lvl}: Значение стата = {statValue} | Требует {needLevel} ур. игрока | Цена: {costStr}");
                        }
                        catch { sw.WriteLine($"Ошибка чтения уровня {lvl}"); }
                    }
                }

                sw.WriteLine("\n==========================================================");
                sw.WriteLine("  ПОЛНЫЕ СКИНЫ (Skins)");
                sw.WriteLine("==========================================================");
                for (int i = 0; i < Kube.IS.skins.Length; i++)
                {
                    try
                    {
                        string name = Localize.skinName != null && Localize.skinName.Length > i ? Localize.skinName[i] : "Неизвестно";
                        int priceM = Kube.GPS.skinPrice[i, 1];
                        int priceG = Kube.GPS.skinPrice[i, 2];
                        
                        sw.WriteLine($"\n[Скин ID: {i}] {name}");
                        sw.WriteLine($"Цена: {priceM} Баксов / {priceG} Золота");
                        sw.WriteLine($"Бонусы: +{Kube.GPS.skinBonus[i, 0]} Здоровья | +{Kube.GPS.skinBonus[i, 1]} Брони | +{Kube.GPS.skinBonus[i, 2]} Скор. | +{Kube.GPS.skinBonus[i, 3]} Прыжок | +{Kube.GPS.skinBonus[i, 4]}% Защита");
                    }
                    catch { }
                }

                sw.WriteLine("\n==========================================================");
                sw.WriteLine("  ОДЕЖДА ПО ЧАСТЯМ (Clothes)");
                sw.WriteLine("==========================================================");
                if (Kube.IS.dressItems != null)
                {
                    for (int i = 0; i < Kube.IS.dressItems.Length; i++)
                    {
                        try
                        {
                            string name = Localize.clothesName != null && Localize.clothesName.Length > i ? Localize.clothesName[i] : "Неизвестно";
                            string type = Kube.IS.dressItems[i].group.ToString(); // Покажет Face, Tors, Back, Arms, Foots и т.д.
                            
                            int priceM = Kube.GPS.clothesPrice[i, 1];
                            int priceG = Kube.GPS.clothesPrice[i, 2];

                            sw.WriteLine($"\n[Одежда ID: {i}] {name} (Слот: {type})");
                            sw.WriteLine($"Цена: {priceM} Баксов / {priceG} Золота");
                            sw.WriteLine($"Бонусы: +{Kube.GPS.clothesBonus[i, 0]} Здоровья | +{Kube.GPS.clothesBonus[i, 1]} Брони | +{Kube.GPS.clothesBonus[i, 2]} Скор. | +{Kube.GPS.clothesBonus[i, 3]} Прыжок | +{Kube.GPS.clothesBonus[i, 4]}% Защита");
                        }
                        catch { }
                    }
                }
            }

            MelonLogger.Msg("УСПЕШНО! Дамп персонажа и одежды сохранен в файл: Character_Info_Dump.txt");
        }
    }
}