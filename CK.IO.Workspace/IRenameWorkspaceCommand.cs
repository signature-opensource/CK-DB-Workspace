using CK.Auth;
using CK.Cris;

namespace CK.IO.Workspace;

/// <summary>
/// Renames the Workspace.
/// Backed by <c>CK.sGroupGroupNameSet</c> on the workspace's Zone group: name uniqueness
/// is enforced and the actual name (possibly suffixed " (n)") is returned in the result.
/// </summary>
public interface IRenameWorkspaceCommand : ICommand<IRenameWorkspaceCommandResult>, ICommandWorkspace, ICommandCurrentCulture, ICommandAuthNormal
{
    public int WorkspaceId { get; set; }
    /// <summary>
    /// The new workspace name.
    /// </summary>
    public string WorkspaceName { get; set; }
}

/// <summary>
/// Result of <see cref="IRenameWorkspaceCommand"/>.
/// </summary>
public interface IRenameWorkspaceCommandResult : IStandardResultPart
{
    /// <summary>
    /// The actual workspace name set (may have been suffixed " (n)" on clash).
    /// </summary>
    public string WorkspaceName { get; set; }
}
