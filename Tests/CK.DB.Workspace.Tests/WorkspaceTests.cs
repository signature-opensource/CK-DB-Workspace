using CK.Core;
using CK.DB.Acl;
using CK.DB.Actor;
using CK.DB.Zone;
using CK.SqlServer;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using static CK.Testing.DBSetupTestHelper;

namespace CK.DB.Workspace.Tests
{
    [TestFixture]
    public class WorkspaceTests
    {
        [Test]
        public void user_created_with_a_preferred_workspace_is_automatically_added_to_the_workspace()
        {
            var group = ObtainPackage<Actor.GroupTable>();
            var workspace = ObtainPackage<Package>();

            using var ctx = new SqlStandardCallContext( TestHelper.Monitor );

            var w = CreateWorkspaceAndOneAdministrator( ctx, group, workspace );

            var userId = workspace.CreateUser( ctx, 1, Guid.NewGuid().ToString(), w.Workspace.WorkspaceId );

            workspace.Database.ExecuteScalar( "select 1 from CK.tActorProfile where ActorId = @0 and GroupId = @1", userId, w.Workspace.WorkspaceId ).Should().Be( 1 );
        }

        [Test]
        public void set_user_preferred_workspace_checks_that_the_user_is_at_least_Viewer_of_the_Workspace()
        {
            var acl = ObtainPackage<AclTable>();
            var user = ObtainPackage<UserTable>();
            var group = ObtainPackage<Actor.GroupTable>();
            var workspace = ObtainPackage<Package>();

            using var ctx = new SqlStandardCallContext( TestHelper.Monitor );

            var w = CreateWorkspaceAndOneAdministrator( ctx, group, workspace );
            var userId = user.CreateUser( ctx, 1, Guid.NewGuid().ToString() );

            workspace.Invoking( _ => _.SetUserPreferredWorkspace( ctx, 1, userId, w.Workspace.WorkspaceId ) ).Should().Throw<SqlDetailedException>();

            int aclId = workspace.Database.ExecuteScalar<int>( "select AclId from CK.tWorkspace where WorkspaceId=@0", w.Workspace.WorkspaceId );
            acl.AclGrantSet( ctx, 1, aclId, userId, "Just for test", 16 );

            workspace.Invoking( _ => _.SetUserPreferredWorkspace( ctx, 1, userId, w.Workspace.WorkspaceId ) ).Should().NotThrow();

            workspace.Database.ExecuteScalar<int>( "select PreferredWorkspaceId from CK.tUser where UserId=@0", userId ).Should().Be( w.Workspace.WorkspaceId );
        }

        [Test]
        public async Task plug_workspace_create_a_workspace_with_same_zone_id_Async()
        {
            var zoneTable = ObtainPackage<ZoneTable>();
            var workspaceTable = ObtainPackage<WorkspaceTable>();

            using var ctx = new SqlStandardCallContext( TestHelper.Monitor );

            int zoneId = await zoneTable.CreateZoneAsync( ctx, 1 );

            workspaceTable.Database.ExecuteScalar<int?>( "select 1 from CK.tWorkspace where WorkspaceId = @0", zoneId ).Should().BeNull();

            await workspaceTable.PlugWorkspaceAsync( ctx, 1, zoneId );

            workspaceTable.Database.ExecuteScalar<int?>( "select 1 from CK.tWorkspace where WorkspaceId = @0", zoneId ).Should().Be( 1 );
        }

        [Test]
        public async Task cannot_plug_workspace_if_zone_have_already_a_workspace_Async()
        {
            var zoneTable = ObtainPackage<ZoneTable>();
            var workspaceTable = ObtainPackage<WorkspaceTable>();

            using var ctx = new SqlStandardCallContext( TestHelper.Monitor );

            int zoneId = await zoneTable.CreateZoneAsync( ctx, 1 );

            await workspaceTable.PlugWorkspaceAsync( ctx, 1, zoneId );
            workspaceTable.Database.ExecuteScalar<int?>( "select 1 from CK.tWorkspace where WorkspaceId = @0", zoneId ).Should().Be( 1 );

            await workspaceTable.Invoking( table => table.PlugWorkspaceAsync( ctx, 1, zoneId ) )
                                .Should().ThrowAsync<Exception>();
        }

        [Test]
        public async Task random_user_cannot_plug_a_workspace_Async()
        {
            var zoneTable = ObtainPackage<ZoneTable>();
            var userTalbe = ObtainPackage<UserTable>();
            var workspaceTable = ObtainPackage<WorkspaceTable>();

            using var ctx = new SqlStandardCallContext( TestHelper.Monitor );

            int zoneId = await zoneTable.CreateZoneAsync( ctx, 1 );
            int userId = await userTalbe.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );

            await workspaceTable.Invoking( table => table.PlugWorkspaceAsync( ctx, userId, zoneId ) ).Should().ThrowAsync<Exception>();
        }

        [Test]
        public async Task unplug_workspace_destroy_workspace_but_let_zone_Async()
        {
            var workspaceTable = ObtainPackage<WorkspaceTable>();

            using var ctx = new SqlStandardCallContext( TestHelper.Monitor );

            var workspace = await workspaceTable.CreateWorkspaceAsync( ctx, 1, Guid.NewGuid().ToString() );

            workspaceTable.Database.ExecuteScalar<int?>( "select 1 from CK.tZone where ZoneId = @0", workspace.WorkspaceId ).Should().Be( 1 );
            workspaceTable.Database.ExecuteScalar<int?>( "select 1 from CK.tWorkspace where WorkspaceId = @0", workspace.WorkspaceId ).Should().Be( 1 );

            await workspaceTable.UnplugWorkspaceAsync( ctx, 1, workspace.WorkspaceId );

            workspaceTable.Database.ExecuteScalar<int?>( "select 1 from CK.tZone where ZoneId = @0", workspace.WorkspaceId ).Should().Be( 1 );
            workspaceTable.Database.ExecuteScalar<int?>( "select 1 from CK.tWorkspace where WorkspaceId = @0", workspace.WorkspaceId ).Should().BeNull();
        }

        [Test]
        public async Task random_user_not_admin_cannot_create_workspace_only_PlatformAdministrators_can_Async()
        {
            var groupTable = ObtainPackage<Actor.GroupTable>();
            var package = ObtainPackage<Package>();
            var workspaceTable = ObtainPackage<WorkspaceTable>();

            using var ctx = new SqlStandardCallContext();

            int idUser = await package.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString(), 0 );

            await workspaceTable.Awaiting( _ => _.CreateWorkspaceAsync( ctx, idUser, Guid.NewGuid().ToString() ) ).Should()
                                .ThrowAsync<SqlDetailedException>();


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

        static T ObtainPackage<T>() where T : SqlPackage
        {
            return TestHelper.StObjMap.StObjs.Obtain<T>()
                ?? throw new NullReferenceException( $"Cannot obtain {typeof( T ).Name} package." );
        }
    }
}
