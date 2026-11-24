The current workspace of a Cris (Command Responsibility Segregation) command: the `IWorkspaceContext` ambient service,
set from the `IWorkspacePart` of the command, and the current workspace in the ambient values.

`CrisWorkspaceService` configures the `IWorkspaceContext` (its default is `WorkspaceContextAmbientServiceDefault`, no
workspace) and fills `IWorkspaceAmbientValues`. Access control stays in the database procedures.
