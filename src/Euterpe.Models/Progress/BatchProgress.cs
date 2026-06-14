using System.Runtime.InteropServices;

namespace Euterpe.Models.Progress;

[StructLayout(LayoutKind.Auto)]
public readonly record struct BatchProgress(int Completed, int Total)
{
    public double Percentage => Total is 0 ? 0 : (double)Completed / Total * 100;

    public string CountDisplay => $"{Completed}/{Total}";
}
