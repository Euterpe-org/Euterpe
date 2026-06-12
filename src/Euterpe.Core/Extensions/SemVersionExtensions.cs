namespace Euterpe.Core.Extensions;

public static class SemVersionExtensions
{
    extension(string version)
    {
        public int ComparePrecedenceTo(string other) =>
            SemVersion.Parse(version).ComparePrecedenceTo(SemVersion.Parse(other));
    }
}
