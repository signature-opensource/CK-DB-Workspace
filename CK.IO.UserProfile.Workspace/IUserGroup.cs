using CK.Core;

namespace CK.IO.UserProfile.Workspace;

public interface IUserGroup : IPoco
{
    public IGroupInfos Group { get; set; }
    public int GrantLevel { get; set; }
}
