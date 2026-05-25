using CK.Core;
using CK.Cris;

namespace CK.IO.Workspace;

/// <summary>
/// Part for commands or events that target the user's current workspace context.
/// Carries the ambient <see cref="CurrentWorkspaceId"/> auto-stamped by the CRIS endpoint
/// (server-collected default or client-side <c>ambientValuesOverride</c>).
/// <para>
/// 0 is the sentinel "no workspace selected" value; validation rejects <c>&lt;= 0</c>.
/// </para>
/// </summary>
public interface IWorkspacePart : ICrisPocoPart
{
    /// <summary>
    /// The current workspace identifier. Auto-stamped via the ambient mechanism.
    /// Must be a workspace the actor is a member of.
    /// </summary>
    [AmbientServiceValue]
    int? CurrentWorkspaceId { get; set; }
}
