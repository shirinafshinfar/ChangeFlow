using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ChangeFlow.Api.Persistence;
using ChangeFlow.Core;
namespace ChangeFlow.Api.Maintenance;
public static class DevelopmentIdentityCommands
{
    public static readonly IReadOnlyDictionary<string,string> SeedIds = new Dictionary<string,string> {
        ["alice"]="00000000-0000-4000-8000-000000000001", ["bob"]="00000000-0000-4000-8000-000000000002",
        ["carol"]="00000000-0000-4000-8000-000000000003", ["dave"]="00000000-0000-4000-8000-000000000004"
    };
    public static bool Run(string[] args,IHostEnvironment environment,SqliteConnectionFactory factory)
    {
        var seed=args.Contains("--seed-dev-identities"); var revoke=Array.IndexOf(args,"--revoke-role");
        if(!seed&&revoke<0)return false;
        if(!environment.IsDevelopment())throw new InvalidOperationException("Maintenance requires Development.");
        using var c=factory.Open(); using var tx=c.BeginTransaction(System.Data.IsolationLevel.Serializable,deferred:false);
        if(seed) {
            var outputIndex=Array.IndexOf(args,"--token-output");
            if(outputIndex<0||outputIndex+1>=args.Length)throw new InvalidOperationException("Token output path required.");
            var output=Path.GetFullPath(args[outputIndex+1]);
            if(File.Exists(output))throw new InvalidOperationException("Token output already exists.");
            using(var exists=c.CreateCommand()) {exists.Transaction=tx;exists.CommandText="SELECT COUNT(*) FROM User";if(Convert.ToInt64(exists.ExecuteScalar())!=0)throw new InvalidOperationException("Identities already exist.");}
            var tokens=new Dictionary<string,string>();
            foreach(var (name,roles) in new[]{("alice",Roles.Requester),("bob",Roles.Reviewer),("carol",Roles.Requester|Roles.Reviewer),("dave",Roles.Requester)}) {
                var id=SeedIds[name]; var token=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
                using var cmd=c.CreateCommand();cmd.Transaction=tx;cmd.CommandText="INSERT INTO User VALUES($id,1,$r,1); INSERT INTO AccessToken VALUES($h,$id,1)";
                cmd.Parameters.AddWithValue("$id",id);cmd.Parameters.AddWithValue("$r",(int)roles);cmd.Parameters.AddWithValue("$h",SHA256.HashData(Encoding.UTF8.GetBytes(token)));cmd.ExecuteNonQuery();
                tokens[name]=token;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            using(var emptyFile=new FileStream(output,FileMode.CreateNew,FileAccess.Write,FileShare.None)) { }
            try {
                if(OperatingSystem.IsWindows()) {
                    var owner=System.Security.Principal.WindowsIdentity.GetCurrent().User!;
                    var security=new System.Security.AccessControl.FileSecurity();
                    security.SetAccessRuleProtection(true,false);
                    security.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(owner,System.Security.AccessControl.FileSystemRights.FullControl,System.Security.AccessControl.AccessControlType.Allow));
                    new FileInfo(output).SetAccessControl(security);
                } else File.SetUnixFileMode(output,UnixFileMode.UserRead|UnixFileMode.UserWrite);
                using var file=new FileStream(output,FileMode.Open,FileAccess.Write,FileShare.None);
                JsonSerializer.Serialize(file,tokens);
            } catch { File.Delete(output); throw; }
            tx.Commit();
        } else {
            if(revoke+2>=args.Length||!Enum.TryParse<Roles>(args[revoke+2],out var role)||role is not (Roles.Requester or Roles.Reviewer))
                throw new InvalidOperationException("Usage: --revoke-role user-id-or-seed-name Requester|Reviewer");
            var id=SeedIds.TryGetValue(args[revoke+1],out var seedId)?seedId:args[revoke+1];
            using var cmd=c.CreateCommand();cmd.Transaction=tx;
            cmd.CommandText="UPDATE User SET RolesMask=RolesMask & ~$role,AuthorizationVersion=AuthorizationVersion+1 WHERE Id=$id";
            cmd.Parameters.AddWithValue("$role",(int)role);cmd.Parameters.AddWithValue("$id",id);
            if(cmd.ExecuteNonQuery()!=1)throw new InvalidOperationException("Identity not found; use the server user id.");
            tx.Commit();
        }
        return true;
    }
}
