--[beginscript]

create table CK.tWorkspace
(
    WorkspaceId int not null,
    AdminGroupId int not null,
    AclId int not null,

    constraint PK_CK_tWorkspace primary key clustered ( WorkspaceId ),
    constraint FK_CK_tWorkspace_ZoneId foreign key ( WorkspaceId ) references CK.tZone( ZoneId ),
    constraint FK_CK_tWorkspace_AdminGroupId foreign key ( AdminGroupId ) references CK.tGroup( GroupId ),
    constraint FK_CK_tWorkspace_AclId foreign key ( AclId ) references CK.tAcl( AclId )
);

insert into CK.tWorkspace( WorkspaceId, AdminGroupId, AclId ) values ( 0, 0, 0 );

--[endscript]

--[beginscript]

-- Note that, by default at this level, nothing prevents a PreferredWorkspaceId to be 0.
alter table CK.tUser add
    PreferredWorkspaceId int not null constraint DF_TEMP default( 0 )
    constraint FK_CK_tUser_PreferredWorkspaceId foreign key( PreferredWorkspaceId ) references CK.tWorkspace( WorkspaceId );

alter table CK.tUser drop constraint DF_TEMP;

--[endscript]

