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

#region using
using IBatisNet.Common.Utilities.Objects;
using IBatisNet.Common.Utilities.Objects.Members;
using IBatisNet.DataMapper.Configuration.Sql.Dynamic.Elements;
using System.Text;
#endregion


namespace IBatisNet.DataMapper.Configuration.Sql.Dynamic.Handlers
{
    /// <summary>
    ///     Summary description for IterateTagHandler.
    /// </summary>
    public sealed class IterateTagHandler : BaseTagHandler
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IterateTagHandler" /> class.
        /// </summary>
        /// <param name="accessorFactory">The accessor factory.</param>
        public IterateTagHandler(AccessorFactory accessorFactory)
            : base(accessorFactory) { }

        /// <summary>
        /// </summary>
        public override bool IsPostParseRequired => true;

        /// <summary>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="tag"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public override int DoStartFragment(SqlTagContext ctx, SqlTag tag, object parameterObject)
        {
            var iterate = (IterateContext)ctx.GetAttribute(tag);
            if (iterate == null)
            {
                var propertyName = ((BaseTag)tag).Property;
                object collection;
                if (propertyName != null && propertyName.Length > 0)
                    collection = ObjectProbe.GetMemberValue(parameterObject, propertyName, AccessorFactory);
                else
                    collection = parameterObject;
                iterate = new IterateContext(collection);
                ctx.AddAttribute(tag, iterate);

                ctx.PushIterateContext(iterate);
            }

            if (iterate != null && iterate.HasNext) return INCLUDE_BODY;

            return SKIP_BODY;
        }


        /// <summary>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="tag"></param>
        /// <param name="parameterObject"></param>
        /// <param name="bodyContent"></param>
        public override void DoPrepend(SqlTagContext ctx, SqlTag tag, object parameterObject, StringBuilder bodyContent)
        {
            var iterate = (IterateContext)ctx.GetAttribute(tag);
            if (iterate.IsFirst) base.DoPrepend(ctx, tag, parameterObject, bodyContent);
        }

        /// <summary>
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="tag"></param>
        /// <param name="parameterObject"></param>
        /// <param name="bodyContent"></param>
        /// <returns></returns>
        public override int DoEndFragment(SqlTagContext ctx, SqlTag tag,
            object parameterObject, StringBuilder bodyContent)
        {
            var iterate = (IterateContext)ctx.GetAttribute(tag);

            if (iterate.MoveNext())
            {
                var propertyName = ((BaseTag)tag).Property;
                if (propertyName == null) propertyName = "";

                var find = propertyName + "[]";
                var replace = propertyName + "[" + iterate.Index + "]"; //Parameter-index-Dynamic
                Replace(bodyContent, find, replace);

                if (iterate.IsFirst)
                {
                    var open = ((Iterate)tag).Open;
                    if (open != null)
                    {
                        bodyContent.Insert(0, open);
                        bodyContent.Insert(0, ' ');
                    }
                }

                if (!iterate.IsLast)
                {
                    var conjunction = ((Iterate)tag).Conjunction;
                    if (conjunction != null)
                    {
                        bodyContent.Append(conjunction);
                        bodyContent.Append(' ');
                    }
                }

                if (iterate.IsLast)
                {
                    var close = ((Iterate)tag).Close;
                    if (close != null) bodyContent.Append(close);
                }

                return REPEAT_BODY;
            }

            ctx.PopIterateContext();

            return INCLUDE_BODY;
        }

        /// <summary>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="find"></param>
        /// <param name="replace"></param>
        private static void Replace(StringBuilder buffer, string find, string replace)
        {
            var start = buffer.ToString().IndexOf(find);
            var length = find.Length;
            while (start > -1)
            {
                buffer = buffer.Replace(find, replace, start, length);
                start = buffer.ToString().IndexOf(find);
            }
        }
    }
}