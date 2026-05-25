using CK.Core;
using CK.Cris;
using CK.IO.Workspace;
using CK.SqlServer;
using System.Threading.Tasks;

namespace CK.DB.Workspace;

public abstract partial class WorkspaceTable : SqlTable
{
    /// <summary>
    /// Creates a new Workspace.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="command">The incoming <see cref="ICreateWorkspaceCommand"/>.</param>
    /// <returns>
    /// A <see cref="ICreateWorkspaceCommandResult"/>.
    /// <para>Note: the executed command result is a <see cref="ICrisResultError"/> when the stored procedure throws.</para>
    /// </returns>
    [CommandHandler]
    [SqlProcedure( "sWorkspaceCreate" )]
    public abstract Task<ICreateWorkspaceCommandResult> CreateWorkspaceAsync( ISqlCallContext ctx, [ParameterSource] ICreateWorkspaceCommand command );

    /// <summary>
    /// Destroys a Workspace.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="command">The incoming <see cref="IDestroyWorkspaceCommand"/>.</param>
    [CommandHandler]
    [SqlProcedure( "sWorkspaceDestroy" )]
    public abstract Task<ICrisBasicCommandResult> DestroyWorkspaceAsync( ISqlCallContext ctx, [ParameterSource] IDestroyWorkspaceCommand command );

    /// <summary>
    /// Plugs a workspace to an existing zone.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="command">The incoming <see cref="IPlugWorkspaceCommand"/>.</param>
    [CommandHandler]
    [SqlProcedure( "sWorkspacePlug" )]
    public abstract Task<ICrisBasicCommandResult> PlugWorkspaceAsync( ISqlCallContext ctx, [ParameterSource] IPlugWorkspaceCommand command );

    /// <summary>
    /// Unplugs a Workspace (the Zone is preserved).
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="command">The incoming <see cref="IUnplugWorkspaceCommand"/>.</param>
    [CommandHandler]
    [SqlProcedure( "sWorkspaceUnplug" )]
    public abstract Task<ICrisBasicCommandResult> UnplugWorkspaceAsync( ISqlCallContext ctx, [ParameterSource] IUnplugWorkspaceCommand command );

    /// <summary>
    /// Renames a Workspace. Delegates to <see cref="Group.SimpleNaming.Package.GroupRenameAsync"/> on the
    /// workspace's Zone group: the property names on the command don't match the SP parameters
    /// (<c>WorkspaceId</c> vs <c>@GroupId</c>), so this handler is implemented manually.
    /// </summary>
    /// <param name="ctx">The call context.</param>
    /// <param name="command">The incoming <see cref="IRenameWorkspaceCommand"/>.</param>
    /// <param name="pocoDirectory">The Poco directory used to create the result.</param>
    /// <param name="groupNaming">The Group.SimpleNaming package that exposes the rename SP.</param>
    [CommandHandler]
    public async Task<IRenameWorkspaceCommandResult> RenameWorkspaceAsync(
        ISqlCallContext ctx,
        IRenameWorkspaceCommand command,
        PocoDirectory pocoDirectory,
        Group.SimpleNaming.Package groupNaming )
    {
        // ICommandAuthNormal guarantees a non-null authenticated ActorId.
        var actualName = await groupNaming.GroupRenameAsync( ctx, command.ActorId!.Value, command.TargetWorkspaceId, command.WorkspaceName );
        var result = pocoDirectory.Create<IRenameWorkspaceCommandResult>();
        result.WorkspaceName = actualName;
        return result;
    }
}
