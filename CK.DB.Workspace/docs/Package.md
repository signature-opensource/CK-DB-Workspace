Implements the `CK.tWorkspace` table: a workspace is a zone (CK.DB.Zone) with an ACL and an administrator group, and a
user can have a preferred workspace.

`WorkspaceTable` creates, destroys, plugs and unplugs the workspaces (`WorkspaceTable.CreateWorkspaceAsync`,
`WorkspaceTable.DestroyWorkspaceAsync`, `WorkspaceTable.PlugWorkspaceAsync`, `WorkspaceTable.UnplugWorkspaceAsync`) and
handles the commands of CK.IO.Workspace; the `Package` sets the preferred workspace of a user
(`Package.SetUserPreferredWorkspaceAsync`, `ISetPreferredWorkspaceIdCommand`).

See the [README](https://github.com/signature-opensource/CK-DB-Workspace/blob/stable/README.md).
