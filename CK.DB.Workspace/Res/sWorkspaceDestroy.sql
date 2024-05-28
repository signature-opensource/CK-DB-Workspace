-- SetupConfig: { "Requires": [] }
--
create procedure CK.sWorkspaceDestroy
(
    @ActorId int,
    @WorkspaceId int,
	@ForceDestroy bit = 0
)
as
begin
    if @WorkspaceId <= 3 throw 50000, 'Workspace.InvalidWorkspaceId', 1;
    
    --[beginsp]
    
    declare @AclId int;
    declare @AdminGroupId int;
    select @AclId = AclId, @AdminGroupId = AdminGroupId from CK.tWorkspace where WorkspaceId = @WorkspaceId;

    if @AclId is not null
    begin
        if CK.fAclGrantLevel( @ActorId, @AclId ) < 127 throw 50000, 'Security.MustBeAdmin', 1;

        --<PreDestroy revert />
    
        exec CK.sWorkspaceUnplug @ActorId, @WorkspaceId;

        exec CK.sZoneDestroy @ActorId, @WorkspaceId, @ForceDestroy;

    	--<PostZoneDestroy />
    end
    
    --[endsp]
end
