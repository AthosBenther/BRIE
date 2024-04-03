using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using BRIE.Types;

namespace BRIE
{
    public class ProjectData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;


        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }


        #region Paths
        private string _projectPath;
        public string ProjectPath
        {
            get => _projectPath;
            set
            {
                if (_projectPath != value)
                {
                    _projectPath = value;
                    OnPropertyChanged(nameof(ProjectPath));
                }
            }
        }

        private string? _exportPath;
        public string? ExportPath
        {
            get => _exportPath;
            set
            {
                if (_exportPath != value)
                {
                    _exportPath = value;
                    OnPropertyChanged(nameof(ExportPath));
                }
            }
        }

        private string? _roadsFolderPath;
        public string? RoadsFolderPath
        {
            get => _roadsFolderPath;
            set
            {
                if (_roadsFolderPath != value)
                {
                    _roadsFolderPath = value;
                    OnPropertyChanged(nameof(RoadsFolderPath));
                }
            }
        }

        private string? _geoJsonPath;
        public string? GeoJsonPath
        {
            get => _geoJsonPath;
            set
            {
                if (_geoJsonPath != value)
                {
                    _geoJsonPath = value;
                    OnPropertyChanged(nameof(GeoJsonPath));
                }
            }
        }

        private string? _heightmapPath;
        public string? HeightmapPath
        {
            get => _heightmapPath;
            set
            {
                if (_heightmapPath != value)
                {
                    _heightmapPath = value;
                    OnPropertyChanged(nameof(HeightmapPath));
                }
            }
        }

        private string? _satMapPath;
        public string? SatMapPath
        {
            get => _satMapPath;
            set
            {
                if (_satMapPath != value)
                {
                    _satMapPath = value;
                    OnPropertyChanged(nameof(SatMapPath));
                }
            }
        }

        #endregion

        #region Sizes
        private int _resolution = 1024;
        public int Resolution
        {
            get => _resolution;
            set
            {
                _resolution = value;
                OnPropertyChanged(nameof(Resolution));
            }
        }

        private int _size = 1024;
        public int Size
        {
            get => _size;
            set
            {
                _size = value;
                OnPropertyChanged(nameof(Size));
            }
        }

        private double _terrainElevationMin;

        public double TerrainElevationMin
        {
            get { return _terrainElevationMin; }
            set
            {
                _terrainElevationMin = value;
                OnPropertyChanged(nameof(Autosave));
            }
        }

        private Extents _extents = new Extents();

        public Extents Extents
        {
            get => _extents;
            set
            {
                _extents = value;
                OnPropertyChanged(nameof(Extents));
            }
        }



        private double _terrainElevationMax;
        public double TerrainElevationMax
        {
            get => _terrainElevationMax; set
            {
                _terrainElevationMax = value;
                OnPropertyChanged(nameof(Autosave));
            }
        }
        #endregion

        #region Configs

        private bool _autosave = true;
        public bool Autosave
        {
            get => _autosave;
            set
            {
                if (_autosave != value)
                {
                    _autosave = value;
                    OnPropertyChanged(nameof(Autosave));
                }
            }
        }

        #endregion


        public ProjectData()
        {
        }



        public ProjectData(string name, string projectPath)
        {
            Name = name;
            ProjectPath = projectPath;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Save()
        {
            if (Name == null)
            {
                throw new System.Exception("The Loaded BRIE Project can't be saved: it's name is null");
            }
            else if (ProjectPath == null)
            {
                throw new System.Exception("The Loaded BRIE Project can't be saved: it's file path is null");
            }
            else
            {
                FileManager.SaveBrie(this);
            }
        }

    }
    public class Project
    {
        public static event PropertyChangedEventHandler PropertyChanged;
        public static event EventHandler DataChanged;

        public static ProjectData Data;
        public static bool IsInitialized { get => Data != null; }
        public static string? Name { get { return Data?.Name; } set { Data.Name = value; } }

        #region Paths
        public static string? ProjectPath { get { return Data?.ProjectPath; } set { Data.ProjectPath = value; } }
        public static string? ExportPath { get { return Data?.ExportPath; } set { Data.ExportPath = value; } }
        public static string? RoadsFolderPath { get { return Data?.RoadsFolderPath; } set { Data.RoadsFolderPath = value; } }
        public static string? GeoJsonPath { get { return Data?.GeoJsonPath; } set { Data.GeoJsonPath = value; } }
        public static string? HeightmapPath { get { return Data?.HeightmapPath; } set { Data.HeightmapPath = value; } }
        public static string? SatMapPath { get { return Data?.SatMapPath; } set { Data.SatMapPath = value; } }
        #endregion

        #region Sizes
        public static int Resolution { get => Data?.Resolution ?? 0; set => Data.Resolution = value; }
        public static int Size { get => Data?.Size ?? 0; set => Data.Size = value; }

        public static Extents Extents { get => Data?.Extents ?? new Extents(); set => Data.Extents = value; }
        public static double TerrainElevationMin { get => Data?.TerrainElevationMin ?? 0; set { Data.TerrainElevationMin = value; } }
        public static double TerrainElevationMax { get => Data?.TerrainElevationMax ?? 0; set { Data.TerrainElevationMax = value; } }
        public static double TerrainDifference => TerrainElevationMax - TerrainElevationMin;

        #endregion

        public static bool Autosave { get { return IsInitialized ? Data.Autosave : false; } set { Data.Autosave = value; } }





        public static void Initialize(ProjectData data)
        {
            Data = data;
            Data.PropertyChanged += Data_PropertyChanged;
            Data_Changed(typeof(Project), new EventArgs());
        }

        private static void Data_Changed(object? sender, EventArgs e)
        {
            if (Autosave) Data.Save();
            List<string> props = typeof(Project).GetProperties().ToList().Select(p => p.Name).ToList();
            props.Sort();
            foreach (string prop in props)
            {
                Data_PropertyChanged(sender, new PropertyChangedEventArgs(prop));
            }
            DataChanged?.Invoke(sender, e);
        }

        private static void Data_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (Autosave) Data.Save();
            PropertyChanged?.Invoke(sender, e);
        }

        public static void Save()
        {
            Data.Save();
        }
    }
}
