using Euterpe.Models.Wizard;

namespace Euterpe.Controls.Models;

public sealed record WizardRole(WizardIdentity Identity, string IconKey, LocalizedString Title, LocalizedString Description, string AccentColor);
