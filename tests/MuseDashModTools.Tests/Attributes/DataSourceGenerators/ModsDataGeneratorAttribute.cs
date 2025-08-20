using DynamicData;

namespace MuseDashModTools.Tests.Attributes;

public sealed class ModsDataGeneratorAttribute : DataSourceGeneratorAttribute<SourceCache<ModDto, string>>
{
    protected override IEnumerable<Func<SourceCache<ModDto, string>>> GenerateDataSources(DataGeneratorMetadata dataGeneratorMetadata)
    {
        yield return () => new SourceCache<ModDto, string>(x => x.Name);
    }
}