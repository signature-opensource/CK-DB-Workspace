using CK.Core;
using CK.DB.Acl;
using CK.DB.Actor;
using CK.DB.Zone;
using CK.SqlServer;
using CK.Testing;
using Shouldly;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.Workspace.Tests;

[TestFixture]
public class WorkspaceTests
{
    [Test]
    public void user_created_with_a_preferred_workspace_is_automatically_added_to_the_workspace()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;

        var group = services.GetRequiredService<Actor.GroupTable>();
        var workspace = services.GetRequiredService<Package>();

        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {
            var w = CreateWorkspaceAndOneAdministrator( ctx, group, workspace );
            var userId = workspace.CreateUser( ctx, 1, Guid.NewGuid().ToString(), w.Workspace.WorkspaceId );
            workspace.Database.ExecuteScalar( "select 1 from CK.tActorProfile where ActorId = @0 and GroupId = @1", userId, w.Workspace.WorkspaceId ).ShouldBe( 1 );
        }
    }

    [Test]
    public void set_user_preferred_workspace_checks_that_the_user_is_at_least_Viewer_of_the_Workspace()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;

        var acl = services.GetRequiredService<AclTable>();
        var user = services.GetRequiredService<UserTable>();
        var group = services.GetRequiredService<Actor.GroupTable>();
        var workspace = services.GetRequiredService<Package>();

        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {
            var w = CreateWorkspaceAndOneAdministrator( ctx, group, workspace );
            var userId = user.CreateUser( ctx, 1, Guid.NewGuid().ToString() );

            Util.Invokable( () => workspace.SetUserPreferredWorkspace( ctx, 1, userId, w.Workspace.WorkspaceId ) )
                     .ShouldThrow<SqlDetailedException>();

            int aclId = workspace.Database.ExecuteScalar<int>( "select AclId from CK.tWorkspace where WorkspaceId=@0", w.Workspace.WorkspaceId );
            acl.AclGrantSet( ctx, 1, aclId, userId, "Just for test", 16 );

            Util.Invokable( () => workspace.SetUserPreferredWorkspace( ctx, 1, userId, w.Workspace.WorkspaceId ) ).ShouldNotThrow();

            workspace.Database.ExecuteScalar<int>( "select PreferredWorkspaceId from CK.tUser where UserId=@0", userId ).ShouldBe( w.Workspace.WorkspaceId );
        }
    }

    [Test]
    public async Task plug_workspace_create_a_workspace_with_same_zone_id_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;

        var zoneTable = services.GetRequiredService<ZoneTable>();
        var workspaceTable = services.GetRequiredService<WorkspaceTable>();

        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {

            int zoneId = await zoneTable.CreateZoneAsync( ctx, 1 );

            workspaceTable.Database.ExecuteScalar<int?>( "select 1 from CK.tWorkspace where WorkspaceId = @0", zoneId ).ShouldBeNull();

            await workspaceTable.PlugWorkspaceAsync( ctx, 1, zoneId );

            workspaceTable.Database.ExecuteScalar<int?>( "select 1 from CK.tWorkspace where WorkspaceId = @0", zoneId ).ShouldBe( 1 );
        }
    }

    [Test]
    public async Task random_user_cannot_plug_a_workspace_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;

        var zoneTable = services.GetRequiredService<ZoneTable>();
        var userTable = services.GetRequiredService<UserTable>();
        var workspaceTable = services.GetRequiredService<WorkspaceTable>();

        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {
            int zoneId = await zoneTable.CreateZoneAsync( ctx, 1 );
            int userId = await userTable.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
            await Util.Awaitable( () => workspaceTable.PlugWorkspaceAsync( ctx, userId, zoneId ) )
                        .ShouldThrowAsync<Exception>();
        }
    }

    [Test]
    public async Task unplug_workspace_destroy_workspace_but_let_zone_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;

        var workspaceTable = services.GetRequiredService<WorkspaceTable>();

        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {

            var workspace = await workspaceTable.CreateWorkspaceAsync( ctx, 1, Guid.NewGuid().ToString() );

            workspaceTable.Database.ExecuteScalar<int?>( "select 1 from CK.tZone where ZoneId = @0", workspace.WorkspaceId ).ShouldBe( 1 );
            workspaceTable.Database.ExecuteScalar<int?>( "select 1 from CK.tWorkspace where WorkspaceId = @0", workspace.WorkspaceId ).ShouldBe( 1 );

            await workspaceTable.UnplugWorkspaceAsync( ctx, 1, workspace.WorkspaceId );

            workspaceTable.Database.ExecuteScalar<int?>( "select 1 from CK.tZone where ZoneId = @0", workspace.WorkspaceId ).ShouldBe( 1 );
            workspaceTable.Database.ExecuteScalar<int?>( "select 1 from CK.tWorkspace where WorkspaceId = @0", workspace.WorkspaceId ).ShouldBeNull();
        }
    }

    [Test]
    public async Task plug_workspace_is_idempotent_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;

        var zoneTable = services.GetRequiredService<ZoneTable>();
        var workspaceTable = services.GetRequiredService<WorkspaceTable>();

        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {

            int zoneId = await zoneTable.CreateZoneAsync( ctx, 1 );
            WorkspaceExists( workspaceTable, zoneId ).ShouldBeFalse();

            for( int i = 0; i < 10; i++ )
            {
                await workspaceTable.PlugWorkspaceAsync( ctx, 1, zoneId );
                WorkspaceExists( workspaceTable, zoneId ).ShouldBeTrue();
            }
        }
    }

    [Test]
    public async Task unplug_workspace_is_idempotent_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;

        var workspaceTable = services.GetRequiredService<WorkspaceTable>();

        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {

            var workspace = await workspaceTable.CreateWorkspaceAsync( ctx, 1, Guid.NewGuid().ToString() );
            WorkspaceExists( workspaceTable, workspace.WorkspaceId ).ShouldBeTrue();

            for( int i = 0; i < 10; i++ )
            {
                await workspaceTable.UnplugWorkspaceAsync( ctx, 1, workspace.WorkspaceId );
                WorkspaceExists( workspaceTable, workspace.WorkspaceId ).ShouldBeFalse();
            }
        }
    }

    [Test]
    public async Task random_user_not_admin_cannot_create_workspace_only_PlatformAdministrators_can_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;

        var groupTable = services.GetRequiredService<Actor.GroupTable>();
        var package = services.GetRequiredService<Package>();
        var workspaceTable = services.GetRequiredService<WorkspaceTable>();

        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {

            int idUser = await package.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString(), 0 );

            await Util.Awaitable( () => workspaceTable.CreateWorkspaceAsync( ctx, idUser, Guid.NewGuid().ToString() ) )
                          .ShouldThrowAsync<SqlDetailedException>();
        }
    }

    [Test]
    public async Task cannot_unplug_workspaceId_0_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;

        var workspaceTable = services.GetRequiredService<WorkspaceTable>();

        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {

            await Util.Awaitable( () => workspaceTable.UnplugWorkspaceAsync( ctx, 1, 0 ) )
                      .ShouldThrowAsync<Exception>();
        }
    }

    [Test]
    public async Task two_workspace_have_same_administrator_group_name_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;

        var workspaceTable = services.GetRequiredService<WorkspaceTable>();

        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {

            var workspace1 = await workspaceTable.CreateWorkspaceAsync( ctx, 1, Guid.NewGuid().ToString() );
            var workspace2 = await workspaceTable.CreateWorkspaceAsync( ctx, 1, Guid.NewGuid().ToString() );

            string? adminGroupName1 = workspaceTable.Database.ExecuteScalar<string>(
                @"select GroupName from CK.vGroup where ZoneId = @0;",
                workspace1.WorkspaceId );

            string? adminGroupName2 = workspaceTable.Database.ExecuteScalar<string>(
                @"select GroupName from CK.vGroup where ZoneId = @0;",
                workspace2.WorkspaceId );

            adminGroupName1.ShouldBe( adminGroupName2 );
        }
    }

    [Test]
    public async Task unplug_workspace_dstroy_admin_group_but_not_zone_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;

        var workspaceTable = services.GetRequiredService<WorkspaceTable>();

        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {

            var workspace = await workspaceTable.CreateWorkspaceAsync( ctx, 1, NewGuid() );

            WorkspaceExists( workspaceTable, workspace.WorkspaceId ).ShouldBeTrue();

            var adminGroupId = workspaceTable.Database.ExecuteScalar<int?>(
                "select AdminGroupId from CK.tWorkspace where WorkspaceId = @0;",
                workspace.WorkspaceId );

            adminGroupId.ShouldNotBeNull();

            await workspaceTable.UnplugWorkspaceAsync( ctx, 1, workspace.WorkspaceId );

            WorkspaceExists( workspaceTable, workspace.WorkspaceId ).ShouldBeFalse();

            workspaceTable.Database.ExecuteScalar<int?>(
                "select isnull( (select 1 from CK.tGroup where GroupId = @0), 0 );",
                adminGroupId! ).ShouldBe( 0 );

            workspaceTable.Database.ExecuteScalar<int?>(
                "select isnull( (select 1 from CK.tGroup where GroupId = @0), 0 );",
                workspace.WorkspaceId ).ShouldBe( 1 );
        }
    }

    [Test]
    public async Task force_destroy_destroy_groups_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;

        var workspaceTable = services.GetRequiredService<WorkspaceTable>();
        var groupTable = services.GetRequiredService<Zone.GroupTable>();

        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {

            var workspace = await workspaceTable.CreateWorkspaceAsync( ctx, 1, NewGuid() );

            var groupId = await groupTable.CreateGroupAsync( ctx, 1, workspace.WorkspaceId );

            WorkspaceExists( workspaceTable, workspace.WorkspaceId ).ShouldBeTrue();

            workspaceTable.Database.ExecuteScalar<int>(
                @"select isnull( (select 1 from CK.tGroup where GroupId = @0 ), 0 );",
                groupId ).ShouldBe( 1 );

            await workspaceTable.DestroyWorkspaceAsync( ctx, 1, workspace.WorkspaceId, forceDestroy: true );

            WorkspaceExists( workspaceTable, workspace.WorkspaceId ).ShouldBeFalse();

            workspaceTable.Database.ExecuteScalar<int>(
                @"select isnull( (select GroupId from CK.tGroup where GroupId = @0), 0 );",
                groupId! ).ShouldBe( 0 );
        }
    }

    [Test]
    public async Task cannot_destroy_AdminZone_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;

        var workspaceTable = services.GetRequiredService<WorkspaceTable>();

        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {

            await Util.Awaitable( () => workspaceTable.DestroyWorkspaceAsync( ctx, 1, 3 /* AdminZone */ ) )
                          .ShouldThrowAsync<Exception>();
        }
    }

    [Test]
    public async Task only_workspace_admin_can_destroy_it_Async()
    {
        using var scopedServices = SharedEngine.AutomaticServices.CreateScope();
        var services = scopedServices.ServiceProvider;

        var workspacePkg = services.GetRequiredService<Package>();
        var workspaceTable = services.GetRequiredService<WorkspaceTable>();
        var groupTable = services.GetRequiredService<Actor.GroupTable>();

        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {

            var workspace = await workspaceTable.CreateWorkspaceAsync( ctx, 1, NewGuid() );

            WorkspaceExists( workspacePkg, workspace.WorkspaceId ).ShouldBeTrue();

            await Util.Awaitable( () => workspaceTable.DestroyWorkspaceAsync( ctx, 0, workspace.WorkspaceId ) )
                .ShouldThrowAsync<Exception>();

            WorkspaceExists( workspacePkg, workspace.WorkspaceId ).ShouldBeTrue();

            await workspaceTable.DestroyWorkspaceAsync( ctx, 1, workspace.WorkspaceId );

            WorkspaceExists( workspacePkg, workspace.WorkspaceId ).ShouldBeFalse();
        }
    }

    static (WorkspaceTable.NamedWorkspace Workspace, int AdminGroupId, int AdminUserId) CreateWorkspaceAndOneAdministrator( ISqlCallContext ctx, Actor.GroupTable group, Package workspace )
    {
        var w = workspace.WorkspaceTable.CreateWorkspace( ctx, 1, "TestWorkspace" );
        var uId = workspace.CreateUser( ctx, 1, $"Admin-{w.Name}-{Guid.NewGuid()}", w.WorkspaceId );
        var gId = workspace.Database.ExecuteScalar<int>( "select AdminGroupId from CK.tWorkspace where WorkspaceId = @0", w.WorkspaceId );
        // The new admin is already a Zone member...
        // ...so we can add it to the group's zone.
        group.AddUser( ctx, 1, gId, uId );
        return (w, gId, uId);
    }

    static bool WorkspaceExists( SqlPackage pkg, int workspaceId )
        => pkg.Database.ExecuteScalar<int>(
            @"select isnull( (select 1 from CK.tWorkspace where WorkspaceId = @0), 0 );",
            workspaceId ) > 0;

    static string NewGuid( int length = 32 ) => Guid.NewGuid().ToString().Substring( 0, length );
}
