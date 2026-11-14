using System.Net;
using ChangeFlow.Core;
using ChangeFlow.Tests.Fixtures;
using Xunit;
namespace ChangeFlow.Tests.Core;
public sealed class ArchitectureTests
{
    [Fact]
    public void CoreHasNoHttpOrPersistenceDependency()
    {
        var names=typeof(ChangeRequestRules).Assembly.GetReferencedAssemblies().Select(a=>a.Name!);
        Assert.DoesNotContain(names,n=>n.Contains("AspNetCore",StringComparison.OrdinalIgnoreCase)||n.Contains("Sqlite",StringComparison.OrdinalIgnoreCase)||n=="ChangeFlow.Api");
    }
    [Theory]
    [InlineData("/login")]
    [InlineData("/api/login")]
    [InlineData("/api/maintenance")]
    [InlineData("/api/fault")]
    [InlineData("/api/roles")]
    public async Task NoMaintenanceAuthenticationOrFaultEndpoints(string path)
    {
        using var f=new SqliteFixture();
        Assert.Equal(HttpStatusCode.NotFound,(await f.Send("POST",path,"{}")).StatusCode);
    }
}
