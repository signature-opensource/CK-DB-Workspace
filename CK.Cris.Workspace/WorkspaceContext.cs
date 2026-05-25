namespace CK.Cris.Workspace;

/// <summary>
/// Default implementation of <see cref="IWorkspaceContext"/>. Initialized with
/// <c>CurrentWorkspaceId == 0</c> and overridden per-command via
/// <c>AmbientServiceHub.Override</c> in <c>CrisWorkspaceService</c>.
/// </summary>
public sealed class WorkspaceContext : IWorkspaceContext
{
    /// <summary>
    /// Creates a context with the given <see cref="CurrentWorkspaceId"/>.
    /// </summary>
    /// <param name="workspaceId">The current workspace identifier.</param>
    public WorkspaceContext( int workspaceId = 0 )
    {
        CurrentWorkspaceId = workspaceId;
    }

    /// <inheritdoc />
    public int CurrentWorkspaceId { get; }
}
