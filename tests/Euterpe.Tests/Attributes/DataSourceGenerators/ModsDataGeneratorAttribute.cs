using Euterpe.Contracts.Mods;

namespace Euterpe.Tests.Attributes;

public sealed class ModsDataGeneratorAttribute : DataSourceGeneratorAttribute<ModDto[]>
{
    protected override IEnumerable<Func<ModDto[]>> GenerateDataSources(DataGeneratorMetadata dataGeneratorMetadata)
    {
        yield return () =>
        [
            new ModDto
            {
                Name = "ModA",
                FileName = "ModA.dll",
                Version = "1.0.0"
            },
            new ModDto
            {
                Name = "ModB",
                FileName = "ModB.dll",
                Version = "2.0.0",
                ModDependencies = ["ModA"]
            },
            new ModDto
            {
                Name = "ModC",
                FileName = "ModC.dll",
                Version = "1.5.0",
                LibDependencies = ["LibX"],
                Screenshots = [new ModScreenshot { Url = "/img/c.png" }]
            }
        ];
    }
}