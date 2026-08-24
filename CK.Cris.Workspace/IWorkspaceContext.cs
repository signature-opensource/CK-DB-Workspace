using CK.Core;

namespace CK.Cris.Workspace;

/// <summary>
/// Scoped ambient service exposing the current <see cref="CurrentWorkspaceId"/>
/// resolved from the <c>IWorkspacePart</c> of the executing command.
/// <para>
/// Handlers can inject this instead of taking <c>IWorkspacePart</c> as a parameter.
/// The value is overridden by <c>CrisWorkspaceService.ConfigureWorkspaceContext</c>
/// from the incoming command's part value.
/// </para>
/// <para>
/// This is an ambient service (like <c>ExtendedCultureInfo</c>): its default value is
/// provided by <see cref="WorkspaceContextAmbientServiceDefault"/>.
/// </para>
/// </summary>
public interface IWorkspaceContext : IAmbientAutoService
{
    /// <summary>
    /// The current workspace identifier from the incoming command's ambient value.
    /// 0 when no workspace is active (e.g. <see cref="IAmbientValuesCollectCommand"/>
    /// before the client has switched).
    /// </summary>
    int CurrentWorkspaceId { get; }
}
