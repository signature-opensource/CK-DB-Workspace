using CK.Cris;
using CK.Cris.AmbientValues;

namespace CK.IO.Workspace;

/// <summary>
/// Extends <see cref="IAmbientValues"/> with the current <see cref="WorkspaceId"/>.
/// Filled server-side by <c>CrisWorkspaceService.GetWorkspaceAmbientValue</c> from
/// the scoped <c>IWorkspaceContext</c>.
/// </summary>
public interface IWorkspaceAmbientValues : IAmbientValues
{
    /// <summary>
    /// The current workspace identifier resolved by the endpoint. 0 means none.
    /// </summary>
    int WorkspaceId { get; set; }
}
