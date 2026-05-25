using CK.Core;
using CK.Cris;

namespace CK.IO.UserProfile.Workspace;

public class IncomingValidators : IRealObject
{
    [IncomingValidator]
    public virtual void ValidateSetPreferredWorkspaceIdCommand( ISetPreferredWorkspaceIdCommand cmd, UserMessageCollector collector )
    {
        if( cmd.UserId <= 0 )
        {
            collector.Error( "UserId must be greater than 0.", "User.InvalidUserId" );
        }
        if( cmd.WorkspaceId <= 0 )
        {
            collector.Error( "PreferredWorkspaceId must be greater than 0.", "UserProfile.InvalidPreferredWorkspaceId" );
        }
    }
}
