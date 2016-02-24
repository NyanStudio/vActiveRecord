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

        SQLiteConnection _conn;
        SQLiteCommand _cmd;

        int _id = 0;

        #endregion

        #region Public Properties

        protected string table_name { get; set; }

        /*public virtual int id
        {
            get { return _id; }
            set { _id = value; }
        }*/

        public SQLiteConnection connection
        {
            get { return _conn; }
            set { _conn = value; }
        }

        public bool is_new_record { get; set; }

        #endregion

        #region Constructors

        public Base()
        {
            _conn = new SQLiteConnection();
        }

        #endregion

        #region Private Methods

        private bool create()
        {
            return true;
        }

        private bool create(Hashtable attributes)
        {
            //INSERT INTO this.table_name() VALUES();
            StringBuilder sb = new StringBuilder();
            bool is_first = true;
            foreach (string key in attributes.Keys)
            {
                if (is_first)
                {
                    sb.Append(key);
                }
                else
                {
                    sb.Append("," + key);
                }
            }

            return true;
        }

        private bool create_or_update()
        {
            return is_new_record ? create() : update();
        }

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

        private bool update()
        {
            return true;
        }

        #endregion

        #region Protected Methods

        protected bool ExecuteNonQuery(string cmdText, ref int rowsAffected)
        {
            try
            {
                _cmd = new SQLiteCommand(cmdText, _conn);
                rowsAffected = _cmd.ExecuteNonQuery();
                return true;
            }
            catch(SQLiteException ex)
            {

            }
            return false;
        }

        protected bool ExecuteReader(string cmdText, ref SQLiteDataReader dr)
        {
            try
            {
                _cmd = new SQLiteCommand(cmdText, _conn);
                dr = _cmd.ExecuteReader();
                return true;
            }
            catch (SQLiteException ex)
            {

            }
            return false;
        }

        protected bool ExecuteScalar(string cmdText, ref object obj)
        {
            try
            {
                _cmd = new SQLiteCommand(cmdText, _conn);
                obj = _cmd.ExecuteScalar();
                return true;
            }
            catch (Exception ex)
            {
            }
            return false;
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

        public int count()
        {
            Hashtable args = new Hashtable();

            args.Add("select", "count(*) AS _count");

            List<Hashtable> rList = this.find(args);

            return (int)rList[0][""];
        }

        public int count(Hashtable options)
        {
            if (options.ContainsKey("select"))
                options["select"] = "count(*) AS _count";

            List<Hashtable> rList = this.find(options);

            return (int)rList[0][""];
        }

        public int count_by_sql(string sql)
        {
            List<Hashtable> rows = null;

            this.ExecuteReader(sql, ref rows);

            return (int)rows[0][""];
        }

        public int delete(int id)
        {
            string conditions = "id=" + id;

            int rowsAffected = 0;

            this.ExecuteNonQuery(conditions, ref rowsAffected);

            return rowsAffected;
        }

        public int delete_all(string conditions)
        {
            string cmdText = "DELETE FROM " + this.table_name + " WHERE " + conditions + ";";

            int rowsAffected = 0;

            this.ExecuteNonQuery(cmdText, ref rowsAffected);

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

            sb.Append("SELECT " + select + " FROM" + table_name);

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

        public SQLiteTransaction transaction()
        {
            return _conn.BeginTransaction();
        }

        #endregion
    }
}
