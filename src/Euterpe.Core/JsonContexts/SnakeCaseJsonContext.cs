using System.Text.Json.Serialization;
using Euterpe.Contracts.Account;
using Euterpe.Contracts.Charts;
using Euterpe.Contracts.Distribution;
using Euterpe.Contracts.Mods;
using Euterpe.Contracts.Telemetry;

namespace Euterpe.Core.JsonContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(Mod[]))]
[JsonSerializable(typeof(Dependency[]))]
[JsonSerializable(typeof(Lib[]))]
[JsonSerializable(typeof(Release[]))]
[JsonSerializable(typeof(Chart[]))]
[JsonSerializable(typeof(CheckChartUpdatesRequest))]
[JsonSerializable(typeof(CheckChartUpdatesResponse))]
[JsonSerializable(typeof(SessionEvent))]
[JsonSerializable(typeof(MuseDashUidRequest))]
[JsonSerializable(typeof(TokenPayload))]
[JsonSerializable(typeof(AppTokenRequest))]
[JsonSerializable(typeof(AppTokenResponse))]
[JsonSerializable(typeof(RefreshRequest))]
[JsonSerializable(typeof(RefreshResponse))]
[JsonSerializable(typeof(LogoutRequest))]
[JsonSerializable(typeof(UserInfo))]
[JsonSerializable(typeof(CurrentUserResponse))]
internal sealed partial class SnakeCaseJsonContext : JsonSerializerContext;