namespace Euterpe.Models.Migrations;

public readonly record struct MigrationResult(MigrationOutcome Outcome, string Destination);
