namespace Euterpe.Core;

internal sealed partial class ModManageService
{
    public async Task ReconcileModsAsync()
    {
        var (diskMods, added, removed) = await SyncLocalModsAsync().ConfigureAwait(false);
        RecomputeModStates(diskMods);
        _sourceCache.Refresh();
        NotifyModSync(added, removed);
    }

    private async Task<(ModDto[] DiskMods, List<string> Added, List<string> Removed)> SyncLocalModsAsync()
    {
        var added = new List<string>();
        var removed = new List<string>();

        var diskMods = (await ModLocalService.GetModFilePaths().WhenAllAsync(ModLocalService.LoadModFromPathAsync).ConfigureAwait(false))
            .OfType<ModDto>()
            .ToArray();
        var diskNames = diskMods.Select(static mod => mod.Name).ToHashSet(StringComparer.Ordinal);

        // One entry per Name (last file wins for the cache, matching AddOrUpdate); duplicate
        // detection runs later on the pristine diskMods array, so never mutate a disk DTO here.
        foreach (var group in diskMods.GroupBy(static mod => mod.Name, StringComparer.Ordinal))
        {
            MergeLocalMod(group.Last(), added);
        }

        foreach (var cached in _sourceCache.Items.Where(static mod => mod is { IsLocal: true, IsProcessing: false }).ToArray())
        {
            if (!diskNames.Contains(cached.Name))
            {
                PruneLocalMod(cached, removed);
            }
        }

        return (diskMods, added, removed);
    }

    private void MergeLocalMod(ModDto disk, List<string> added)
    {
        if (_sourceCache.Lookup(disk.Name) is not { HasValue: true, Value: var cached })
        {
            _sourceCache.AddOrUpdate(disk);
            added.Add(disk.Name);
            return;
        }

        if (cached.IsProcessing)
        {
            return;
        }

        var unchanged = cached.IsLocal
            && cached.LocalFileName == disk.LocalFileName
            && cached.LocalVersion == disk.LocalVersion
            && (cached.HasDownloadSource || cached.SHA256 == disk.SHA256);
        if (unchanged)
        {
            return;
        }

        var wasLocal = cached.IsLocal;
        cached.FileNameWithoutExtension = disk.FileNameWithoutExtension;
        cached.IsDisabled = disk.IsDisabled;
        cached.LocalVersion = disk.LocalVersion;
        if (!cached.HasDownloadSource)
        {
            cached.SHA256 = disk.SHA256;
        }

        CheckConfigFile(cached);
        if (!cached.IsDisabled)
        {
            CheckLibDependencies(cached);
        }

        _sourceCache.AddOrUpdate(cached);
        if (!wasLocal)
        {
            added.Add(cached.Name);
        }
    }

    private void PruneLocalMod(ModDto cached, List<string> removed)
    {
        removed.Add(cached.Name);
        if (cached.HasDownloadSource)
        {
            cached.RemoveLocalInfo();
            _sourceCache.AddOrUpdate(cached);
        }
        else
        {
            _sourceCache.RemoveKey(cached.Name);
        }
    }

    private void RecomputeModStates(ModDto[] diskMods)
    {
        foreach (var mod in _sourceCache.Items)
        {
            if (mod.IsProcessing)
            {
                continue;
            }

            mod.DuplicatedModPaths = [];
            if (!mod.HasDownloadSource)
            {
                mod.State = ModState.Normal;
            }
            else if (mod.IsLocal)
            {
                if (diskMods.FirstOrDefault(disk => disk.Name == mod.Name) is { } disk)
                {
                    mod.State = DetermineModState(disk, mod);
                }
            }
            else
            {
                mod.State = IsModIncompatible(mod.MelonVersion, mod.GameVersion) ? ModState.Incompatible : ModState.Normal;
            }
        }

        CheckDuplicatedMods(diskMods);
        CheckIncompatibleMods();
    }

    private void NotifyModSync(IReadOnlyList<string> added, IReadOnlyList<string> removed)
    {
        switch (added.Count, removed.Count)
        {
            case (0, 0):
                break;
            case (1, 0):
                NotificationService.SuccessLight(Notification_Content_Mod_Sync_Added, added[0]);
                break;
            case (0, 1):
                NotificationService.NoticeLight(Notification_Content_Mod_Sync_Removed, removed[0]);
                break;
            default:
                NotificationService.NoticeLight(Notification_Content_Mod_Sync_Summary, added.Count, removed.Count);
                break;
        }
    }
}
