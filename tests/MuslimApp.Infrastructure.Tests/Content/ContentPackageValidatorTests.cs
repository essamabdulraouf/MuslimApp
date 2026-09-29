using System.Text;
using MuslimApp.Infrastructure.Content;

namespace MuslimApp.Infrastructure.Tests.Content;

/// <summary>
/// Content validation is a trust boundary: a package that does not verify must be rejected whole rather
/// than partially applied.
/// </summary>
public class ContentPackageValidatorTests
{
    private const string Payload = """{"surahs":[{"number":1,"ayahs":7}]}""";

    [Fact]
    public void A_matching_hash_is_accepted()
    {
        using MemoryStream payload = Stream(Payload);
        ContentManifest manifest = Manifest(ContentPackageValidator.ComputeSha256(payload));

        ContentPackageValidator.Validate(manifest, payload).Should().Be(ContentValidationResult.Valid);
    }

    [Fact]
    public void A_single_altered_byte_is_rejected()
    {
        using MemoryStream original = Stream(Payload);
        ContentManifest manifest = Manifest(ContentPackageValidator.ComputeSha256(original));

        using MemoryStream tampered = Stream(Payload.Replace("7", "8", StringComparison.Ordinal));

        ContentPackageValidator.Validate(manifest, tampered).Should().Be(ContentValidationResult.HashMismatch);
    }

    [Fact]
    public void A_truncated_download_is_rejected()
    {
        using MemoryStream original = Stream(Payload);
        ContentManifest manifest = Manifest(ContentPackageValidator.ComputeSha256(original));

        using MemoryStream truncated = Stream(Payload[..(Payload.Length / 2)]);

        ContentPackageValidator.Validate(manifest, truncated).Should().Be(ContentValidationResult.HashMismatch);
    }

    [Fact]
    public void A_package_built_for_a_newer_schema_is_rejected()
    {
        using MemoryStream payload = Stream(Payload);
        ContentManifest manifest = Manifest(ContentPackageValidator.ComputeSha256(payload)) with
        {
            SchemaVersion = ContentPackageValidator.SupportedSchemaVersion + 1
        };

        ContentPackageValidator.Validate(manifest, payload).Should().Be(ContentValidationResult.SchemaTooNew);
    }

    [Fact]
    public void A_missing_manifest_is_rejected()
    {
        using MemoryStream payload = Stream(Payload);

        ContentPackageValidator.Validate(null, payload).Should().Be(ContentValidationResult.ManifestUnreadable);
    }

    [Fact]
    public void A_missing_payload_is_rejected()
    {
        ContentPackageValidator.Validate(Manifest("abc"), null).Should().Be(ContentValidationResult.PayloadMissing);
    }

    [Fact]
    public void A_manifest_without_a_hash_is_rejected()
    {
        using MemoryStream payload = Stream(Payload);

        ContentPackageValidator.Validate(Manifest("   "), payload).Should().Be(ContentValidationResult.ManifestUnreadable);
    }

    [Fact]
    public void Malformed_manifest_json_does_not_throw()
    {
        ContentPackageValidator.TryReadManifest("{ this is not json").Should().BeNull();
    }

    [Fact]
    public void A_well_formed_manifest_is_read()
    {
        const string json = """
        {
            "version": "1.0.0",
            "createdAt": "2026-09-29T00:00:00+00:00",
            "contentHash": "ABCDEF",
            "schemaVersion": 1
        }
        """;

        ContentManifest? manifest = ContentPackageValidator.TryReadManifest(json);

        manifest.Should().NotBeNull();
        manifest!.Version.Should().Be("1.0.0");
        manifest.SchemaVersion.Should().Be(1);
    }

    [Fact]
    public void Hash_comparison_ignores_case_and_surrounding_whitespace()
    {
        using MemoryStream payload = Stream(Payload);
        string hash = ContentPackageValidator.ComputeSha256(payload);

        ContentManifest manifest = Manifest($"  {hash.ToUpperInvariant()}  ");

        ContentPackageValidator.Validate(manifest, payload).Should().Be(ContentValidationResult.Valid);
    }

    [Fact]
    public void Hashing_is_independent_of_the_stream_position()
    {
        using MemoryStream payload = Stream(Payload);
        string first = ContentPackageValidator.ComputeSha256(payload);
        string second = ContentPackageValidator.ComputeSha256(payload);

        second.Should().Be(first);
    }

    private static MemoryStream Stream(string content) => new(Encoding.UTF8.GetBytes(content));

    private static ContentManifest Manifest(string hash) => new()
    {
        Version = "1.0.0",
        CreatedAt = new DateTimeOffset(2026, 9, 29, 0, 0, 0, TimeSpan.Zero),
        ContentHash = hash,
        SchemaVersion = 1
    };
}
