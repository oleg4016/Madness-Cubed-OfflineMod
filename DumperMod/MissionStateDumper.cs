using System;
using System.IO;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using kube;

[assembly: MelonInfo(typeof(MadnessCubedOffline.MissionStateDumper), "Madness Cubed Mission Dumper", "1.0", "Modder")]
[assembly: MelonGame("nobodyshot", "Madness Cubed")]

namespace MadnessCubedOffline
{
    public class MissionStateDumper : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("==================================================");
            MelonLogger.Msg("[DUMPER] Мод-шпион за миссиями загружен!");
            MelonLogger.Msg("Нажми F8 в игре, чтобы сделать дамп переменных миссии.");
            MelonLogger.Msg("Нажми F9 в игре, чтобы принудительно завершить миссию победой.");
            MelonLogger.Msg("==================================================");
        }

        public override void OnUpdate()
        {
            // F8 - Сделать дамп переменных текущей миссии
            if (Input.GetKeyDown(KeyCode.F8))
            {
                DumpMissionVariables();
            }

            // F9 - Принудительно отправить сигнал победы в миссии
            if (Input.GetKeyDown(KeyCode.F9))
            {
                ForceWinMission();
            }
        }

        private void DumpMissionVariables()
        {
            if (Kube.BCS == null || Kube.BCS.gameProcess != BattleControllerScript.GameProcess.game)
            {
                MelonLogger.Warning("Вы должны находиться в бою, чтобы сделать дамп!");
                return;
            }

            string path = Path.Combine(Environment.CurrentDirectory, "Mission_State_Dump.txt");
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine("==========================================================");
                sw.WriteLine("  СОСТОЯНИЕ МИССИИ И БОЕВОГО КОНТРОЛЛЕРА НА ДАННЫЙ МОМЕНТ");
                sw.WriteLine("==========================================================");
                sw.WriteLine($"Время: {Time.time}");
                sw.WriteLine($"Тип игры (GameType): {Kube.BCS.gameType}");
                
                // Проверяем, какой контроллер сейчас управляет игрой (MissionKillNMonsters, SurvivalController и т.д.)
                if (Kube.BCS.gameTypeController != null)
                {
                    Type controllerType = Kube.BCS.gameTypeController.GetType();
                    sw.WriteLine($"\n[АКТИВНЫЙ КЛАСС КОНТРОЛЛЕРА]: {controllerType.Name}");
                    sw.WriteLine("--- ПЕРЕМЕННЫЕ И ИХ ЗНАЧЕНИЯ ---");

                    // Извлекаем абсолютно все закрытые (private) и открытые (public) переменные!
                    FieldInfo[] fields = controllerType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (FieldInfo field in fields)
                    {
                        try 
                        {
                            object value = field.GetValue(Kube.BCS.gameTypeController);
                            sw.WriteLine($"[{field.FieldType.Name}] {field.Name} = {value}");
                        } 
                        catch { }
                    }
                }
                else
                {
                    sw.WriteLine("\n[!] ОШИБКА: gameTypeController == null. Миссия не инициализирована!");
                }

                sw.WriteLine("\n==========================================================");
                sw.WriteLine("  ГЛАВНЫЕ ПЕРЕМЕННЫЕ BATTLE CONTROLLER SCRIPT");
                sw.WriteLine("==========================================================");
                sw.WriteLine($"gameEndTime: {Kube.BCS.gameEndTime}");
                sw.WriteLine($"gameStartTime: {Kube.BCS.gameStartTime}");
                sw.WriteLine($"missionId: {Kube.BCS._missionId}");
                sw.WriteLine($"missionType: {Kube.BCS.missionType}");
                sw.WriteLine($"currentNumMonsters: {Kube.BCS.currentNumMonsters}");
                sw.WriteLine($"currentNumPlayers: {Kube.BCS.currentNumPlayers}");
            }

            MelonLogger.Msg("УСПЕШНО! Все переменные миссии сохранены в файл: Mission_State_Dump.txt");
        }

        private void ForceWinMission()
        {
            if (Kube.BCS != null && Kube.BCS.gameProcess == BattleControllerScript.GameProcess.game)
            {
                MelonLogger.Msg("[ФИКС] Принудительно завершаем миссию победой...");
                
                // В Madness Cubed "exitTrigger" означает, что игрок дошел до конца миссии и победил
                Kube.BCS.EndGame(BattleControllerScript.EndGameType.exitTrigger);
            }
        }
    }
}