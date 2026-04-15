using Euterpe.Models.Enums;

namespace Euterpe.Styles.Models;

public sealed record WizardRole(WizardIdentity Identity, string IconKey, LocalizedString Title, LocalizedString Description, string AccentColor);