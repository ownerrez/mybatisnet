#region Apache Notice
/*****************************************************************************
 * $Revision: 408164 $
 * $LastChangedDate: 2006-05-21 06:27:09 -0600 (Sun, 21 May 2006) $
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
#endregion


namespace IBatisNet.DataMapper.Configuration.Sql.Dynamic.Handlers
{
    /// <summary>
    ///     Summary description for SqlTagContext.
    /// </summary>
    public sealed class SqlTagContext
    {
        /// <summary>
        /// </summary>
        public SqlTagContext()
        {
            IsOverridePrepend = false;
        }

        /// <summary>
        /// </summary>
        public string BodyText => buffer.ToString().Trim();

        /// <summary>
        /// </summary>
        public bool IsOverridePrepend { set; get; }

        /// <summary>
        /// </summary>
        public SqlTag FirstNonDynamicTagWithPrepend { get; set; }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public StringBuilder GetWriter()
        {
            return buffer;
        }


        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddAttribute(object key, object value)
        {
            _attributes.Add(key, value);
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetAttribute(object key)
        {
            return _attributes[key];
        }

        /// <summary>
        /// </summary>
        /// <param name="mapping"></param>
        public void AddParameterMapping(ParameterProperty mapping)
        {
            _parameterMappings.Add(mapping);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public IList GetParameterMappings()
        {
            return _parameterMappings;
        }

        public void PushIterateContext(IterateContext iterate)
        {
            _iterates.Push(iterate);
        }

        public IterateContext PeekIterateContext()
        {
            if (_iterates.Count > 0)
                return _iterates.Peek();
            return null;
        }

        public IterateContext PopIterateContext()
        {
            return _iterates.Pop();
        }

        #region Fields
        private readonly Hashtable _attributes = new Hashtable();
        private readonly ArrayList _parameterMappings = new ArrayList();
        private readonly StringBuilder buffer = new StringBuilder();
        private readonly Stack<IterateContext> _iterates = new Stack<IterateContext>();
        #endregion
    }
}