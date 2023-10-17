using System.IO;

namespace MuseDashModToolsUI.Services;

public partial class ChartService
{
    private async Task<MapInfo> GetMapInfo(string filePath)
    {
        var mapInfo = new MapInfo();
        using var sr = new StreamReader(filePath);
        while (await sr.ReadLineAsync() is { } line)
        {
            if (line.Contains("#LEVELDESIGN"))
                mapInfo.LevelDesigner = line[12..];
            else if (line.Contains("#PLAYLEVEL"))
                mapInfo.Difficulty = line[10..];

            if (!string.IsNullOrEmpty(mapInfo.LevelDesigner) && !string.IsNullOrEmpty(mapInfo.Difficulty))
                return mapInfo;
        }

        return mapInfo;
    }
}