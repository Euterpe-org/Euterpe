namespace MuseDashModTools.Models.Enums;

public enum ModState
{
    Incompatible = -3,
    Duplicated = -2,
    Outdated = -1,
    Normal = 0,
    Modified = 1,
    Newer = 2
}