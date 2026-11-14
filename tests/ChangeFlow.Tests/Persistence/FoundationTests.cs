using ChangeFlow.Api.Persistence;
using ChangeFlow.Core;
using ChangeFlow.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Xunit;
namespace ChangeFlow.Tests.Persistence;
public sealed class FoundationTests
{
    [Fact]
    public void PragmaSchemaAndRestartPreserveData()
    {
        using var f=new SqliteFixture();var id=f.Seed();
        Assert.Equal(3L,f.Scalar("PRAGMA synchronous"));Assert.Equal(1L,f.Scalar("PRAGMA foreign_keys"));
        Assert.Equal("delete",f.Scalar("PRAGMA journal_mode"));
        SchemaInitializer.Initialize(f.Database);
        Assert.Equal(1L,f.Scalar("SELECT COUNT(*) FROM ChangeRequest"));
        Assert.NotNull(f.Scalar("SELECT sqlite_version()"));
    }
    [Fact]
    public void LedgerScopeAndReplayAreAtomic()
    {
        using var f=new SqliteFixture();var services=new ServiceCollection().BuildServiceProvider();
        var executor=new MutationExecutor(f.Database,services);
        var actor=new ActorContext(SqliteFixture.User("alice"),Roles.Requester,1);
        var request=new MutationRequest("Create",null,null,"same",new("x",null,null));
        var calls=0; var id=Guid.NewGuid().ToString();
        OperationResult Write(Microsoft.Data.Sqlite.SqliteConnection c,Microsoft.Data.Sqlite.SqliteTransaction tx) {
            calls++;using var cmd=c.CreateCommand();cmd.Transaction=tx;
            cmd.CommandText="INSERT INTO ChangeRequest(Id,Title,RequesterId,Status,Version) VALUES($id,'x',$owner,'Draft',1)";
            cmd.Parameters.AddWithValue("$id",id);cmd.Parameters.AddWithValue("$owner",actor.UserId);cmd.ExecuteNonQuery();
            return OperationResult.Receipt(201,id,"Created");
        }
        var first=executor.Execute(actor,request,Write);var replay=executor.Execute(actor,request,Write);
        Assert.Equal(first,replay);Assert.Equal(1,calls);Assert.Equal(1L,f.Scalar("SELECT COUNT(*) FROM ChangeRequest"));
        Assert.Equal(1L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
        Assert.Equal(409,executor.Execute(actor,request with{Content=new("different",null,null)},Write).Status);
    }
    [Theory]
    [InlineData(MutationPoint.AfterMutation)]
    [InlineData(MutationPoint.AfterLedger)]
    public void FailureRollsBackEffectAndLedger(MutationPoint point)
    {
        using var f=new SqliteFixture();var id=f.Seed();
        f.Gates.Callback=(p,_)=>{if(p==point)throw new IOException("injected");};
        var services=new ServiceCollection().AddSingleton<IMutationObserver>(f.Gates).BuildServiceProvider();
        var executor=new MutationExecutor(f.Database,services);
        var result=executor.Execute(new(SqliteFixture.User("alice"),Roles.Requester,1),new("ReplaceContent",id,1,"failure",null),(c,tx)=>{
            using var cmd=c.CreateCommand();cmd.Transaction=tx;cmd.CommandText="UPDATE ChangeRequest SET Title='changed',Version=2 WHERE Id=$id";cmd.Parameters.AddWithValue("$id",id);cmd.ExecuteNonQuery();
            return OperationResult.Receipt(200,id,"Updated");
        });
        Assert.Equal(503,result.Status);Assert.Equal("original",f.Scalar("SELECT Title FROM ChangeRequest"));Assert.Equal(1L,f.Scalar("SELECT Version FROM ChangeRequest"));
        Assert.Equal(0L,f.Scalar("SELECT COUNT(*) FROM OperationResult"));
    }
    [Fact]
    public void NormalHostHasNoFaultObserver()
    {
        using var f=new SqliteFixture();using var normal=new ChangeFlowFactory(f.Database.Path);
        _=normal.CreateClient();Assert.Null(normal.Services.GetService<IMutationObserver>());
    }
    [Fact]
    public void MaintenanceSeedsOnceAndRevocationSurvivesStartup()
    {
        using var f=new SqliteFixture();
        var directory=System.IO.Path.GetDirectoryName(f.Database.Path)!;
        var configuration=new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string,string?>{["ChangeFlow:DatabasePath"]=System.IO.Path.Combine(directory,"maintenance.db")}).Build();
        var database=new SqliteConnectionFactory(configuration);SchemaInitializer.Initialize(database);
        var output=System.IO.Path.Combine(directory,"tokens.json");
        var env=f.Host.Services.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>();
        var seed=new[]{"--seed-dev-identities","--token-output",output};
        Assert.True(ChangeFlow.Api.Maintenance.DevelopmentIdentityCommands.Run(seed,env,database));
        var tokens=System.Text.Json.JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(output))!;
        Assert.Equal(4,tokens.Count);Assert.All(tokens.Values,t=>Assert.Equal(64,t.Length));
        Assert.True(ChangeFlow.Api.Maintenance.DevelopmentIdentityCommands.Run(new[]{"--revoke-role","bob","Reviewer"},env,database));
        Assert.Throws<InvalidOperationException>(()=>ChangeFlow.Api.Maintenance.DevelopmentIdentityCommands.Run(seed,env,database));
        SchemaInitializer.Initialize(database);
        using var c=database.Open();using var cmd=c.CreateCommand();cmd.CommandText="SELECT RolesMask FROM User WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id",SqliteFixture.User("bob"));Assert.Equal(0L,cmd.ExecuteScalar());
        Assert.Equal(tokens,System.Text.Json.JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(output))!);
    }
}
