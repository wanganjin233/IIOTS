
using System.Linq.Expressions;
using System.Reflection;
using Dapper.Contrib.Extensions;
using System.Data;
using Dapper;
using Newtonsoft.Json;
using System.Collections;
using IIOTS.Util; 

namespace IIOTS.Util
{
    public static class MySqlBuilderSQL
    {
        #region 查询数据 
        public static ExpressionBuilderSQL<T> Select<T>(this IDbConnection Db) where T : class, new() => new ExpressionBuilderSQL<T>(Db).Select();
        #endregion
        #region 添加数据   
        public static bool Insert<T>(this IDbConnection Db, T data) where T : class, new() => new ExpressionBuilderSQL<T>(Db).Insert(data);
        public static bool Insert<T>(this IDbConnection Db, List<T> data) where T : class, new() => new ExpressionBuilderSQL<T>(Db).Insert(data);
        #endregion
        #region 删除数据 
        public static bool Delete<T>(this IDbConnection Db, Expression<Func<T, bool>> expression) where T : class, new() => new ExpressionBuilderSQL<T>(Db).Delete(expression);
        #endregion
        #region 修改数据  
        public static bool Update<T, T2>(this IDbConnection Db, Expression<Func<T, bool>> expression, T2 data) where T : class, new() => new ExpressionBuilderSQL<T>(Db).Update(expression, data);
        public static bool Update<T>(this IDbConnection Db, Expression<Func<T, bool>> expression, Expression<Func<T, object>> columns, T data) where T : class, new() => new ExpressionBuilderSQL<T>(Db).Update(expression, columns, data);
        #endregion
        #region 事务
        public static Transaction<T> Transaction<T>(this IDbConnection Db) where T : class, new() => new(Db);
        #endregion
    }
    /// <summary>
    /// 事务
    /// </summary>
    public class Transaction<T> where T : class, new()
    {
        public IDbTransaction? _Transaction { get; set; }
        IDbConnection Db { get; set; }
        public Transaction(IDbConnection _Db)
        {
            Db = _Db;
            Db.Open();
            _Transaction = Db.BeginTransaction();
        }
        public ExpressionBuilderSQL<T> BuilderSQL()
        {
            return BuilderSQL<T>();
        }
        public ExpressionBuilderSQL<TBuilderSQL> BuilderSQL<TBuilderSQL>() where TBuilderSQL : class, new()
        {
            ExpressionBuilderSQL<TBuilderSQL> transactionMode = new(Db)
            {
                DbTransaction = _Transaction
            };
            return transactionMode;
        }
        /// <summary>
        /// 回滚
        /// </summary>
        public void Rollback()
        {
            _Transaction?.Rollback();
            _Transaction?.Dispose();
            _Transaction = null;
        }
        public void Commit()
        {
            _Transaction?.Commit();
            _Transaction?.Dispose();
            Db.Dispose();
            _Transaction = null;
        }
    }

    public class ExpressionBuilderSQL<T> where T : class, new()
    {
        #region 内部属性  
        /// <summary>
        /// 是否拼接条件
        /// </summary>
        private bool BeWhere { get; set; } = false;

        private int valueTableName = 0;
        private string ValueTableName
        {
            get
            {
                valueTableName++;
                return $"Table_{valueTableName}";
            }
        }

        private int valueName = 0;
        private string ValueName
        {
            get
            {
                valueName++;
                return $"@V_{valueName}";
            }
        }

        private Dictionary<string, object?> parameters { get; set; } = [];
        private IDbConnection Db { get; set; }
        public IDbTransaction? DbTransaction { get; set; }
        #endregion
        #region 外部属性  
        /// <summary>
        /// 生成的sql
        /// </summary>
        public string SQL { get; set; } = string.Empty;
        #endregion
        #region 初始化 
        /// <summary>
        /// 初始化实例
        /// </summary>
        /// <param name="_dapperClient"></param>
        public ExpressionBuilderSQL(IDbConnection dbConnection)
        {
            Db = dbConnection;
        }
        #endregion
        #region 公共方法  
        #region 删除  
        /// <summary>
        /// 事务删除数据库方法
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public bool Delete(Expression<Func<T, bool>> expression)
        {
            SQL = $"DELETE FROM `{typeof(T).Name}`";
            Where(expression);
            return Execute();
        }
        #endregion
        #region 更新 
        /// <summary>
        /// 更新数据库方法
        /// </summary>
        /// <param name="expression">条件</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public bool Update<T2>(Expression<Func<T, bool>> expression, T2 data)
        {
            SQL = $"UPDATE `{typeof(T).Name}` SET {string.Join(",", ModelToDic(data).Select(p => $"`{p.Key}`={TransitionValueName(p.Value)}"))}";
            Where(expression);
            return Execute();
        }

        /// <summary>
        /// 更新数据库方法
        /// </summary>
        /// <param name="expression">条件</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public bool Update(Expression<Func<T, bool>> expression, Expression<Func<T, object>> columnsExpression, T data)
        {
            var columns = GetExpressionAlias(columnsExpression);
            var dic = ModelToDic(data).Where(p => columns.Any(o => o?.ToUpper().Equals(p.Key.ToUpper()) ?? false));
            SQL = $"UPDATE `{typeof(T).Name}` SET {string.Join(",", dic.Select(p => $"`{p.Key}`={TransitionValueName(p.Value)}"))}";
            Where(expression);
            return Execute();
        }


        #endregion
        #region 增加  
        /// <summary>
        /// 事务添加数据库方法
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public bool Insert(T data) => Db.Execute(InsertSql<T>(), data, DbTransaction) > 0;
        /// <summary>
        /// 事务添加数据库方法
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public bool Insert(List<T> data) => Db.Execute(InsertSql<T>(), data, DbTransaction) > 0;
        #endregion
        #region 查询 
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public ExpressionBuilderSQL<T> Select()
        {
            TableAttribute? table = typeof(T).GetCustomAttribute(typeof(TableAttribute)) as TableAttribute;
            string tableName = string.Empty;
            if (table == null)
            {
                tableName = typeof(T).Name;
            }
            else
            {
                tableName = table.Name.Replace(".", "`.`");
            }
            SQL = $"SELECT `{tableName}`.* FROM `{tableName}` ";
            return this;
        }
        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="curPage">当前页</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="totalRecord">总数</param>
        /// <returns></returns>
        public ExpressionBuilderSQL<T> Page(int curPage, int pageSize)
        {
            SQL += $" limit {(curPage - 1) * pageSize},{pageSize}";
            return this;
        }
        /// <summary>
        /// 正序
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public ExpressionBuilderSQL<T> Order(Expression<Func<T, object?>> expression)
        {
            SQL += $" ORDER BY `{string.Join("`,`", GetExpressionAlias(expression))}`";
            return this;
        }
        /// <summary>
        /// 正序
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public ExpressionBuilderSQL<T> Order(string column)
        {

            if (!typeof(T).GetProperties().Any(p => p.Name.ToUpper() == column.ToUpper()))
            {
                throw new Exception("排序未找到此列");
            }
            SQL += $" ORDER BY `{column}` ";
            return this;
        }
        /// <summary>
        /// 倒序
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public ExpressionBuilderSQL<T> DescOrder(Expression<Func<T, object?>> expression)
        {
            SQL += $" ORDER BY `{string.Join("`,`", GetExpressionAlias(expression))}` DESC";
            return this;
        }
        /// <summary>
        /// 倒序
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public ExpressionBuilderSQL<T> DescOrder(string column)
        {
            if (!typeof(T).GetProperties().Any(p => p.Name.ToUpper() == column.ToUpper()))
            {
                throw new Exception("排序未找到此列");
            }
            SQL += $" ORDER BY `{column}` DESC";
            return this;
        }
        /// <summary>
        /// 获取集合方法
        /// </summary>
        /// <returns></returns>
        public List<T> ToList() => Db.Query<T>(SQL, parameters).AsList();

        public bool Execute() => Db.Execute(SQL, parameters, DbTransaction) > 0;
        public ulong Count()
        {
            int fromIndex = SQL.IndexOf(" FROM");
            return Db.Query<ulong?>($"SELECT COUNT(*) {SQL[fromIndex..SQL.Length]} LIMIT 0,1 ", parameters).FirstOrDefault() ?? 0;
        }
        /// <summary>
        /// 获取为指定集合方法
        /// </summary>
        /// <typeparam name="Tobj"></typeparam>
        /// <returns></returns>
        public List<Tobj> ToList<Tobj>() where Tobj : new()
        {
            Type type = typeof(Tobj);
            if (type.BaseType?.Name == "ValueType")
            {
                var query = Db.Query(SQL, parameters);
                List<Tobj> tobjs = new List<Tobj>();
                foreach (IDictionary<string, object> _rows in query)
                {
                    List<object> list = new List<object>();
                    int offset = 0;
                    foreach (Type typeArgument in type.GenericTypeArguments.Reverse())
                    {
                        object? instance = Activator.CreateInstance(typeArgument);
                        if (instance == null) continue;
                        PropertyInfo[] sourcePropertyInfoList = typeArgument.GetProperties();
                        foreach (PropertyInfo propertyInfo in sourcePropertyInfoList)
                        {
                            for (int i = offset; i < sourcePropertyInfoList.Length + offset; i++)
                            {
                                if (_rows.Count == i)
                                {
                                    break;
                                }
                                var field = _rows.ElementAt(i);
                                if (field.Key == propertyInfo.Name)
                                {
                                    if (propertyInfo.PropertyType.BaseType == typeof(System.Enum))
                                    {
                                        System.Enum.TryParse(propertyInfo.PropertyType, field.Value.ToString(), out object? _enum);
                                        if (_enum != null)
                                        {
                                            propertyInfo.SetValue(instance, _enum);
                                        }
                                    }
                                    else
                                    {
                                        propertyInfo.SetValue(instance, field.Value);
                                    }



                                }
                            }
                        }
                        offset += sourcePropertyInfoList.Length;
                        list.Add(instance);
                    }
                    var result = Activator.CreateInstance(type);
                    if (result != null)
                    {
                        foreach (var item in type.GetFields())
                        {
                            item.SetValue(result, list
                                .FirstOrDefault(p => p.GetType().Name == item.FieldType.Name));
                        }
                        tobjs.Add((Tobj)result);
                    }
                }
                return tobjs;
            }
            return Db.Query<Tobj>(SQL, parameters).AsList();
        }

        /// <summary>
        /// 判断数据是否存在
        /// </summary>
        /// <returns></returns>
        public bool Any() => Db.Query<T>($"{SQL} LIMIT 0,1 ", parameters).Any();

        /// <summary>
        /// 获取第一条数据 
        /// </summary>
        /// <returns></returns>
        public T? First() => Db.Query<T>($"{SQL} LIMIT 0,1 ", parameters).FirstOrDefault();
        /// <summary>
        /// 获取第一条数据 
        /// </summary>
        /// <returns></returns>
        public TFirst? First<TFirst>() => Db.Query<TFirst>($"{SQL} LIMIT 0,1 ", parameters).FirstOrDefault();
        #endregion
        #region 条件 
        /// <summary>
        /// 根据条件设定SQL条件
        /// </summary>
        /// <param name="flag">条件</param>
        /// <param name="expression">SQL条件</param>
        /// <returns></returns>
        public ExpressionBuilderSQL<T> IfWhere(bool flag, Expression<Func<T, bool>> expression)
        {
            if (flag) Where(expression);
            return this;
        }
        /// <summary>
        /// 根据条件设定SQL条件
        /// </summary>
        /// <param name="flag">条件</param>
        /// <param name="expression">SQL条件</param>
        /// <returns></returns>
        public ExpressionBuilderSQL<T> IfWhere<TIfWhere>(bool flag, Expression<Func<TIfWhere, bool>> expression) where TIfWhere : class, new()
        {
            if (flag) Where(expression);
            return this;
        }
        /// <summary>
        /// 设定条件
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public ExpressionBuilderSQL<T> Where(Expression<Func<T, bool>> expression)
        {
            Where<T>(expression); return this;
        }
        /// <summary>
        /// 设定条件
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public ExpressionBuilderSQL<T> Where<Tobj>(Expression<Func<Tobj, bool>> expression) where Tobj : class, new()
        {
            var expr = expression?.Body;
            var whereSql = GetExpressionobj<Tobj>(expr, typeof(T)).First().obj?.ToString();
            if (!string.IsNullOrEmpty(whereSql))
            {
                SQL += $" {(BeWhere ? "AND" : "WHERE")} {whereSql}";
                BeWhere = true;
            }
            return this;
        }
        #endregion
        #region 连接 
        /// <summary>
        /// 左连接
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public ExpressionBuilderSQL<T> LeftJoin<TInner>(Expression<Func<T, object?>> outerKeySelector, Expression<Func<TInner, object?>> innerKeySelector) where TInner : class
        {
            CombineJoinSql("LEFT JOIN", outerKeySelector, innerKeySelector);
            return this;
        }
        public ExpressionBuilderSQL<T> RightJoin<TInner>(Expression<Func<T, object?>> outerKeySelector, Expression<Func<TInner, object?>> innerKeySelector) where TInner : class
        {
            CombineJoinSql("RIGHT JOIN", outerKeySelector, innerKeySelector);
            return this;
        }
        public ExpressionBuilderSQL<T> Join<TInner>(Expression<Func<T, object?>> outerKeySelector, Expression<Func<TInner, object?>> innerKeySelector) where TInner : class
        {
            CombineJoinSql("JOIN", outerKeySelector, innerKeySelector);
            return this;
        }
        #endregion
        #endregion
        #region 私有方法 
        #region 动态方法
        private void CombineJoinSql<TInner>(string JoinType, Expression<Func<T, object?>> outerKeySelector, Expression<Func<TInner, object?>> innerKeySelector)
        {
            string virtualTableA = BeWhere ? ValueTableName : typeof(T).Name;
            string virtualTableB = typeof(TInner).Name;
            string joinSql = $" {JoinType} `{virtualTableB}` ON `{virtualTableA}`.`{GetExpressionAlias(outerKeySelector).First()}` = `{virtualTableB}`.`{GetExpressionAlias(innerKeySelector).First()}`";
            if (BeWhere)
            {
                SQL = $"SELECT `{virtualTableB}`.*,`{virtualTableA}`.* FROM ({SQL}) {virtualTableA} {joinSql}";
            }
            else
            {
                SQL = $"SELECT `{virtualTableB}`.*,{SQL[6..SQL.Length]} {joinSql}";
            }
            BeWhere = false;
        }
        #endregion
        private string ProcessByType(string? leftStr, object? obj, ref string sign)
        {
            if (obj is IList enumerable)
            {
                sign = " IN ";
                List<string> valueNames = new() { "''" };
                foreach (var item in enumerable)
                {
                    valueNames.Add(TransitionValueName(item));
                }
                return $"({string.Join(" , ", valueNames)})";
            }
            else if (obj is Array array)
            {
                sign = " IN ";
                List<string> valueNames = new List<string>() { "''" };
                foreach (var item in array)
                {
                    valueNames.Add(TransitionValueName(item));
                }
                return $"({string.Join(" , ", valueNames)})";
            }
            else
            {
                if (sign.Contains("AND") || sign.Contains("OR") || string.IsNullOrEmpty(leftStr))
                {
                    return $"{obj}";
                }
                else if (sign == " LIKE " && !string.IsNullOrEmpty(leftStr))
                {
                    return $"{TransitionValueName(obj, true)}";
                }
                else
                {
                    return $"{TransitionValueName(obj)}";
                }
            }
        }
        /// <summary>
        /// 转换value
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string TransitionValueName(object? obj, bool wildcard = false)
        {
            if (wildcard) obj = $"%{obj}%";
            string valueName = ValueName;
            parameters.Add(valueName, obj);
            return valueName;
        }

        public static Dictionary<string, object?> ModelToDic<TModelToDic>(TModelToDic obj)
        {
            Dictionary<string, object?> map = new Dictionary<string, object?>();
            if (obj != null)
            {
                Type t = obj.GetType();
                PropertyInfo[] PropertyList = t.GetProperties();
                foreach (var item in PropertyList)
                {
                    string name = item.Name;
                    object? value = item.GetValue(obj, null);
                    map.Add(name, value);
                }
            }
            return map;
        }

        /// <summary>
        /// 整合左右
        /// </summary>
        /// <typeparam name="TIntegration"></typeparam>
        /// <param name="type"></param>
        /// <param name="itemlist"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private string Integration<TIntegration>(Type type, List<(Type? type, object? obj)> itemlist, string sign) where TIntegration : class, new()
        {
            if (itemlist.Any(p => p.type == null))
            {
                var obj = itemlist.First(p => p != (null, null)).obj;
                if (obj != null)
                {
                    object? _Object = null;
                    if (obj is TIntegration integration)
                    {
                        _Object = integration;
                    }
                    else if (obj is System.Text.Json.JsonElement jsonElement
                        && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        _Object = JsonConvert.DeserializeObject<TIntegration>(jsonElement.ToString());
                    }
                    if (_Object != null)
                    {
                        List<string> whereSqls = [];
                        var dic = ModelToDic(_Object);
                        foreach (var item in dic)
                        {
                            if (item.Value != null)
                            {
                                Type _type = item.Value.GetType();
                                if (!item.Value.Equals(_type.IsValueType ? Activator.CreateInstance(_type) : string.Empty))
                                {
                                    whereSqls.Add($"`{item.Key}` LIKE {TransitionValueName(item.Value, true)}");
                                }
                            }
                        }
                        if (whereSqls.Count != 0)
                        {
                            return $"({string.Join(" AND ", whereSqls)})";
                        }
                    }
                }
                return string.Empty;
            }
            List<string> joinStr = [];
            var thisItem = itemlist.FirstOrDefault(p => type == p.type);
            if (thisItem != (null, null))
            {
                joinStr.Add(thisItem.obj?.ToString() ?? string.Empty);
            }
            foreach (var (_type, obj) in itemlist.Where(p => type != p.type))
            {
                joinStr.Add(ProcessByType(thisItem.obj?.ToString(), obj, ref sign));
            }
            return $"({string.Join($" {sign} ", joinStr)})";
        }
        private string ExpressionTypeToString(ExpressionType expressionType)
        {
            return expressionType switch
            {
                ExpressionType.Not => "NOT",
                ExpressionType.Convert => "LIKE",
                ExpressionType.OrElse => "OR",
                ExpressionType.AndAlso => "AND",
                ExpressionType.Equal => "=",
                ExpressionType.GreaterThan => ">",
                ExpressionType.LessThan => "<",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.NotEqual => "!=",
                _ => throw new Exception("未找到匹配符号"),
            };
        }
        private List<(Type? type, object? obj)> GetExpressionobj<TExpression>(Expression? expression, Type type) where TExpression : class, new()
        {
            var list = new List<(Type? type, object? obj)>();
            string column = string.Empty;
            string sign = string.Empty;
            string value = string.Empty;
            //模糊匹配
            if (expression is MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.Name == "Contains")
                {
                    sign += " LIKE ";
                    var itemlist = new List<(Type? type, object? obj)>();
                    foreach (var item in methodCallExpression.Arguments)
                    {
                        itemlist.AddRange(GetExpressionobj<TExpression>(item, type));
                    }
                    itemlist.AddRange(GetExpressionobj<TExpression>(methodCallExpression.Object, type));
                    list.Add((methodCallExpression.Type, Integration<TExpression>(type, itemlist, sign)));
                }
            }
            //匹配
            else if (expression is BinaryExpression binaryExpression)
            {
                sign = ExpressionTypeToString(binaryExpression.NodeType);
                var itemlist = new List<(Type? type, object? obj)>();
                itemlist.AddRange(GetExpressionobj<TExpression>(binaryExpression.Left, type));
                itemlist.AddRange(GetExpressionobj<TExpression>(binaryExpression.Right, type));
                list.Add((binaryExpression.Type, Integration<TExpression>(type, itemlist, sign)));
            }
            //类属性
            else if (expression is MemberExpression memberExpression)
            {
                //左
                if (memberExpression.Expression is ParameterExpression parameterExpr && type == parameterExpr.Type)
                {
                    if (memberExpression.NodeType == ExpressionType.MemberAccess)
                    {
                        list.Add((parameterExpr.Type, $"`{memberExpression.Member.Name}`"));
                    }
                    else
                    {
                        throw new NotSupportedException("生成SQL异常");
                    }
                }
                //右
                else
                {
                    UnaryExpression cast = Expression.Convert(memberExpression, typeof(object));
                    object obj = Expression.Lambda<Func<object>>(cast).Compile().Invoke();
                    list.Add((memberExpression.Type, obj));
                }
            }
            else if (expression is ConstantExpression constantExpression)
            {
                list.Add((constantExpression.Type, constantExpression.Value));
            }
            else if (expression is UnaryExpression unaryExpression)
            {
                sign = ExpressionTypeToString(unaryExpression.NodeType);
                foreach (var item in GetExpressionobj<TExpression>(unaryExpression.Operand, type))
                {
                    list.Add((unaryExpression.Type, unaryExpression.IsLifted ? item.obj : $"({sign} {item.obj})"));
                };
            }
            else if (expression is ParameterExpression)
            {
                list.Add((null, null));
            }
            return list;
        }
        #region 静态转换方法 
        private static List<string?> GetExpressionAlias(LambdaExpression lambdaExpression)
        {
            if (lambdaExpression.Body is NewExpression newExpression)
            {
                return newExpression.Arguments.Select(p => (p as MemberExpression)?.Member.Name).ToList();
            }
            else if (lambdaExpression.Body is MemberExpression outerMemberExpression)
            {
                return new List<string?> { outerMemberExpression.Member.Name };
            }
            else if (lambdaExpression.Body is UnaryExpression unaryExpression
              )
            {
                if (unaryExpression.Operand is MemberExpression memberExpression)
                {
                    return new List<string?> { memberExpression.Member.Name };
                }
            }

            throw new NotSupportedException("生成SQL异常");
        }
        private static string InsertSql<TInsert>()
        {
            Type type = typeof(TInsert);
            PropertyInfo[] Properties = type.GetProperties().Where(item => !item.CustomAttributes.Any(p => p.AttributeType == typeof(KeyAttribute))).ToArray();
            string sql = $"INSERT INTO `{type.Name}` ";
            string columnName = $"(`{string.Join("`,`", Properties.Select(p => p.Name))}`)";
            string columnValues = $" VALUES(@{string.Join(",@", Properties.Select(p => p.Name))})";
            return $"{sql} {columnName} {columnValues}";
        }
        #endregion
        #endregion

    }
}

