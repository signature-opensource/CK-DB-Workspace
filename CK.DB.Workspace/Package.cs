using CK.Core;
using CK.SqlServer;
using System.Threading.Tasks;

namespace CK.DB.Workspace
{
    [SqlPackage( Schema = "CK", ResourcePath = "Res", FullName = "CK.DB.Workspace", ResourceType = typeof( Package ) )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "transform:CK.sUserCreate, transform:vUser" )]
    public abstract class Package : SqlPackage
    {
        void StObjConstruct(
            CK.DB.Acl.Package acl,
            CK.DB.Group.SimpleNaming.Package groupNaming,
            CK.DB.Zone.Package zone )
        {
        }

        /// <summary>
        /// Tries to create a new user. If the user name is not unique, returns -1.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userName">The user name (when not unique, a " (n)" suffix is automaticaaly added).</param>
        /// <returns>The user identifier.</returns>
        [SqlProcedure( "transform:sUserCreate" )]
        public abstract Task<int> CreateUserAsync( ISqlCallContext ctx,
                                                   int actorId,
                                                   string userName,
                                                   int preferredWorkspaceId );

        /// <summary>
        /// Attributes a preferred workspace to the use.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="workspaceId">The workspace identifier.</param>
        [SqlProcedure( "sUserPreferredWorkspaceIdSet" )]
        public abstract Task SetUserPreferredWorkspaceAsync( ISqlCallContext ctx, int actorId, int userId, int workspaceId );
    }
}
