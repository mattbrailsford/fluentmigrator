using FirebirdSql.Data.FirebirdClient;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Tests.Helpers;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    [TestFixture]
    [Category("Integration")]
    [Category("Firebird")]
    public class FirebirdColumnTests : BaseColumnTests
    {
        private readonly FirebirdLibraryProber _prober = new FirebirdLibraryProber();
        private TemporaryDatabase _temporaryDatabase;

        public FbConnection Connection { get; set; }
        public FirebirdProcessor Processor { get; set; }
        public IQuoter Quoter { get; set; }

        [SetUp]
        public void SetUp()
        {
            if (!IntegrationTestOptions.Firebird.IsEnabled)
                Assert.Ignore();
            _temporaryDatabase = new TemporaryDatabase(
                IntegrationTestOptions.Firebird,
                _prober);
            Connection = new FbConnection(_temporaryDatabase.ConnectionString);
            var options = FirebirdOptions.AutoCommitBehaviour();
            Processor = new FirebirdProcessor(Connection, new FirebirdGenerator(options), new TextWriterAnnouncer(TestContext.Out), new ProcessorOptions(), new FirebirdDbFactory(), options);
            Quoter = new FirebirdQuoter(false);
            Connection.Open();
            Processor.BeginTransaction();
        }

        [TearDown]
        public void TearDown()
        {
            if (Processor == null)
                return;

            if (!Processor.WasCommitted)
                Processor.CommitTransaction();
            Connection.Close();

            FbDatabase.DropDatabase(_temporaryDatabase.ConnectionString);
        }

        [Test]
        public override void CallingColumnExistsCanAcceptColumnNameWithSingleQuote()
        {
            var columnNameWithSingleQuote = "\"i'd\"";
            using (var table = new FirebirdTestTable(Processor, null, string.Format("{0} int", columnNameWithSingleQuote)))
                Processor.ColumnExists(null, table.Name, "\"i'd\"").ShouldBeTrue();
        }

        [Test]
        public override void CallingColumnExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new FirebirdTestTable("\"Test'Table\"", Processor, null, "id int"))
                Processor.ColumnExists(null, table.Name, "ID").ShouldBeTrue();
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int"))
                Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "TestSchema", "id int"))
                Processor.ColumnExists("TestSchema", table.Name, "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ColumnExists("TestSchema", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExists()
        {
            using (var table = new FirebirdTestTable(Processor, null, "id int"))
                Processor.ColumnExists(null, table.Name, "ID").ShouldBeTrue();
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
        {
            using (var table = new FirebirdTestTable(Processor, "TestSchema", "id int"))
                Processor.ColumnExists("TestSchema", table.Name, "ID").ShouldBeTrue();
        }
    }
}
