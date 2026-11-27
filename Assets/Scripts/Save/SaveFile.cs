using System;
using System.IO;
using UnityEngine;

namespace SproutOfDebt.Save
{
    /// <summary>The save file on disk. Writes go to a temporary file first, so a crash never leaves a half-written save.</summary>
    public static class SaveFile
    {
        private const string LogPrefix = "[Save] ";
        private const string FileName = "save.json";

        public static string Path => System.IO.Path.Combine(Application.persistentDataPath, FileName);

        public static bool Exists => File.Exists(Path);

        public static bool TryRead(out string json)
        {
            json = null;
            try
            {
                if (!File.Exists(Path))
                {
                    return false;
                }
                json = File.ReadAllText(Path);
                return !string.IsNullOrWhiteSpace(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(LogPrefix + "Could not read the save file: " + exception.Message);
                return false;
            }
        }

        public static bool TryWrite(string json)
        {
            string temp = Path + ".tmp";
            try
            {
                File.WriteAllText(temp, json);
                if (File.Exists(Path))
                {
                    File.Delete(Path);
                }
                File.Move(temp, Path);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning(LogPrefix + "Could not write the save file: " + exception.Message);
                return false;
            }
        }

        public static void Delete()
        {
            try
            {
                if (File.Exists(Path))
                {
                    File.Delete(Path);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning(LogPrefix + "Could not delete the save file: " + exception.Message);
            }
        }
    }
}
