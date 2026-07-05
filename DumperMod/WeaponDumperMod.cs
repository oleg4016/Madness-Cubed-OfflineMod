using System;
using System.IO;
using MelonLoader;
using UnityEngine;
using kube;
using kube.data;

[assembly: MelonInfo(typeof(MadnessCubedOffline.WeaponDumperMod), "Madness Cubed Weapon Dumper", "1.0", "Modder")]
[assembly: MelonGame("nobodyshot", "Madness Cubed")]

namespace MadnessCubedOffline
{
    public class WeaponDumperMod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("==================================================");
            MelonLogger.Msg("[DUMPER] Мод-шпион за Оружием загружен!");
            MelonLogger.Msg("Зайди в игру (в меню или матч) и нажми F7 для дампа.");
            MelonLogger.Msg("==================================================");
        }

        public override void OnUpdate()
        {
            // F7 - Сделать дамп характеристик всего оружия
            if (Input.GetKeyDown(KeyCode.F7))
            {
                DumpWeapons();
            }
        }

        private void DumpWeapons()
        {
            if (Kube.IS == null || Kube.IS.weaponParams == null)
            {
                MelonLogger.Warning("Инвентарь еще не загружен! Дождитесь Главного Меню.");
                return;
            }

            string path = Path.Combine(Environment.CurrentDirectory, "Weapon_Info_Dump.txt");
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("==========================================================");
                sw.WriteLine("  БАЗА ДАННЫХ ОРУЖИЯ И УЛУЧШЕНИЙ (Madness Cubed)");
                sw.WriteLine("==========================================================");

                for (int i = 0; i < Kube.IS.weaponParams.Length; i++)
                {
                    try
                    {
                        var wpn = Kube.IS.weaponParams[i];
                        if (wpn == null) continue;

                        string name = "Неизвестно";
                        try { name = Localize.weaponNames[i]; } catch { }

                        sw.WriteLine($"\n--- [ID: {i}] {name} ---");
                        sw.WriteLine($"Группа оружия: {wpn.weaponGroup}");
                        sw.WriteLine($"Тип патронов (BulletsType ID): {wpn.BulletsType}");

                        // Базовые параметры (уровень 0) и максимальные (последний уровень)
                        try 
                        {
                            sw.WriteLine($"Урон (Damage): баз. {wpn.Damage[0]} -> макс. {wpn.Damage[wpn.Damage.Length - 1]} (Уровней прокачки: {wpn.Damage.Length})");
                        } catch {}

                        try 
                        {
                            sw.WriteLine($"Точность (Accuracy): баз. {wpn.Accuracy[0]} -> макс. {wpn.Accuracy[wpn.Accuracy.Length - 1]} (Уровней прокачки: {wpn.Accuracy.Length})");
                        } catch {}

                        try 
                        {
                            sw.WriteLine($"Скорострельность (DeltaShot): баз. {wpn.DeltaShotArray[0]} -> макс. {wpn.DeltaShotArray[wpn.DeltaShotArray.Length - 1]}");
                        } catch {}

                        try 
                        {
                            sw.WriteLine($"Обойма (ClipSize): баз. {wpn.clipSize[0]} -> макс. {wpn.clipSize[wpn.clipSize.Length - 1]}");
                        } catch {}

                        // Цены на покупку оружия и апгрейды
                        if (Kube.GPS != null)
                        {
                            try 
                            {
                                sw.WriteLine($"Цена покупки навсегда: {Kube.GPS.weaponsPrice1[i, 0]} Баксов / {Kube.GPS.weaponsPrice2[i, 0]} Золота");
                            } catch {}

                            sw.WriteLine("  [Цены на улучшение (1-й уровень)]:");
                            try 
                            {
                                var priceDmg = Kube.GPS.upgradePrice[i, 0, 0]; // i - ID пушки, 0 - тип улучшения (Урон), 0 - уровень апгрейда
                                sw.WriteLine($"   - Прокачать Урон: {priceDmg.price} {(priceDmg.isGold ? "Золота" : "Баксов")}");
                            } catch {}
                        }
                    }
                    catch (Exception ex)
                    {
                        sw.WriteLine($"Ошибка при чтении оружия ID {i}: {ex.Message}");
                    }
                }

                // =========================================================================
                // Дамп патронов
                // =========================================================================
                sw.WriteLine("\n==========================================================");
                sw.WriteLine("  ПАТРОНЫ (BulletParams)");
                sw.WriteLine("==========================================================");
                if (Kube.IS.bulletParams != null)
                {
                    for (int i = 0; i < Kube.IS.bulletParams.Length; i++)
                    {
                        try 
                        {
                            var bp = Kube.IS.bulletParams[i];
                            sw.WriteLine($"[ID: {i}] Имя: {bp.name} | Группа: {bp.bulletGroup}");
                            sw.WriteLine($"   Выдается при респавне: {bp.initialAmount}");
                            sw.WriteLine($"   Подбирается с коробки: {bp.puckupAmount}");
                        } 
                        catch {}
                    }
                }
            }

            MelonLogger.Msg("УСПЕШНО! Дамп оружия сохранен в файл: Weapon_Info_Dump.txt");
        }
    }
}