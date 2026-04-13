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
    public static partial void CopyFrom([MappingTarget] this Config currentConfig, Config savedConfig);
}