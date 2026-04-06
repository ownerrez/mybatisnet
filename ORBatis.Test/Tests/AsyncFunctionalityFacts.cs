using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using IBatisNet.Common.Utilities;
using IBatisNet.DataMapper;
using IBatisNet.DataMapper.Configuration;
using ORBatis.Test.Common;
using ORBatis.Test.Common.Models;
using Xunit;

namespace ORBatis.Test.Tests;

/// <summary>
///     Tests specific to async behavior that don't fit the shared sync/async CRUD base class:
///     mixed sync/async usage, await using, and sync/async result parity.
/// </summary>
public class AsyncFunctionalityFacts
{
    private static ISqlMapper BuildMapper()
    {
        var path = "ORBatis.Test.Config.SqlMap.config, ORBatis.Test";
        var sqlMapConfig = Resources.GetEmbeddedResourceAsXmlDocument(path);
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

    #region Sync/Async Result Parity
    [Fact]
    public async Task QueryForObject_SyncAndAsync_ReturnSameResult()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await session.OpenConnectionAsync();

        var holiday = CreateTestHoliday();
        var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

        try
        {
            var syncResult = mapper.QueryForObject<Holiday>("Holiday.Select", id, session);
            var asyncResult = await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", id, session);

            Assert.Equal(syncResult.Id, asyncResult.Id);
            Assert.Equal(syncResult.Name, asyncResult.Name);
            Assert.Equal(syncResult.UserId, asyncResult.UserId);
        }
        finally
        {
            await mapper.DeleteAsync("Holiday.Delete", id, session);
        }
    }

    [Fact]
    public async Task QueryForList_SyncAndAsync_ReturnSameResults()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await session.OpenConnectionAsync();

        var parameters = new Hashtable { { "active", true }, { "userId", 347317427 } };

        // Insert a known row so we have a guaranteed match
        var holiday = CreateTestHoliday();
        var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

        try
        {
            var syncResults = mapper.QueryForList<Holiday>("Holiday.SelectAll", parameters, session);
            var asyncResults = await mapper.QueryForListAsync<Holiday>("Holiday.SelectAll", parameters, session);

            // Both should contain our known row
            Assert.Contains(syncResults, h => h.Id == id);
            Assert.Contains(asyncResults, h => h.Id == id);

            // Verify the known row has the same data in both
            var syncRow = syncResults.First(h => h.Id == id);
            var asyncRow = asyncResults.First(h => h.Id == id);
            Assert.Equal(syncRow.Name, asyncRow.Name);
            Assert.Equal(syncRow.UserId, asyncRow.UserId);
        }
        finally
        {
            await mapper.DeleteAsync("Holiday.Delete", id, session);
        }
    }

    [Fact]
    public async Task Insert_SyncAndAsync_BothProduceQueryableRows()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await session.OpenConnectionAsync();

        var syncHoliday = CreateTestHoliday();
        var syncId = (int)mapper.Insert("Holiday.Insert", syncHoliday, session);

        var asyncHoliday = CreateTestHoliday();
        var asyncId = (int)(await mapper.InsertAsync("Holiday.Insert", asyncHoliday, session));

        try
        {
            Assert.True(syncId > 0);
            Assert.True(asyncId > 0);
            Assert.NotEqual(syncId, asyncId);

            var syncFetched = await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", syncId, session);
            var asyncFetched = await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", asyncId, session);

            Assert.Equal(syncHoliday.Name, syncFetched.Name);
            Assert.Equal(asyncHoliday.Name, asyncFetched.Name);
        }
        finally
        {
            await mapper.DeleteAsync("Holiday.Delete", syncId, session);
            await mapper.DeleteAsync("Holiday.Delete", asyncId, session);
        }
    }
    #endregion

    #region Mixed Sync/Async
    [Fact]
    public async Task MixedSyncAsync_SyncInsertAsyncQuery()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await session.OpenConnectionAsync();

        var holiday = CreateTestHoliday();
        var id = (int)mapper.Insert("Holiday.Insert", holiday, session);

        try
        {
            var result = await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", id, session);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
        }
        finally
        {
            await mapper.DeleteAsync("Holiday.Delete", id, session);
        }
    }

    [Fact]
    public async Task MixedSyncAsync_AsyncInsertSyncQuery()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await session.OpenConnectionAsync();

        var holiday = CreateTestHoliday();
        var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

        try
        {
            var result = mapper.QueryForObject<Holiday>("Holiday.Select", id, session);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
        }
        finally
        {
            await mapper.DeleteAsync("Holiday.Delete", id, session);
        }
    }

    [Fact]
    public async Task MixedSyncAsync_AsyncOpenConnectionThenSyncOperations()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await session.OpenConnectionAsync();

        var holiday = CreateTestHoliday();
        var id = (int)mapper.Insert("Holiday.Insert", holiday, session);

        try
        {
            var result = mapper.QueryForObject<Holiday>("Holiday.Select", id, session);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
        }
        finally
        {
            mapper.Delete("Holiday.Delete", id, session);
        }
    }
    #endregion

    #region AwaitUsing
    [Fact]
    public async Task AwaitUsing_DisposesSessionCorrectly()
    {
        var mapper = BuildMapper();
        ISqlMapSession session;

        await using (session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            var holiday = CreateTestHoliday();
            var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

            var fetched = await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", id, session);
            Assert.NotNull(fetched);

            await mapper.DeleteAsync("Holiday.Delete", id, session);
        }

        Assert.True(
            session.Connection == null || session.Connection.State == System.Data.ConnectionState.Closed
        );
    }
    #endregion
}
