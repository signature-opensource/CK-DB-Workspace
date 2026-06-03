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

    if (len(@WorkspaceName) = 0 or patindex('%[^0-9a-zA-Z-._]%', @WorkspaceName) > 0) throw 50000, 'Workspace.InvalidName', 1;

    --[beginsp]

    -- The @WorkspaceIdResult is the ZoneId.
    exec CK.sZoneCreate @ActorId, @WorkspaceIdResult output;

    -- If the new workspace already uses an existing name, the default behavior of
    -- the following CK.sGroupGroupNameSet procedure is to append ' (XX)' to the name.
    -- However, CK.sWorkspacePlug does not allow spaces or parentheses.
    -- Therefore, we compute a unique workspace name using only valid characters
    -- for the before/after naming pattern.
    exec @WorkspaceName = CK.fGroupGroupNameComputeUnique @WorkspaceIdResult, @WorkspaceName, N'-', N'';

    exec CK.sGroupGroupNameSet @ActorId, @WorkspaceIdResult, @WorkspaceName output;

    --<PreCreate revert />

    exec CK.sWorkspacePlug @ActorId, @WorkspaceIdResult;

    --<PostCreate />

    --[endsp]
end
