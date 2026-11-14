using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ChangeFlow.Api.Maintenance;
using ChangeFlow.Api.Persistence;
using ChangeFlow.Core;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
namespace ChangeFlow.Tests.Fixtures;
public sealed class SqliteFixture:IDisposable
{
    private readonly string root=Path.Combine(Path.GetTempPath(),"ChangeFlow-tests",Guid.NewGuid().ToString("N"));
    public SqliteConnectionFactory Database {get;}
    public ChangeFlowFactory Host {get;}
    public HttpClient Client {get;}
    public OperationGates Gates {get;}=new();
    public Dictionary<string,string> Tokens {get;}=new();
    public static string User(string name)=>DevelopmentIdentityCommands.SeedIds[name];
    public SqliteFixture()
    {
        Directory.CreateDirectory(root);
        Database=new(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> {
            ["ChangeFlow:DatabasePath"]=Path.Combine(root,"test.db")
        }).Build());
        SchemaInitializer.Initialize(Database);
        foreach(var (name,roles) in new[]{("alice",1),("bob",2),("carol",3),("dave",1)}) {
            var token=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));Tokens[name]=token;
            using var c=Database.Open();using var cmd=c.CreateCommand();
            cmd.CommandText="INSERT INTO User VALUES($id,1,$roles,1); INSERT INTO AccessToken VALUES($hash,$id,1)";
            cmd.Parameters.AddWithValue("$id",User(name));cmd.Parameters.AddWithValue("$roles",roles);
            cmd.Parameters.AddWithValue("$hash",SHA256.HashData(Encoding.UTF8.GetBytes(token)));cmd.ExecuteNonQuery();
        }
        Host=new(Database.Path,Gates);Client=Host.CreateClient(new(){BaseAddress=new Uri("https://localhost"),AllowAutoRedirect=false});
    }
    public string Seed(string status="Draft",string owner="alice",long version=1,string title="original",string? description="old description",string? reason="old reason")
    {
        var id=Guid.NewGuid().ToString();using var c=Database.Open();using var cmd=c.CreateCommand();
        cmd.CommandText="INSERT INTO ChangeRequest VALUES($id,$title,$desc,$reason,$owner,$status,$version,$baseline,$bd,$br)";
        cmd.Parameters.AddWithValue("$id",id);cmd.Parameters.AddWithValue("$title",title);
        cmd.Parameters.AddWithValue("$desc",(object?)description??DBNull.Value);cmd.Parameters.AddWithValue("$reason",(object?)reason??DBNull.Value);
        cmd.Parameters.AddWithValue("$owner",User(owner));cmd.Parameters.AddWithValue("$status",status);cmd.Parameters.AddWithValue("$version",version);
        cmd.Parameters.AddWithValue("$baseline",status=="ChangesRequested"?title:DBNull.Value);
        cmd.Parameters.AddWithValue("$bd",status=="ChangesRequested"?(object?)description??DBNull.Value:DBNull.Value);
        cmd.Parameters.AddWithValue("$br",status=="ChangesRequested"?(object?)reason??DBNull.Value:DBNull.Value);
        cmd.ExecuteNonQuery();return id;
    }
    public void Revoke(string user,Roles role)
    {
        using var c=Database.Open();using var cmd=c.CreateCommand();
        cmd.CommandText="UPDATE User SET RolesMask=RolesMask & ~$role,AuthorizationVersion=AuthorizationVersion+1 WHERE Id=$id";
        cmd.Parameters.AddWithValue("$role",(int)role);cmd.Parameters.AddWithValue("$id",User(user));cmd.ExecuteNonQuery();
    }
    public object? Scalar(string sql)
    {
        using var c=Database.Open();using var cmd=c.CreateCommand();cmd.CommandText=sql;return cmd.ExecuteScalar();
    }
    public async Task<HttpResponseMessage> Send(string method,string path,string? json=null,string user="alice",string? key=null,long? version=null)
    {
        using var r=new HttpRequestMessage(new HttpMethod(method),path);
        r.Headers.Authorization=new AuthenticationHeaderValue("Bearer",Tokens.GetValueOrDefault(user,user));
        if(key is not null)r.Headers.Add("Idempotency-Key",key);
        if(version.HasValue)r.Headers.TryAddWithoutValidation("If-Match",$"\"{version}\"");
        if(json is not null)r.Content=new StringContent(json,Encoding.UTF8,"application/json");
        return await Client.SendAsync(r);
    }
    public static async Task<JsonElement> Json(HttpResponseMessage r)=>JsonDocument.Parse(await r.Content.ReadAsStringAsync()).RootElement.Clone();
    public void Dispose()
    {
        Client.Dispose();Host.Dispose();SqliteConnection.ClearAllPools();
        foreach(var f in Directory.GetFiles(root))File.Delete(f);
        Directory.Delete(root);
    }
}
