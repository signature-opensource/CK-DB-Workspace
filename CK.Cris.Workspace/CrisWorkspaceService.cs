using CK.Core;
using CK.Cris.AmbientValues;
using CK.IO.Workspace;

namespace CK.Cris.Workspace;

/// <summary>
/// Handles the <see cref="IWorkspacePart"/> ambient value: configures the scoped
/// <see cref="IWorkspaceContext"/> from the incoming command, and fills the
/// <see cref="IWorkspaceAmbientValues"/> on <see cref="IAmbientValuesCollectCommand"/>.
/// <para>
/// Access control is deferred to the SQL layer: stored procedures enforce per-workspace
/// authorization via <c>CK.fAclGrantLevel</c>. This service does <em>not</em> perform
/// membership or ACL validation — platform administrators can act on workspaces they
/// are not members of, and the SP layer is the single source of truth for access checks.
/// </para>
/// </summary>
public class CrisWorkspaceService : IAutoService
{
    /// <summary>
    /// Overrides the scoped <see cref="IWorkspaceContext"/> with the value carried
    /// by <see cref="IWorkspacePart"/> when set. Runs both on incoming command execution
    /// and on stored-command restoration.
    /// <para>
    /// When the part's <c>WorkspaceId</c> is null or 0 (e.g. <see cref="IAmbientValuesCollectCommand"/>
    /// before the client has switched), the default <see cref="WorkspaceContext"/> with
    /// <c>CurrentWorkspaceId == 0</c> is left in place.
    /// </para>
    /// </summary>
    [ConfigureAmbientServices]
    [RestoreAmbientServices]
    public virtual void ConfigureWorkspaceContext( IWorkspacePart part, AmbientServiceHub hub )
    {
        if( part.CurrentWorkspaceId.HasValue && part.CurrentWorkspaceId.Value > 0 )
        {
            hub.Override<IWorkspaceContext>( new WorkspaceContext( part.CurrentWorkspaceId.Value ) );
        }
    }

    /// <summary>
    /// Fills the <see cref="IWorkspaceAmbientValues.CurrentWorkspaceId"/> from the current
    /// <see cref="IWorkspaceContext"/> when an <see cref="IAmbientValuesCollectCommand"/>
    /// is handled.
    /// </summary>
    [CommandPostHandler]
    public virtual void GetWorkspaceAmbientValue( IAmbientValuesCollectCommand cmd,
                                                  IWorkspaceContext ctx,
                                                  IWorkspaceAmbientValues values )
    {
        values.CurrentWorkspaceId = ctx.CurrentWorkspaceId;
    }
}
