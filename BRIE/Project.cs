namespace BRIE
{
    public class Project
    {
        public string Name { get; set; }
        public string ProjectPath { get; set; }
        public string? ExportPath { get; set; }
        public string? RoadsFolderPath { get; set; }
        public string? GeoJsonPath { get; set; }
        public string? HeightmapPath { get; set; }
        public string? SatMapPath { get; set; }

        public Project(string name, string projectPath)
        {
            Name = name;
            ProjectPath = projectPath;
        }
    }
}
