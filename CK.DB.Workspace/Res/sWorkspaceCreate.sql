-- SetupConfig: { "Requires": ["CK.sZoneCreate", "CK.sGroupGroupNameSet"] }
--
-- On output, the @WorkspaceName may be suffixed by " (n)" if the name already exists.
--
create procedure CK.sWorkspaceCreate
(
    @ActorId int, -- not null
    @WorkspaceName nvarchar(128) /*input*/output,
    @WorkspaceIdResult int output
)
as
begin

    declare @SystemAclId int;
    select @SystemAclId = SystemAclId from CKCore.tSystem where Id = 1;
    if CK.fAclGrantLevel( @ActorId, @SystemAclId ) < 112 throw 50000, 'Security.MustBeSafeAdminOnSystemAcl', 1;

    --[beginsp]

    -- The @WorkspaceIdResult is the ZoneId.
    exec CK.sZoneCreate @ActorId, @WorkspaceIdResult output;
    exec CK.sGroupGroupNameSet @ActorId, @WorkspaceIdResult, @WorkspaceName output;

    declare @WorkspaceAdminGroupId int;
    -- The @WorkspaceAdminGroupId is the workspace's administrators groupId.
    exec CK.sGroupCreate @ActorId, @WorkspaceAdminGroupId output, @WorkspaceIdResult;
    exec CK.sGroupGroupNameSet @ActorId, @WorkspaceAdminGroupId, 'Administrators';
    exec CK.sGroupUserAdd @ActorId, @WorkspaceAdminGroupId, @ActorId, 1;

    declare @AclId int;
    -- Creating its Acl...
    exec CK.sAclCreate @ActorId, @AclId output;
    -- ...and configures it: the member of the Workspace can see it.
	exec CK.sAclGrantSet @ActorId, @AclId, @WorkspaceIdResult, 'Default.Workspace.Level', 16;
    -- ...and configures it: the workspace's administrators have full control.
    exec CK.sAclGrantSet @ActorId, @AclId, @WorkspaceAdminGroupId, 'Workspace.Administrator.Level', 127;
    -- And the Plateform Administrators group has full control.
	declare @AdminGroupId int;
	select @AdminGroupId = GroupId from CK.vGroup where GroupName = 'Administrators' and ZoneId = 2;
	exec CK.sAclGrantSet 1, @AclId, @AdminGroupId, 'Plateform.Administrator', 127;

    -- Inserting the Workspace.
    insert into CK.tWorkspace( WorkspaceId, AclId ) values( @WorkspaceIdResult, @AclId );

    --[endsp]
end
