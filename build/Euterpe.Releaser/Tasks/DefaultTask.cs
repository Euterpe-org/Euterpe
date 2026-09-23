namespace Euterpe.Releaser.Tasks;

[TaskName("Default")]
[IsDependentOn(typeof(PackTask))]
public sealed class DefaultTask : FrostingTask<ReleaseContext>;
