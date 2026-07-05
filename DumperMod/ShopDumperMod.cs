using System;
using System.IO;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using kube;

[assembly: MelonInfo(typeof(MadnessCubedOffline.ShopDumperMod), "Madness Cubed Shop Dumper", "1.0", "Modder")]
[assembly: MelonGame("nobodyshot", "Madness Cubed")]

namespace MadnessCubedOffline
{
    public class ShopDumperMod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("==================================================");
            MelonLogger.Msg("[DUMPER] Мод-шпион за Магазином загружен!");
            MelonLogger.Msg("Зайди в любой магазин в игре и нажми F11 для дампа.");
            MelonLogger.Msg("==================================================");
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                DumpShopVariables();
            }
        }

        private void DumpShopVariables()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Shop_State_Dump.txt");
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("==========================================================");
                sw.WriteLine("  СОСТОЯНИЕ МАГАЗИНА И ИНТЕРФЕЙСА НА ДАННЫЙ МОМЕНТ");
                sw.WriteLine("==========================================================");

                if (Kube.GPS != null)
                {
                    sw.WriteLine($"Баланс игрока: {Kube.GPS.playerMoney1} Баксов | {Kube.GPS.playerMoney2} Золота");
                }

                sw.WriteLine("\n==========================================================");
                sw.WriteLine("  АКТИВНЫЕ МЕНЮ МАГАЗИНА В ПАМЯТИ");
                sw.WriteLine("==========================================================");

                // Ищем все активные скрипты меню
                MonoBehaviour[] allScripts = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
                bool foundAny = false;

                foreach (MonoBehaviour script in allScripts)
                {
                    string name = script.GetType().Name;
                    // Проверяем, относится ли скрипт к магазину
                    if (name.Contains("Menu") || name.Contains("Shop") || name == "CharMenu" || name == "DecorMenu" || name == "BoxesMenu" || name == "BankMenu")
                    {
                        if (script.gameObject.activeInHierarchy) // Если меню сейчас открыто на экране
                        {
                            foundAny = true;
                            sw.WriteLine($"\n[ОТКРЫТОЕ МЕНЮ]: {name} (Объект: {script.gameObject.name})");
                            sw.WriteLine("--- ПЕРЕМЕННЫЕ ---");
                            DumpFields(script, sw);
                        }
                    }
                }

                if (!foundAny)
                {
                    sw.WriteLine("\n[!] Активные меню магазина не найдены. Вы точно открыли магазин?");
                }

                sw.WriteLine("\n==========================================================");
                sw.WriteLine("  ПРОВЕРКА ЦЕН В GameParamsScript (GPS)");
                sw.WriteLine("==========================================================");
                if (Kube.GPS != null)
                {
                    try { sw.WriteLine($"Цена первого блока (ID 0): {Kube.GPS.inventarItemPrice1[0]} баксов / {Kube.GPS.inventarItemPrice2[0]} золота"); } catch {}
                    try { sw.WriteLine($"Цена первого оружия (ID 0): {Kube.GPS.weaponsPrice1[0,0]} баксов / {Kube.GPS.weaponsPrice2[0,0]} золота"); } catch {}
                    try { sw.WriteLine($"Цена первого скина (ID 0): {Kube.GPS.skinPrice[0,1]} баксов / {Kube.GPS.skinPrice[0,2]} золота"); } catch {}
                }

                sw.WriteLine("\n==========================================================");
                sw.WriteLine("  ИНСТРУКЦИЯ: КАК ОБНОВИТЬ ИНТЕРФЕЙС ПОСЛЕ ПОКУПКИ В МОДЕ");
                sw.WriteLine("==========================================================");
                sw.WriteLine("В оригинале игра использует систему сообщений Unity (SendMonoMessage), чтобы убрать замок с кнопки после покупки.");
                sw.WriteLine("Если вы перехватываете покупку (Prefix) и возвращаете false, вы ОБЯЗАНЫ сами послать этот сигнал, иначе кнопка не обновится!");
                sw.WriteLine("");
                sw.WriteLine("1. После покупки Оружия вызывайте:");
                sw.WriteLine("   kube.Kube.SendMonoMessage(\"WeaponsUpdate\");");
                sw.WriteLine("");
                sw.WriteLine("2. После покупки Блоков, Декора или Спецпредметов:");
                sw.WriteLine("   kube.Kube.SendMonoMessage(\"ItemsCubesUpdate\");");
                sw.WriteLine("");
                sw.WriteLine("3. После покупки Одежды или Скинов:");
                sw.WriteLine("   kube.Kube.SendMonoMessage(\"UpdateChar\");");
                sw.WriteLine("   if (kube.Kube.IS.ps != null) kube.Kube.IS.ps.SendMessage(\"PlayerDressSkin\");");
                sw.WriteLine("");
                sw.WriteLine("4. После обмена валюты в Банке (чтобы обновились циферки денег вверху экрана):");
                sw.WriteLine("   // Если вы перехватили GoldToMoney:");
                sw.WriteLine("   go.SendMessage(method, \"0^0^\" + Kube.GPS.playerMoney1 + \"^\" + Kube.GPS.playerMoney2);");
            }

            MelonLogger.Msg("УСПЕШНО! Дамп Магазина сохранен в файл: Shop_State_Dump.txt");
        }

        private void DumpFields(object obj, StreamWriter sw)
        {
            Type type = obj.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                try 
                {
                    object value = field.GetValue(obj);
                    if (value == null) value = "null";
                    sw.WriteLine($"[{field.FieldType.Name}] {field.Name} = {value}");
                } 
                catch { }
            }
        }
    }
}