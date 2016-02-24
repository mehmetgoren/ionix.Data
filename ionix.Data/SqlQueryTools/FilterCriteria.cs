﻿namespace ionix.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public sealed partial class FilterCriteria : ISqlQueryProvider//Array lerde Kullanılacağı İçin Struct yapılmadı.
    {
        private string columnName;
        private string parameterName;
        private ConditionOperator op;
        private char prefix;
        private IList<object> values;

        public FilterCriteria(string columnName, ConditionOperator op, char prefix, params object[] values)
        {
            if (String.IsNullOrEmpty(columnName))
                throw new ArgumentNullException(nameof(columnName));
            if (null == values)
                throw new ArgumentNullException(nameof(values));
            if (values.Length == 0)
                throw new ArgumentException("'values.Length' must be greater than zero");
            for (int j = 0; j < values.Length; ++j)
            {
                if (values[j] == null)
                    throw new NullReferenceException($"'values[{j}]'");
            }
            if (op == ConditionOperator.Between)
            {
                if (values.Length < 2)
                    throw new ArgumentException($"'Between' Operatörü İçin Eksik Sayıda Parametre --> 'values.Legth = '{values.Length}'");
                if (values.Length > 2)
                    throw new ArgumentException( $"'Between' Operatörü İçin Fazla Sayıda Parametre --> 'values.Legth = '{values.Length}'");
            }
            else if (op != ConditionOperator.In && values.Length > 1)
            {
                throw new ArgumentException($"'{op}' Operatörü İçin sadece Bir Adet Parametre Girilebilir.");
            }

            this.columnName = columnName;
            this.op = op;
            this.prefix = prefix;
            this.values = new List<object>(values);
        }


        public SqlQuery ToQuery()
        {
            string parameterName = this.ParameterName;

            SqlQuery query = new SqlQuery();
            StringBuilder text = query.Text;
            SqlQueryParameterList parameters = query.Parameters;

            text.Append(this.columnName);

            string opString = this.GetOpString();
            if (null != opString)
            {
                text.Append(opString);
                text.Append(this.prefix);
                text.Append(parameterName);

                parameters.Add(parameterName, this.values[0]);
            }
            else if (this.op == ConditionOperator.StartsWith || this.op == ConditionOperator.Contains || this.op == ConditionOperator.EndsWith)
            {
                text.Append(" LIKE ");
                text.Append(this.prefix);
                text.Append(parameterName);

                string firstPrefix = String.Empty;
                string lastPrefix = String.Empty;

                if (this.op == ConditionOperator.EndsWith || this.op == ConditionOperator.Contains)
                    firstPrefix = "%";
                if (this.op == ConditionOperator.StartsWith || this.op == ConditionOperator.Contains)
                    lastPrefix = "%";


                parameters.Add(parameterName, firstPrefix + this.values[0] + lastPrefix);
            }
            else
            {
                switch (this.op)
                {
                    case ConditionOperator.In:
                        text.Append(" IN (");
                        for (int j = 0; j < this.values.Count; ++j)
                        {
                            text.Append(this.prefix);
                            text.Append(parameterName);
                            text.Append(j);
                            text.Append(", ");

                            parameters.Add(parameterName + j, values[j]);
                        }
                        text.Remove(text.Length - 2, 2);
                        text.Append(")");
                        break;
                    case ConditionOperator.Between:
                        text.Append(" BETWEEN ");
                        text.Append(this.prefix);
                        text.Append(parameterName);
                        text.Append('1');

                        parameters.Add(parameterName + '1', values[0]);

                        text.Append(" AND ");
                        text.Append(this.prefix);
                        text.Append(parameterName);
                        text.Append('2');

                        parameters.Add(parameterName + '2', values[1]);
                        break;
                    default:
                        throw new NotSupportedException(this.op.ToString());
                }
            }
            return query;
        }

        private string GetOpString()
        {
            switch (this.op)
            {
                case ConditionOperator.Equals:
                    return "=";
                case ConditionOperator.NotEquals:
                    return " <> ";
                case ConditionOperator.GreaterThan:
                    return " > ";
                case ConditionOperator.LessThan:
                    return " < ";
                case ConditionOperator.GreaterThanOrEqualsTo:
                    return ">=";
                case ConditionOperator.LessThanOrEqualsTo:
                    return "<=";
                default:
                    return null;
            }
        }

        public string ParameterName
        {
            get
            {
                if (null == this.parameterName)
                    return this.columnName;
                return this.parameterName;
            }
            set { this.parameterName = value; }
        }
    }

    public sealed partial class FilterCriteriaList : List<FilterCriteria>, ISqlQueryProvider
    {
        private char prefix;
        public FilterCriteriaList(char prefix)
        {
            this.prefix = prefix;
        }

        public char Prefix
        {
            get { return this.prefix; }
            set { this.prefix = value; }
        }

        public void Add(string columnName, string parameterName, ConditionOperator op, params object[] values)
        {
            FilterCriteria item = new FilterCriteria(columnName, op, this.prefix, values);
            item.ParameterName = parameterName;
            base.Add(item);
        }
        public void Add(string columnName, ConditionOperator op, params object[] values)
        {
            this.Add(columnName, null, op, values);
        }

        public SqlQuery ToQuery(string tableNameOp)
        {
            if (base.Count > 0)
            {
                bool hasTableNameOp = !String.IsNullOrEmpty(tableNameOp);

                SqlQuery query = new SqlQuery();
                StringBuilder text = query.Text;
                text.AppendLine();
                text.Append("WHERE ");

                foreach (FilterCriteria filter in this)
                {
                    if (null != filter)
                    {
                        if (hasTableNameOp)
                            text.Append(tableNameOp);

                        SqlQuery temp = filter.ToQuery();

                        query.Combine(temp);

                        text.Append(" AND ");
                    }
                }
                text.Remove(text.Length - 5, 5);
                return query;
            }

            return null;
        }
        public SqlQuery ToQuery()
        {
            return this.ToQuery(null);
        }
    }
}

