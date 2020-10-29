--[beginscript]

create table CK.tWorkspace
(
    WorkspaceId int not null,
    AclId int not null,

    constraint PK_CK_tWorkspace primary key clustered ( WorkspaceId ),
    constraint FK_CK_tWorkspace_ZoneId foreign key ( WorkspaceId ) references CK.tZone( ZoneId ),
    constraint FK_CK_tWorkspace_AclId foreign key ( AclId ) references CK.tAcl( AclId )
);

insert into CK.tWorkspace( WorkspaceId, AclId ) values ( 0, 0 );

--[endscript]

--[beginscript]

alter table CK.tUser add
    PreferredWorkspaceId int not null constraint DF_TEMP default( 0 )
    constraint FK_CK_tUser_PreferredWorkspaceId foreign key( PreferredWorkspaceId ) references CK.tWorkspace( WorkspaceId );

alter table CK.tUser drop constraint DF_TEMP;

--[endscript]

--[beginscript]

-- TO BE MOVED TO CK.DB.Acl.
alter table CKCore.tSystem add SystemAclId int constraint DF_TEMP0 default( 0 );
alter table CKCore.tSystem add constraint FK_CKCore_tSystem_SystemAclId foreign key ( SystemAclId ) references CK.tAcl( AclId );
alter table CKCore.tSystem drop constraint DF_TEMP0;

--[endscript]
