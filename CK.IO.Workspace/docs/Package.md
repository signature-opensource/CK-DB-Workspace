The workspace commands of CK.DB.Workspace and the current workspace ambient value: `ICreateWorkspaceCommand`,
`IDestroyWorkspaceCommand`, `IPlugWorkspaceCommand`, `IUnplugWorkspaceCommand` and `IRenameWorkspaceCommand`.

`IWorkspacePart` carries the current workspace of a command (`ICommandWorkspace` is its command part) and
`IWorkspaceAmbientValues` exposes it in the ambient values.
