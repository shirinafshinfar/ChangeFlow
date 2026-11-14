using ChangeFlow.Api.Application;
using ChangeFlow.Api.Persistence;
using ChangeFlow.Core;
namespace ChangeFlow.Api.Endpoints;
public static class SubmitEndpoints
{
    public static void Map(WebApplication app)=>app.MapPost("/api/change-requests/{id}/submit",async(HttpContext ctx,string id,SubmitCommand command)=>{
        await HttpContract.Read(ctx.Request,false);
        var actor=HttpContract.Actor(ctx,Roles.Requester);var key=HttpContract.Key(ctx.Request);var version=HttpContract.Version(ctx.Request);
        await HttpContract.Write(ctx,command.Execute(actor,new("Submit",ContentEndpoints.Id(id),version,key,null)));
    });
}
