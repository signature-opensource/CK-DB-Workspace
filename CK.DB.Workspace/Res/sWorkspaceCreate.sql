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
    -- No need to check Actor grant level because, check is in sWorkspacePlug + it's a transaction

    --[beginsp]

    -- The @WorkspaceIdResult is the ZoneId.
    exec CK.sZoneCreate @ActorId, @WorkspaceIdResult output;
    exec CK.sGroupGroupNameSet @ActorId, @WorkspaceIdResult, @WorkspaceName output;

    --<PreCreate revert />

    exec CK.sWorkspacePlug @ActorId, @WorkspaceIdResult;

    --<PostCreate />

    --[endsp]
end
