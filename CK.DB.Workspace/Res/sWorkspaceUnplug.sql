-- SetupConfig: { "Requires": [] }
--
-- Destroys a Workspace: can work only if there is no Users inside the Workspace.
create procedure CK.sWorkspaceUnplug
(
    @ActorId int, -- not null
    @WorkspaceId int -- not null
)
as
begin
    if @WorkspaceId <= 0 throw 50000, 'Workspace.InvalidWorkspaceId', 1;

    -- Only if the workspace exists, try to unplug it...
    if exists (select 1 from CK.tWorkspace where WorkspaceId = @WorkspaceId)
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
    
        --<PreUnplug revert />
    
        -- Delete workspace (this frees the AdminGroupId FK).
        delete from CK.tWorkspace where WorkspaceId = @WorkspaceId;
    
        exec CK.sGroupDestroy @ActorId, @AdminGroupId, @ForceDestroy = 1;

        --<PostUnplug />

        --[endsp]
    end
end
