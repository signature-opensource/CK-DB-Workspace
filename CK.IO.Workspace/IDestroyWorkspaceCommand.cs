using CK.Auth;
using CK.Cris;

namespace CK.IO.Workspace;

/// <summary>
/// Destroys a Workspace identified by <see cref="ITargetWorkspacePart.TargetWorkspaceId"/>.
/// The caller must be operating from a workspace where they have admin rights
/// (ambient <c>WorkspaceId</c>).
/// </summary>
public interface IDestroyWorkspaceCommand : ICommand<ICrisBasicCommandResult>, ICommandWorkspace, ITargetWorkspacePart, ICommandCurrentCulture, ICommandAuthNormal
{
    /// <summary>
    /// True to destroy the Zone even when it contains Users or Groups (Groups are destroyed).
    /// </summary>
    public bool ForceDestroy { get; set; }
}
