using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MuslimApp.Infrastructure.Content;

/// <summary>
/// Describes a versioned package of religious content (Quran, azkar, duas).
/// </summary>
/// <remarks>
/// Content is the other half of the app's trust promise. A package is only ever accepted whole: the hash
/// is verified before anything is written, so a truncated download or a tampered file can never leave the
/// database holding half of one version and half of another.
/// </remarks>
public sealed record ContentManifest
{
    [JsonPropertyName("version")]
    public required string Version { get; init; }

    [JsonPropertyName("createdAt")]
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>Lower-case hexadecimal SHA-256 of the payload the manifest describes.</summary>
    [JsonPropertyName("contentHash")]
    public required string ContentHash { get; init; }

    /// <summary>
    /// The database schema this package targets. A package built for a newer schema must be rejected
    /// rather than partially applied.
    /// </summary>
    [JsonPropertyName("schemaVersion")]
    public required int SchemaVersion { get; init; }

    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}

/// <summary>Why a content package was rejected.</summary>
public enum ContentValidationResult
{
    Valid = 0,
    ManifestMissing = 1,
    ManifestUnreadable = 2,
    SchemaTooNew = 3,
    HashMismatch = 4,
    PayloadMissing = 5
}

/// <summary>
/// Verifies a content package before it is trusted. Pure and file-system based, so it is fully testable
/// including the corrupt-package cases.
/// </summary>
public static class ContentPackageValidator
{
    /// <summary>The highest schema version this build of the app knows how to read.</summary>
    public const int SupportedSchemaVersion = 1;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ContentManifest? TryReadManifest(string manifestJson)
    {
        try
        {
            return JsonSerializer.Deserialize<ContentManifest>(manifestJson, SerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static ContentValidationResult Validate(ContentManifest? manifest, Stream? payload)
    {
        if (manifest is null)
        {
            return ContentValidationResult.ManifestUnreadable;
        }

        if (string.IsNullOrWhiteSpace(manifest.Version) || string.IsNullOrWhiteSpace(manifest.ContentHash))
        {
            return ContentValidationResult.ManifestUnreadable;
        }

        if (manifest.SchemaVersion > SupportedSchemaVersion)
        {
            return ContentValidationResult.SchemaTooNew;
        }

        if (payload is null)
        {
            return ContentValidationResult.PayloadMissing;
        }

        string actualHash = ComputeSha256(payload);

        return string.Equals(actualHash, manifest.ContentHash.Trim(), StringComparison.OrdinalIgnoreCase)
            ? ContentValidationResult.Valid
            : ContentValidationResult.HashMismatch;
    }

    public static string ComputeSha256(Stream payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        if (payload.CanSeek)
        {
            payload.Position = 0;
        }

        byte[] hash = SHA256.HashData(payload);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
