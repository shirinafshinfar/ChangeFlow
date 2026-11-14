using System.Text.Json;
using ChangeFlow.Api.Application;
using ChangeFlow.Api.Persistence;
using ChangeFlow.Core;
namespace ChangeFlow.Api.Endpoints;
public static class ReadEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/change-requests",async(HttpContext ctx,ListRequests query)=>{
            var actor=HttpContract.Actor(ctx,Roles.Requester|Roles.Reviewer,either:true);
            var limit=50;
            if(ctx.Request.Query.TryGetValue("limit",out var values)&&
                (values.Count!=1||!int.TryParse(values[0],System.Globalization.NumberStyles.None,System.Globalization.CultureInfo.InvariantCulture,out limit)||limit is <1 or >100))
                throw new InputFailure(400,"INVALID_LIMIT");
            string? cursor=null;
            if(ctx.Request.Query.TryGetValue("cursor",out var cursors)){
                if(cursors.Count!=1)throw new InputFailure(400,"INVALID_CURSOR");
                cursor=cursors[0];
            }
            var page=query.Execute(actor,limit,cursor);
            await HttpContract.Write(ctx,new(200,JsonSerializer.Serialize(page,new JsonSerializerOptions(JsonSerializerDefaults.Web)),"List"));
        });
        app.MapGet("/api/change-requests/{id}",async (HttpContext ctx,string id,ReadRequest query)=>{
            var actor=HttpContract.Actor(ctx,Roles.Requester|Roles.Reviewer,either:true);
            var view=query.Execute(actor,ContentEndpoints.Id(id));
            if(view is null){await HttpContract.Write(ctx,OperationResult.Error(404,"NOT_FOUND"));return;}
            ctx.Response.Headers.ETag=$"\"{view.Version}\"";
            await HttpContract.Write(ctx,new(200,JsonSerializer.Serialize(view,new JsonSerializerOptions(JsonSerializerDefaults.Web)),"Read"));
        });
    }
}
