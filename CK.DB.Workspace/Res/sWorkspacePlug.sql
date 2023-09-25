-- SetupConfig: {}
create procedure CK.sWorkspacePlug
(
    @ActorId int,
    @ZoneId int
)
as
begin
    if CK.fAclGrantLevel( @ActorId, 1 ) < 112 throw 50000, 'Security.MustBeSafeAdminOnSystemAcl', 1;

    --[beginsp]

    -- If a workspace is already plugged to the Zone, do not plug a new workspace.
    if not exists (select 1 from CK.tWorkspace where WorkspaceId = @ZoneId)
    begin
        declare @AdminGroupId int;
        -- The @AdminGroupId is the workspace's administrators groupId.
        exec CK.sGroupCreate @ActorId, @AdminGroupId output, @ZoneId;
        exec CK.sGroupGroupNameSet @ActorId, @AdminGroupId, 'Administrators';
        exec CK.sGroupUserAdd @ActorId, @AdminGroupId, @ActorId, 1;

        declare @AclId int;
        -- Creating its Acl...
        exec CK.sAclCreate @ActorId, @AclId output;
        -- ...and configures it: the member of the Workspace can see it.
    	exec CK.sAclGrantSet @ActorId, @AclId, @ZoneId, 'Default.Workspace.Level', 16;
        -- ...and configures it: the workspace's administrators have full control.
        exec CK.sAclGrantSet @ActorId, @AclId, @AdminGroupId, 'Workspace.Administrator.Level', 127;
        -- And the Platform Administrators group (that is 2 by design) has full control.
    	exec CK.sAclGrantSet 1, @AclId, 2, 'Platform.Administrator', 127;

        --<PrePlug revert />

        -- Inserting the Workspace.
        insert into CK.tWorkspace( WorkspaceId, AdminGroupId, AclId ) values( @ZoneId, @AdminGroupId, @AclId );

        --<PostPlug />
    end

    --[endsp]
end
