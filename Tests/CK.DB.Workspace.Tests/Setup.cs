using static CK.Testing.MonitorTestHelper;
using NUnit.Framework;

namespace CK.DB.Workspace.Tests
{
    [TestFixture]
    public class Setup
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            if( TestHelper.EnsureDatabase() ) return;

            TestHelper.DropDatabase();
            TestHelper.RunDBSetup();
        }
    }
}
