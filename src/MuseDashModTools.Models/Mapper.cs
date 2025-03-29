using Riok.Mapperly.Abstractions;

namespace MuseDashModTools.Models;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public static partial class Mapper
{
    public static partial ModDto ToDto(this Mod mod);
    public static partial LibDto ToDto(this Lib lib);
    public static partial ChartDTO ToDto(this Chart chart);
    public static partial void UpdateFromMod([MappingTarget] this ModDto modDto, Mod mod);
    public static partial void CopyFrom([MappingTarget] this Config currentConfig, Config savedConfig);
}