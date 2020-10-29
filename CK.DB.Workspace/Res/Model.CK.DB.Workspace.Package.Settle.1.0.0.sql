--[beginscript]

declare @PlatformZoneId int;
exec CK.sZoneCreate 1, @PlatformZoneId output;
if @PlatformZoneId <> 2 throw 50000, 'Initialization.PlatformZoneMustBe2', 1;
exec CK.sGroupGroupNameSet 1, @PlatformZoneId, N'Platform Zone';

declare @AdminGroupId int;
exec CK.sGroupCreate 1, @AdminGroupId output, @PlatformZoneId;
exec CK.sGroupGroupNameSet 1, @AdminGroupId, N'Administrators';

if @AdminGroupId <> 3 throw 50000, 'Initialization.AdminGroupMustBe3', 1;

-- Creates the SystemAcl ==> TO BE MOVED TO CK.DB.Acl.
declare @SystemAclId int;
exec CK.sAclCreate 1, @SystemAclId output;
update CKCore.tSystem set SystemAclId = @SystemAclId where Id = 1;
-- /TO BE MOVED TO CK.DB.Acl.

-- These lines will be require once the "TO BE MOVED TO CK.DB.Acl" will be done...
-- ...unless we manage to define a standard, fixed, AclId for this SystemAclId. TBI.
--
-- declare @SystemAclId int;
-- select @SystemAclId = SystemAclId from CKCore.tSystem where Id = 1;
--

-- Every member of the PlatformZone (Id=2) are "Viewer" on the System acl.
exec CK.sAclGrantSet 1, @SystemAclId, @PlatformZoneId, 'PlatformZone', 16;
-- Every member of the Administrator groupe of the PlatformZone (Id=2) are "Administrator" on the System acl.
exec CK.sAclGrantSet 1, @SystemAclId, @AdminGroupId, 'AdminGroup', 127;

--[endscript]
