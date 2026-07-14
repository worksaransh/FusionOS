using System.Reflection;
using FluentAssertions;
using Xunit;

namespace FusionOS.Architecture.Tests;

/// <summary>
/// Enforces the module-boundary rules in 03_SYSTEM_ARCHITECTURE.md §2 at build
/// time rather than relying on code review alone: a module's Domain assembly must
/// never reference another module's Domain or Infrastructure assembly.
/// </summary>
public class ModuleBoundaryTests
{
    [Fact]
    public void CoreDomain_DoesNotReferenceAnyOtherModule()
    {
        var coreDomainAssembly = typeof(FusionOS.Modules.Core.Domain.Companies.Company).Assembly;

        var referencedAssemblyNames = coreDomainAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToList();

        referencedAssemblyNames.Should().NotContain(name =>
            name != null &&
            name.StartsWith("FusionOS.Modules.") &&
            !name.StartsWith("FusionOS.Modules.Core"));
    }
}
