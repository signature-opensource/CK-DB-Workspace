using CK.Cris;

namespace CK.IO.Workspace;

/// <summary>
/// Command part for commands operating in the context of a specific workspace.
/// Inherits the ambient <see cref="IWorkspacePart.CurrentWorkspaceId"/>: the current workspace
/// is auto-stamped via the CRIS ambient-value mechanism (no need to set it manually).
/// </summary>
public interface ICommandWorkspace : ICommandPart, IWorkspacePart
{
}
