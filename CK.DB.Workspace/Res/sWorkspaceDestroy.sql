-- SetupConfig: { "Requires": [] }
--
create procedure CK.sWorkspaceDestroy
(
    @ActorId int, -- not null
    @WorkspaceId int -- not null 
)
as
begin
    if not exists( select 1
                    from CK.tWorkspace w
                    inner join CK.vAclActor a on a.AclId = w.AclId and a.ActorId = @ActorId
                    where w.WorkspaceId = @WorkspaceId and a.GrantLevel = 127 )
	    throw 50000, 'Security.WorkspaceAdministratorLevelOnly', 1;	

    --[beginsp]

    --<PreCreate revert />

    -- Set PreferredWorkspaceId to 0
    declare @UserId int;
    declare @Cursor cursor;
    set @Cursor = cursor static read_only for
        select UserId
            from CK.tUser u
            where u.PreferredWorkspaceId = @WorkspaceId;

    open @Cursor;
    fetch next from @Cursor into @UserId;

    while @@FETCH_STATUS = 0
    begin
        exec CK.sUserPreferredWorkspaceIdSet @ActorId, @UserId, 0;
        fetch next from @Cursor into @UserId;
    end

    close @Cursor;
    deallocate @Cursor;

    -- Delete workspace
    delete from CK.tWorkspace where WorkspaceId = @WorkspaceId;

    -- Delete Zone and Group, which removes all users from the specified workspace
    exec CK.sZoneDestroy @ActorId, @WorkspaceId, 1;

    --<PostCreate />

    --[endsp]
end
