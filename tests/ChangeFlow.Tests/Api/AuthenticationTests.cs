using ChangeFlow.Api.Persistence;
using ChangeFlow.Core;
using ChangeFlow.Tests.Fixtures;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace ChangeFlow.Tests.Api;
public sealed class AuthenticationTests
{
    [Fact]
    public async Task RealHandlerUsesCurrentRolesAndImmutableSnapshot()
    {
        using var f=new SqliteFixture();
        async Task<(AuthenticateResult,ActorContext?)> Auth(string token) {
            using var scope=f.Host.Services.CreateScope();
            var ctx=new DefaultHttpContext {RequestServices=scope.ServiceProvider};
            ctx.Request.Headers.Authorization="Bearer "+token;
            var r=await ctx.AuthenticateAsync("Bearer");
            return (r,ctx.Items[typeof(ActorContext)] as ActorContext);
        }
        var before=await Auth(f.Tokens["alice"]);Assert.True(before.Item1.Succeeded);Assert.True(before.Item2!.Has(Roles.Requester));
        Assert.False((await Auth("invalid")).Item1.Succeeded);
        f.Revoke("alice",Roles.Requester);
        var after=await Auth(f.Tokens["alice"]);Assert.True(after.Item1.Succeeded);Assert.False(after.Item2!.Has(Roles.Requester));
        Assert.True(before.Item2.Has(Roles.Requester));
    }
    [Fact]
    public async Task DatabaseFailureIsNotInvalidCredential()
    {
        using var f=new SqliteFixture();using var scope=f.Host.Services.CreateScope();
        // An exclusive schema lock is a real provider failure at the authentication query.
        using var c=f.Database.Open();using var locked=c.CreateCommand();locked.CommandText="BEGIN EXCLUSIVE";locked.ExecuteNonQuery();
        var ctx=new DefaultHttpContext{RequestServices=scope.ServiceProvider};ctx.Request.Headers.Authorization="Bearer "+f.Tokens["alice"];
        await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(()=>ctx.AuthenticateAsync("Bearer"));
        using var rollback=c.CreateCommand();rollback.CommandText="ROLLBACK";rollback.ExecuteNonQuery();
    }
}
