using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using ChangeFlow.Core;
using ChangeFlow.Api.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
namespace ChangeFlow.Api.Authentication;
public sealed class OpaqueBearerHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,ILoggerFactory logger,UrlEncoder encoder,SqliteConnectionFactory factory)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options,logger,encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header=Request.Headers.Authorization.ToString();
        if(!header.StartsWith("Bearer ",StringComparison.OrdinalIgnoreCase)||header.Length>512)
            return Task.FromResult(AuthenticateResult.NoResult());
        var token=header[7..];
        using var c=factory.Open(); using var cmd=c.CreateCommand();
        cmd.CommandText="SELECT u.Id,u.RolesMask,u.AuthorizationVersion FROM User u JOIN AccessToken t ON t.UserId=u.Id WHERE t.TokenHash=$h AND t.Enabled=1 AND u.Enabled=1";
        cmd.Parameters.AddWithValue("$h",SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        using var reader=cmd.ExecuteReader();
        if(!reader.Read())return Task.FromResult(AuthenticateResult.Fail("Invalid credential."));
        var actor=new ActorContext(reader.GetString(0),(Roles)reader.GetInt32(1),reader.GetInt64(2));
        Context.Items[typeof(ActorContext)]=actor;
        var principal=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,actor.UserId)},Scheme.Name));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal,Scheme.Name)));
    }
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers.WWWAuthenticate="Bearer";
        return ChangeFlow.Api.Endpoints.HttpContract.Write(Context,OperationResult.Error(401,"UNAUTHORIZED"));
    }
}
