using CK.Auth;
using CK.Cris;

namespace CK.IO.Workspace;

/// <summary>
/// Unplugs the Workspace identified by <see cref="ITargetWorkspacePart.TargetWorkspaceId"/>
/// from its Zone. The Zone is preserved.
/// </summary>
public interface IUnplugWorkspaceCommand : ICommand<ICrisBasicCommandResult>, ICommandWorkspace, ITargetWorkspacePart, ICommandCurrentCulture, ICommandAuthNormal
{
}
