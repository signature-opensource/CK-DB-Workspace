-- SetupConfig: { "Requires": [] }
--
-- Destroys a Workspace: can work only if there is no Users inside the Workspace except if @ForceDestroy = 1.
create procedure CK.sWorkspaceDestroy
(
    @ActorId int, -- not null
    @WorkspaceId int, -- not null 
	@ForceDestroy bit = 0
)
as
begin

    -- Retrieves the AdminGroupId and challenges the workspace's acl at once.
    declare @AdminGroupId int;
    select @AdminGroupId = w.AdminGroupId
        from CK.tWorkspace w
        inner join CK.vAclActor a on a.AclId = w.AclId and a.ActorId = @ActorId
        where w.WorkspaceId = @WorkspaceId and a.GrantLevel = 127;

    if @AdminGroupId is null
        throw 50000, 'Security.UnexistingWorkspaceOrAdministratorLevelRequired', 1;	

    --[beginsp]

    --<PreClearPreferredWorkspaceId revert />

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
        -- Sets the PreferredWorkspaceId to 0.
        exec CK.sUserPreferredWorkspaceIdSet @ActorId, @UserId, 0;
        fetch next from @Cursor into @UserId;
    end

    close @Cursor;
    deallocate @Cursor;

    --<PostClearPreferredWorkspaceId />


    --<PreDestroy revert />

    -- Delete workspace (this frees the AdminGroupId FK).
    delete from CK.tWorkspace where WorkspaceId = @WorkspaceId;

    -- When @ForceDestroy is false, the sZoneDestroy will error if a Group or a User exists.
    -- The Administrator group BELONGS to the Workspace: even if @ForceDestroy is false, we
    -- call the sGroupDestroy. Note that if there are any user in it, this will fail since
    -- we call it with its own @ForceDestroy to false.
    if @ForceDestroy = 0
    begin
        exec CK.sGroupDestroy @ActorId, @AdminGroupId, @ForceDestroy = 0;
    end

    -- Delete Zone and its Groups.
    exec CK.sZoneDestroy @ActorId, @WorkspaceId, @ForceDestroy;

    --<PostDestroy />

    --[endsp]
end
