using ChangeFlow.Api.Application;
using ChangeFlow.Api.Persistence;
using ChangeFlow.Core;
namespace ChangeFlow.Api.Endpoints;
public static class DeleteEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/change-requests/{id}",async(HttpContext ctx,string id,DeleteDraft command)=>{
            await HttpContract.Read(ctx.Request,false);
            var actor=HttpContract.Actor(ctx,Roles.Requester);var key=HttpContract.Key(ctx.Request);var version=HttpContract.Version(ctx.Request);
            await HttpContract.Write(ctx,command.Execute(actor,new("Delete",ContentEndpoints.Id(id),version,key,null)));
        });
    }
}
