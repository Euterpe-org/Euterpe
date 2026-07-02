using AsmResolver.DotNet;

namespace Euterpe.Core;

internal sealed class ModLocalService : IModLocalService
{
    public IEnumerable<string> GetModFilePaths() => Directory.EnumerateFiles(GameConfig.ModsFolder)
        .Where(ModFiles.IsModFile);

    public IEnumerable<string> GetLibFilePaths() => Directory.EnumerateFiles(GameConfig.UserLibsFolder)
        .Where(static x => Path.GetExtension(x) is ModFiles.DllExtension);

    public async Task<ModDto?> LoadModFromPathAsync(string filePath)
    {
        var mod = new ModDto
        {
            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath),
            IsDisabled = Path.GetExtension(filePath) is ModFiles.DisabledExtension
        };

        try
        {
            var bytes = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);
            var assembly = AssemblyDefinition.FromBytes(bytes);

            var attribute = assembly.FindCustomAttributes("MelonLoader", "MelonInfoAttribute").FirstOrDefault();
            if (attribute is null)
            {
                Logger.ZLogWarning($"{filePath} has no MelonInfoAttribute and is not a MelonLoader mod");
                return null;
            }

            mod.Name = attribute.Signature!.FixedArguments[1].ToString();
            mod.LocalVersion = attribute.Signature!.FixedArguments[2].ToString();
            mod.Author = attribute.Signature!.FixedArguments[3].ToString();
            mod.LocalSHA256 = SHA256Utils.HexLowerFromBytes(bytes);

            return mod;
        }
        catch (Exception ex)
        {
            Logger.ZLogError(ex, $"Failed to load mod from {filePath}, skipping");
            return null;
        }
    }

    public async Task<LibDto> LoadLibFromPathAsync(string filePath) =>
        new()
        {
            Name = Path.GetFileNameWithoutExtension(filePath),
            FileName = Path.GetFileName(filePath),
            SHA256 = await SHA256Utils.HexLowerFromPathAsync(filePath).ConfigureAwait(false),
            IsLocal = true
        };

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required ILogger<ModLocalService> Logger { get; init; }

    #endregion Injections
}
