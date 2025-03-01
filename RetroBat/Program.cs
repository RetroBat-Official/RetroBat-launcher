using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using static RetroBat.IniFileReader;
using Microsoft.Win32;

namespace RetroBat
{
    class Program
    {
        static void Main()
        {
            string appFolder = Directory.GetCurrentDirectory();

            string iniPath = Path.Combine(appFolder, "retrobat.ini"); // Ensure this file is next to your executable

            if (!File.Exists(iniPath))
            {
                LogToFile("[WARNING] retrobat.ini not found");
                return;
            }

            // Write values to registry
            SetRegistryKey(appFolder + "\\");

            // Get values from ini file
            IniFileReader iniReader = new(iniPath);
            LogToFile("[INFO] Reading retrobat.ini.");

            RetroBatConfig config = new()
            {
                LanguageDetection = ReadInt(iniReader, "RetroBat", "LanguageDetection", 0),
                ResetConfigMode = ReadBool(iniReader, "RetroBat", "ResetConfigMode", false),
                Autostart = ReadBool(iniReader, "RetroBat", "Autostart", false),
                WiimoteGun = ReadBool(iniReader, "RetroBat", "WiimoteGun", false),

                EnableIntro = ReadBool(iniReader, "SplashScreen", "EnableIntro", true),
                FileName = ReadValue("SplashScreen", "FileName", "RetroBat-neon.mp4"),
                FilePath = ReadValue("SplashScreen", "FilePath", "default"),
                RandomVideo = ReadBool(iniReader, "SplashScreen", "RandomVideo", true),
                VideoDuration = ReadInt(iniReader, "SplashScreen", "VideoDuration", 6500),

                Fullscreen = ReadBool(iniReader, "EmulationStation", "Fullscreen", true),
                ForceFullscreenRes = ReadBool(iniReader, "EmulationStation", "ForceFullscreenRes", false),
                GameListOnly = ReadBool(iniReader, "EmulationStation", "GameListOnly", false),
                InterfaceMode = ReadInt(iniReader, "EmulationStation", "InterfaceMode", 0),
                MonitorIndex = ReadInt(iniReader, "EmulationStation", "MonitorIndex", 0),
                NoExitMenu = ReadBool(iniReader, "EmulationStation", "NoExitMenu", false),
                WindowXSize = ReadInt(iniReader, "EmulationStation", "WindowXSize", 1280),
                WindowYSize = ReadInt(iniReader, "EmulationStation", "WindowYSize", 720)
            };

            // Get emulationstation.exe path
            string esPath = Path.Combine(appFolder, "emulationstation");
            string emulationStationExe = Path.Combine(esPath, "emulationstation.exe");

            if (!File.Exists(emulationStationExe))
            {
                LogToFile("[ERROR] Emulationstation executable not found in: " + emulationStationExe);
                return;
            }

            // Arguments
            LogToFile("[INFO] Setting up arguments to run EmulationStation.");
            List<string> commandArray = [];

            if (config.Fullscreen && config.ForceFullscreenRes)
            {
                commandArray.Add("--resolution");
                commandArray.Add(config.WindowXSize.ToString());
                commandArray.Add(config.WindowYSize.ToString());
            }

            else if (!config.Fullscreen)
            {
                commandArray.Add("--windowed");
                commandArray.Add("--resolution");
                commandArray.Add(config.WindowXSize.ToString());
                commandArray.Add(config.WindowYSize.ToString());
            }

            if (config.GameListOnly)
                commandArray.Add("--gamelist-only");

            if (config.InterfaceMode == 2)
                commandArray.Add("--force-kid");
            else if (config.InterfaceMode == 1)
                commandArray.Add("--force-kiosk");

            if (config.MonitorIndex > 0)
            {
                commandArray.Add("--monitor");
                commandArray.Add(config.MonitorIndex.ToString());
            }

            if (config.NoExitMenu)
                commandArray.Add("--no-exit");

            commandArray.Add("--home");
            commandArray.Add(esPath);
            
            string args = string.Join(" ", commandArray);

            // Run EmulationStation
            LogToFile("[INFO] Running " + emulationStationExe + " " + args);
            
            var start = new ProcessStartInfo()
            {
                FileName = emulationStationExe,
                WorkingDirectory = esPath,
                Arguments = args,
            };
            
            if (start == null)
                return;

            var exe = Process.Start(start);

            exe?.WaitForExit();
        }

        class RetroBatConfig
        {
            public int LanguageDetection { get; set; }
            public bool ResetConfigMode { get; set; }
            public bool Autostart { get; set; }
            public bool WiimoteGun { get; set; }

            public bool EnableIntro { get; set; }
            public string? FileName { get; set; }
            public string? FilePath { get; set; }
            public bool RandomVideo { get; set; }
            public int VideoDuration { get; set; }

            public bool Fullscreen { get; set; }
            public bool ForceFullscreenRes { get; set; }
            public bool GameListOnly { get; set; }
            public int InterfaceMode { get; set; }
            public int MonitorIndex { get; set; }
            public bool NoExitMenu { get; set; }
            public int WindowXSize { get; set; }
            public int WindowYSize { get; set; }
        }

        static void LogToFile(string message)
        {
            string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "retrobat.log");

            using (StreamWriter writer = new(logFilePath, append: true))
                writer.WriteLine($"{DateTime.Now}: {message}");
        }

        public static void SetRegistryKey(string appFolder)
        {
            LogToFile("[INFO] Writing values to registry.");

            string registryPath = @"SOFTWARE\RetroBat";
            string ftpPath = "InstallRootUrl";
            string installPath = "LatestKnownInstallPath";

            string urlValue = "http://www.retrobat.ovh/repo/win64";

            using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(registryPath, true))
            {
                if (registryKey.GetValue(ftpPath) == null)
                    registryKey.SetValue(ftpPath, urlValue);
                else
                {
                    string currentFTPPath = registryKey.GetValue(ftpPath).ToString();
                    if (currentFTPPath != urlValue)
                        registryKey.SetValue(ftpPath, urlValue);
                }

                if (registryKey.GetValue(installPath) == null)
                    registryKey.SetValue(installPath, appFolder);
                else
                {
                    string currentInstallPath = registryKey.GetValue(installPath).ToString();
                    if (currentInstallPath != appFolder)
                        registryKey.SetValue(installPath, appFolder);
                }
            }
        }
    }
}