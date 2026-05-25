using CK.Auth;
using CK.Cris;

namespace CK.IO.Workspace;

/// <summary>
/// Creates a new Workspace. Restricted to Platform Administrators.
/// </summary>
public interface ICreateWorkspaceCommand : ICommand<ICreateWorkspaceCommandResult>, ICommandCurrentCulture, ICommandAuthNormal
{
    /// <summary>
    /// The name of the workspace to create. May be suffixed by " (n)" on the
    /// returned result if a clash occurred.
    /// </summary>
    public string WorkspaceName { get; set; }
}

/// <summary>
/// Result of <see cref="ICreateWorkspaceCommand"/>.
/// </summary>
public interface ICreateWorkspaceCommandResult : IStandardResultPart
{
    /// <summary>
    /// The new workspace identifier (matches the SQL output parameter <c>@WorkspaceIdResult</c>).
    /// </summary>
    public int WorkspaceIdResult { get; set; }

    /// <summary>
    /// The actual workspace name set (may have been suffixed " (n)" on clash).
    /// </summary>
    public string WorkspaceName { get; set; }
}
