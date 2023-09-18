using System.Text.RegularExpressions;
using static MuseDashModToolsUI.Localization.Resources;

namespace MuseDashModToolsUI.Services;

public partial class LogAnalyzeService
{
    private void CheckModVersion()
    {
        var modVersionMatches = ModVersionRegex().Matches(LogContent);
        if (modVersionMatches.Count == 0) return;

        foreach (var mod in modVersionMatches.Select(x => x.Groups))
        {
            var modName = mod[1].Value;
            var modVersion = mod[2].Value;

            var outdated = ModService.CompareVersion(modName, modVersion);
            if (!outdated) continue;
            LogErrorBuilder.AppendFormat(MsgBox_Content_OutdatedMod, modName).AppendLine();
            Logger.Information("Outdated Mod: {ModName}", modName);
        }
    }

    private void CheckHeadQuarterRegister()
    {
        if (!HeadQuarterRegisterRegex().Match(LogContent).Success) return;
        Logger.Information("Didn't register HQ, showing message box");
        LogErrorBuilder.AppendLine(MsgBox_Content_RegisterHQ);
    }

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

    [GeneratedRegex("You have not registered for Headquarters", RegexOptions.Multiline)]
    private static partial Regex HeadQuarterRegisterRegex();
}