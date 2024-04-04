using System;
using System.Diagnostics;
using System.IO;
using BRIE.Dialogs;
using BRIE.Etc;
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

        internal static ProjectData? OpenBrie()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BRIE Project File (*.brie)|*.brie";
            openFileDialog.ShowDialog();

            return (!string.IsNullOrWhiteSpace(openFileDialog.FileName)) ? OpenBrie(openFileDialog.FileName) : null;
        }
        internal static ProjectData OpenBrie(string FileName)
        {
            ProjectData p = OpenJson<ProjectData>(FileName);
            Cache.UpsertRecentProject(p.Name, p.ProjectPath);
            return p;
        }

        internal static ProjectData? NewBrie(string name, string fileName)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "BRIE Project File (*.brie)|*.brie";
            saveFileDialog.FileName = fileName;

            if (saveFileDialog.ShowDialog() == true)
            {
                ProjectData p = new ProjectData(name, saveFileDialog.FileName);
                File.WriteAllText(p.ProjectPath, SerializeObject(p));
                Cache.UpsertRecentProject(p.Name, p.ProjectPath);
                return p;
            }
            return null;
        }

        internal static void SaveBrie(ProjectData p)
        {
            if (File.Exists(p.ProjectPath))
            {
                if (p.Name == null)
                {
                    ProjectNameDialog projectNameDialog = new ProjectNameDialog();
                    projectNameDialog.ShowDialog();

                    if (projectNameDialog.IsSafe)
                    {
                        p.Name = projectNameDialog.ProjectName;
                        File.WriteAllText(p.ProjectPath, SerializeObject(p));
                        Cache.UpsertRecentProject(p.Name, p.ProjectPath);
                    }
                    else
                    {
                        throw new NullReferenceException("This Project can't be saved: it has no name. Add a name an try again.");
                    }
                }
                if (Path.GetExtension(p.ProjectPath).ToLower() == ".brie")
                {
                    SaveJson(p, p.ProjectPath);
                    Cache.UpsertRecentProject(p.Name, p.ProjectPath);
                }
                else
                {
                    Uri uri = new Uri(p.ProjectPath);
                    throw new FileFormatException(uri, "The target Project file is not a .brie. How did you do that?");
                }
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "BRIE Project File (*.brie)|*.brie";
                saveFileDialog.FileName = p.Name.SanitizeFileName();
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(p.ProjectPath, SerializeObject(p));
                    Cache.UpsertRecentProject(p.Name, p.ProjectPath);
                }
            }
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
            File.WriteAllText(path, SerializeObject(Object));
        }

        private static string SerializeObject(object obj)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new IgnoreReadOnlyPropertiesResolver();
            settings.Formatting = Formatting.Indented;
            settings.NullValueHandling = NullValueHandling.Ignore;
            return JsonConvert.SerializeObject(obj, settings);
        }

        public static void Start(string FilePath)
        {
            if (File.Exists(FilePath))
                Process.Start(new ProcessStartInfo
                {
                    FileName = FilePath,
                    UseShellExecute = true
                });
        }
    }
}
