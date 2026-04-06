#region Apache Notice
/*****************************************************************************
 * $Revision: 374175 $
 * $LastChangedDate: 2006-02-19 12:37:22 +0100 (Sun, 19 Feb 2006) $
 * $LastChangedBy: gbayon $
 *
 * iBATIS.NET Data Mapper
 * Copyright (C) 2006/2005 - The Apache Software Foundation
 *
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 ********************************************************************************/
#endregion

#region using
using IBatisNet.Common;
using IBatisNet.Common.Utilities;
using IBatisNet.Common.Utilities.Objects;
using IBatisNet.Common.Utilities.Objects.Members;
using IBatisNet.DataMapper.Configuration.Cache;
using IBatisNet.DataMapper.Configuration.ParameterMapping;
using IBatisNet.DataMapper.Configuration.ResultMapping;
using IBatisNet.DataMapper.DataExchange;
using IBatisNet.DataMapper.Exceptions;
using IBatisNet.DataMapper.MappedStatements;
using IBatisNet.DataMapper.TypeHandlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Threading.Tasks;

//using IBatisNet.DataMapper.SessionStore;
#endregion

namespace IBatisNet.DataMapper
{
    /// <summary>
    ///     Contract for an <see cref="ISqlMapper" />
    /// </summary>
    public interface ISqlMapper
    {
        /// <summary>
        ///     Name used to identify the the <see cref="SqlMapper" />
        /// </summary>
        string Id { get; }

        /// <summary>
        ///     Gets the DB helper parameter cache.
        /// </summary>
        /// <value>The DB helper parameter cache.</value>
        DBHelperParameterCache DBHelperParameterCache { get; }

        /// <summary>
        ///     A flag that determines whether cache models were enabled
        ///     when this SqlMap was built.
        /// </summary>
        bool IsCacheModelsEnabled { get; set; }

        /// <summary>
        ///     Factory for DataExchange objects
        /// </summary>
        DataExchangeFactory DataExchangeFactory { get; }

        /// <summary>
        ///     The TypeHandlerFactory
        /// </summary>
        TypeHandlerFactory TypeHandlerFactory { get; }

        /// <summary>
        ///     The meta factory for object factory
        /// </summary>
        IObjectFactory ObjectFactory { get; }

        /// <summary>
        ///     The factory which build <see cref="IAccessor" />
        /// </summary>
        AccessorFactory AccessorFactory { get; }

        /// <summary>
        ///     The ParameterMap collection
        /// </summary>
        HybridDictionary ParameterMaps { get; }

        /// <summary>
        ///     The ResultMap collection
        /// </summary>
        HybridDictionary ResultMaps { get; }

        /// <summary>
        ///     The MappedStatements collection
        /// </summary>
        HybridDictionary MappedStatements { get; }

        /// <summary>
        ///     Gets or sets the data source.
        /// </summary>
        /// <value>The data source.</value>
        IDataSource DataSource { get; set; }

        /// <summary>
        ///     Creates a new SqlMapSession that will be used to query the data source.
        /// </summary>
        /// <returns>A new session</returns>
        ISqlMapSession CreateSqlMapSession();

        /// <summary>
        ///     Get a ParameterMap by name
        /// </summary>
        /// <param name="name">The name of the ParameterMap</param>
        /// <returns>The ParameterMap</returns>
        ParameterMap GetParameterMap(string name);

        /// <summary>
        ///     Adds a (named) ParameterMap.
        /// </summary>
        /// <param name="parameterMap">the ParameterMap to add</param>
        void AddParameterMap(ParameterMap parameterMap);

        /// <summary>
        ///     Gets a ResultMap by name
        /// </summary>
        /// <param name="name">The name of the result map</param>
        /// <returns>The ResultMap</returns>
        IResultMap GetResultMap(string name);

        /// <summary>
        ///     Adds a (named) ResultMap
        /// </summary>
        /// <param name="resultMap">The ResultMap to add</param>
        void AddResultMap(IResultMap resultMap);

        /// <summary>
        ///     Gets a cache by name
        /// </summary>
        /// <param name="name">The name of the cache to get</param>
        /// <returns>The cache object</returns>
        CacheModel GetCache(string name);

        /// <summary>
        ///     Adds a (named) cache.
        /// </summary>
        /// <param name="cache">The cache to add</param>
        void AddCache(CacheModel cache);

        void RegisterEntityToMap(string entityName, Action configure);

        /// <summary>
        ///     Adds a (named) MappedStatement.
        /// </summary>
        /// <param name="key"> The key name</param>
        /// <param name="mappedStatement">The statement to add</param>
        void AddMappedStatement(string key, IMappedStatement mappedStatement);

        /// <summary>
        ///     Executes a Sql DELETE statement.
        ///     Delete returns the number of rows effected.
        /// </summary>
        /// <param name="statementName">The name of the statement to execute.</param>
        /// <param name="parameterObject">The parameter object.</param>
        /// <returns>The number of rows effected.</returns>
        int Delete(string statementName, object parameterObject, ISqlMapSession session);

        /// <summary>
        ///     Flushes all cached objects that belong to this SqlMap
        /// </summary>
        void FlushCaches();

        /// <summary>
        ///     Gets the data cache stats.
        /// </summary>
        /// <returns></returns>
        string GetDataCacheStats();

        /// <summary>
        ///     Gets a MappedStatement by name
        /// </summary>
        /// <param name="id"> The id of the statement</param>
        /// <returns> The MappedStatement</returns>
        IMappedStatement GetMappedStatement(string id);
        
        /// <summary>
        /// Trigger the LazyLoad procedure for a given entity.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool LazyLoadMappedStatement(string id);

        /// <summary>
        ///     Executes a Sql INSERT statement.
        ///     Insert is a bit different from other update methods, as it
        ///     provides facilities for returning the primary key of the
        ///     newly inserted row (rather than the effected rows).  This
        ///     functionality is of course optional.
        ///     <p />
        ///     The parameter object is generally used to supply the input
        ///     data for the INSERT values.
        /// </summary>
        /// <param name="statementName">The name of the statement to execute.</param>
        /// <param name="parameterObject">The parameter object.</param>
        /// <returns>
        ///     The primary key of the newly inserted row.
        ///     This might be automatically generated by the RDBMS,
        ///     or selected from a sequence table or other source.
        /// </returns>
        object Insert(string statementName, object parameterObject, ISqlMapSession session);

        ///// <summary>
        ///// Opens the connection.
        ///// </summary>
        ///// <returns></returns>
        //ISqlMapSession OpenConnection();

        ///// <summary>
        ///// Opens the connection.
        ///// </summary>
        ///// <param name="connectionString">The connection string.</param>
        ///// <returns></returns>
        //ISqlMapSession OpenConnection(string connectionString);

        /// <summary>
        ///     Alias to QueryForMap, .NET spirit.
        ///     Feature idea by Ted Husted.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="keyProperty">The property of the result object to be used as the key.</param>
        /// <param name="valueProperty">The property of the result object to be used as the value (or null)</param>
        /// <returns>A IDictionary (Hashtable) of object containing the rows keyed by keyProperty.</returns>
        /// <exception cref="DataMapperException">If a transaction is not in progress, or the database throws an exception.</exception>
        IDictionary QueryForDictionary(string statementName, object parameterObject, string keyProperty, string valueProperty, ISqlMapSession session);

        /// <summary>
        ///     Alias to QueryForMap, .NET spirit.
        ///     Feature idea by Ted Husted.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="keyProperty">The property of the result object to be used as the key.</param>
        /// <returns>A IDictionary (Hashtable) of object containing the rows keyed by keyProperty.</returns>
        IDictionary QueryForDictionary(string statementName, object parameterObject, string keyProperty, ISqlMapSession session);

        /// <summary>
        ///     Executes a Sql SELECT statement that returns data to populate
        ///     a number of result objects.
        ///     <p />
        ///     The parameter object is generally used to supply the input
        ///     data for the WHERE clause parameter(s) of the SELECT statement.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="resultObject">An Ilist object used to hold the objects.</param>
        /// <returns>A List of result objects.</returns>
        void QueryForList(string statementName, object parameterObject, IList resultObject, ISqlMapSession session);

        /// <summary>
        ///     Executes a Sql SELECT statement that returns data to populate
        ///     a number of result objects.
        ///     <p />
        ///     The parameter object is generally used to supply the input
        ///     data for the WHERE clause parameter(s) of the SELECT statement.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <returns>A List of result objects.</returns>
        IList QueryForList(string statementName, object parameterObject, ISqlMapSession session);

        /// <summary>
        ///     Executes the SQL and retuns all rows selected.
        ///     <p />
        ///     The parameter object is generally used to supply the input
        ///     data for the WHERE clause parameter(s) of the SELECT statement.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="skipResults">The number of rows to skip over.</param>
        /// <param name="maxResults">The maximum number of rows to return.</param>
        /// <returns>A List of result objects.</returns>
        IList QueryForList(string statementName, object parameterObject, int skipResults, int maxResults, ISqlMapSession session);


        /// <summary>
        ///     Executes the SQL and retuns all rows selected in a map that is keyed on the property named
        ///     in the keyProperty parameter.  The value at each key will be the entire result object.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="keyProperty">The property of the result object to be used as the key.</param>
        /// <returns>A IDictionary (Hashtable) of object containing the rows keyed by keyProperty.</returns>
        IDictionary QueryForMap(string statementName, object parameterObject, string keyProperty, ISqlMapSession session);

        /// <summary>
        ///     Executes the SQL and retuns all rows selected in a map that is keyed on the property named
        ///     in the keyProperty parameter.  The value at each key will be the value of the property specified
        ///     in the valueProperty parameter.  If valueProperty is null, the entire result object will be entered.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="keyProperty">The property of the result object to be used as the key.</param>
        /// <param name="valueProperty">The property of the result object to be used as the value (or null)</param>
        /// <returns>A IDictionary (Hashtable) of object containing the rows keyed by keyProperty.</returns>
        /// <exception cref="DataMapperException">If a transaction is not in progress, or the database throws an exception.</exception>
        IDictionary QueryForMap(string statementName, object parameterObject, string keyProperty, string valueProperty, ISqlMapSession session);

        /// <summary>
        ///     Runs a query with a custom object that gets a chance to deal
        ///     with each row as it is processed.
        ///     <p />
        ///     The parameter object is generally used to supply the input
        ///     data for the WHERE clause parameter(s) of the SELECT statement.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="keyProperty">The property of the result object to be used as the key.</param>
        /// <param name="valueProperty">The property of the result object to be used as the value (or null)</param>
        /// <param name="rowDelegate"></param>
        /// <returns>A IDictionary (Hashtable) of object containing the rows keyed by keyProperty.</returns>
        /// <exception cref="DataMapperException">If a transaction is not in progress, or the database throws an exception.</exception>
        IDictionary QueryForMapWithRowDelegate(string statementName, object parameterObject, string keyProperty, string valueProperty, DictionaryRowDelegate rowDelegate, ISqlMapSession session);

        /// <summary>
        ///     Executes a Sql SELECT statement that returns a single object of the type of the
        ///     resultObject parameter.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="resultObject">An object of the type to be returned.</param>
        /// <returns>The single result object populated with the result set data.</returns>
        object QueryForObject(string statementName, object parameterObject, object resultObject, ISqlMapSession session);

        /// <summary>
        ///     Executes a Sql SELECT statement that returns that returns data
        ///     to populate a single object instance.
        ///     <p />
        ///     The parameter object is generally used to supply the input
        ///     data for the WHERE clause parameter(s) of the SELECT statement.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <returns> The single result object populated with the result set data.</returns>
        object QueryForObject(string statementName, object parameterObject, ISqlMapSession session);

        /// <summary>
        ///     Executes the SQL and retuns a subset of the results in a dynamic PaginatedList that can be used to
        ///     automatically scroll through results from a database table.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL</param>
        /// <param name="pageSize">The maximum number of objects to store in each page</param>
        /// <returns>A PaginatedList of beans containing the rows</returns>
        [Obsolete("This method will be remove in future version.", false)]
        PaginatedList QueryForPaginatedList(string statementName, object parameterObject, int pageSize, ISqlMapSession session);

        /// <summary>
        ///     Runs a query for list with a custom object that gets a chance to deal
        ///     with each row as it is processed.
        ///     <p />
        ///     The parameter object is generally used to supply the input
        ///     data for the WHERE clause parameter(s) of the SELECT statement.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="rowDelegate"></param>
        /// <returns>A List of result objects.</returns>
        IList QueryWithRowDelegate(string statementName, object parameterObject, RowDelegate rowDelegate, ISqlMapSession session);

        ///// <summary>
        ///// Rolls the back transaction.
        ///// </summary>
        //void RollBackTransaction();

        ///// <summary>
        ///// Rolls the back transaction.
        ///// </summary>
        ///// <param name="closeConnection">if set to <c>true</c> [close connection].</param>
        //void RollBackTransaction(bool closeConnection);

        /// <summary>
        ///     Executes a Sql UPDATE statement.
        ///     Update can also be used for any other update statement type,
        ///     such as inserts and deletes.  Update returns the number of
        ///     rows effected.
        ///     <p />
        ///     The parameter object is generally used to supply the input
        ///     data for the UPDATE values as well as the WHERE clause parameter(s).
        /// </summary>
        /// <param name="statementName">The name of the statement to execute.</param>
        /// <param name="parameterObject">The parameter object.</param>
        /// <returns>The number of rows effected.</returns>
        int Update(string statementName, object parameterObject, ISqlMapSession session);

        /// <summary>
        ///     Executes the SQL and retuns all rows selected in a map that is keyed on the property named
        ///     in the keyProperty parameter.  The value at each key will be the value of the property specified
        ///     in the valueProperty parameter.  If valueProperty is null, the entire result object will be entered.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="keyProperty">The property of the result object to be used as the key.</param>
        /// <param name="valueProperty">The property of the result object to be used as the value (or null)</param>
        /// <returns>A IDictionary of object containing the rows keyed by keyProperty.</returns>
        /// <exception cref="DataMapperException">If a transaction is not in progress, or the database throws an exception.</exception>
        IDictionary<K, V> QueryForDictionary<K, V>(string statementName, object parameterObject, string keyProperty, string valueProperty, ISqlMapSession session);


        /// <summary>
        ///     Executes the SQL and retuns all rows selected in a map that is keyed on the property named
        ///     in the keyProperty parameter.  The value at each key will be the entire result object.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="keyProperty">The property of the result object to be used as the key.</param>
        /// <returns>A IDictionary of object containing the rows keyed by keyProperty.</returns>
        IDictionary<K, V> QueryForDictionary<K, V>(string statementName, object parameterObject, string keyProperty, ISqlMapSession session);

        /// <summary>
        ///     Runs a query with a custom object that gets a chance to deal
        ///     with each row as it is processed.
        ///     <p />
        ///     The parameter object is generally used to supply the input
        ///     data for the WHERE clause parameter(s) of the SELECT statement.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="keyProperty">The property of the result object to be used as the key.</param>
        /// <param name="valueProperty">The property of the result object to be used as the value (or null)</param>
        /// <param name="rowDelegate">A delegate called once per row in the QueryForDictionary method></param>
        /// <returns>A IDictionary (Hashtable) of object containing the rows keyed by keyProperty.</returns>
        /// <exception cref="DataMapperException">If a transaction is not in progress, or the database throws an exception.</exception>
        IDictionary<K, V> QueryForDictionary<K, V>(string statementName, object parameterObject, string keyProperty, string valueProperty, DictionaryRowDelegate<K, V> rowDelegate, ISqlMapSession session);

        /// <summary>
        ///     Executes a Sql SELECT statement that returns a single object of the type of the
        ///     resultObject parameter.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="instanceObject">An object of the type to be returned.</param>
        /// <returns>The single result object populated with the result set data.</returns>
        T QueryForObject<T>(string statementName, object parameterObject, T instanceObject, ISqlMapSession session);

        /// <summary>
        ///     Executes a Sql SELECT statement that returns that returns data
        ///     to populate a single object instance.
        ///     <p />
        ///     The parameter object is generally used to supply the input
        ///     data for the WHERE clause parameter(s) of the SELECT statement.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <returns> The single result object populated with the result set data.</returns>
        T QueryForObject<T>(string statementName, object parameterObject, ISqlMapSession session);

        /// <summary>
        ///     Executes a Sql SELECT statement that returns data to populate
        ///     a number of result objects.
        ///     <p />
        ///     The parameter object is generally used to supply the input
        ///     data for the WHERE clause parameter(s) of the SELECT statement.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <returns>A List of result objects.</returns>
        IList<T> QueryForList<T>(string statementName, object parameterObject, ISqlMapSession session);

        /// <summary>
        ///     Executes a Sql SELECT statement that returns data to populate
        ///     a number of result objects.
        ///     <p />
        ///     The parameter object is generally used to supply the input
        ///     data for the WHERE clause parameter(s) of the SELECT statement.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="resultObject">An Ilist object used to hold the objects.</param>
        void QueryForList<T>(string statementName, object parameterObject, IList<T> resultObject, ISqlMapSession session);

        /// <summary>
        ///     Executes the SQL and retuns all rows selected.
        ///     <p />
        ///     The parameter object is generally used to supply the input
        ///     data for the WHERE clause parameter(s) of the SELECT statement.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="skipResults">The number of rows to skip over.</param>
        /// <param name="maxResults">The maximum number of rows to return.</param>
        /// <returns>A List of result objects.</returns>
        IList<T> QueryForList<T>(string statementName, object parameterObject, int skipResults, int maxResults, ISqlMapSession session);

        /// <summary>
        ///     Runs a query for list with a custom object that gets a chance to deal
        ///     with each row as it is processed.
        ///     <p />
        ///     The parameter object is generally used to supply the input
        ///     data for the WHERE clause parameter(s) of the SELECT statement.
        /// </summary>
        /// <param name="statementName">The name of the sql statement to execute.</param>
        /// <param name="parameterObject">The object used to set the parameters in the SQL.</param>
        /// <param name="rowDelegate"></param>
        /// <returns>A List of result objects.</returns>
        IList<T> QueryWithRowDelegate<T>(string statementName, object parameterObject, RowDelegate<T> rowDelegate, ISqlMapSession session);

        /// <summary>
        ///     Executes the SQL and retuns all rows selected.
        ///     <p />
        /// </summary>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="parameterObject">The parameter object.</param>
        /// <returns></returns>
        DataTable QueryForDataTable(string statementName, object parameterObject, ISqlMapSession session);

        #region Async
        /// <summary>
        ///     Executes a Sql SELECT statement that returns data to populate a single object instance.
        /// </summary>
        Task<T> QueryForObjectAsync<T>(string statementName, object parameterObject, ISqlMapSession session);

        /// <summary>
        ///     Executes a Sql SELECT statement that returns a single object of the type of the
        ///     resultObject parameter.
        /// </summary>
        Task<T> QueryForObjectAsync<T>(string statementName, object parameterObject, T instanceObject, ISqlMapSession session);

        /// <summary>
        ///     Executes a Sql SELECT statement that returns data to populate a number of result objects.
        /// </summary>
        Task<IList<T>> QueryForListAsync<T>(string statementName, object parameterObject, ISqlMapSession session);

        /// <summary>
        ///     Executes the SQL and retuns a subset of the rows selected.
        /// </summary>
        Task<IList<T>> QueryForListAsync<T>(string statementName, object parameterObject, int skipResults, int maxResults, ISqlMapSession session);

        /// <summary>
        ///     Executes a Sql SELECT statement that returns data to populate
        ///     a number of result objects into an existing collection.
        /// </summary>
        Task QueryForListAsync<T>(string statementName, object parameterObject, IList<T> resultObject, ISqlMapSession session);

        /// <summary>
        ///     Executes a Sql INSERT statement asynchronously.
        /// </summary>
        Task<object> InsertAsync(string statementName, object parameterObject, ISqlMapSession session);

        /// <summary>
        ///     Executes a Sql UPDATE statement asynchronously.
        /// </summary>
        Task<int> UpdateAsync(string statementName, object parameterObject, ISqlMapSession session);

        /// <summary>
        ///     Executes a Sql DELETE statement asynchronously.
        /// </summary>
        Task<int> DeleteAsync(string statementName, object parameterObject, ISqlMapSession session);
        #endregion
    }
}