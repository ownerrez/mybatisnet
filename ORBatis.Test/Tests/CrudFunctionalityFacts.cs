using System.Collections;
using System.Collections.Specialized;
using IBatisNet.Common.Utilities;
using IBatisNet.DataMapper;
using IBatisNet.DataMapper.Configuration;
using ORBatis.Test.Common;
using ORBatis.Test.Common.Models;
using Xunit;

namespace ORBatis.Test.Tests;

/// <summary>
///     Shared CRUD test logic. Subclasses provide sync or async implementations
///     of each database operation so the same assertions run against both paths.
/// </summary>
public abstract class CrudFunctionalityFacts
{
    protected static ISqlMapper BuildMapper()
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

    protected static Holiday CreateTestHoliday() => new Holiday
    {
        UserId = 347317427,
        Name = "Test" + Guid.NewGuid().ToString("N")[..8],
        AllowAllProperties = true,
        Active = true,
        CreatedUtc = DateTime.UtcNow,
        CreatedUserId = 347317427,
        UpdatedUtc = DateTime.UtcNow,
        UpdatedUserId = 347317427
    };

    protected abstract Task OpenConnection(ISqlMapSession session);
    protected abstract Task<Holiday> QueryForObject(ISqlMapper mapper, string statementName, object parameter, ISqlMapSession session);
    protected abstract Task<IList<Holiday>> QueryForList(ISqlMapper mapper, string statementName, object parameter, ISqlMapSession session);
    protected abstract Task<IList<Holiday>> QueryForList(ISqlMapper mapper, string statementName, object parameter, int skip, int max, ISqlMapSession session);
    protected abstract Task<int> Insert(ISqlMapper mapper, string statementName, object parameter, ISqlMapSession session);
    protected abstract Task<int> Update(ISqlMapper mapper, string statementName, object parameter, ISqlMapSession session);
    protected abstract Task<int> Delete(ISqlMapper mapper, string statementName, object parameter, ISqlMapSession session);

    #region QueryForObject
    [Fact]
    public async Task QueryForObject_ReturnsObject()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await OpenConnection(session);

        var holiday = CreateTestHoliday();
        var id = await Insert(mapper, "Holiday.Insert", holiday, session);

        try
        {
            var result = await QueryForObject(mapper, "Holiday.Select", id, session);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal(holiday.Name, result.Name);
        }
        finally
        {
            await Delete(mapper, "Holiday.Delete", id, session);
        }
    }

    [Fact]
    public async Task QueryForObject_ReturnsNullForMissingRow()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await OpenConnection(session);

        var result = await QueryForObject(mapper, "Holiday.Select", -1, session);

        Assert.Null(result);
    }
    #endregion

    #region QueryForList
    [Fact]
    public async Task QueryForList_ReturnsList()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await OpenConnection(session);

        var parameters = new Hashtable { { "active", true }, { "userId", 347317427 } };
        var results = await QueryForList(mapper, "Holiday.SelectAll", parameters, session);

        Assert.NotNull(results);
    }

    [Fact]
    public async Task QueryForList_WithPaging_ReturnsSubset()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await OpenConnection(session);

        var parameters = new Hashtable { { "active", true }, { "userId", 347317427 } };
        var allResults = await QueryForList(mapper, "Holiday.SelectAll", parameters, session);
        if (allResults.Count < 2)
            return;

        var pagedResults = await QueryForList(mapper, "Holiday.SelectAll", parameters, 0, 1, session);

        Assert.Single(pagedResults);
        Assert.Equal(allResults[0].Id, pagedResults[0].Id);
    }

    [Fact]
    public async Task QueryForList_EmptyResult_ReturnsEmptyList()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await OpenConnection(session);

        var parameters = new Hashtable { { "active", true }, { "userId", -99999 } };
        var results = await QueryForList(mapper, "Holiday.SelectAll", parameters, session);

        Assert.NotNull(results);
        Assert.Empty(results);
    }
    #endregion

    #region Insert
    [Fact]
    public async Task Insert_ReturnsGeneratedKey()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await OpenConnection(session);

        var holiday = CreateTestHoliday();
        var id = await Insert(mapper, "Holiday.Insert", holiday, session);

        try
        {
            Assert.True(id > 0);
        }
        finally
        {
            await Delete(mapper, "Holiday.Delete", id, session);
        }
    }

    [Fact]
    public async Task Insert_RowIsQueryable()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await OpenConnection(session);

        var holiday = CreateTestHoliday();
        var id = await Insert(mapper, "Holiday.Insert", holiday, session);

        try
        {
            var fetched = await QueryForObject(mapper, "Holiday.Select", id, session);

            Assert.NotNull(fetched);
            Assert.Equal(holiday.Name, fetched.Name);
            Assert.Equal(holiday.UserId, fetched.UserId);
        }
        finally
        {
            await Delete(mapper, "Holiday.Delete", id, session);
        }
    }
    #endregion

    #region Update
    [Fact]
    public async Task Update_ReturnsRowCount()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await OpenConnection(session);

        var holiday = CreateTestHoliday();
        var id = await Insert(mapper, "Holiday.Insert", holiday, session);

        try
        {
            holiday.Id = id;
            holiday.Name = "Updated";
            var rowsAffected = await Update(mapper, "Holiday.Update", holiday, session);

            Assert.Equal(1, rowsAffected);
        }
        finally
        {
            await Delete(mapper, "Holiday.Delete", id, session);
        }
    }

    [Fact]
    public async Task Update_PersistsChanges()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await OpenConnection(session);

        var holiday = CreateTestHoliday();
        var id = await Insert(mapper, "Holiday.Insert", holiday, session);

        try
        {
            holiday.Id = id;
            holiday.Name = "NewName";
            holiday.Active = false;
            await Update(mapper, "Holiday.Update", holiday, session);

            var fetched = await QueryForObject(mapper, "Holiday.Select", id, session);
            Assert.Equal("NewName", fetched.Name);
            Assert.False(fetched.Active);
        }
        finally
        {
            await Delete(mapper, "Holiday.Delete", id, session);
        }
    }

    [Fact]
    public async Task Update_NoMatchingRow_ReturnsZero()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await OpenConnection(session);

        var holiday = CreateTestHoliday();
        holiday.Id = -1;

        var rowsAffected = await Update(mapper, "Holiday.Update", holiday, session);

        Assert.Equal(0, rowsAffected);
    }
    #endregion

    #region Delete
    [Fact]
    public async Task Delete_ReturnsRowCount()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await OpenConnection(session);

        var holiday = CreateTestHoliday();
        var id = await Insert(mapper, "Holiday.Insert", holiday, session);

        var rowsAffected = await Delete(mapper, "Holiday.Delete", id, session);

        Assert.Equal(1, rowsAffected);
    }

    [Fact]
    public async Task Delete_RowIsRemoved()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await OpenConnection(session);

        var holiday = CreateTestHoliday();
        var id = await Insert(mapper, "Holiday.Insert", holiday, session);

        await Delete(mapper, "Holiday.Delete", id, session);

        var fetched = await QueryForObject(mapper, "Holiday.Select", id, session);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task Delete_NoMatchingRow_ReturnsZero()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await OpenConnection(session);

        var rowsAffected = await Delete(mapper, "Holiday.Delete", -1, session);

        Assert.Equal(0, rowsAffected);
    }
    #endregion

    #region Full CRUD Lifecycle
    [Fact]
    public async Task FullCrudLifecycle()
    {
        var mapper = BuildMapper();
        await using var session = mapper.CreateSqlMapSession();
        await OpenConnection(session);

        // Insert
        var holiday = CreateTestHoliday();
        var id = await Insert(mapper, "Holiday.Insert", holiday, session);
        Assert.True(id > 0);

        // Read
        var fetched = await QueryForObject(mapper, "Holiday.Select", id, session);
        Assert.NotNull(fetched);
        Assert.Equal(holiday.Name, fetched.Name);

        // Update
        fetched.Name = "Updated";
        fetched.Active = false;
        var rowsUpdated = await Update(mapper, "Holiday.Update", fetched, session);
        Assert.Equal(1, rowsUpdated);

        // Verify update
        var updated = await QueryForObject(mapper, "Holiday.Select", id, session);
        Assert.Equal("Updated", updated.Name);
        Assert.False(updated.Active);

        // List (should include our row)
        var list = await QueryForList(mapper, "Holiday.SelectAll", new Hashtable { { "userId", 347317427 } }, session);
        Assert.Contains(list, h => h.Id == id);

        // Delete
        var rowsDeleted = await Delete(mapper, "Holiday.Delete", id, session);
        Assert.Equal(1, rowsDeleted);

        // Verify delete
        var deleted = await QueryForObject(mapper, "Holiday.Select", id, session);
        Assert.Null(deleted);
    }
    #endregion
}

public class SyncCrudFacts : CrudFunctionalityFacts
{
    protected override Task OpenConnection(ISqlMapSession session)
    {
        session.OpenConnection();
        return Task.CompletedTask;
    }

    protected override Task<Holiday> QueryForObject(ISqlMapper mapper, string statementName, object parameter, ISqlMapSession session)
        => Task.FromResult(mapper.QueryForObject<Holiday>(statementName, parameter, session));

    protected override Task<IList<Holiday>> QueryForList(ISqlMapper mapper, string statementName, object parameter, ISqlMapSession session)
        => Task.FromResult(mapper.QueryForList<Holiday>(statementName, parameter, session));

    protected override Task<IList<Holiday>> QueryForList(ISqlMapper mapper, string statementName, object parameter, int skip, int max, ISqlMapSession session)
        => Task.FromResult(mapper.QueryForList<Holiday>(statementName, parameter, skip, max, session));

    protected override Task<int> Insert(ISqlMapper mapper, string statementName, object parameter, ISqlMapSession session)
        => Task.FromResult((int)mapper.Insert(statementName, parameter, session));

    protected override Task<int> Update(ISqlMapper mapper, string statementName, object parameter, ISqlMapSession session)
        => Task.FromResult(mapper.Update(statementName, parameter, session));

    protected override Task<int> Delete(ISqlMapper mapper, string statementName, object parameter, ISqlMapSession session)
        => Task.FromResult(mapper.Delete(statementName, parameter, session));
}

public class AsyncCrudFacts : CrudFunctionalityFacts
{
    protected override Task OpenConnection(ISqlMapSession session)
        => session.OpenConnectionAsync();

    protected override Task<Holiday> QueryForObject(ISqlMapper mapper, string statementName, object parameter, ISqlMapSession session)
        => mapper.QueryForObjectAsync<Holiday>(statementName, parameter, session);

    protected override Task<IList<Holiday>> QueryForList(ISqlMapper mapper, string statementName, object parameter, ISqlMapSession session)
        => mapper.QueryForListAsync<Holiday>(statementName, parameter, session);

    protected override Task<IList<Holiday>> QueryForList(ISqlMapper mapper, string statementName, object parameter, int skip, int max, ISqlMapSession session)
        => mapper.QueryForListAsync<Holiday>(statementName, parameter, skip, max, session);

    protected override async Task<int> Insert(ISqlMapper mapper, string statementName, object parameter, ISqlMapSession session)
        => (int)(await mapper.InsertAsync(statementName, parameter, session));

    protected override Task<int> Update(ISqlMapper mapper, string statementName, object parameter, ISqlMapSession session)
        => mapper.UpdateAsync(statementName, parameter, session);

    protected override Task<int> Delete(ISqlMapper mapper, string statementName, object parameter, ISqlMapSession session)
        => mapper.DeleteAsync(statementName, parameter, session);
}
