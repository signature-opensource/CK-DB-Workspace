namespace CK.IO.UserProfile.Workspace;

public interface IUserProfile : CK.IO.Actor.IUserProfile
{
    public int PreferredWorkspaceId { get; set; }
    public IList<IGroupInfos> Groups { get; }
}
