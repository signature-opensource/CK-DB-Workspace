using CK.Core;
using CK.Cris;

namespace CK.IO.Workspace;

public class IncomingValidators : IRealObject
{
    [IncomingValidator]
    public virtual void ValidateCreateWorkspaceCommand( ICreateWorkspaceCommand cmd, UserMessageCollector collector )
    {
        if( string.IsNullOrWhiteSpace( cmd.WorkspaceName ) )
        {
            collector.Error( "WorkspaceName cannot be null, empty or whitespace.", "Workspace.InvalidWorkspaceName" );
        }
    }

    [IncomingValidator]
    public virtual void ValidateDestroyWorkspaceCommand( IDestroyWorkspaceCommand cmd, UserMessageCollector collector )
    {
        // 0/1/2/3 are reserved (anonymous, System, Administrators, AdminZone).
        if( cmd.WorkspaceId <= 3 )
        {
            collector.Error( "WorkspaceId must be greater than 3.", "Workspace.InvalidWorkspaceId" );
        }
    }

    [IncomingValidator]
    public virtual void ValidatePlugWorkspaceCommand( IPlugWorkspaceCommand cmd, UserMessageCollector collector )
    {
        if( cmd.ZoneId <= 0 )
        {
            collector.Error( "ZoneId must be greater than 0.", "Workspace.InvalidZoneId" );
        }
    }

    [IncomingValidator]
    public virtual void ValidateUnplugWorkspaceCommand( IUnplugWorkspaceCommand cmd, UserMessageCollector collector )
    {
        if( cmd.WorkspaceId <= 0 )
        {
            collector.Error( "WorkspaceId must be greater than 0.", "Workspace.InvalidWorkspaceId" );
        }
    }

    [IncomingValidator]
    public virtual void ValidateRenameWorkspaceCommand( IRenameWorkspaceCommand cmd, UserMessageCollector collector )
    {
        if( cmd.WorkspaceId <= 3 )
        {
            collector.Error( "WorkspaceId must be greater than 3.", "Workspace.InvalidWorkspaceId" );
        }
        if( string.IsNullOrWhiteSpace( cmd.WorkspaceName ) )
        {
            collector.Error( "WorkspaceName cannot be null, empty or whitespace.", "Workspace.InvalidWorkspaceName" );
        }
    }
}
