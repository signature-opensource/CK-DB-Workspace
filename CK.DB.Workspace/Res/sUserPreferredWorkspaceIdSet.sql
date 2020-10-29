-- SetupConfig: { "Requires": [] }
--

create procedure CK.sUserPreferredWorkspaceIdSet (
	@ActorId int, --not null
	@UserId int, --not null
	@WorkspaceId int --not null
)
as
begin
	--[beginsp]

    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;

	--<PreSetPreferredWorkspace revert />
	
	update CK.tUser set PreferredWorkspaceId = @WorkspaceId where UserId = @UserId;

	--<PostSetPreferredWorkspace />

	--[endsp]
end
