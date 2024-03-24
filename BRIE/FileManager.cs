using System;
using System.IO;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace BRIE
{
    internal class FileManager
    {

        public static string? OpenPng()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PNG files (*.png)|*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }

        internal static Project? OpenBrie()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BRIE Project File (*.brie)|*.brie";

            if (openFileDialog.ShowDialog() == true)
            {
                return OpenJson<Project>(openFileDialog.FileName);
            }

            return null;
        }
        internal static Project? NewBrie(string name, string fileName)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "BRIE Project File (*.brie)|*.brie";
            saveFileDialog.FileName = fileName;

            if (saveFileDialog.ShowDialog() == true)
            {
                Project p = new Project(name, saveFileDialog.FileName);
                File.WriteAllText(p.ProjectPath,JsonConvert.SerializeObject(p));
                return p;
            }
            return null;
        }

        public static T OpenJson<T>(string filePath)
        {
            try
            {
                // Read the JSON file
                string json = File.ReadAllText(filePath);

                // Deserialize JSON to the specified type
                T result = JsonConvert.DeserializeObject<T>(json);

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void SaveJson(object Object, string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(Object));
        }
    }
}
