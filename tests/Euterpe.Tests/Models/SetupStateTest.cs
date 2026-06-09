namespace Euterpe.Tests;

[Category("SetupStateTests")]
[TestSubject(typeof(SetupState))]
public sealed class SetupStateTest
{
    private static SetupStepState NewStep(SetupStepStatus status = SetupStepStatus.Pending) =>
        new()
        {
            Kinds = SetupOptionKinds.MelonLoader,
            DisplayName = "step",
            Status = status
        };

    [Test]
    public async Task DefaultState_IsNotStarted_AndNotRunning()
    {
        var state = new SetupState();

        using var _ = Assert.Multiple();
        await Assert.That(state.Stage).IsEqualTo(SetupExecutionStage.NotStarted);
        await Assert.That(state.IsRunning).IsFalse();
        await Assert.That(state.AllSucceeded).IsFalse();
        await Assert.That(state.Steps).IsEmpty();
    }

    [Test]
    public async Task IsRunning_ReflectsRunningStage()
    {
        var state = new SetupState { Stage = SetupExecutionStage.Running };
        await Assert.That(state.IsRunning).IsTrue();
    }

    [Test]
    public async Task AllSucceeded_True_WhenFinishedAndAllStepsSucceeded()
    {
        var state = new SetupState();
        state.Steps.Add(NewStep(SetupStepStatus.Succeeded));
        state.Steps.Add(NewStep(SetupStepStatus.Succeeded));
        state.Stage = SetupExecutionStage.Finished;

        await Assert.That(state.AllSucceeded).IsTrue();
    }

    [Test]
    public async Task AllSucceeded_False_WhenAnyStepFailed()
    {
        var state = new SetupState();
        state.Steps.Add(NewStep(SetupStepStatus.Succeeded));
        state.Steps.Add(NewStep(SetupStepStatus.Failed));
        state.Stage = SetupExecutionStage.Finished;

        await Assert.That(state.AllSucceeded).IsFalse();
    }

    [Test]
    public async Task AllSucceeded_False_WhenStageNotFinished()
    {
        var state = new SetupState();
        state.Steps.Add(NewStep(SetupStepStatus.Succeeded));
        state.Stage = SetupExecutionStage.Running;

        await Assert.That(state.AllSucceeded).IsFalse();
    }

    [Test]
    public async Task Reset_ClearsStepsAndResetsStage()
    {
        var state = new SetupState();
        state.Steps.Add(NewStep());
        state.Stage = SetupExecutionStage.Finished;

        state.Reset();

        using var _ = Assert.Multiple();
        await Assert.That(state.Steps).IsEmpty();
        await Assert.That(state.Stage).IsEqualTo(SetupExecutionStage.NotStarted);
    }

    [Test]
    public async Task SettingStage_RaisesPropertyChangedForDependents()
    {
        var state = new SetupState();
        var changed = new List<string?>();
        state.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        state.Stage = SetupExecutionStage.Running;

        using var _ = Assert.Multiple();
        await Assert.That(changed).Contains(nameof(SetupState.Stage));
        await Assert.That(changed).Contains(nameof(SetupState.IsRunning));
        await Assert.That(changed).Contains(nameof(SetupState.AllSucceeded));
    }
}
