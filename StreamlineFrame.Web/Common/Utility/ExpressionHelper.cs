﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace StreamlineFrame.Web.Common
{
    public class ExpressionHelper
    {
        public static string GetOperator(ExpressionType expressiontype)
        {
            switch (expressiontype)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return " AND ";
                case ExpressionType.Equal:
                    return " = ";
                case ExpressionType.GreaterThan:
                    return " > ";
                case ExpressionType.GreaterThanOrEqual:
                    return " >= ";
                case ExpressionType.LessThan:
                    return " < ";
                case ExpressionType.LessThanOrEqual:
                    return " <= ";
                case ExpressionType.NotEqual:
                    return " <> ";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return " OR ";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return " + ";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return " - ";
                case ExpressionType.Divide:
                    return " / ";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return " * ";
                case ExpressionType.Modulo:
                    return " % ";
                case ExpressionType.Coalesce:
                    throw new Exception("Expression no support ?? ,Use SqlFunc.IsNull");
                default:
                    return null;
            }
        }

        public static string ExpressionToSql(Expression exp)
        {
            var sql = string.Empty;
            if (exp is BinaryExpression)
            {
                return ExpressionToSql((BinaryExpression)exp);
            }
            else if (exp is MemberExpression)
            {
                return ExpressionToSql((MemberExpression)exp);
            }
            else if (exp is ConstantExpression)
            {
                return ExpressionToSql((ConstantExpression)exp);
            }
            else if (exp is MethodCallExpression)
            {
                return ExpressionToSql((MethodCallExpression)exp);
            }
            else if (exp is NewArrayExpression)
            {
                return ExpressionToSql((NewArrayExpression)exp);
            }

            return sql;
        }

        public static string ExpressionToSql(BinaryExpression be)
        {
            var sql = string.Empty;

            sql += ExpressionToSql(be.Left);
            sql += GetOperator(be.NodeType);

            var sbTmp = ExpressionToSql(be.Right);
            if (sbTmp == "null")
            {
                if (sql.EndsWith("= "))
                    sql = Regex.Replace(sql, "=", "is null");
                else if (sql.EndsWith("<> "))
                    sql = Regex.Replace(sql, "<>", "is null");
            }
            else
                sql += sbTmp;

            return sql;
        }

        public static string ExpressionToSql(MemberExpression me)
        {
            return me.Member.IsDefined(typeof(DBNameAttribute), true) ? me.Member.GetCustomAttribute<DBNameAttribute>(false).Name : me.Member.Name;
        }

        public static string ExpressionToSql(ConstantExpression ce)
        {
            if (ce.Value == null)
                return "null";
            else if (ce.Value is ValueType)
                return ce.Value.ToString();
            else if (ce.Value is string || ce.Value is DateTime || ce.Value is char)
                return $"'{ce.Value.ToString()}'";
            return null;
        }

        public static string ExpressionToSql(MethodCallExpression mce)
        {
            if (mce.Method.Name.Equals("Contains"))
                return $"({ExpressionToSql(mce.Arguments[0])} in {ExpressionToSql(mce.Arguments[1])})";
            else if (mce.Method.Name.Equals("EndsWith"))
                return $"({ExpressionToSql(mce.Arguments[0])} not in %'{ExpressionToSql(mce.Arguments[1])})'";
            else if (mce.Method.Name.Equals("StartsWith"))
                return $"({ExpressionToSql(mce.Arguments[0])} like '{ExpressionToSql(mce.Arguments[1])}'%)";
            return null;
        }

        public static string ExpressionToSql(NewArrayExpression nae)
        {
            var tmpstr = new StringBuilder();
            foreach (Expression ex in nae.Expressions)
            {
                tmpstr.Append(ExpressionToSql(ex));
                tmpstr.Append(",");
            }
            return tmpstr.ToString(0, tmpstr.Length - 1);
        }
    }
}