using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ChangeFlow.Api.Persistence;
namespace ChangeFlow.Tests.Fixtures;
public sealed class ChangeFlowFactory(string database,OperationGates? gates=null,Action<IServiceCollection>? configure=null):WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var dir=new DirectoryInfo(AppContext.BaseDirectory);
        while(dir is not null&&!File.Exists(Path.Combine(dir.FullName,"ChangeFlow.slnx")))dir=dir.Parent;
        builder.UseContentRoot(Path.Combine(dir!.FullName,"src","ChangeFlow.Api"));
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_,config)=>config.AddInMemoryCollection(new Dictionary<string,string?> {
            ["ChangeFlow:DatabasePath"]=database,
            ["Logging:LogLevel:Default"]="Warning"
        }));
        if(gates is not null)builder.ConfigureServices(services=>services.AddSingleton<IMutationObserver>(gates));
        if(configure is not null)builder.ConfigureServices(configure);
    }
}
