using System;
using System.IO;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using kube;

[assembly: MelonInfo(typeof(MadnessCubedOffline.PlayerDumperMod), "Madness Cubed Player Dumper", "1.0", "Modder")]
[assembly: MelonGame("nobodyshot", "Madness Cubed")]

namespace MadnessCubedOffline
{
    public class PlayerDumperMod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("==================================================");
            MelonLogger.Msg("[DUMPER] Мод-шпион за Игроком загружен!");
            MelonLogger.Msg("Зайди на карту и нажми F12 для создания дампа.");
            MelonLogger.Msg("==================================================");
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                DumpPlayerVariables();
            }
        }

        private void DumpPlayerVariables()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "Player_State_Dump.txt");
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("==========================================================");
                sw.WriteLine("  СОСТОЯНИЕ ИГРОКА (PlayerScript) НА ДАННЫЙ МОМЕНТ");
                sw.WriteLine("==========================================================");

                if (Kube.BCS == null || Kube.BCS.ps == null)
                {
                    sw.WriteLine("[!] ОШИБКА: Игрок не найден! Вы точно находитесь на карте?");
                    MelonLogger.Warning("Игрок не найден. Дамп прерван.");
                    return;
                }

                PlayerScript player = Kube.BCS.ps;

                sw.WriteLine("\n==========================================================");
                sw.WriteLine("  ПОДОЗРИТЕЛЬНЫЕ ПЕРЕМЕННЫЕ (Искать баги здесь!)");
                sw.WriteLine("==========================================================");
                sw.WriteLine("Если вы не можете двигаться, проверьте эти флаги:");
                
                try { sw.WriteLine($"dead (Мертв ли игрок?): {player.dead}"); } catch {}
                try { sw.WriteLine($"paused (Игра на паузе?): {player.paused}"); } catch {}
                try { sw.WriteLine($"freezed (Игрок заморожен?): {GetValue(player, "freezed")}"); } catch {}
                try { sw.WriteLine($"onlyMove (Заблокирована камера?): {player.onlyMove}"); } catch {}
                try { sw.WriteLine($"isDriveTransport (В транспорте?): {player.isDriveTransport}"); } catch {}
                if (Kube.OH != null) sw.WriteLine($"Kube.OH.emptyScreen (Блок мыши): {Kube.OH.emptyScreen}");
                
                // Исправлено: используем Cursor.visible вместо устаревшего Screen.lockCursor
                try { sw.WriteLine($"Cursor.visible (Курсор на экране?): {Cursor.visible}"); } catch {}
                try { sw.WriteLine($"Cursor.lockState (Состояние курсора): {Cursor.lockState}"); } catch {}

                sw.WriteLine("\n==========================================================");
                sw.WriteLine("  ПОКАЗАТЕЛИ ЗДОРОВЬЯ И БОЯ");
                sw.WriteLine("==========================================================");
                try { sw.WriteLine($"Координаты (Position): {player.transform.position}"); } catch {}
                try { sw.WriteLine($"Здоровье (health): {player.health} / {player.maxHealth}"); } catch {}
                try { sw.WriteLine($"Броня (armor): {player.armor} / {player.maxArmor}"); } catch {}
                try { sw.WriteLine($"Очки/Фраги/Убийства: points={player.points}, frags={player.frags}, kills={player.kills}"); } catch {}

                sw.WriteLine("\n==========================================================");
                sw.WriteLine("  ОРУЖИЕ В РУКАХ");
                sw.WriteLine("==========================================================");
                try 
                { 
                    // Исправлено: currentWeapon это класс WeaponParamsObj, берем его ID
                    int curWpnId = -1;
                    if (player.currentWeapon != null) 
                    {
                        curWpnId = player.currentWeapon.id;
                    }

                    sw.WriteLine($"currentWeapon (ID текущего оружия): {curWpnId}");
                    
                    if (curWpnId != -1)
                    {
                        int bulletType = Kube.IS.weaponParams[curWpnId].BulletsType;
                        sw.WriteLine($"Патроны в запасе: {player.bullets[bulletType]}");
                        sw.WriteLine($"Патроны в обойме: {player.clips[curWpnId]}");
                        sw.WriteLine($"Перезаряжается ли сейчас? (rechargingWeapon): {GetValue(player, "rechargingWeapon")}");
                    }
                } 
                catch (Exception ex) { sw.WriteLine("Ошибка чтения оружия: " + ex.Message); }

                sw.WriteLine("\n==========================================================");
                sw.WriteLine("  ПОЛНЫЙ ДАМП ВСЕХ ПЕРЕМЕННЫХ КЛАССА PlayerScript");
                sw.WriteLine("==========================================================");
                DumpFields(player, sw);

                sw.WriteLine("\n==========================================================");
                sw.WriteLine("  ГЛОБАЛЬНЫЕ СТАТЫ (GameParamsScript)");
                sw.WriteLine("==========================================================");
                if (Kube.GPS != null)
                {
                    DumpFields(Kube.GPS, sw);
                }
            }

            MelonLogger.Msg("УСПЕШНО! Дамп игрока сохранен в файл: Player_State_Dump.txt");
        }

        private object GetValue(object obj, string fieldName)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null) return field.GetValue(obj);
            return "NOT_FOUND";
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
                    
                    if (value is Array arr)
                    {
                        sw.WriteLine($"[{field.FieldType.Name}] {field.Name} = [Массив из {arr.Length} элементов]");
                    }
                    else
                    {
                        if (value == null) value = "null";
                        sw.WriteLine($"[{field.FieldType.Name}] {field.Name} = {value}");
                    }
                } 
                catch { }
            }
        }
    }
}