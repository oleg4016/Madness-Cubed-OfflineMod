using System;
using System.IO;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using kube;
using kube.data;

[assembly: MelonInfo(typeof(MadnessCubedOffline.MapTopDumperMod), "Madness Cubed MapTop Dumper", "1.0", "Modder")]
[assembly: MelonGame("nobodyshot", "Madness Cubed")]

namespace MadnessCubedOffline
{
    public class MapTopDumperMod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("==================================================");
            MelonLogger.Msg("[DUMPER] Мод-шпион за Картотопом загружен!");
            MelonLogger.Msg("Зайди во вкладку Картотоп и нажми F10 для дампа.");
            MelonLogger.Msg("==================================================");
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                DumpMapTopVariables();
            }
        }

        private void DumpMapTopVariables()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "MapTop_State_Dump.txt");
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("==========================================================");
                sw.WriteLine("  СОСТОЯНИЕ МЕНЮ КАРТОТОПА НА ДАННЫЙ МОМЕНТ");
                sw.WriteLine("==========================================================");

                // Ищем класс онлайн картотопа
                MaptopOnlineTab onlineTab = UnityEngine.Object.FindObjectOfType<MaptopOnlineTab>();
                if (onlineTab != null)
                {
                    sw.WriteLine("\n[КЛАСС]: MaptopOnlineTab (Вкладка Топа Карт)");
                    sw.WriteLine("--- ПЕРЕМЕННЫЕ ---");
                    DumpFields(onlineTab, sw);
                }
                else
                {
                    sw.WriteLine("\n[!] Вкладка MaptopOnlineTab не найдена. Откройте меню Картотопа!");
                }

                // Ищем класс своих карт в картотопе
                MaptopMyTab myTab = UnityEngine.Object.FindObjectOfType<MaptopMyTab>();
                if (myTab != null)
                {
                    sw.WriteLine("\n[КЛАСС]: MaptopMyTab (Вкладка Своих Карт)");
                    sw.WriteLine("--- ПЕРЕМЕННЫЕ ---");
                    DumpFields(myTab, sw);
                }

                // ================= РАСШИФРОВКА JSON ДЛЯ МОДА =================
                sw.WriteLine("\n==========================================================");
                sw.WriteLine("  ИНСТРУКЦИЯ: КАК ПОЧИНИТЬ КАРТОТОП В МОДЕ (UltimateOfflineMod)");
                sw.WriteLine("==========================================================");
                sw.WriteLine("Оригинальный скрипт MapTop.cs парсит JSON по таким правилам:");
                sw.WriteLine("1. id         (int)    - Уникальный ID записи в топе");
                sw.WriteLine("2. mapid      (long)   - Уникальный номер карты (с минусом = встроенные)");
                sw.WriteLine("3. name       (string) - Название карты");
                sw.WriteLine("4. type       (int)    - Режим игры (0=Творчество, 1=Шутер, 2=Команды, 3=Выживание, 4=Команды...)");
                sw.WriteLine("5. canbreak   (int)    - Можно ли ломать блоки (1=Да, 0=Нет)");
                sw.WriteLine("6. daytime    (int)    - Время суток (1=День, 0=Ночь, 2=Смена)");
                sw.WriteLine("7. hits       (int)    - Количество лайков/посещений");
                
                sw.WriteLine("\n[ПРИМЕР РАБОЧЕГО JSON ДЛЯ ЗАПРОСА 800 и 801]:");
                sw.WriteLine("{");
                sw.WriteLine("  \"price\": 10,");
                sw.WriteLine("  \"items\": [");
                sw.WriteLine("    {");
                sw.WriteLine("      \"id\": 1,");
                sw.WriteLine("      \"mapid\": -1,");
                sw.WriteLine("      \"name\": \"Легендарная Арена\",");
                sw.WriteLine("      \"type\": 2,");
                sw.WriteLine("      \"canbreak\": 0,");
                sw.WriteLine("      \"daytime\": 1,");
                sw.WriteLine("      \"hits\": 9999");
                sw.WriteLine("    }");
                sw.WriteLine("  ]");
                sw.WriteLine("}");
            }

            MelonLogger.Msg("УСПЕШНО! Дамп Картотопа сохранен в файл: MapTop_State_Dump.txt");
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