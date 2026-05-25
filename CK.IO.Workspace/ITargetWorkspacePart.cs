using CK.Core;
using CK.Cris;

namespace CK.IO.Workspace;

/// <summary>
/// Part for admin commands that act on a workspace different from (or in addition to)
/// the current ambient <c>WorkspaceId</c>. E.g. destroying or renaming a workspace from
/// the admin's current workspace context.
/// <para>
/// Always explicit — never an ambient value. Per-command validation should reject
/// reserved-id values (typically <c>&lt;= 3</c> for built-in groups).
/// </para>
/// </summary>
public interface ITargetWorkspacePart : ICrisPocoPart
{
    /// <summary>
    /// The identifier of the workspace being acted upon.
    /// </summary>
    int TargetWorkspaceId { get; set; }
}
