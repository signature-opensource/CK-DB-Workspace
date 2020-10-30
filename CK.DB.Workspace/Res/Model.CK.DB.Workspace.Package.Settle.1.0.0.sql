--[beginscript]

-- The Administrators group is n°2.
-- This should be created by CK.DB.Actor (and named 'Administrators' by CK.DB.Group.SimpleNaming).
declare @AdminGroupId int;
exec CK.sGroupCreate 1, @AdminGroupId output, 0;
exec CK.sGroupGroupNameSet 1, @AdminGroupId, N'Administrators';

if @AdminGroupId <> 2 throw 50000, 'Initialization.AdministratorsGroupMustBe2', 1;

-- This should be done in CK.DB.Acl.
-- Every member of this Administrator group (Id=2) are "Administrator" on the System acl.
exec CK.sAclGrantSet 1, 2, @AdminGroupId, 'AdministratorsGroup', 127;


-- The Platform Zone is n°3.
-- This should be created by CK.DB.Zone (and named 'Platform Zone' by CK.DB.Zone.SimpleNaming).
--
-- The fact that it is CK.DB.Zone.SimpleNaming that names the zone is not perfect... Since as soon
-- as Zone is installed AND Group.SimpleNaming are both installed, the name should be set.
-- This is perfectly feasible thanks to dynamic sql in one of the 2 packages: they can
-- adjust their behavior according to the VFeature existence (and potentially its version).
-- We already have what is needed to do this:
--
--    select * from CKCore.tItemVersionStore where ItemType = 'VFeature';
--
declare @PlatformZoneId int;
exec CK.sZoneCreate 1, @PlatformZoneId output;
exec CK.sGroupGroupNameSet 1, @PlatformZoneId, N'Platform Zone';
-- We move the 'Administrators' (Id=3) group into the PlatformZone, auto registrering its potential
-- users in the PlatformZone.
exec CK.sGroupMove 1, 3, @PlatformZoneId, 2 /*AutoUserRegistration*/;
-- Every member of the PlatformZone (Id=3) are "Viewer" on the System acl.
exec CK.sAclGrantSet 1, 1, @PlatformZoneId, 'PlatformZone', 16;


if @PlatformZoneId <> 3 throw 50000, 'Initialization.PlatformZoneMustBe2', 1;

--[endscript]
