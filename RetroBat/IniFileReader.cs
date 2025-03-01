using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RetroBat
{
    public class IniFileReader
    {
        private static string? filePath;

        public IniFileReader(string iniPath)
        {
            filePath = iniPath;
            EnsureIniFileExists();
        }

        [DllImport("kernel32", CharSet = CharSet.Auto)]
        private static extern int GetPrivateProfileString(
            string section, string key, string defaultValue,
            StringBuilder retVal, int size, string filePath);

        private void EnsureIniFileExists()
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("INI file not found. Creating a default INI file...");
                File.WriteAllText(filePath, GetDefaultIniContent(), Encoding.UTF8);
            }
        }

        public static string ReadValue(string section, string key, string defaultValue = "")
        {
            StringBuilder result = new StringBuilder(255);
            int length = GetPrivateProfileString(section, key, defaultValue, result, result.Capacity, filePath);

            // Debugging output
            Console.WriteLine($"File Path: {filePath}");
            Console.WriteLine($"Section: {section}, Key: {key}");
            Console.WriteLine($"Result Length: {length}");
            Console.WriteLine($"Result: '{result.ToString()}'");

            return result.ToString();
        }

        public static int ReadInt(IniFileReader iniReader, string section, string key, int defaultValue)
        {
            string value = ReadValue(section, key, defaultValue.ToString());
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        public static bool ReadBool(IniFileReader iniReader, string section, string key, bool defaultValue)
        {
            string value = ReadValue(section, key).Trim().ToLower();

            // Remove any unexpected invisible characters, just in case
            value = value.Replace("\r", "").Replace("\n", "").Trim();

            if (value == "1" || value == "true") return true;
            if (value == "0" || value == "false") return false;

            // Fallback to the default value if the string is unrecognized
            return defaultValue;
        }

        private string GetDefaultIniContent()
        {
return @"; RETROBAT GLOBAL CONFIG FILE

[RetroBat]

; At startup RetroBat will detect or not the language used in Windows to set automatically the same language in the frontend and RetroArch emulator.
LanguageDetection=0

; At startup RetroBat will reset the default config files options of the emulators. Values depend on the configuration index choosen.
; Use at your own risk.	
ResetConfigMode=0

; Run automatically RetroBat at Windows startup.
Autostart=0

; Run WiimoteGun at RetroBat's startup. You can use your wiimote as a gun and navigate through EmulationStation.
WiimoteGun=0

[SplashScreen]

; Set if video introduction is played before running the interface.
EnableIntro=1

; The name of the video file to play. RandomVideo must be set on 0 to take effect.
FileName=""RetroBat-neon.mp4""

; If ""default"" is set, RetroBat will use the default video path where video files are stored.
; Enter a full path to use a custom directory for video files.
FilePath=""default""

; Play video files randomly when RetroBat starts.
RandomVideo=1

; Set the video duration in milliseconds. The value must be less or equal to real duration of the video to be played. 
VideoDuration=6500

[EmulationStation]

; Start the frontend in fullscreen or in windowed mode.
Fullscreen=1

; Force the fullscreen resolution with the parameters set at WindowXSize and WindowYSize.
ForceFullscreenRes=0

; The frontend will parse only the gamelist.xml files in roms directories to display available games.
; If files are added when this option is enabled, they will not appear in the gamelists of the frontend. The option must be enabled again to display new entries properly.
GameListOnly=0
 
; 0 = run the frontend normally.
; 1 = run the frontend in kiosk mode.
; 2 = run the frontend in kid mode.
InterfaceMode=0

; Set to which monitor index the frontend will be displayed.
MonitorIndex=0

; Set if the option to quit the frontend is displayed or not when the full menu is enabled.
NoExitMenu=0

; Set the windows width of the frontend.
WindowXSize=1280

; Set the windows height of the frontend.
WindowYSize=720";
        }
    }
}
