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

    if CK.fAclGrantLevel( @ActorId, 1 ) < 112 throw 50000, 'Security.MustBeSafeAdminOnSystemAcl', 1;

    --[beginsp]

    -- The @WorkspaceIdResult is the ZoneId.
    exec CK.sZoneCreate @ActorId, @WorkspaceIdResult output;
    exec CK.sGroupGroupNameSet @ActorId, @WorkspaceIdResult, @WorkspaceName output;

    declare @AdminGroupId int;
    -- The @AdminGroupId is the workspace's administrators groupId.
    exec CK.sGroupCreate @ActorId, @AdminGroupId output, @WorkspaceIdResult;
    exec CK.sGroupGroupNameSet @ActorId, @AdminGroupId, 'Administrators';
    exec CK.sGroupUserAdd @ActorId, @AdminGroupId, @ActorId, 1;

    declare @AclId int;
    -- Creating its Acl...
    exec CK.sAclCreate @ActorId, @AclId output;
    -- ...and configures it: the member of the Workspace can see it.
	exec CK.sAclGrantSet @ActorId, @AclId, @WorkspaceIdResult, 'Default.Workspace.Level', 16;
    -- ...and configures it: the workspace's administrators have full control.
    exec CK.sAclGrantSet @ActorId, @AclId, @AdminGroupId, 'Workspace.Administrator.Level', 127;
    -- And the Platform Administrators group (that is 2 by design) has full control.
	exec CK.sAclGrantSet 1, @AclId, 2, 'Platform.Administrator', 127;

    --<PreCreate revert />

    -- Inserting the Workspace.
    insert into CK.tWorkspace( WorkspaceId, AdminGroupId, AclId ) values( @WorkspaceIdResult, @AdminGroupId, @AclId );

    --<PostCreate />

    --[endsp]
end
