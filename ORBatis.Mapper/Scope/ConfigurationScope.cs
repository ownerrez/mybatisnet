#region Apache Notice
/*****************************************************************************
 * $Header: $
 * $Revision: 576082 $
 * $Date: 2007-09-16 06:04:01 -0600 (Sun, 16 Sep 2007) $
 *
 * iBATIS.NET Data Mapper
 * Copyright (C) 2004 - Gilles Bayon
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

#region Using
using IBatisNet.Common;
using IBatisNet.Common.Utilities;
using IBatisNet.Common.Utilities.Objects;
using IBatisNet.DataMapper.Configuration;
using IBatisNet.DataMapper.DataExchange;
using IBatisNet.DataMapper.TypeHandlers;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml;
#endregion

namespace IBatisNet.DataMapper.Scope
{
    /// <summary>
    ///     The ConfigurationScope maintains the state of the build process.
    /// </summary>
    public class ConfigurationScope : IScope
    {
        /// <summary>
        ///     Empty parameter map
        /// </summary>
        public const string EMPTY_PARAMETER_MAP = "iBATIS.Empty.ParameterMap";

        #region Constructors
        /// <summary>
        ///     Default constructor
        /// </summary>
        public ConfigurationScope()
        {
            ErrorContext = new ErrorContext();

            Providers.Clear();
        }
        #endregion

        /// <summary>
        ///     Register under Statement Name or Fully Qualified Statement Name
        /// </summary>
        /// <param name="id">An Identity</param>
        /// <returns>The new Identity</returns>
        public string ApplyNamespace(string id)
        {
            var newId = id;

            if (SqlMapNamespace != null && SqlMapNamespace.Length > 0
                                        && id != null && id.Length > 0 && id.IndexOf(".") < 0)
                newId = SqlMapNamespace + DomSqlMapBuilder.DOT + id;
            return newId;
        }

        /// <summary>
        ///     Resolves the type handler.
        /// </summary>
        /// <param name="clazz">The clazz.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="clrType">Type of the CLR.</param>
        /// <param name="dbType">Type of the db.</param>
        /// <param name="forSetter">if set to <c>true</c> [for setter].</param>
        /// <returns></returns>
        public ITypeHandler ResolveTypeHandler(Type clazz, string memberName, string clrType, string dbType, bool forSetter)
        {
            ITypeHandler handler = null;
            if (clazz == null)
            {
                handler = DataExchangeFactory.TypeHandlerFactory.GetUnkownTypeHandler();
            }
            else if (typeof(IDictionary).IsAssignableFrom(clazz))
            {
                // IDictionary
                if (clrType == null || clrType.Length == 0)
                    handler = DataExchangeFactory.TypeHandlerFactory.GetUnkownTypeHandler();
                else
                    try
                    {
                        var type = TypeUtils.ResolveType(clrType);
                        handler = DataExchangeFactory.TypeHandlerFactory.GetTypeHandler(type, dbType);
                    }
                    catch (Exception e)
                    {
                        throw new ConfigurationErrorsException("Error. Could not set TypeHandler.  Cause: " + e.Message, e);
                    }
            }
            else if (DataExchangeFactory.TypeHandlerFactory.GetTypeHandler(clazz, dbType) != null)
            {
                // Primitive
                handler = DataExchangeFactory.TypeHandlerFactory.GetTypeHandler(clazz, dbType);
            }
            else
            {
                // .NET object
                if (clrType == null || clrType.Length == 0)
                {
                    Type type = null;
                    if (forSetter)
                        type = ObjectProbe.GetMemberTypeForSetter(clazz, memberName);
                    else
                        type = ObjectProbe.GetMemberTypeForGetter(clazz, memberName);
                    handler = DataExchangeFactory.TypeHandlerFactory.GetTypeHandler(type, dbType);
                }
                else
                {
                    try
                    {
                        var type = TypeUtils.ResolveType(clrType);
                        handler = DataExchangeFactory.TypeHandlerFactory.GetTypeHandler(type, dbType);
                    }
                    catch (Exception e)
                    {
                        throw new ConfigurationErrorsException("Error. Could not set TypeHandler.  Cause: " + e.Message, e);
                    }
                }
            }

            return handler;
        }


        #region Properties
        /// <summary>
        ///     The list of sql fragment
        /// </summary>
        public HybridDictionary SqlIncludes { get; } = new HybridDictionary();

        /// <summary>
        ///     XmlNamespaceManager
        /// </summary>
        public XmlNamespaceManager XmlNamespaceManager { set; get; }

        /// <summary>
        ///     Set if the parser should validate the sqlMap documents
        /// </summary>
        public bool ValidateSqlMap { set; get; }

        /// <summary>
        ///     Tells us if the xml configuration file validate the schema
        /// </summary>
        public bool IsXmlValid { set; get; } = true;


        /// <summary>
        ///     The current SqlMap namespace.
        /// </summary>
        public string SqlMapNamespace { set; get; }

        /// <summary>
        ///     The SqlMapper we are building.
        /// </summary>
        public ISqlMapper SqlMapper { set; get; }


        /// <summary>
        ///     A factory for DataExchange objects
        /// </summary>
        public DataExchangeFactory DataExchangeFactory => SqlMapper.DataExchangeFactory;

        /// <summary>
        ///     Tell us if we are in a DataAccess context.
        /// </summary>
        public bool IsCallFromDao { set; get; }

        /// <summary>
        ///     Tell us if we cache model is enabled.
        /// </summary>
        public bool IsCacheModelsEnabled { set; get; }

        /// <summary>
        ///     External data source
        /// </summary>
        public DataSource DataSource { set; get; }

        /// <summary>
        ///     The current context node we are analizing
        /// </summary>
        public XmlNode NodeContext { set; get; }

        /// <summary>
        ///     The XML SqlMap config file
        /// </summary>
        public XmlDocument SqlMapConfigDocument { set; get; }

        /// <summary>
        ///     A XML SqlMap file
        /// </summary>
        public XmlDocument SqlMapDocument { set; get; }

        /// <summary>
        ///     A XML SqlMap file
        /// </summary>
        public XmlDocument SqlMapModuleDocument { set; get; }

        /// <summary>
        ///     Tell us if we use Configuration File Watcher
        /// </summary>
        public bool UseConfigFileWatcher { set; get; }

        /// <summary>
        ///     Tell us if we use statements namespaces
        /// </summary>
        public bool UseStatementNamespaces { set; get; }

        /// <summary>
        ///     Get the request's error context
        /// </summary>
        public ErrorContext ErrorContext { get; }

        /// <summary>
        ///     List of providers
        /// </summary>
        public HybridDictionary Providers { get; } = new HybridDictionary();

        /// <summary>
        ///     List of global properties
        /// </summary>
        public NameValueCollection Properties { get; } = new NameValueCollection();

        /// <summary>
        ///     Indicates if we can use reflection optimizer.
        /// </summary>
        public bool UseReflectionOptimizer { get; set; } = true;

        /// <summary>
        ///     Temporary storage for mapping cache model ids (key is System.String) to statements (value is IList which contains
        ///     IMappedStatements).
        /// </summary>
        public HybridDictionary CacheModelFlushOnExecuteStatements { get; set; } = new HybridDictionary();
        #endregion
    }
}