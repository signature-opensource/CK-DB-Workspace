using CK.Core;
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
        #region Test OK

        [Test]
        public async Task create_workspace()
        {
            DropAllWorkspaces();

            var workspaceTable = TestHelper.StObjMap.StObjs.Obtain<WorkspaceTable>();

            using var ctx = new SqlStandardCallContext( TestHelper.Monitor );
            string workspaceName = Guid.NewGuid().ToString();

            var actorId = GetPlateformAdministratorId();

            var workspace = await workspaceTable.CreateWorkspaceAsync( ctx, actorId, workspaceName );
            var workspaceCount = workspaceTable.Database.ExecuteScalar<int>(
                "select count(*) from CK.vWorkspace where WorkspaceId = @0 and WorkspaceName = @1", workspace.WorkspaceId, workspaceName );

            workspace.WorkspaceId.Should().NotBe( 0 );
            workspace.Name.Should().Be( workspaceName );
            workspaceCount.Should().Be( 1 );
        }

        [Test]
        public async Task destroy_workspace()
        {
            var workspaceTable = TestHelper.StObjMap.StObjs.Obtain<WorkspaceTable>();

            using var ctx = new SqlStandardCallContext( TestHelper.Monitor );
            string workspaceName = Guid.NewGuid().ToString();
            var actorId = GetPlateformAdministratorId();

            var workspace = await workspaceTable.CreateWorkspaceAsync( ctx, actorId, workspaceName );

            await workspaceTable.DestroyWorkspaceAsync( ctx, actorId, workspace.WorkspaceId );

            var workspaceCount = workspaceTable.Database.ExecuteScalar<int>(
                "select count(*) from CK.vWorkspace where WorkspaceId = @0", workspace.WorkspaceId );

            workspaceCount.Should().Be( 0 );
        }

        [Test]
        public async Task create_user_with_preferred_workspace_id()
        {
            var package = TestHelper.StObjMap.StObjs.Obtain<Package>();
            var workspaceTable = TestHelper.StObjMap.StObjs.Obtain<WorkspaceTable>();

            using var ctx = new SqlStandardCallContext( TestHelper.Monitor );

            var workspace = await workspaceTable.CreateWorkspaceAsync( ctx, GetPlateformAdministratorId(), Guid.NewGuid().ToString() );
            var userId = await package.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString(), workspace.WorkspaceId );

            var userPreferredWorkspaceId = workspaceTable.Database.ExecuteScalar<int>(
                "select top 1 PreferredWorkspaceId from CK.vUser where UserId = @0", userId );

            userPreferredWorkspaceId.Should().Be( workspace.WorkspaceId );
        }

        [Test]
        public async Task user_preferred_workspace_id_should_be_0_if_workspace_is_destroy()
        {
            var package = TestHelper.StObjMap.StObjs.Obtain<Package>();
            var workspaceTable = TestHelper.StObjMap.StObjs.Obtain<WorkspaceTable>();
            var actorId = GetPlateformAdministratorId();

            using var ctx = new SqlStandardCallContext( TestHelper.Monitor );

            var workspace = await workspaceTable.CreateWorkspaceAsync( ctx, actorId, Guid.NewGuid().ToString() );
            var userId = await package.CreateUserAsync( ctx, actorId, Guid.NewGuid().ToString(), workspace.WorkspaceId );

            var userPreferredWorkspaceId = workspaceTable.Database.ExecuteScalar<int>(
                "select top 1 PreferredWorkspaceId from CK.vUser where UserId = @0", userId );

            userPreferredWorkspaceId.Should().Be( workspace.WorkspaceId );

            await workspaceTable.DestroyWorkspaceAsync( ctx, actorId, workspace.WorkspaceId );

            userPreferredWorkspaceId = workspaceTable.Database.ExecuteScalar<int>(
                "select top 1 PreferredWorkspaceId from CK.vUser where UserId = @0", userId );

            userPreferredWorkspaceId.Should().Be( 0 );
        }

        [Test]
        public async Task set_user_preferred_workspace_id()
        {
            var package = TestHelper.StObjMap.StObjs.Obtain<Package>();
            var workspaceTable = TestHelper.StObjMap.StObjs.Obtain<WorkspaceTable>();
            var actorId = GetPlateformAdministratorId();

            using var ctx = new SqlStandardCallContext( TestHelper.Monitor );

            var workspace = await workspaceTable.CreateWorkspaceAsync( ctx, actorId, Guid.NewGuid().ToString() );
            var userId = await  package.CreateUserAsync( ctx, actorId, Guid.NewGuid().ToString(), 0 );

            var userPreferredWorkspaceId = workspaceTable.Database.ExecuteScalar<int>(
                "select top 1 PreferredWorkspaceId from CK.vUser where UserId = @0", userId );

            userPreferredWorkspaceId.Should().Be( 0 );

            await package.SetUserPreferredWorkspaceAsync( ctx, actorId, userId, workspace.WorkspaceId );

            userPreferredWorkspaceId = workspaceTable.Database.ExecuteScalar<int>(
                "select top 1 PreferredWorkspaceId from CK.vUser where UserId = @0", userId );

            userPreferredWorkspaceId.Should().Be( workspace.WorkspaceId );
        }

        #endregion

        #region Test Error

        [Test]
        public async Task actor_not_admin_cant_create_workspace()
        {
            var package = TestHelper.StObjMap.StObjs.Obtain<Package>();
            var workspaceTable = TestHelper.StObjMap.StObjs.Obtain<WorkspaceTable>();

            using var ctx = new SqlStandardCallContext();

            var actorId = package.Database.ExecuteScalar<int>( @"select top 1 ac.ActorId
                                                           from CK.tAclConfig ac
                                                           join CKCore.tSystem s on ac.AclId = s.SystemAclId
                                                           where ac.GrantLevel < 112" );

            Func<Task> sut = async () => await workspaceTable.CreateWorkspaceAsync( ctx, actorId, Guid.NewGuid().ToString() );
            await sut.Should().ThrowAsync<SqlDetailedException>();
        }

        #endregion

        void DropAllWorkspaces()
        {
            var p = TestHelper.StObjMap.StObjs.Obtain<Package>();
            p.Database.ExecuteNonQuery( @"while 1 = 1
	                                      begin
	                                          declare @WorkspaceId int = null;
	                                          declare @ActorId int = null;
	                                          
	                                      	  select top 1 @WorkspaceId = WorkspaceId from CK.tWorkspace where WorkspaceId > 2;
	                                          
	                                      	  if @WorkspaceId is null break;
	                                          
	                                      	  select top 1 @ActorId = a.ActorId
	                                          from CK.tAclConfigMemory a join CK.tWorkspace w on a.AclId = w.AclId
	                                          where a.GrantLevel = 127 and w.WorkspaceId = @WorkspaceId;
	                                          
	                                      	  exec CK.sWorkspaceDestroy @ActorId, @WorkspaceId;
	                                      end" );
        }

        int GetPlateformAdministratorId()
        {
            var p = TestHelper.StObjMap.StObjs.Obtain<Package>();
            var actorId = p.Database.ExecuteScalar<int>( @"select top 1 ac.ActorId
                                                           from CK.tAclConfig ac
                                                           join CKCore.tSystem s on ac.AclId = s.SystemAclId
                                                           where ac.GrantLevel >= 112" );
            return actorId;
        }
    }
}
