using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Threading;
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

    #region QueryForDictionaryAsync
    [Fact]
    public async Task QueryForDictionaryAsync_ReturnsDictionaryKeyedByProperty()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await session.OpenConnectionAsync();

        var holiday = CreateTestHoliday();
        var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

        try
        {
            var parameters = new Hashtable { { "userId", 347317427 } };
            var dict = await mapper.QueryForDictionaryAsync<int, Holiday>(
                "Holiday.SelectAll", parameters, "Id", null, session);

            Assert.NotNull(dict);
            Assert.True(dict.ContainsKey(id));
            Assert.Equal(holiday.Name, dict[id].Name);
        }
        finally
        {
            await mapper.DeleteAsync("Holiday.Delete", id, session);
        }
    }

    [Fact]
    public async Task QueryForDictionaryAsync_WithValueProperty_ReturnsScalarValues()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await session.OpenConnectionAsync();

        var holiday = CreateTestHoliday();
        var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

        try
        {
            var parameters = new Hashtable { { "userId", 347317427 } };
            var dict = await mapper.QueryForDictionaryAsync<int, string>(
                "Holiday.SelectAll", parameters, "Id", "Name", session);

            Assert.NotNull(dict);
            Assert.True(dict.ContainsKey(id));
            Assert.Equal(holiday.Name, dict[id]);
        }
        finally
        {
            await mapper.DeleteAsync("Holiday.Delete", id, session);
        }
    }

    [Fact]
    public async Task QueryForDictionaryAsync_WithRowDelegate_InvokesDelegate()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await session.OpenConnectionAsync();

        var holiday = CreateTestHoliday();
        var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

        try
        {
            var parameters = new Hashtable { { "userId", 347317427 } };
            var callCount = 0;

            var dict = await mapper.QueryForDictionaryAsync<int, Holiday>(
                "Holiday.SelectAll", parameters, "Id", null,
                (key, value, param, dictionary) =>
                {
                    callCount++;
                    dictionary[key] = value;
                },
                session);

            Assert.True(callCount > 0);
            Assert.True(dict.ContainsKey(id));
        }
        finally
        {
            await mapper.DeleteAsync("Holiday.Delete", id, session);
        }
    }

    [Fact]
    public async Task QueryForDictionaryAsync_SyncAndAsync_ReturnSameResults()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await session.OpenConnectionAsync();

        var holiday = CreateTestHoliday();
        var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

        try
        {
            var parameters = new Hashtable { { "userId", 347317427 } };

            var syncDict = mapper.QueryForDictionary<int, string>(
                "Holiday.SelectAll", parameters, "Id", "Name", session);
            var asyncDict = await mapper.QueryForDictionaryAsync<int, string>(
                "Holiday.SelectAll", parameters, "Id", "Name", session);

            Assert.Equal(syncDict.Count, asyncDict.Count);
            Assert.Equal(syncDict[id], asyncDict[id]);
        }
        finally
        {
            await mapper.DeleteAsync("Holiday.Delete", id, session);
        }
    }
    #endregion

    #region QueryWithRowDelegateAsync
    [Fact]
    public async Task QueryWithRowDelegateAsync_InvokesDelegatePerRow()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await session.OpenConnectionAsync();

        var holiday = CreateTestHoliday();
        var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

        try
        {
            var parameters = new Hashtable { { "userId", 347317427 } };
            var delegateCalled = false;

            var results = await mapper.QueryWithRowDelegateAsync<Holiday>(
                "Holiday.SelectAll", parameters,
                (obj, param, list) =>
                {
                    delegateCalled = true;
                    list.Add((Holiday)obj);
                },
                session);

            Assert.True(delegateCalled);
            Assert.Contains(results, h => h.Id == id);
        }
        finally
        {
            await mapper.DeleteAsync("Holiday.Delete", id, session);
        }
    }

    [Fact]
    public async Task QueryWithRowDelegateAsync_DelegateCanFilter()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await session.OpenConnectionAsync();

        var holiday = CreateTestHoliday();
        var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

        try
        {
            var parameters = new Hashtable { { "userId", 347317427 } };

            var results = await mapper.QueryWithRowDelegateAsync<Holiday>(
                "Holiday.SelectAll", parameters,
                (obj, param, list) =>
                {
                    var h = (Holiday)obj;
                    if (h.Id == id) list.Add(h);
                },
                session);

            Assert.Single(results);
            Assert.Equal(id, results[0].Id);
        }
        finally
        {
            await mapper.DeleteAsync("Holiday.Delete", id, session);
        }
    }
    #endregion

    #region QueryForDataTableAsync
    [Fact]
    public async Task QueryForDataTableAsync_ReturnsDataTable()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await session.OpenConnectionAsync();

        var holiday = CreateTestHoliday();
        var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

        try
        {
            var dt = await mapper.QueryForDataTableAsync("Holiday.Select", id, session);

            Assert.NotNull(dt);
            Assert.Equal(1, dt.Rows.Count);
            Assert.Equal(id, Convert.ToInt32(dt.Rows[0]["Id"]));
        }
        finally
        {
            await mapper.DeleteAsync("Holiday.Delete", id, session);
        }
    }

    [Fact]
    public async Task QueryForDataTableAsync_SyncAndAsync_ReturnSameResults()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await session.OpenConnectionAsync();

        var holiday = CreateTestHoliday();
        var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

        try
        {
            var syncDt = mapper.QueryForDataTable("Holiday.Select", id, session);
            var asyncDt = await mapper.QueryForDataTableAsync("Holiday.Select", id, session);

            Assert.Equal(syncDt.Rows.Count, asyncDt.Rows.Count);
            Assert.Equal(
                Convert.ToInt32(syncDt.Rows[0]["Id"]),
                Convert.ToInt32(asyncDt.Rows[0]["Id"]));
        }
        finally
        {
            await mapper.DeleteAsync("Holiday.Delete", id, session);
        }
    }
    #endregion

    #region CancellationToken
    [Fact]
    public async Task CancelledToken_QueryForObjectAsync_ThrowsOperationCancelled()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await session.OpenConnectionAsync();

        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => mapper.QueryForObjectAsync<Holiday>("Holiday.Select", -1, session, cts.Token));
    }

    [Fact]
    public async Task CancelledToken_QueryForListAsync_ThrowsOperationCancelled()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await session.OpenConnectionAsync();

        var cts = new CancellationTokenSource();
        cts.Cancel();

        var parameters = new Hashtable { { "userId", 347317427 } };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => mapper.QueryForListAsync<Holiday>("Holiday.SelectAll", parameters, session, cts.Token));
    }

    [Fact]
    public async Task CancelledToken_InsertAsync_ThrowsOperationCancelled()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await session.OpenConnectionAsync();

        var cts = new CancellationTokenSource();
        cts.Cancel();

        var holiday = CreateTestHoliday();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => mapper.InsertAsync("Holiday.Insert", holiday, session, cts.Token));
    }

    [Fact]
    public async Task CancelledToken_OpenConnectionAsync_ThrowsOperationCancelled()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();

        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => session.OpenConnectionAsync(cts.Token));
    }

    [Fact]
    public async Task DefaultCancellationToken_OperatesNormally()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await session.OpenConnectionAsync(CancellationToken.None);

        var holiday = CreateTestHoliday();
        var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session, CancellationToken.None));

        try
        {
            var result = await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", id, session, CancellationToken.None);
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
        }
        finally
        {
            await mapper.DeleteAsync("Holiday.Delete", id, session, CancellationToken.None);
        }
    }
    #endregion
}
