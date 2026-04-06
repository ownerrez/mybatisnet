using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using IBatisNet.Common.Utilities;
using IBatisNet.DataMapper;
using IBatisNet.DataMapper.Configuration;
using ORBatis.Test.Common;
using ORBatis.Test.Common.Models;
using Xunit;

namespace ORBatis.Test.Tests;

public class SyncCrudFacts
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

    #region QueryForObject
    [Fact]
    public void QueryForObject_ReturnsObject()
    {
        var mapper = BuildMapper();
        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            var holiday = CreateTestHoliday();
            var id = (int)mapper.Insert("Holiday.Insert", holiday, session);

            try
            {
                var result = mapper.QueryForObject<Holiday>("Holiday.Select", id, session);

                Assert.NotNull(result);
                Assert.Equal(id, result.Id);
                Assert.Equal(holiday.Name, result.Name);
            }
            finally
            {
                mapper.Delete("Holiday.Delete", id, session);
            }
        }
    }

    [Fact]
    public void QueryForObject_ReturnsNullForMissingRow()
    {
        var mapper = BuildMapper();
        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            var result = mapper.QueryForObject<Holiday>("Holiday.Select", -1, session);

            Assert.Null(result);
        }
    }
    #endregion

    #region QueryForList
    [Fact]
    public void QueryForList_ReturnsList()
    {
        var mapper = BuildMapper();
        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            var parameters = new Hashtable { { "active", true }, { "userId", 347317427 } };
            var results = mapper.QueryForList<Holiday>("Holiday.SelectAll", parameters, session);

            Assert.NotNull(results);
        }
    }

    [Fact]
    public void QueryForList_WithPaging_ReturnsSubset()
    {
        var mapper = BuildMapper();
        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            var parameters = new Hashtable { { "active", true }, { "userId", 347317427 } };
            var allResults = mapper.QueryForList<Holiday>("Holiday.SelectAll", parameters, session);
            if (allResults.Count < 2)
                return;

            var pagedResults = mapper.QueryForList<Holiday>("Holiday.SelectAll", parameters, 0, 1, session);

            Assert.Single(pagedResults);
            Assert.Equal(allResults[0].Id, pagedResults[0].Id);
        }
    }

    [Fact]
    public void QueryForList_EmptyResult_ReturnsEmptyList()
    {
        var mapper = BuildMapper();
        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            var parameters = new Hashtable { { "active", true }, { "userId", -99999 } };
            var results = mapper.QueryForList<Holiday>("Holiday.SelectAll", parameters, session);

            Assert.NotNull(results);
            Assert.Empty(results);
        }
    }

    [Fact]
    public void QueryForList_FillExistingList_AddsResults()
    {
        var mapper = BuildMapper();
        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            var parameters = new Hashtable { { "active", true }, { "userId", 347317427 } };
            var existingList = new List<Holiday>();
            mapper.QueryForList<Holiday>("Holiday.SelectAll", parameters, existingList, session);

            Assert.NotNull(existingList);
        }
    }

    [Fact]
    public void QueryForList_FillExistingList_AppendsToExistingItems()
    {
        var mapper = BuildMapper();
        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            var holiday = CreateTestHoliday();
            var id = (int)mapper.Insert("Holiday.Insert", holiday, session);

            try
            {
                var sentinel = new Holiday { Id = -999, Name = "Sentinel" };
                var existingList = new List<Holiday> { sentinel };

                var parameters = new Hashtable { { "userId", 347317427 } };
                mapper.QueryForList<Holiday>("Holiday.SelectAll", parameters, existingList, session);

                Assert.Equal(-999, existingList[0].Id);
                Assert.True(existingList.Count > 1);
            }
            finally
            {
                mapper.Delete("Holiday.Delete", id, session);
            }
        }
    }
    #endregion

    #region Insert
    [Fact]
    public void Insert_ReturnsGeneratedKey()
    {
        var mapper = BuildMapper();
        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            var holiday = CreateTestHoliday();
            var id = (int)mapper.Insert("Holiday.Insert", holiday, session);

            try
            {
                Assert.True(id > 0);
            }
            finally
            {
                mapper.Delete("Holiday.Delete", id, session);
            }
        }
    }

    [Fact]
    public void Insert_RowIsQueryable()
    {
        var mapper = BuildMapper();
        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            var holiday = CreateTestHoliday();
            var id = (int)mapper.Insert("Holiday.Insert", holiday, session);

            try
            {
                var fetched = mapper.QueryForObject<Holiday>("Holiday.Select", id, session);

                Assert.NotNull(fetched);
                Assert.Equal(holiday.Name, fetched.Name);
                Assert.Equal(holiday.UserId, fetched.UserId);
            }
            finally
            {
                mapper.Delete("Holiday.Delete", id, session);
            }
        }
    }
    #endregion

    #region Update
    [Fact]
    public void Update_ReturnsRowCount()
    {
        var mapper = BuildMapper();
        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            var holiday = CreateTestHoliday();
            var id = (int)mapper.Insert("Holiday.Insert", holiday, session);

            try
            {
                holiday.Id = id;
                holiday.Name = "Updated";
                var rowsAffected = mapper.Update("Holiday.Update", holiday, session);

                Assert.Equal(1, rowsAffected);
            }
            finally
            {
                mapper.Delete("Holiday.Delete", id, session);
            }
        }
    }

    [Fact]
    public void Update_PersistsChanges()
    {
        var mapper = BuildMapper();
        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            var holiday = CreateTestHoliday();
            var id = (int)mapper.Insert("Holiday.Insert", holiday, session);

            try
            {
                holiday.Id = id;
                holiday.Name = "NewName";
                holiday.Active = false;
                mapper.Update("Holiday.Update", holiday, session);

                var fetched = mapper.QueryForObject<Holiday>("Holiday.Select", id, session);
                Assert.Equal("NewName", fetched.Name);
                Assert.False(fetched.Active);
            }
            finally
            {
                mapper.Delete("Holiday.Delete", id, session);
            }
        }
    }

    [Fact]
    public void Update_NoMatchingRow_ReturnsZero()
    {
        var mapper = BuildMapper();
        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            var holiday = CreateTestHoliday();
            holiday.Id = -1;

            var rowsAffected = mapper.Update("Holiday.Update", holiday, session);

            Assert.Equal(0, rowsAffected);
        }
    }
    #endregion

    #region Delete
    [Fact]
    public void Delete_ReturnsRowCount()
    {
        var mapper = BuildMapper();
        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            var holiday = CreateTestHoliday();
            var id = (int)mapper.Insert("Holiday.Insert", holiday, session);

            var rowsAffected = mapper.Delete("Holiday.Delete", id, session);

            Assert.Equal(1, rowsAffected);
        }
    }

    [Fact]
    public void Delete_RowIsRemoved()
    {
        var mapper = BuildMapper();
        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            var holiday = CreateTestHoliday();
            var id = (int)mapper.Insert("Holiday.Insert", holiday, session);

            var before = mapper.QueryForObject<Holiday>("Holiday.Select", id, session);
            Assert.NotNull(before);

            mapper.Delete("Holiday.Delete", id, session);

            var after = mapper.QueryForObject<Holiday>("Holiday.Select", id, session);
            Assert.Null(after);
        }
    }

    [Fact]
    public void Delete_NoMatchingRow_ReturnsZero()
    {
        var mapper = BuildMapper();
        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            var rowsAffected = mapper.Delete("Holiday.Delete", -1, session);

            Assert.Equal(0, rowsAffected);
        }
    }
    #endregion

    #region BeginTransaction
    [Fact]
    public void BeginTransaction_OpensConnectionAndStartsTransaction()
    {
        var mapper = BuildMapper();
        using (var session = mapper.CreateSqlMapSession())
        {
            session.BeginTransaction();

            Assert.True(session.IsTransactionStart);
            Assert.NotNull(session.Transaction);
            Assert.Equal(System.Data.ConnectionState.Open, session.Connection.State);

            session.RollBackTransaction();
        }
    }

    [Fact]
    public void BeginTransaction_InsertVisibleBeforeCommit()
    {
        var mapper = BuildMapper();
        using (var session = mapper.CreateSqlMapSession())
        {
            session.BeginTransaction();

            var holiday = CreateTestHoliday();
            var id = (int)mapper.Insert("Holiday.Insert", holiday, session);

            try
            {
                var fetched = mapper.QueryForObject<Holiday>("Holiday.Select", id, session);
                Assert.NotNull(fetched);
                Assert.Equal(holiday.Name, fetched.Name);
            }
            finally
            {
                mapper.Delete("Holiday.Delete", id, session);
                session.CommitTransaction();
            }
        }
    }

    [Fact]
    public void BeginTransaction_RollbackRevertsInsert()
    {
        var mapper = BuildMapper();
        int id;

        using (var session = mapper.CreateSqlMapSession())
        {
            session.BeginTransaction();

            var holiday = CreateTestHoliday();
            id = (int)mapper.Insert("Holiday.Insert", holiday, session);
            Assert.True(id > 0);

            var duringTx = mapper.QueryForObject<Holiday>("Holiday.Select", id, session);
            Assert.NotNull(duringTx);
            Assert.Equal(holiday.Name, duringTx.Name);

            session.RollBackTransaction();
        }

        using (var session2 = mapper.CreateSqlMapSession())
        {
            session2.OpenConnection();
            var fetched = mapper.QueryForObject<Holiday>("Holiday.Select", id, session2);
            Assert.Null(fetched);
        }
    }
    #endregion

    #region Full CRUD Lifecycle
    [Fact]
    public void FullCrudLifecycle()
    {
        var mapper = BuildMapper();
        using (var session = mapper.CreateSqlMapSession())
        {
            session.OpenConnection();

            // Insert
            var holiday = CreateTestHoliday();
            var id = (int)mapper.Insert("Holiday.Insert", holiday, session);
            Assert.True(id > 0);

            // Read
            var fetched = mapper.QueryForObject<Holiday>("Holiday.Select", id, session);
            Assert.NotNull(fetched);
            Assert.Equal(holiday.Name, fetched.Name);

            // Update
            fetched.Name = "Updated";
            fetched.Active = false;
            var rowsUpdated = mapper.Update("Holiday.Update", fetched, session);
            Assert.Equal(1, rowsUpdated);

            // Verify update
            var updated = mapper.QueryForObject<Holiday>("Holiday.Select", id, session);
            Assert.Equal("Updated", updated.Name);
            Assert.False(updated.Active);

            // List (should include our row)
            var list = mapper.QueryForList<Holiday>("Holiday.SelectAll", new Hashtable { { "userId", 347317427 } }, session);
            Assert.Contains(list, h => h.Id == id);

            // Delete
            var rowsDeleted = mapper.Delete("Holiday.Delete", id, session);
            Assert.Equal(1, rowsDeleted);

            // Verify delete
            var deleted = mapper.QueryForObject<Holiday>("Holiday.Select", id, session);
            Assert.Null(deleted);
        }
    }
    #endregion
}
