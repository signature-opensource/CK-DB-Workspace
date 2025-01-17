using CK.Core;
using CK.SqlServer;
using System.Threading.Tasks;

namespace CK.DB.Workspace;

/// <summary>
/// Models CK.tWorkspace table. Handles workspace creation and destruction.
/// </summary>
[SqlTable( "tWorkspace", Package = typeof( Package ), ResourcePath = "Res" )]
[Versions( "1.0.0, 1.0.1, 1.0.2" )]
[SqlObjectItem( "vWorkspace" )]
public abstract class WorkspaceTable : SqlTable
{
    void StObjConstruct( CK.DB.Zone.ZoneTable zoneTable )
    {
    }

    /// <summary>
    /// Plug a workspace to an existing zone.
    /// Creates Zone group Administrator.
    /// <para>
    /// This is (by default) possible only for global Administrators (members of the Administrator group
    /// which has the special reserved identifer 2).
    /// </para>
    /// </summary>
    /// <param name="ctx">The call context to use.</param>
    /// <param name="actorId">The acting user.</param>
    /// <param name="zoneId">The zone to decorate.</param>
    [SqlProcedure( "sWorkspacePlug" )]
    public abstract void PlugWorkspace( ISqlCallContext ctx, int actorId, int zoneId );

    /// <inheritdoc cref="PlugWorkspace(ISqlCallContext, int, int)"/>
    [SqlProcedure( "sWorkspacePlug" )]
    public abstract Task PlugWorkspaceAsync( ISqlCallContext ctx, int actorId, int zoneId );

    /// <summary>
    /// Unplug the Workspace.
    /// This is possible only for workspace Administrators: <paramref name="actorId"/> must have Administrator level (127)
    /// on the workspace's acl.
    /// <para>
    /// The Zone will not be destroy.
    /// </para>
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="workspaceId">The workspace identifier.</param>
    [SqlProcedure( "sWorkspaceUnplug" )]
    public abstract void UnplugWorkspace( ISqlCallContext ctx, int actorId, int workspaceId );

    /// <inheritdoc cref="UnplugWorkspace(ISqlCallContext, int, int)"/>.
    [SqlProcedure( "sWorkspaceUnplug" )]
    public abstract Task UnplugWorkspaceAsync( ISqlCallContext ctx, int actorId, int workspaceId );

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
    /// This is (by default) possible only for global Administrators (members of the Administrator group
    /// which has the special reserved identifier 2).
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="workspaceName">The name of the workspace to create.</param>
    /// <returns>The name and identifier of the new workspace.</returns>
    [SqlProcedure( "sWorkspaceCreate" )]
    public abstract NamedWorkspace CreateWorkspace( ISqlCallContext ctx, int actorId, string workspaceName );

    /// <inheritdoc cref="CreateWorkspace"/>.
    [SqlProcedure( "sWorkspaceCreate" )]
    public abstract Task<NamedWorkspace> CreateWorkspaceAsync( ISqlCallContext ctx, int actorId, string workspaceName );

    /// <summary>
    /// Destroys a Workspace, optionally destroying its groups.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="actorId">The acting actor identifier.</param>
    /// <param name="workspaceId">The Workspace identifier to destroy.</param>
    /// <param name="forceDestroy">True to destroy the Zone even it is contains User or Groups (its Groups are destroyed).</param>
    [SqlProcedure( "sWorkspaceDestroy" )]
    public abstract void DestroyWorkspace( ISqlCallContext ctx, int actorId, int workspaceId, bool forceDestroy = false );

    /// <inheritdoc cref="DestroyWorkspace(ISqlCallContext, int, int, bool)"/>
    [SqlProcedure( "sWorkspaceDestroy" )]
    public abstract Task DestroyWorkspaceAsync( ISqlCallContext ctx, int actorId, int workspaceId, bool forceDestroy = false );
}
