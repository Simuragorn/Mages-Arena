using Assets.Scripts.Constants;
using Madhur.InfoPopup;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public static class GameSettingsHelper
    {
        private static string GameSettingsFilePath => Path.Combine(Application.dataPath, ConnectionConstants.GameSettingsFileName);
        public static GameSettings GetGameSettings()
        {
            GameSettings gameSettings;
            if (!File.Exists(GameSettingsFilePath))
            {
                gameSettings = new GameSettings
                {
                    HostIpAddress = ConnectionConstants.DefaultLocalConnectionAddress,
                    Port = ConnectionConstants.DefaultPort,
                    ListenOn = ConnectionConstants.ListenOn,
                    SoundVolume = AudioConstants.DefaultSoundVolume,
                    MusicVolume = AudioConstants.DefaultMusicVolume
                };
            }
            else
            {
                string json = File.ReadAllText(GameSettingsFilePath);
                gameSettings = JsonConvert.DeserializeObject<GameSettings>(json);
            }
            return gameSettings;
        }
        public static void SaveGameSettings(GameSettings gameSettings)
        {
            File.WriteAllText(GameSettingsFilePath, JsonConvert.SerializeObject(gameSettings));
        }
    }
}
