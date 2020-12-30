-- SetupConfig: { "Requires": [] }
--
-- Note that the user must be at least Viewer of the workspace otherwise an error is thrown.
-- 
create procedure CK.sUserPreferredWorkspaceIdSet (
	@ActorId int, -- not null
	@UserId int, -- not null
	@WorkspaceId int -- not null
)
as
begin
	--[beginsp]

    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

    --<PreCheckAccess revert />

    if not exists( select 1
                        from CK.tWorkspace w
                        inner join CK.vAclActor a on a.AclId = w.AclId and a.ActorId = @UserId
                        where w.WorkspaceId = @WorkspaceId and a.GrantLevel >= 16 )
        throw 50000, 'Workspace.UserIsNotViewerOfTheWorkspace', 1;

    --<PostCheckAccess revert />

	--<PreSetPreferredWorkspace revert />

	update CK.tUser set PreferredWorkspaceId = @WorkspaceId where UserId = @UserId;

	--<PostSetPreferredWorkspace />

	--[endsp]
end
