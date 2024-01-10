using System.Text.RegularExpressions;

namespace MuseDashModToolsUI.Services;

public sealed partial class LogAnalyzeService
{
    /// <summary>
    ///     Check whether the user has registered HQ or not
    /// </summary>
    private void CheckHeadQuarterRegister()
    {
        if (!LogContent.ContainsString("You have not registered for Headquarters"))
        {
            return;
        }

        Logger.Information("Didn't register HQ, showing message box");
        LogErrorBuilder.AppendLine(MsgBox_Content_RegisterHQ);
    }

    /// <summary>
    ///     Check whether the mod is outdated or not
    /// </summary>
    private void CheckModVersion()
    {
        var modVersionMatches = ModVersionRegex().Matches(LogContent);
        if (modVersionMatches.Count == 0)
        {
            return;
        }

        foreach (var mod in modVersionMatches.Select(x => x.Groups))
        {
            var modName = mod[1].Value;
            var modVersion = mod[2].Value;

            var outdated = ModService.CompareVersion(modName, modVersion);
            if (!outdated)
            {
                continue;
            }

            LogErrorBuilder.AppendFormat(MsgBox_Content_OutdatedMod, modName).AppendLine();
            Logger.Information("Outdated Mod: {ModName}", modName);
        }
    }

    /// <summary>
    ///     Start log file monitor
    /// </summary>
    private void StartLogFileMonitor()
    {
        _watcher.Path = SavingService.Settings.MelonLoaderFolder;
        _watcher.Filter = "Latest.log";
        _watcher.Renamed += (_, _) => LogAnalysisViewModel.Value.Initialize();
        _watcher.Changed += (_, _) => LogAnalysisViewModel.Value.Initialize();
        _watcher.Created += (_, _) => LogAnalysisViewModel.Value.Initialize();
        _watcher.Deleted += (_, _) => LogAnalysisViewModel.Value.Initialize();
        _watcher.EnableRaisingEvents = true;
        Logger.Information("Log File Monitor Started");
    }

    [GeneratedRegex(@"ApplicationPath = (.*steamapps)\\common\\Muse Dash\\musedash.exe")]
    private static partial Regex ApplicationPathRegex();

    [GeneratedRegex(@"\bMelonLoader v(\d+\.\d+\.\d+)")]
    private static partial Regex MelonLoaderVersionRegex();

    [GeneratedRegex(@"\b(?!MelonLoader\b)([\w\s]+) v(\d+\.\d+\.\d+)", RegexOptions.Multiline)]
    private static partial Regex ModVersionRegex();
}