using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using IBatisNet.Common.Utilities;
using IBatisNet.DataMapper;
using IBatisNet.DataMapper.Configuration;
using ORBatis.Test.Common;
using ORBatis.Test.Common.Models;
using Xunit;

namespace ORBatis.Test.Tests;

public class AsyncCrudFacts
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

    #region QueryForObjectAsync
    [Fact]
    public async Task QueryForObject_ReturnsObject()
    {
        var mapper = BuildMapper();
        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            var holiday = CreateTestHoliday();
            var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

            try
            {
                var result = await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", id, session);

                Assert.NotNull(result);
                Assert.Equal(id, result.Id);
                Assert.Equal(holiday.Name, result.Name);
            }
            finally
            {
                await mapper.DeleteAsync("Holiday.Delete", id, session);
            }
        }
    }

    [Fact]
    public async Task QueryForObject_ReturnsNullForMissingRow()
    {
        var mapper = BuildMapper();
        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            var result = await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", -1, session);

            Assert.Null(result);
        }
    }
    #endregion

    #region QueryForListAsync
    [Fact]
    public async Task QueryForList_ReturnsList()
    {
        var mapper = BuildMapper();
        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            var parameters = new Hashtable { { "active", true }, { "userId", 347317427 } };
            var results = await mapper.QueryForListAsync<Holiday>("Holiday.SelectAll", parameters, session);

            Assert.NotNull(results);
        }
    }

    [Fact]
    public async Task QueryForList_WithPaging_ReturnsSubset()
    {
        var mapper = BuildMapper();
        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            var parameters = new Hashtable { { "active", true }, { "userId", 347317427 } };
            var allResults = await mapper.QueryForListAsync<Holiday>("Holiday.SelectAll", parameters, session);
            if (allResults.Count < 2)
                return;

            var pagedResults = await mapper.QueryForListAsync<Holiday>("Holiday.SelectAll", parameters, 0, 1, session);

            Assert.Single(pagedResults);
            Assert.Equal(allResults[0].Id, pagedResults[0].Id);
        }
    }

    [Fact]
    public async Task QueryForList_EmptyResult_ReturnsEmptyList()
    {
        var mapper = BuildMapper();
        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            var parameters = new Hashtable { { "active", true }, { "userId", -99999 } };
            var results = await mapper.QueryForListAsync<Holiday>("Holiday.SelectAll", parameters, session);

            Assert.NotNull(results);
            Assert.Empty(results);
        }
    }

    [Fact]
    public async Task QueryForList_FillExistingList_AddsResults()
    {
        var mapper = BuildMapper();
        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            var parameters = new Hashtable { { "active", true }, { "userId", 347317427 } };
            var existingList = new List<Holiday>();
            await mapper.QueryForListAsync("Holiday.SelectAll", parameters, existingList, session);

            Assert.NotNull(existingList);
        }
    }

    [Fact]
    public async Task QueryForList_FillExistingList_AppendsToExistingItems()
    {
        var mapper = BuildMapper();
        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            var holiday = CreateTestHoliday();
            var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

            try
            {
                var sentinel = new Holiday { Id = -999, Name = "Sentinel" };
                var existingList = new List<Holiday> { sentinel };

                var parameters = new Hashtable { { "userId", 347317427 } };
                await mapper.QueryForListAsync("Holiday.SelectAll", parameters, existingList, session);

                Assert.Equal(-999, existingList[0].Id);
                Assert.True(existingList.Count > 1);
            }
            finally
            {
                await mapper.DeleteAsync("Holiday.Delete", id, session);
            }
        }
    }
    #endregion

    #region InsertAsync
    [Fact]
    public async Task Insert_ReturnsGeneratedKey()
    {
        var mapper = BuildMapper();
        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            var holiday = CreateTestHoliday();
            var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

            try
            {
                Assert.True(id > 0);
            }
            finally
            {
                await mapper.DeleteAsync("Holiday.Delete", id, session);
            }
        }
    }

    [Fact]
    public async Task Insert_RowIsQueryable()
    {
        var mapper = BuildMapper();
        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            var holiday = CreateTestHoliday();
            var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

            try
            {
                var fetched = await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", id, session);

                Assert.NotNull(fetched);
                Assert.Equal(holiday.Name, fetched.Name);
                Assert.Equal(holiday.UserId, fetched.UserId);
            }
            finally
            {
                await mapper.DeleteAsync("Holiday.Delete", id, session);
            }
        }
    }
    #endregion

    #region UpdateAsync
    [Fact]
    public async Task Update_ReturnsRowCount()
    {
        var mapper = BuildMapper();
        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            var holiday = CreateTestHoliday();
            var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

            try
            {
                holiday.Id = id;
                holiday.Name = "Updated";
                var rowsAffected = await mapper.UpdateAsync("Holiday.Update", holiday, session);

                Assert.Equal(1, rowsAffected);
            }
            finally
            {
                await mapper.DeleteAsync("Holiday.Delete", id, session);
            }
        }
    }

    [Fact]
    public async Task Update_PersistsChanges()
    {
        var mapper = BuildMapper();
        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            var holiday = CreateTestHoliday();
            var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

            try
            {
                holiday.Id = id;
                holiday.Name = "NewName";
                holiday.Active = false;
                await mapper.UpdateAsync("Holiday.Update", holiday, session);

                var fetched = await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", id, session);
                Assert.Equal("NewName", fetched.Name);
                Assert.False(fetched.Active);
            }
            finally
            {
                await mapper.DeleteAsync("Holiday.Delete", id, session);
            }
        }
    }

    [Fact]
    public async Task Update_NoMatchingRow_ReturnsZero()
    {
        var mapper = BuildMapper();
        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            var holiday = CreateTestHoliday();
            holiday.Id = -1;

            var rowsAffected = await mapper.UpdateAsync("Holiday.Update", holiday, session);

            Assert.Equal(0, rowsAffected);
        }
    }
    #endregion

    #region DeleteAsync
    [Fact]
    public async Task Delete_ReturnsRowCount()
    {
        var mapper = BuildMapper();
        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            var holiday = CreateTestHoliday();
            var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

            var rowsAffected = await mapper.DeleteAsync("Holiday.Delete", id, session);

            Assert.Equal(1, rowsAffected);
        }
    }

    [Fact]
    public async Task Delete_RowIsRemoved()
    {
        var mapper = BuildMapper();
        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            var holiday = CreateTestHoliday();
            var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

            var before = await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", id, session);
            Assert.NotNull(before);

            await mapper.DeleteAsync("Holiday.Delete", id, session);

            var after = await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", id, session);
            Assert.Null(after);
        }
    }

    [Fact]
    public async Task Delete_NoMatchingRow_ReturnsZero()
    {
        var mapper = BuildMapper();
        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            var rowsAffected = await mapper.DeleteAsync("Holiday.Delete", -1, session);

            Assert.Equal(0, rowsAffected);
        }
    }
    #endregion

    #region BeginTransactionAsync
    [Fact]
    public async Task BeginTransaction_OpensConnectionAndStartsTransaction()
    {
        var mapper = BuildMapper();
        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.BeginTransactionAsync();

            Assert.True(session.IsTransactionStart);
            Assert.NotNull(session.Transaction);
            Assert.Equal(System.Data.ConnectionState.Open, session.Connection.State);

            session.RollBackTransaction();
        }
    }

    [Fact]
    public async Task BeginTransaction_InsertVisibleBeforeCommit()
    {
        var mapper = BuildMapper();
        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.BeginTransactionAsync();

            var holiday = CreateTestHoliday();
            var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));

            try
            {
                var fetched = await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", id, session);
                Assert.NotNull(fetched);
                Assert.Equal(holiday.Name, fetched.Name);
            }
            finally
            {
                await mapper.DeleteAsync("Holiday.Delete", id, session);
                session.CommitTransaction();
            }
        }
    }

    [Fact]
    public async Task BeginTransaction_RollbackRevertsInsert()
    {
        var mapper = BuildMapper();
        int id;

        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.BeginTransactionAsync();

            var holiday = CreateTestHoliday();
            id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));
            Assert.True(id > 0);

            var duringTx = await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", id, session);
            Assert.NotNull(duringTx);
            Assert.Equal(holiday.Name, duringTx.Name);

            session.RollBackTransaction();
        }

        await using (var session2 = mapper.CreateSqlMapSession())
        {
            await session2.OpenConnectionAsync();
            var fetched = await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", id, session2);
            Assert.Null(fetched);
        }
    }
    #endregion

    #region Full CRUD Lifecycle
    [Fact]
    public async Task FullCrudLifecycle()
    {
        var mapper = BuildMapper();
        await using (var session = mapper.CreateSqlMapSession())
        {
            await session.OpenConnectionAsync();

            // Insert
            var holiday = CreateTestHoliday();
            var id = (int)(await mapper.InsertAsync("Holiday.Insert", holiday, session));
            Assert.True(id > 0);

            // Read
            var fetched = await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", id, session);
            Assert.NotNull(fetched);
            Assert.Equal(holiday.Name, fetched.Name);

            // Update
            fetched.Name = "Updated";
            fetched.Active = false;
            var rowsUpdated = await mapper.UpdateAsync("Holiday.Update", fetched, session);
            Assert.Equal(1, rowsUpdated);

            // Verify update
            var updated = await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", id, session);
            Assert.Equal("Updated", updated.Name);
            Assert.False(updated.Active);

            // List (should include our row)
            var list = await mapper.QueryForListAsync<Holiday>("Holiday.SelectAll", new Hashtable { { "userId", 347317427 } }, session);
            Assert.Contains(list, h => h.Id == id);

            // Delete
            var rowsDeleted = await mapper.DeleteAsync("Holiday.Delete", id, session);
            Assert.Equal(1, rowsDeleted);

            // Verify delete
            var deleted = await mapper.QueryForObjectAsync<Holiday>("Holiday.Select", id, session);
            Assert.Null(deleted);
        }
    }
    #endregion
}
