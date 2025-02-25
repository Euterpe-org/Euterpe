namespace MuseDashModTools.Models.Controls;

public sealed class ContributorGroup(string groupName, ContributorCardItem[] contributors)
{
    public string GroupName { get; } = groupName;
    public ContributorCardItem[] Contributors { get; } = contributors;
}