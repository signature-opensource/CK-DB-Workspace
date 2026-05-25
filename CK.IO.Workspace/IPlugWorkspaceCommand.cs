using CK.Auth;
using CK.Cris;

namespace CK.IO.Workspace;

/// <summary>
/// Plugs a workspace to an existing zone. Creates the workspace's Administrators group.
/// Idempotent: succeeds without effect when a workspace is already plugged to the zone.
/// </summary>
public interface IPlugWorkspaceCommand : ICommand<ICrisBasicCommandResult>, ICommandCurrentCulture, ICommandAuthNormal
{
    /// <summary>
    /// The identifier of the zone to plug.
    /// </summary>
    public int ZoneId { get; set; }
}
