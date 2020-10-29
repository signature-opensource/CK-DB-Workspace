using CK.Core;
using CK.SqlServer;
using System.Threading.Tasks;

namespace CK.DB.Workspace
{
    [SqlTable( "tWorkspace", Package = typeof( Package ), ResourcePath = "Res" )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "vWorkspace" )]
    public abstract class WorkspaceTable : SqlTable
    {
        void StObjConstruct( CK.DB.Zone.ZoneTable zoneTable )
        {
        }

        /// <summary>
        /// Captures the result of the creation of a workspace.
        /// </summary>
        public readonly struct NamedWorkspace
        {
            /// <summary>
            /// Initializes a new <see cref="NamedWorkspace"/>.
            /// </summary>
            /// <param name="workspaceIdResult">The identifier.</param>
            /// <param name="workspaceName">The name.</param>
            public NamedWorkspace( int workspaceIdResult, string workspaceName )
            {
                WorkspaceId = workspaceIdResult;
                Name = workspaceName;
            }

            /// <summary>
            /// Gets the workspace identifier.
            /// </summary>
            public int WorkspaceId { get; }

            /// <summary>
            /// Gets the workspace's name.
            /// </summary>
            public string Name { get; }
        }

        /// <summary>
        /// Creates a new Workspace.
        /// This is possible only for global Administrators.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting user.</param>
        /// <param name="workspaceName">The name of the workspace to create.</param>
        /// <returns>The name and identifier of the new workspace and the default channel identifier.</returns>
        [SqlProcedure( "sWorkspaceCreate" )]
        public abstract NamedWorkspace CreateWorkspace( ISqlCallContext ctx, int actorId, string workspaceName );

        /// <inheritdoc cref="CreateWorkspace(ISqlCallContext, int, string)"/>.
        [SqlProcedure( "sWorkspaceCreate" )]
        public abstract Task<NamedWorkspace> CreateWorkspaceAsync( ISqlCallContext ctx, int actorId, string workspaceName );



        /// <summary>
        /// Destroy the Workspace.
        /// This is possible only for workspace Administrators.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The acting user.</param>
        /// <param name="workspaceId">The workspace identifier.</param>
        [SqlProcedure( "sWorkspaceDestroy" )]
        public abstract void DestroyWorkspace( ISqlCallContext ctx, int actorId, int workspaceId );

        [SqlProcedure( "sWorkspaceDestroy" )]
        public abstract Task DestroyWorkspaceAsync( ISqlCallContext ctx, int actorId, int workspaceId );
    }
}
