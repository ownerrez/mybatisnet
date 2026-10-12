using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using IBatisNet.Common.Utilities;
using IBatisNet.DataMapper;
using IBatisNet.DataMapper.Configuration;
using MySqlConnector;
using ORBatis.Test.Common;
using ORBatis.Test.Common.Infrastructure;
using ORBatis.Test.Common.Models;
using Xunit;

namespace ORBatis.Test.Tests;

[Collection("Database")]
public class GeneratedKeyReaderFacts : IDisposable
{
    // Above any Holiday id the test database has generated, so a key that reached the entity came from the reader.
    const long ReaderKey = 2_000_000_000;

    public GeneratedKeyReaderFacts() => GeneratedKeyReader.Reset();

    public void Dispose() => GeneratedKeyReader.Reset();

    private static ISqlMapper BuildMapper(string config)
    {
        var sqlMapConfig = Resources.GetEmbeddedResourceAsXmlDocument($"ORBatis.Test.Config.{config}, ORBatis.Test");
        var builder = new DomSqlMapBuilder()
        {
            Properties = new NameValueCollection()
            {
                { "ConnectionString", Constants.ConnectionString }
            }
        };
        return builder.Configure(sqlMapConfig);
    }

    private static Holiday CreateTestHoliday() => new Holiday
    {
        UserId = 347317427,
        Name = "Test" + Guid.NewGuid().ToString("N").Substring(0, 8),
        AllowAllProperties = true,
        Active = true,
        CreatedUtc = DateTime.UtcNow,
        CreatedUserId = 347317427,
        UpdatedUtc = DateTime.UtcNow,
        UpdatedUserId = 347317427
    };

    [Fact]
    public void Insert_TakesTheReadersKey_AndSkipsThePostSelectKey()
    {
        var mapper = BuildMapper("SqlMapGeneratedKeys.config");
        long insertedId = 0;
        GeneratedKeyReader.Read = command =>
        {
            insertedId = ((MySqlCommand)command).LastInsertedId;
            return ReaderKey;
        };

        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            var holiday = CreateTestHoliday();
            var key = mapper.Insert("Holiday.Insert", holiday, session);

            try
            {
                Assert.Equal((int)ReaderKey, key);
                Assert.Equal((int)ReaderKey, holiday.Id);
                Assert.Equal(1, GeneratedKeyReader.Calls);
                Assert.Equal(holiday.Name, mapper.QueryForObject<Holiday>("Holiday.Select", (int)insertedId, session)?.Name);
            }
            finally
            {
                mapper.Delete("Holiday.Delete", (int)insertedId, session);
            }
        }
    }

    [Fact]
    public async Task InsertAsync_TakesTheReadersKey_AndSkipsThePostSelectKey()
    {
        var mapper = BuildMapper("SqlMapGeneratedKeys.config");
        long insertedId = 0;
        GeneratedKeyReader.Read = command =>
        {
            insertedId = ((MySqlCommand)command).LastInsertedId;
            return ReaderKey;
        };

        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            var holiday = CreateTestHoliday();
            var key = await mapper.InsertAsync("Holiday.Insert", holiday, session);

            try
            {
                Assert.Equal((int)ReaderKey, key);
                Assert.Equal((int)ReaderKey, holiday.Id);
                Assert.Equal(1, GeneratedKeyReader.Calls);
                Assert.Equal(holiday.Name, (await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", (int)insertedId, session))?.Name);
            }
            finally
            {
                await mapper.DeleteAsync("Holiday.Delete", (int)insertedId, session);
            }
        }
    }

    [Fact]
    public void Insert_RunsThePostSelectKey_WhenTheReaderHasNoKey()
    {
        var mapper = BuildMapper("SqlMapGeneratedKeys.config");
        long insertedId = 0;
        GeneratedKeyReader.Read = command =>
        {
            insertedId = ((MySqlCommand)command).LastInsertedId;
            return null;
        };

        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            var holiday = CreateTestHoliday();
            var key = (int)mapper.Insert("Holiday.Insert", holiday, session);

            try
            {
                Assert.Equal(1, GeneratedKeyReader.Calls);
                Assert.Equal(insertedId, key);
                Assert.Equal(key, holiday.Id);
            }
            finally
            {
                mapper.Delete("Holiday.Delete", key, session);
            }
        }
    }

    [Fact]
    public async Task InsertAsync_RunsThePostSelectKey_WhenTheReaderHasNoKey()
    {
        var mapper = BuildMapper("SqlMapGeneratedKeys.config");
        long insertedId = 0;
        GeneratedKeyReader.Read = command =>
        {
            insertedId = ((MySqlCommand)command).LastInsertedId;
            return null;
        };

        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            var holiday = CreateTestHoliday();
            var key = (int)await mapper.InsertAsync("Holiday.Insert", holiday, session);

            try
            {
                Assert.Equal(1, GeneratedKeyReader.Calls);
                Assert.Equal(insertedId, key);
                Assert.Equal(key, holiday.Id);
            }
            finally
            {
                await mapper.DeleteAsync("Holiday.Delete", key, session);
            }
        }
    }

    [Fact]
    public void Insert_WithNoReaderConfigured_RunsThePostSelectKey()
    {
        var mapper = BuildMapper("SqlMap.config");
        GeneratedKeyReader.Read = _ => ReaderKey;

        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            var holiday = CreateTestHoliday();
            var key = (int)mapper.Insert("Holiday.Insert", holiday, session);

            try
            {
                Assert.Equal(0, GeneratedKeyReader.Calls);
                Assert.Equal(key, holiday.Id);
                Assert.Equal(holiday.Name, mapper.QueryForObject<Holiday>("Holiday.Select", key, session)?.Name);
            }
            finally
            {
                mapper.Delete("Holiday.Delete", key, session);
            }
        }
    }
}
