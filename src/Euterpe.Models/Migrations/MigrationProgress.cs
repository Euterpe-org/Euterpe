using System.Runtime.InteropServices;

namespace Euterpe.Models.Migrations;

[StructLayout(LayoutKind.Auto)]
public readonly record struct MigrationProgress(int Completed, int Total)
{
    public double Percentage => Total is 0 ? 0 : (double)Completed / Total * 100;
}
