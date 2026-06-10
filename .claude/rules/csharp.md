# C# Rules

## Design

- Design the change, don't just complete the feature: a working implementation that leaves tech debt is not acceptable. If every viable approach leaves debt, the module itself is due for a redesign — surface that instead of bolting on.
- Build along the known extension axes; this app already paid for assuming a single game (the `[PerGame]`/game-scope rework when multi-game support arrived). Ask "what varies when the next game/variant arrives" before hardcoding, and keep game-specific state and services game-scoped.

## Naming & comments

- Method and variable names must be self-explanatory; if a name needs a comment to be understood, rename it (e.g. `PlayingFolderName`, not `CurrentlyPlaying` + a comment).
- When a name is hard to settle, mirror an existing name in this project first; with no precedent, follow the general .NET/community convention for that concept (how the BCL or well-known libraries name it).
- No narration comments: never comment what the code does, where it came from, or why a change is correct.
- The only acceptable comment is a single short line stating a constraint the code cannot show (threading requirements, external library quirks). No multi-line essays.
- No XML doc comments on members whose name already says everything.

## Async

- Never wrap an async call chain in `Task.Run`. If the chain has real awaits (file I/O, network), it does not block the caller. `Task.Run` is only justified to push genuinely synchronous blocking work (e.g. synchronous audio decode) off the UI thread.
- `ConfigureAwait(false)` on every await outside UI-bound continuations; use `Dispatcher.UIThread` explicitly when touching UI-bound state afterwards.

## Code organization

- Where a type belongs is defined by the project map in CLAUDE.md.
- Values derivable from a model are computed properties on the model itself (see `ChartDto.SizeDisplay`, `BpmDisplay`, `DifficultyBadges`), not static helper classes or converter wrappers.
- Large types split into `Name.Topic.cs` partial files.
- Service layout: the suffix-less `<Service>.cs` holds only the public interface implementation. Everything else lives in partials — `<Service>.Private.cs` when the helpers are few, `<Service>.<Topic>.cs` (`.Core`, `.Import`, ...) when there is enough to split by topic.
- Private helper types are nested inside the owning partial class file, not separate top-level types.

## Services & notifications

- Toast notifications live in core services, not viewmodels. Worker methods return results; the public method notifies per granularity: single operation = per-item toast, bulk operation = one summary toast. Never add a notification-suppress flag.
- Bulk methods return a count so an explicit UI action can toast "nothing to do" (silent no-op stays correct for automatic startup paths).
- No redundant wrapper services or pass-through methods: put the method on the existing domain service and make the real method public.
- File/IO primitives go through `IFileSystemService` (`Try*` pattern); never raw `Directory`/`File` calls with ad-hoc try/catch in services.

## Misc

- Deduplicate shared one-liners into a single home (e.g. `ChartFiles.MapName`), even tiny ones.
- Before writing anything by hand, check the packages already referenced (`Directory.Packages.props`) for an existing facility — especially the easily-forgotten ones: CommunityToolkit.Mvvm's `[NotifyPropertyChangedFor]` for dependent properties instead of manually raising `OnPropertyChanged`, R3 `Observable`s for event wiring/composition instead of manual event handlers.
- Prefer an existing library over hand-rolling, but verify its actual behavior (decompile if needed) before adopting or rejecting it.
- Localized strings: during development add the neutral `.resx` plus `zh-Hans`/`zh-Hant` only — those are the locales the user can read and test with, and keys may still churn while the feature settles. The remaining locales get batch-translated once the feature is final.
- Logging via ZLogger interpolation (`Logger.ZLogInformation($"...")`).
- DI: `public required T Name { get; init; }` properties inside `#region Injections`.
- `.cs` files end with exactly one trailing newline (`insert_final_newline` applies to `[*.cs]` only).
