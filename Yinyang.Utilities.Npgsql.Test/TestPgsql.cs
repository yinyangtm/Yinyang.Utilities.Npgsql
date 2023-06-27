using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yinyang.Utilities.Npgsql;

namespace Yinyang.Utilities.NpgsqlTest
{

    [TestClass]
    public class TestPgsql
    {
        private readonly string _connectionString;

        private IConfiguration Configuration { get; }

        public TestPgsql()
        {
            Configuration = new ConfigurationBuilder()
                .AddUserSecrets<TestPgsql>()
                .AddEnvironmentVariables()
                .Build();

            _connectionString = Configuration["pgsql"];
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentException();
            }
        }

        [TestMethod]
        public void Select()
        {
            using (var pgsql = new Pgsql(_connectionString))
            {
                pgsql.Open();
                pgsql.CommandText = "select * from test where \"id\" = 1;";
                var result = pgsql.ExecuteReader<EntityTest>().First();

                var answer = new EntityTest {id = 1, key = 1, value = "あいうえお"};
                Assert.AreEqual(answer.id, result.id);
                Assert.AreEqual(answer.key, result.key);
                Assert.AreEqual(answer.value, result.value);
                pgsql.Close();
            }
        }

        [TestMethod]
        public void ExecuteReaderFirst()
        {
            using (var pgsql = new Pgsql(_connectionString))
            {
                pgsql.Open();
                pgsql.CommandText = "select * from test where \"id\" = 1;";
                var result = pgsql.ExecuteReaderFirst<EntityTest>();

                var answer = new EntityTest {id = 1, key = 1, value = "あいうえお"};
                Assert.AreEqual(answer.id, result.id);
                Assert.AreEqual(answer.key, result.key);
                Assert.AreEqual(answer.value, result.value);
                pgsql.Close();
            }
        }

        [TestMethod]
        public void EasySelect()
        {
            Pgsql.ConnectionString = _connectionString;
            using (var pgsql = new Pgsql(_connectionString))
            {
                var result = pgsql.EasySelect<EntityTest>("select * from test where \"id\" = 1;").First();

                var answer = new EntityTest {id = 1, key = 1, value = "あいうえお"};
                Assert.AreEqual(answer.id, result.id);
                Assert.AreEqual(answer.key, result.key);
                Assert.AreEqual(answer.value, result.value);
                pgsql.Close();
            }
        }

        [TestMethod]
        public void SelectCount()
        {
            using (var pgsql = new Pgsql(_connectionString))
            {
                pgsql.Open();
                pgsql.CommandText = "select count(*) from test where \"id\" = 1;";
                var result = pgsql.ExecuteScalarToInt();

                Assert.AreEqual(1, result);
                pgsql.Close();
            }
        }

        [TestMethod]
        public void TableRowsCount()
        {
            using (var pgsql = new Pgsql(_connectionString))
            {
                pgsql.Open();
                Assert.AreEqual(1, pgsql.TableRowsCount("test"));
                pgsql.Close();
            }
        }

        /// <summary>
        /// Breaking changes :<see href="https://www.npgsql.org/doc/release-notes/7.0.html#commandtypestoredprocedure-now-invokes-procedures-instead-of-functions">Npgsql 7.0</see>
        /// </summary>
        [TestMethod]
        public void Function()
        {
            using (var SqlServer = new Pgsql(_connectionString))
            {
                SqlServer.Open();

                SqlServer.CommandText = "SELECT * FROM gettestdata(@uid);";
                SqlServer.AddParameter("@uid", 1);
                var result = SqlServer.ExecuteReaderFirst<EntityTest>();

                var answer = new EntityTest {id = 1, key = 1, value = "あいうえお"};
                Assert.AreEqual(answer.id, result.id);
                Assert.AreEqual(answer.key, result.key);
                Assert.AreEqual(answer.value, result.value);

                SqlServer.Close();
            }
        }
    }
}
