using CK.Core;
using System.Threading.Tasks;

namespace CK.Cris.Workspace;

/// <summary>
/// Abstraction that answers "is this actor a member of this workspace?".
/// Implementations typically query <c>CK.tActorProfile</c> via Dapper or a cache.
/// </summary>
/// <remarks>
/// No default implementation ships in <c>CK.Cris.Workspace</c> to keep this layer
/// DB-agnostic. The companion package <c>CK.DB.Workspace.MembershipChecker</c>
/// provides a SQL-backed default; applications may also ship their own.
/// </remarks>
public interface IWorkspaceMembershipChecker : IScopedAutoService
{
    /// <summary>
    /// Returns whether <paramref name="userId"/> is a member of <paramref name="workspaceId"/>.
    /// </summary>
    /// <param name="monitor">The activity monitor.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="workspaceId">The workspace (group) identifier.</param>
    /// <returns>True when the user is a member, false otherwise.</returns>
    Task<bool> IsMemberAsync( IActivityMonitor monitor, int userId, int workspaceId );
}
