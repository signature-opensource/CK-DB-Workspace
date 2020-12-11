--[beginscript]

-- This prevents this migration script to be executed: this package alreday defined
-- the Administrators group (n°2) and the Platform Zone (n°3). 
insert into CKCore.tSetupMemoryItem( ItemKey, ItemValue ) values( N'sql|[]db^Model.CK.GroupTable|Settle|5.0.1|5.0.2|AutoNum0', N'' );

--[endscript]

