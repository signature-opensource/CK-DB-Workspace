using CK.Auth;
using CK.Cris;

namespace CK.IO.UserProfile.Workspace;

public interface ISetPreferredWorkspaceIdCommand : ICommand<ISetPreferredWorkspaceIdCommandResult>, ICommandCurrentCulture, ICommandAuthNormal
{
    public int UserId { get; set; }
    public int WorkspaceId { get; set; }
}

public interface ISetPreferredWorkspaceIdCommandResult : IStandardResultPart
{
    public int PreferredWorkspaceId { get; set; }
}
