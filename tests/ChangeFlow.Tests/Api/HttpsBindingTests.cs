using ChangeFlow.Api;
using Microsoft.Extensions.Configuration;
using Xunit;
namespace ChangeFlow.Tests.Api;
public sealed class HttpsBindingTests
{
    [Fact]
    public void DefaultIsHttpsAndMaintenanceHasNoNetworkBinding()
    {
        Assert.Equal(new[]{"https://localhost:7043"},HttpsBindings.Resolve(new ConfigurationBuilder().Build()));
    }
    [Theory]
    [InlineData("urls","http://localhost:5000")]
    [InlineData("urls","https://localhost:7043;http://localhost:5000")]
    [InlineData("Kestrel:Endpoints:Plain:Url","http://127.0.0.1:5000")]
    public void PlaintextBindingIsRejectedBeforeServiceStart(string key,string url)
    {
        var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{[key]=url}).Build();
        Assert.Throws<InvalidOperationException>(()=>HttpsBindings.Resolve(config));
    }
    [Fact]
    public void ExplicitTlsConfigurationIsPreserved()
    {
        var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
            ["urls"]="https://localhost:8443",["Kestrel:Endpoints:Secure:Url"]="https://localhost:9443"}).Build();
        Assert.Equal(new[]{"https://localhost:8443"},HttpsBindings.Resolve(config));
    }
}
