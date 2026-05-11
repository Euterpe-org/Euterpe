using Euterpe.Contracts.Distribution;
using Euterpe.Contracts.Mods;
using Riok.Mapperly.Abstractions;

namespace Euterpe.Models;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class Mapper
{
    public static partial ModDto ToModel(this Mod mod);

    public static LibDto ToModel(this Lib lib)
    {
        var version = lib.Versions.Single().Value;
        return new LibDto
        {
            Name = lib.Slug,
            FileName = $"{lib.Slug}.{lib.FileExtension}",
            SHA256 = version.SHA256,
            DownloadUrl = version.DownloadUrl
        };
    }

    public static partial void UpdateFromMod([MappingTarget] this ModDto modDto, Mod mod);

    [MapDerivedType<MuseDashConfig, MuseDashConfig>]
    [MapDerivedType<MuseDash2Config, MuseDash2Config>]
    private static partial void UpdateGameConfig([MappingTarget] this GameConfig current, GameConfig saved);

    [MapperIgnoreTarget(nameof(Config.MuseDash))]
    [MapperIgnoreTarget(nameof(Config.MuseDash2))]
    [MapperIgnoreTarget(nameof(Config.ActiveGameConfig))]
    private static partial void CopyFromCore([MappingTarget] this Config currentConfig, Config savedConfig);

    public static void CopyFrom(this Config currentConfig, Config savedConfig)
    {
        currentConfig.CopyFromCore(savedConfig);
        currentConfig.MuseDash.UpdateGameConfig(savedConfig.MuseDash);
        currentConfig.MuseDash2.UpdateGameConfig(savedConfig.MuseDash2);
    }
}