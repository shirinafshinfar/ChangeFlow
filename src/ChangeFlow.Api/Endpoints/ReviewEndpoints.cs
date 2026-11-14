using ChangeFlow.Api.Application;
using ChangeFlow.Api.Persistence;
using ChangeFlow.Core;
namespace ChangeFlow.Api.Endpoints;
public static class ReviewEndpoints
{
    public static void Map(WebApplication app)
    {
        foreach(var (route,operation) in new[]{("approve","Approve"),("request-changes","RequestChanges")})
            app.MapPost("/api/change-requests/{id}/"+route,async(HttpContext ctx,string id,ReviewCommands command)=>{
                await HttpContract.Read(ctx.Request,false);
                var actor=HttpContract.Actor(ctx,Roles.Reviewer);var key=HttpContract.Key(ctx.Request);var version=HttpContract.Version(ctx.Request);
                await HttpContract.Write(ctx,command.Execute(actor,new(operation,ContentEndpoints.Id(id),version,key,null)));
            });
    }
}
