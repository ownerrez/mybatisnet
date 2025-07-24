#region Apache Notice
/*****************************************************************************
 * $Revision: 476843 $
 * $LastChangedDate: 2006-11-19 09:07:45 -0700 (Sun, 19 Nov 2006) $
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

#region Imports
using IBatisNet.DataMapper.Configuration.ParameterMapping;
using IBatisNet.DataMapper.Configuration.Sql.Dynamic.Elements;
using IBatisNet.DataMapper.Configuration.Sql.Dynamic.Handlers;
using IBatisNet.DataMapper.Configuration.Sql.SimpleDynamic;
using IBatisNet.DataMapper.Configuration.Statements;
using IBatisNet.DataMapper.DataExchange;
using IBatisNet.DataMapper.MappedStatements;
using IBatisNet.DataMapper.Scope;
using System.Collections;
using System.Data;
using System.Text;
#endregion

namespace IBatisNet.DataMapper.Configuration.Sql.Dynamic
{
    /// <summary>
    ///     DynamicSql represent the root element of a dynamic sql statement
    /// </summary>
    /// <example>
    ///     <dynamic prepend="where">...</dynamic>
    /// </example>
    internal sealed class DynamicSql : ISql, IDynamicParent
    {
        #region Constructor (s) / Destructor
        /// <summary>
        ///     Initializes a new instance of the <see cref="DynamicSql" /> class.
        /// </summary>
        /// <param name="configScope">The config scope.</param>
        /// <param name="statement">The statement.</param>
        internal DynamicSql(ConfigurationScope configScope, IStatement statement)
        {
            _statement = statement;

            _usePositionalParameters = configScope.DataSource.DbProvider.UsePositionalParameters;
            _dataExchangeFactory = configScope.DataExchangeFactory;
        }
        #endregion

        #region Fields
        private readonly IList _children = new ArrayList();
        private readonly IStatement _statement;
        private readonly bool _usePositionalParameters;
        private InlineParameterMapParser _paramParser;
        private readonly DataExchangeFactory _dataExchangeFactory;
        #endregion

        #region Methods
        #region ISql IDynamicParent
        /// <summary>
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(ISqlChild child)
        {
            _children.Add(child);
        }
        #endregion

        #region ISql Members
        /// <summary>
        ///     Builds a new <see cref="RequestScope" /> and the <see cref="IDbCommand" /> text to execute.
        /// </summary>
        /// <param name="parameterObject">The parameter object (used in DynamicSql)</param>
        /// <param name="session">The current session</param>
        /// <param name="mappedStatement">The <see cref="IMappedStatement" />.</param>
        /// <returns>A new <see cref="RequestScope" />.</returns>
        public RequestScope GetRequestScope(IMappedStatement mappedStatement,
            object parameterObject, ISqlMapSession session)
        {
            var request = new RequestScope(_dataExchangeFactory, session, _statement);

            _paramParser = new InlineParameterMapParser();

            var sqlStatement = Process(request, parameterObject);
            request.PreparedStatement = BuildPreparedStatement(session, request, sqlStatement);
            request.MappedStatement = mappedStatement;

            return request;
        }
        #endregion

        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        private string Process(RequestScope request, object parameterObject)
        {
            var ctx = new SqlTagContext();
            var localChildren = _children;

            ProcessBodyChildren(request, ctx, parameterObject, localChildren);

            // Builds a 'dynamic' ParameterMap
            var map = new ParameterMap(request.DataExchangeFactory);
            map.Id = _statement.Id + "-InlineParameterMap";
            map.Initialize(_usePositionalParameters, request);
            map.Class = _statement.ParameterClass;

            // Adds 'dynamic' ParameterProperty
            var parameters = ctx.GetParameterMappings();
            var count = parameters.Count;
            for (var i = 0; i < count; i++) map.AddParameterProperty((ParameterProperty)parameters[i]);
            request.ParameterMap = map;

            var dynSql = ctx.BodyText;

            // Processes $substitutions$ after DynamicSql
            if (SimpleDynamicSql.IsSimpleDynamicSql(dynSql)) dynSql = new SimpleDynamicSql(request, dynSql, _statement).GetSql(parameterObject);
            return dynSql;
        }


        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ctx"></param>
        /// <param name="parameterObject"></param>
        /// <param name="localChildren"></param>
        private void ProcessBodyChildren(RequestScope request, SqlTagContext ctx,
            object parameterObject, IList localChildren)
        {
            var buffer = ctx.GetWriter();
            ProcessBodyChildren(request, ctx, parameterObject, localChildren.GetEnumerator(), buffer);
        }


        /// <summary>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ctx"></param>
        /// <param name="parameterObject"></param>
        /// <param name="localChildren"></param>
        /// <param name="buffer"></param>
        private void ProcessBodyChildren(RequestScope request, SqlTagContext ctx,
            object parameterObject, IEnumerator localChildren, StringBuilder buffer)
        {
            while (localChildren.MoveNext())
            {
                var child = (ISqlChild)localChildren.Current;

                if (child is SqlText)
                {
                    var sqlText = (SqlText)child;
                    var sqlStatement = sqlText.Text;
                    if (sqlText.IsWhiteSpace)
                    {
                        buffer.Append(sqlStatement);
                    }
                    else
                    {
                        //						if (SimpleDynamicSql.IsSimpleDynamicSql(sqlStatement)) 
                        //						{
                        //							sqlStatement = new SimpleDynamicSql(sqlStatement, _statement).GetSql(parameterObject);
                        //							SqlText newSqlText = _paramParser.ParseInlineParameterMap( null, sqlStatement );
                        //							sqlStatement = newSqlText.Text;
                        //							ParameterProperty[] mappings = newSqlText.Parameters;
                        //							if (mappings != null) 
                        //							{
                        //								for (int i = 0; i < mappings.Length; i++) 
                        //								{
                        //									ctx.AddParameterMapping(mappings[i]);
                        //								}
                        //							}
                        //						}
                        // BODY OUT
                        buffer.Append(" ");
                        buffer.Append(sqlStatement);

                        var parameters = sqlText.Parameters;
                        if (parameters != null)
                        {
                            var length = parameters.Length;
                            for (var i = 0; i < length; i++) ctx.AddParameterMapping(parameters[i]);
                        }
                    }
                }
                else if (child is SqlTag)
                {
                    var tag = (SqlTag)child;
                    var handler = tag.Handler;
                    var response = BaseTagHandler.INCLUDE_BODY;

                    do
                    {
                        var body = new StringBuilder();

                        response = handler.DoStartFragment(ctx, tag, parameterObject);
                        if (response != BaseTagHandler.SKIP_BODY)
                        {
                            if (ctx.IsOverridePrepend
                                && ctx.FirstNonDynamicTagWithPrepend == null
                                && tag.IsPrependAvailable
                                && !(tag.Handler is DynamicTagHandler))
                                ctx.FirstNonDynamicTagWithPrepend = tag;

                            ProcessBodyChildren(request, ctx, parameterObject, tag.GetChildrenEnumerator(), body);

                            response = handler.DoEndFragment(ctx, tag, parameterObject, body);
                            handler.DoPrepend(ctx, tag, parameterObject, body);
                            if (response != BaseTagHandler.SKIP_BODY)
                                if (body.Length > 0)
                                {
                                    // BODY OUT
                                    if (handler.IsPostParseRequired)
                                    {
                                        var sqlText = _paramParser.ParseInlineParameterMap(request, null, body.ToString());
                                        buffer.Append(sqlText.Text);
                                        var mappings = sqlText.Parameters;
                                        if (mappings != null)
                                        {
                                            var length = mappings.Length;
                                            for (var i = 0; i < length; i++) ctx.AddParameterMapping(mappings[i]);
                                        }
                                    }
                                    else
                                    {
                                        buffer.Append(" ");
                                        buffer.Append(body);
                                    }

                                    if (tag.IsPrependAvailable && tag == ctx.FirstNonDynamicTagWithPrepend) ctx.IsOverridePrepend = false;
                                }
                        }
                    } while (response == BaseTagHandler.REPEAT_BODY);
                }
            }
        }


        /// <summary>
        /// </summary>
        /// <param name="session"></param>
        /// <param name="request"></param>
        /// <param name="sqlStatement"></param>
        /// <returns></returns>
        private PreparedStatement BuildPreparedStatement(ISqlMapSession session, RequestScope request, string sqlStatement)
        {
            var factory = new PreparedStatementFactory(session, request, _statement, sqlStatement);
            return factory.Prepare();
        }
        #endregion
    }
}