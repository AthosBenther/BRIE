using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using BRIE.Dialogs;
using BRIE.Etc;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace BRIE.Classes.Statics
{
    internal class FileManager
    {

        #region Files Creation
        public static string? NewFile(string Filter, string root = null)
        {
            root = root ?? Project.ProjectPath ?? string.Empty;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = Filter;

            if (saveFileDialog.ShowDialog() == true)
            {
                return saveFileDialog.FileName;
            }

            return null;
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
        #endregion
        #region File Opening

        public static void StartFile(string FilePath)
        {
            if (File.Exists(FilePath))
                Process.Start(new ProcessStartInfo
                {
                    FileName = FilePath,
                    UseShellExecute = true
                });
        }
        public static string? OpenFile(string Filter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = Filter;

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }

        public static string? OpenPng() => OpenFile("PNG files (*.png)|*.png");

        internal static ProjectData? OpenBrie()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BRIE Project File (*.brie)|*.brie";
            openFileDialog.ShowDialog();

            return !string.IsNullOrWhiteSpace(openFileDialog.FileName) ? OpenBrie(openFileDialog.FileName) : null;
        }
        internal static ProjectData OpenBrie(string FileName)
        {
            ProjectData p = OpenJson<ProjectData>(FileName);
            Cache.UpsertRecentProject(p.Name, p.ProjectPath);
            return p;
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


        #endregion
        #region File Saving
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

        #endregion
        #region File Validation

        public static bool IsPathValid(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            try
            {
                // Check for invalid characters
                if (filePath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                {
                    return false;
                }

                // Path is valid at this point
                return true;
            }
            catch (ArgumentException)
            {
                // Invalid path format
                return false;
            }
        }
        public static bool IsSafeFileName(string fileName)
        {
            // Get the list of invalid file name characters for the current file system
            char[] invalidChars = Path.GetInvalidFileNameChars();

            // Check if the file name contains any invalid characters
            return !fileName.Any(c => invalidChars.Contains(c));
        }
        #endregion
    }
}
