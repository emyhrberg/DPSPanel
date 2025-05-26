using System;
using System.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria;

namespace DPSPanel.Helpers
{
    public static class PanelPositionJsonHelper
    {
        private const string FileName = "PanelPosition.json";
        private const string FolderName = "DPSPanel"; // Define the folder name consistently

        public static void WritePanelPosition(Vector2 panelPosition)
        {
            string filePath = GetFolderPath(FolderName, FileName);
            if (string.IsNullOrEmpty(filePath))
            {
                Log.Error($"Failed to get file path for {FileName}. Cannot write position.");
                return;
            }

            try
            {
                string json = JsonConvert.SerializeObject(panelPosition, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to write panel position to {FileName}: {ex.Message}");
            }
        }

        public static Vector2 ReadPanelPosition()
        {
            string filePath = GetFolderPath(FolderName, FileName);
            Vector2 defaultPosition = new(0.5f, 0.07f); // top center

            if (string.IsNullOrEmpty(filePath))
            {
                Log.Error($"Failed to get file path for {FileName}. Returning default position.");
                return defaultPosition;
            }

            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);

                    if (string.IsNullOrWhiteSpace(json))
                    {
                        Log.Info($"{FileName} is empty. Returning default position.");
                        return defaultPosition;
                    }

                    Vector2 data = JsonConvert.DeserializeObject<Vector2>(json);

                    if (data == default)
                    {
                        Log.Info($"Deserialized {FileName} resulted in a default Vector2. Using default panel position.");
                        return defaultPosition;
                    }
                    return data;
                }
                else
                {
                    Log.Info($"{FileName} not found. Returning default position and creating the file with default values.");
                    WritePanelPosition(defaultPosition);
                    return defaultPosition;
                }
            }
            catch (JsonReaderException jsonEx)
            {
                Log.Error($"Failed to parse {FileName}: {jsonEx.Message}. Returning default position.");
                return defaultPosition;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to read panel position from {FileName}: {ex.Message}. Returning default position.");
                return defaultPosition;
            }
        }

        public static string GetFolderPath(string folderName, string fileNameWithExt)
        {
            try
            {
                // Path.Combine can take multiple arguments to build the path
                string modDataPath = Path.Combine(Main.SavePath, "Mods", folderName);
                Directory.CreateDirectory(modDataPath); // Ensure the mod-specific directory exists
                string filePath = Path.Combine(modDataPath, fileNameWithExt);
                return filePath;
            }
            catch (Exception ex)
            {
                Log.Error($"Could not get/create folder path for {folderName}/{fileNameWithExt}: {ex.Message}");
                return null;
            }
        }
    }
}