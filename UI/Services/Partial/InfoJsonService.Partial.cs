using System.IO;
using System.Text.RegularExpressions;

namespace MuseDashModToolsUI.Services;

public sealed partial class InfoJsonService
{
    /// <summary>
    ///     Parse map info from map file
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private async Task<MapInfo> ParseMapInfo(string filePath)
    {
        using var streamReader = new StreamReader(filePath);
        if (!_isInfoJsonInitialized)
        {
            var lines = await streamReader.ReadLineAsync(4, 9);
            _infoJson.Scene = lines[0]![7..];
            _infoJson.Name = lines[1]![7..];
            _infoJson.Author = lines[2]![8..];
            _infoJson.Bpm = lines[4]![5..];

            _isInfoJsonInitialized = true;

            return new MapInfo
            {
                Difficulty = lines[5]![11..],
                LevelDesigner = lines[3]![13..]
            };
        }
        else
        {
            var lines = await streamReader.ReadLineAsync(7, 9);
            return new MapInfo
            {
                Difficulty = lines[2]![11..],
                LevelDesigner = lines[0]![13..]
            };
        }
    }

    /// <summary>
    ///     Fill info json
    /// </summary>
    /// <param name="id"></param>
    /// <param name="mapInfo"></param>
    private void FillInfoJson(string id, MapInfo mapInfo)
    {
        switch (id)
        {
            case "1":
                _infoJson.Difficulty1 = mapInfo.Difficulty;
                _infoJson.LevelDesigner1 = mapInfo.LevelDesigner;
                break;

            case "2":
                _infoJson.Difficulty2 = mapInfo.Difficulty;
                _infoJson.LevelDesigner2 = mapInfo.LevelDesigner;
                break;

            case "3":
                _infoJson.Difficulty3 = mapInfo.Difficulty;
                _infoJson.LevelDesigner3 = mapInfo.LevelDesigner;
                break;

            case "4":
                _infoJson.Difficulty4 = mapInfo.Difficulty;
                _infoJson.LevelDesigner4 = mapInfo.LevelDesigner;
                break;
        }
    }

    [GeneratedRegex("map([1-4]).bms")]
    private static partial Regex MapRegex();
}