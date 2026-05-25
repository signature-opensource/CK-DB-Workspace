using CK.Core;
using CK.Cris;
using CK.DB.Actor;
using CK.DB.Zone;
using CK.IO.Workspace;
using CK.SqlServer;
using CK.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using System;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.Workspace.Tests;

[TestFixture]
public class WorkspaceCrisTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    AsyncServiceScope _scope;
    PocoDirectory _pocoDir;
    CrisExecutionContext _exec;
    WorkspaceTable _workspaceTable;
    ZoneTable _zoneTable;
#pragma warning restore CS8618

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _scope = SharedEngine.AutomaticServices.CreateAsyncScope();
        var services = _scope.ServiceProvider;
        _exec = services.GetRequiredService<CrisExecutionContext>();
        _pocoDir = services.GetRequiredService<PocoDirectory>();
        _workspaceTable = services.GetRequiredService<WorkspaceTable>();
        _zoneTable = services.GetRequiredService<ZoneTable>();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDownAsync()
    {
        await _scope.DisposeAsync();
    }

    [Test]
    public async Task can_create_workspace_Async()
    {
        var name = NewName();
        var executedCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<ICreateWorkspaceCommand>( c =>
        {
            c.ActorId = 1;
            c.WorkspaceName = name;
        } ) );

        var res = executedCmd.WithResult<ICreateWorkspaceCommandResult>().Result;
        res.ShouldNotBeNull();
        res.Success.ShouldBeTrue();
        res.WorkspaceIdResult.ShouldBeGreaterThan( 3 );
        res.WorkspaceName.ShouldBe( name );
        res.UserMessages.ShouldBeEmpty();
    }

    [Test]
    public async Task create_workspace_with_clashing_name_returns_suffixed_name_Async()
    {
        var name = NewName();
        var first = await CreateWorkspaceAsync( name );

        var executedCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<ICreateWorkspaceCommand>( c =>
        {
            c.ActorId = 1;
            c.WorkspaceName = name;
        } ) );
        var res = executedCmd.WithResult<ICreateWorkspaceCommandResult>().Result;
        res.ShouldNotBeNull();
        res.Success.ShouldBeTrue();
        res.WorkspaceIdResult.ShouldNotBe( first.WorkspaceIdResult );
        res.WorkspaceName.ShouldStartWith( name );
        res.WorkspaceName.ShouldNotBe( name );
    }

    [Test]
    public async Task can_destroy_workspace_Async()
    {
        var created = await CreateWorkspaceAsync();

        var execDestroyCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<IDestroyWorkspaceCommand>( c =>
        {
            c.ActorId = 1;
            c.TargetWorkspaceId = created.WorkspaceIdResult;
        } ) );

        var destroyRes = execDestroyCmd.WithResult<ICrisBasicCommandResult>().Result;
        destroyRes.ShouldNotBeNull();
        destroyRes.Success.ShouldBeTrue();

        WorkspaceExists( created.WorkspaceIdResult ).ShouldBeFalse();
    }

    [Test]
    public async Task cannot_destroy_workspace_with_id_lesser_than_or_equal_to_3_Async()
    {
        var execDestroyCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<IDestroyWorkspaceCommand>( c =>
        {
            c.ActorId = 1;
            c.TargetWorkspaceId = 3; // AdminZone is reserved.
        } ) );
        var destroyRes = execDestroyCmd.Result.ShouldNotBeNull();
        destroyRes.ShouldBeAssignableTo<ICrisResultError>().Errors.ShouldNotBeEmpty();
    }

    [Test]
    public async Task can_plug_workspace_on_existing_zone_Async()
    {
        int zoneId;
        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {
            zoneId = await _zoneTable.CreateZoneAsync( ctx, 1 );
        }
        WorkspaceExists( zoneId ).ShouldBeFalse();

        var executedCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<IPlugWorkspaceCommand>( c =>
        {
            c.ActorId = 1;
            c.ZoneId = zoneId;
        } ) );
        var res = executedCmd.WithResult<ICrisBasicCommandResult>().Result;
        res.ShouldNotBeNull();
        res.Success.ShouldBeTrue();

        WorkspaceExists( zoneId ).ShouldBeTrue();
    }

    [Test]
    public async Task can_unplug_workspace_keeps_zone_Async()
    {
        var created = await CreateWorkspaceAsync();
        WorkspaceExists( created.WorkspaceIdResult ).ShouldBeTrue();

        var executedCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<IUnplugWorkspaceCommand>( c =>
        {
            c.ActorId = 1;
            c.TargetWorkspaceId = created.WorkspaceIdResult;
        } ) );
        var res = executedCmd.WithResult<ICrisBasicCommandResult>().Result;
        res.ShouldNotBeNull();
        res.Success.ShouldBeTrue();

        WorkspaceExists( created.WorkspaceIdResult ).ShouldBeFalse();
        _workspaceTable.Database.ExecuteScalar<int?>( "select 1 from CK.tZone where ZoneId = @0", created.WorkspaceIdResult )
            .ShouldBe( 1 );
    }

    [Test]
    public async Task can_rename_workspace_Async()
    {
        var created = await CreateWorkspaceAsync();

        var newName = NewName();
        var executedCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<IRenameWorkspaceCommand>( c =>
        {
            c.ActorId = 1;
            c.TargetWorkspaceId = created.WorkspaceIdResult;
            c.WorkspaceName = newName;
        } ) );

        var res = executedCmd.WithResult<IRenameWorkspaceCommandResult>().Result;
        res.ShouldNotBeNull();
        res.Success.ShouldBeTrue();
        res.WorkspaceName.ShouldBe( newName );

        _workspaceTable.Database.ExecuteScalar<string>(
            "select GroupName from CK.tGroup where GroupId = @0", created.WorkspaceIdResult )
            .ShouldBe( newName );
    }

    [Test]
    public async Task rename_workspace_with_clashing_name_returns_suffixed_name_Async()
    {
        var sharedName = NewName();
        await CreateWorkspaceAsync( sharedName );
        var second = await CreateWorkspaceAsync();

        var executedCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<IRenameWorkspaceCommand>( c =>
        {
            c.ActorId = 1;
            c.TargetWorkspaceId = second.WorkspaceIdResult;
            c.WorkspaceName = sharedName;
        } ) );
        var res = executedCmd.WithResult<IRenameWorkspaceCommandResult>().Result;
        res.ShouldNotBeNull();
        res.Success.ShouldBeTrue();
        res.WorkspaceName.ShouldStartWith( sharedName );
        res.WorkspaceName.ShouldNotBe( sharedName );
    }

    [Test]
    public async Task incoming_validator_rejects_empty_workspace_name_on_create_Async()
    {
        var executedCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<ICreateWorkspaceCommand>( c =>
        {
            c.ActorId = 1;
            c.WorkspaceName = "";
        } ) );
        var res = executedCmd.Result.ShouldNotBeNull();
        res.ShouldBeAssignableTo<ICrisResultError>().Errors.ShouldNotBeEmpty();
    }

    [Test]
    public async Task incoming_validator_rejects_invalid_zone_on_plug_Async()
    {
        var executedCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<IPlugWorkspaceCommand>( c =>
        {
            c.ActorId = 1;
            c.ZoneId = 0;
        } ) );
        var res = executedCmd.Result.ShouldNotBeNull();
        res.ShouldBeAssignableTo<ICrisResultError>().Errors.ShouldNotBeEmpty();
    }

    async Task<ICreateWorkspaceCommandResult> CreateWorkspaceAsync( string? name = null )
    {
        name ??= NewName();
        var executedCmd = await _exec.ExecuteRootCommandAsync( (IAbstractCommand)_pocoDir.Create<ICreateWorkspaceCommand>( c =>
        {
            c.ActorId = 1;
            c.WorkspaceName = name;
        } ) );
        var res = executedCmd.WithResult<ICreateWorkspaceCommandResult>().Result;
        res.ShouldNotBeNull().Success.ShouldBeTrue();
        return res!;
    }

    bool WorkspaceExists( int workspaceId )
        => _workspaceTable.Database.ExecuteScalar<int>(
            "select isnull( (select 1 from CK.tWorkspace where WorkspaceId = @0), 0 );",
            workspaceId ) > 0;

    static string NewName() => $"CrisWs-{Guid.NewGuid():N}".Substring( 0, 32 );
}
