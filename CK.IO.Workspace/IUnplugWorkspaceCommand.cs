using CK.Auth;
using CK.Cris;

namespace CK.IO.Workspace;

/// <summary>
/// Unplugs the Workspace from its Zone. The Zone is preserved.
/// </summary>
public interface IUnplugWorkspaceCommand : ICommand<ICrisBasicCommandResult>, ICommandWorkspace, ICommandCurrentCulture, ICommandAuthNormal
{
    public int WorkspaceId { get; set; }
}
