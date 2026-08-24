-- SetupConfig: { "Requires": [ "CK.fAclGrantLevel" ] }
--
-- Returns the GrantLevel (0..127) that @ActorId has on @WorkspaceId.
-- Returns 0 when the workspace does not exist or the actor has no grant.
--
create function CK.fUserWorkspaceGrantLevel
(
    @ActorId int,
    @WorkspaceId int
)
returns tinyint
as
begin
    declare @GrantLevel tinyint = 0;

    declare @AclId int;
    select @AclId = AclId from CK.tWorkspace where WorkspaceId = @WorkspaceId;

    if @AclId is not null
        set @GrantLevel = CK.fAclGrantLevel( @ActorId, @AclId );

    return @GrantLevel;
end
