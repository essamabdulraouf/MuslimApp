using System.Reflection;
using MuslimApp.Domain.Common;

namespace MuslimApp.Domain.Tests.Architecture;

/// <summary>
/// The dependency rule is the project's most important constraint, so it is enforced by a test rather
/// than by convention. If the domain ever references a framework, this fails at build time instead of
/// being discovered when someone tries to swap the UI or the database.
/// </summary>
public class DependencyRuleTests
{
    private static readonly string[] ForbiddenPrefixes =
    [
        "Microsoft.Maui",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.Data.Sqlite",
        "SQLitePCLRaw",
        "Mono.Android",
        "Microsoft.iOS",
        "Microsoft.Extensions.DependencyInjection",
        "Microsoft.Extensions.Logging",
        "System.Net.Http",
        "Xamarin"
    ];

    [Fact]
    public void The_domain_depends_on_nothing_but_the_base_library()
    {
        Assembly domain = typeof(Coordinates).Assembly;

        IEnumerable<string> referenced = domain.GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty);

        foreach (string name in referenced)
        {
            ForbiddenPrefixes.Should().NotContain(
                prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase),
                $"the domain must not reference {name}");
        }
    }

    [Fact]
    public void The_domain_declares_no_project_or_package_references()
    {
        Assembly domain = typeof(Coordinates).Assembly;

        IEnumerable<string> nonFrameworkReferences = domain.GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .Where(name => !name.StartsWith("System", StringComparison.Ordinal)
                && !name.Equals("netstandard", StringComparison.OrdinalIgnoreCase)
                && !name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase));

        nonFrameworkReferences.Should().BeEmpty();
    }
}
