using CK.Core;

namespace CK.Cris.Workspace;

/// <summary>
/// Provides the default <see cref="IWorkspaceContext"/> (<c>CurrentWorkspaceId == 0</c>)
/// for endpoints that don't resolve any ambient workspace.
/// <para>
/// This satisfies the <see cref="IWorkspaceContext"/> ambient service, mirroring how
/// <c>NormalizedCultureInfoAmbientServiceDefault</c> satisfies <c>ExtendedCultureInfo</c>.
/// </para>
/// </summary>
public sealed class WorkspaceContextAmbientServiceDefault : IAmbientServiceDefaultProvider<IWorkspaceContext>
{
    /// <summary>
    /// Gets a <see cref="WorkspaceContext"/> with <c>CurrentWorkspaceId == 0</c>.
    /// </summary>
    public IWorkspaceContext Default => new WorkspaceContext( 0 );
}
