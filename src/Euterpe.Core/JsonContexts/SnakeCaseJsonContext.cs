using System.Text.Json.Serialization;
using Euterpe.Contracts.Account;
using Euterpe.Contracts.Telemetry;

namespace Euterpe.Core.JsonContexts;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(SessionEvent))]
[JsonSerializable(typeof(ModDownloadEvent))]
[JsonSerializable(typeof(MuseDashUidRequest))]
internal sealed partial class SnakeCaseJsonContext : JsonSerializerContext;