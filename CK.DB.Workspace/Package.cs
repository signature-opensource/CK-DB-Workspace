using CK.Core;
using CK.SqlServer;
using System.Threading.Tasks;

namespace CK.DB.Workspace
{
    /// <summary>
    /// Workspace package handles user's PreferredWorkspaceId colum.
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res", ResourceType = typeof( Package ) )]
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
        /// Gets the workspace table that handle workspace creation and destruction.
        /// </summary>
        [InjectObject]
        public WorkspaceTable WorkspaceTable { get; private set; }

        /// <summary>
        /// Tries to create a new user with a preferred workspace. If the user name is not unique, returns -1.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userName">The user name (when not unique, a " (n)" suffix is automaticaaly added).</param>
        /// <param name="preferredWorkspaceId">The user's preferred workspace identifier.</param>
        /// <returns>The user identifier, or -1 if the user name is not unique.</returns>
        [SqlProcedure( "transform:sUserCreate" )]
        public abstract Task<int> CreateUserAsync( ISqlCallContext ctx,
                                                   int actorId,
                                                   string userName,
                                                   int preferredWorkspaceId );

        /// <summary>
        /// Sets the preferred workspace of a user.
        /// </summary>
        /// <param name="ctx">The call context.</param>
        /// <param name="actorId">The acting actor identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="workspaceId">The workspace identifier.</param>
        [SqlProcedure( "sUserPreferredWorkspaceIdSet" )]
        public abstract Task SetUserPreferredWorkspaceAsync( ISqlCallContext ctx, int actorId, int userId, int workspaceId );

        /// <inheritdoc cref="CreateUserAsync"/>
        [SqlProcedure( "transform:sUserCreate" )]
        public abstract int CreateUser( ISqlCallContext ctx,
                                        int actorId,
                                        string userName,
                                        int preferredWorkspaceId );

        /// <inheritdoc cref="SetUserPreferredWorkspaceAsync"/>
        [SqlProcedure( "sUserPreferredWorkspaceIdSet" )]
        public abstract void SetUserPreferredWorkspace( ISqlCallContext ctx, int actorId, int userId, int workspaceId );

    }
}
