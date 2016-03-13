using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace vActiveRecord
{
    public class Base
    {
        #region Local Variables

        SQLiteCommand _cmd;
        SQLiteTransaction _transaction;

        #endregion

        #region Public Properties

        protected string table_name { get; set; }

        public bool is_new_record { get; set; }

        #endregion

        #region Public Static Properties

        public static SQLiteConnection connection { get; set; }

        #endregion

        #region Constructors

        public Base()
        {
        }

        #endregion

        #region Private Methods

        //private bool create_or_update()
        //{
        //    return is_new_record ? create() : update();
        //}

        private List<Hashtable> ExecuteReader(string sql, ref List<Hashtable> rows)
        {
            SQLiteDataReader dr = null;

            if (this.ExecuteReader(sql, ref dr))
            {
                if (dr.HasRows)
                {
                    rows = new List<Hashtable>();

                    while (dr.Read())
                    {
                        Hashtable c = new Hashtable();

                        int i = 0;
                        while (i < dr.FieldCount)
                        {
                            c.Add(dr.GetName(i), dr.GetValue(i));
                            i++;
                        }
                        rows.Add(c);
                    }
                }
                dr.Close();
            }
            return rows;
        }

        //private bool update()
        //{
        //    return true;
        //}

        #endregion

        #region Protected Methods

        protected bool ExecuteNonQuery(string cmdText, ref int rowsAffected)
        {
            if (Base.connection.State == System.Data.ConnectionState.Closed)
                Base.connection.Open();

            _cmd = new SQLiteCommand(cmdText, Base.connection);
            rowsAffected = _cmd.ExecuteNonQuery();
            return true;
        }

        protected bool ExecuteReader(string cmdText, ref SQLiteDataReader dr)
        {
            if (Base.connection.State == System.Data.ConnectionState.Closed)
                Base.connection.Open();

            _cmd = new SQLiteCommand(cmdText, Base.connection);
            dr = _cmd.ExecuteReader();
            return true;
        }

        protected bool ExecuteScalar(string cmdText, ref object obj)
        {
            if (Base.connection.State == System.Data.ConnectionState.Closed)
                Base.connection.Open();

            _cmd = new SQLiteCommand(cmdText, Base.connection);
            obj = _cmd.ExecuteScalar();
            return true;
        }

        #endregion

        #region Public Methods

        public List<Hashtable> all()
        {
            List<Hashtable> cList = null;

            string cmdText = "SELECT * FROM " + table_name + ";";

            SQLiteDataReader dr = null;

            if (this.ExecuteReader(cmdText, ref dr))
            {
                if (dr.HasRows)
                {
                    cList = new List<Hashtable>();

                    while (dr.Read())
                    {
                        Hashtable c = new Hashtable();

                        int i = 0;
                        while (i < dr.FieldCount)
                        {
                            c.Add(dr.GetName(i), dr.GetValue(i));
                            i++;
                        }

                        cList.Add(c);
                    }
                }
                dr.Close();
            }
            return cList;
        }

        public long count()
        {
            Hashtable args = new Hashtable();

            args.Add("select", "count(*) AS _count");

            List<Hashtable> rList = this.find(args);

            return (long)rList[0]["_count"];
        }

        public long count(Hashtable options)
        {
            if (options.ContainsKey("select"))
                options["select"] = "count(*) AS _count";

            List<Hashtable> rList = this.find(options);

            return (long)rList[0]["_count"];
        }

        public long count_by_sql(string sql)
        {
            List<Hashtable> rows = null;

            this.ExecuteReader(sql, ref rows);

            return (long)rows[0]["_count"];
        }

        public List<Hashtable> create(Hashtable attributes)
        {
            StringBuilder sb0 = new StringBuilder();
            StringBuilder sb1 = new StringBuilder();
            bool is_first = true;

            if (attributes.ContainsKey("id"))
            {
                attributes.Remove("id");
            }

            sb0.Append("INSERT INTO " + this.table_name + "(");
            
            foreach (string key in attributes.Keys)
            {
                if (is_first)
                {
                    sb0.Append(key);
                    sb1.Append("'" + attributes[key].ToString() + "'");
                    is_first = false;
                }
                else
                {
                    sb0.Append("," + key);
                    sb1.Append(",'" + attributes[key].ToString() + "'");
                }
            }

            sb0.Append(") VALUES(" + sb1.ToString() + ");");

            int rowsAffected = 0;

            this.ExecuteNonQuery(sb0.ToString(), ref rowsAffected);

            if (rowsAffected == 0)
            {
                return null;
            }

            object obj;

            List<Hashtable> rows = this.last();

            return rows;
        }

        public int delete(int id)
        {
            string conditions = "id=" + id;

            int rowsAffected = 0;

            this.ExecuteNonQuery(conditions, ref rowsAffected);

            return rowsAffected;
        }

        public int delete_all()
        {
            return this.delete_all(null);
        }

        public int delete_all(string conditions)
        {
            string sql = "DELETE FROM " + this.table_name;

            if (conditions != null)
                sql += " WHERE " + conditions + ";";

            int rowsAffected = 0;

            this.ExecuteNonQuery(sql, ref rowsAffected);

            return rowsAffected;
        }

        public List<Hashtable> find(Hashtable args)
        {
            string conditions = string.Empty;
            string order = string.Empty;
            string group = string.Empty;
            string having = string.Empty;
            string limit = string.Empty;
            string offset = string.Empty;
            string joins = string.Empty;
            string include = string.Empty;
            string select = string.Empty;
            string from = string.Empty;

            foreach (string k in args.Keys)
            {
                switch (k)
                {
                    case "conditions":
                        conditions = args["conditions"].ToString();
                        break;
                    case "order":
                        order = args["order"].ToString();
                        break;
                    case "group":
                        group = args["group"].ToString();
                        break;
                    case "limit":
                        limit = args["limit"].ToString();
                        break;
                    case "offset":
                        offset = args["offset"].ToString();
                        break;
                    case "joins":
                        joins = args["joins"].ToString();
                        break;
                    case "select":
                        select = args["select"].ToString();
                        break;
                }
            }

            List<Hashtable> rows = null;

            StringBuilder sb = new StringBuilder();

            if (string.IsNullOrEmpty(select))
                select = "*";

            sb.Append("SELECT " + select + " FROM " + table_name);

            if (!string.IsNullOrEmpty(joins))
                sb.Append(" " + joins);

            if (!string.IsNullOrEmpty(conditions))
                sb.Append(" WHERE " + conditions);

            if (!string.IsNullOrEmpty(group))
                sb.Append(" GROUP BY " + group);

            if (!string.IsNullOrEmpty(order))
                sb.Append(" ORDER BY " + order);

            if (!string.IsNullOrEmpty(limit))
                sb.Append(" LIMIT " + limit);

            if (!string.IsNullOrEmpty(offset))
                sb.Append(" OFFSET " + group);

            return this.ExecuteReader(sb.ToString(), ref rows);
        }

        public List<Hashtable> find(int id)
        {
            List<Hashtable> rows = null;

            string sql = "SELETC * FROM " + this.table_name + " WHERE id=" + id;

            return this.ExecuteReader(sql, ref rows);
        }

        public List<Hashtable> find_by_sql(string sql)
        {
            List<Hashtable> rows = null;

            return this.ExecuteReader(sql, ref rows);
        }

        public List<Hashtable> last()
        {
            List<Hashtable> rows = null;

            string sql = "SELECT * FROM " + this.table_name + " ORDER BY id DESC LIMIT 1";

            return this.ExecuteReader(sql, ref rows);
        }

        public bool save(Hashtable attributes)
        {
            StringBuilder sb0 = new StringBuilder();
            bool is_first = true;

            sb0.Append("UPDATE " + this.table_name + " SET ");

            foreach (string key in attributes.Keys)
            {
                if (key == "id")
                    continue;

                if (is_first)
                {
                    sb0.Append(key + "='" + attributes[key].ToString() + "'");
                    is_first = false;
                }
                else
                {
                    sb0.Append("," + key + "='" + attributes[key].ToString() + "'");
                }
            }

            sb0.Append(" WHERE id=" + attributes["id"].ToString() + ";");

            int rowsAffected = 0;

            this.ExecuteNonQuery(sb0.ToString(), ref rowsAffected);

            return (rowsAffected != 0);
        }

        public void transaction_do()
        {
            _transaction = Base.connection.BeginTransaction();
        }

        public void transaction_end()
        {
            _transaction.Commit();
        }

        #endregion

        #region Public Static Methods

        public static void establish_connection(string database)
        {
            if (Base.connection != null)
            {
                if (Base.connection.State == System.Data.ConnectionState.Open)
                {
                    if (Base.connection.FileName == database)
                        return;
                }
            }

            Base.close_connection();

            string connectionString = "Data Source=" + database + ";new=false;Version=3;";
            Base.connection = new SQLiteConnection(connectionString);

            if (Base.connection.State == System.Data.ConnectionState.Closed)
                Base.connection.Open();
        }

        public static void close_connection()
        {
            if (Base.connection != null)
            {
                if (Base.connection.State == System.Data.ConnectionState.Open)
                    Base.connection.Close();
            }
        }

        #endregion
    }
}
