using CK.Core;

namespace CK.IO.UserProfile.Workspace;

public interface IGroupInfos : IPoco
{
    public int GroupId { get; set; }
    public string GroupName { get; set; }
    public int ZoneId { get; set; }
    public string ZoneName { get; set; }
}
