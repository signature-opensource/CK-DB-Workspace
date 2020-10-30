using CK.Core;
using CK.DB.Actor;
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
            var group = TestHelper.StObjMap.StObjs.Obtain<GroupTable>();
            var workspace = TestHelper.StObjMap.StObjs.Obtain<Package>();

            using var ctx = new SqlStandardCallContext( TestHelper.Monitor );

            var w = CreateWorkspaceAndOneAdministrator( ctx, group, workspace  );

            var userId = workspace.CreateUser( ctx, 1, Guid.NewGuid().ToString(), w.Workspace.WorkspaceId );

            workspace.Database.ExecuteScalar( "select 1 from CK.tActorProfile where ActorId = @0 and GroupId = @1", userId, w.Workspace.WorkspaceId ).Should().Be( 1 );
        }

        [Test]
        public void set_user_preferred_workspace_checks_thet_the_user_is_at_least_Viewer_of_the_Workspace()
        {
            var acl = TestHelper.StObjMap.StObjs.Obtain<Acl.AclTable>();
            var user = TestHelper.StObjMap.StObjs.Obtain<UserTable>();
            var group = TestHelper.StObjMap.StObjs.Obtain<GroupTable>();
            var workspace = TestHelper.StObjMap.StObjs.Obtain<Package>();

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
        public async Task destroy_workspace_cannot_be_done_if_user_exists_unless_forceDestroy_is_specified()
        {
            var userTable = TestHelper.StObjMap.StObjs.Obtain<UserTable>();
            var groupTable = TestHelper.StObjMap.StObjs.Obtain<GroupTable>();
            var workspace = TestHelper.StObjMap.StObjs.Obtain<Package>();

            using var ctx = new SqlStandardCallContext( TestHelper.Monitor );

            // With forceDestroy: true
            {
                var w = CreateWorkspaceAndOneAdministrator( ctx, groupTable, workspace );
                await workspace.WorkspaceTable.Awaiting( _ => _.DestroyWorkspaceAsync( ctx, 1, w.Workspace.WorkspaceId, forceDestroy: true ) ).Should().NotThrowAsync();
                workspace.Database.ExecuteScalar( "select 1 from CK.vWorkspace where WorkspaceId = @0", w.Workspace.WorkspaceId ).Should().BeNull();
            }
            // With forceDestroy: false
            {
                var w = CreateWorkspaceAndOneAdministrator( ctx, groupTable, workspace );

                // There is the administrator User in the zone. Without forceDestroy, this fails.
                await workspace.WorkspaceTable.Awaiting( _ => _.DestroyWorkspaceAsync( ctx, 1, w.Workspace.WorkspaceId ) ).Should()
                        .ThrowAsync<SqlDetailedException>();

                // Destroying the Admin user...
                await userTable.DestroyUserAsync( ctx, 1, w.AdminUserId );
                // So now it can be destroyed, even with forceDestroy false.
                await workspace.WorkspaceTable.Awaiting( _ => _.DestroyWorkspaceAsync( ctx, 1, w.Workspace.WorkspaceId, forceDestroy: false ) ).Should().NotThrowAsync();

                workspace.Database.ExecuteScalar( "select 1 from CK.vWorkspace where WorkspaceId = @0", w.Workspace.WorkspaceId ).Should().BeNull();
            }
        }

        [Test]
        public async Task random_user_not_admin_cannot_create_workspace_only_PlatformAdministrators_can()
        {
            var groupTable = TestHelper.StObjMap.StObjs.Obtain<GroupTable>();
            var package = TestHelper.StObjMap.StObjs.Obtain<Package>();
            var workspaceTable = TestHelper.StObjMap.StObjs.Obtain<WorkspaceTable>();

            using var ctx = new SqlStandardCallContext();

            int idUser = package.CreateUser( ctx, 1, Guid.NewGuid().ToString(), 0 );

            await workspaceTable.Awaiting( _ => _.CreateWorkspaceAsync( ctx, idUser, Guid.NewGuid().ToString() ) ).Should()
                                .ThrowAsync<SqlDetailedException>();


        }

        (WorkspaceTable.NamedWorkspace Workspace, int AdminGroupId, int AdminUserId) CreateWorkspaceAndOneAdministrator( ISqlCallContext ctx, GroupTable group, Package workspace )
        {
            var w = workspace.WorkspaceTable.CreateWorkspace( ctx, 1, "TestWorkspace" );
            var uId = workspace.CreateUser( ctx, 1, $"Admin-{w.Name}-{Guid.NewGuid()}", w.WorkspaceId );
            var gId = workspace.Database.ExecuteScalar<int>( "select AdminGroupId from CK.tWorkspace where WorkspaceId = @0", w.WorkspaceId );
            // The new admin is already a Zone member...
            // ...so we can add it to the group's zone.
            group.AddUser( ctx, 1, gId, uId );
            return (w, gId, uId);
        }

    }
}
