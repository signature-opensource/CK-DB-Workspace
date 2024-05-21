--[beginscript]

-- This is the 'Plateform Zone' workspace with id 3. It is settled by CK.DB.Zone.
-- It's AdminGroupId is 2 'Administrators' group. It is settled by CK.DB.Group.
-- It's AclId is 1 'System Acl'. Members of this workspace are viewer of the Sytem Acl.
insert into CK.tWorkspace( WorkspaceId, AdminGroupId, AclId ) values ( 3, 2, 1 );

--[endscript]
