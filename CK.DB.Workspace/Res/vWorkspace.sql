-- SetupConfig: {}
--
create view CK.vWorkspace
as
    select	w.WorkspaceId,
			WorkspaceName = g.GroupName,
            w.AclId
	from CK.tWorkspace w
    join CK.vGroup g on g.GroupId = w.WorkspaceId;
