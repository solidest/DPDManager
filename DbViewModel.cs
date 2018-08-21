using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPDManager
{
    public class DbViewModel:IDisposable
    {
        private SQLiteConnection _conn;
        private SQLiteCommand _cmd;
        private SQLiteHelper _hp;
        public DbViewModel()
        {
            InitialDb();
            TableList = _hp.GetTableList().AsEnumerable().Select(t => t.Field<string>("Tables").ToString());
        }

        private void InitialDb()
        {
            _conn = new SQLiteConnection(@"data source=D:\DPDParser\DB\parser.db");
            _cmd = new SQLiteCommand();
            _cmd.Connection = _conn;
            _conn.Open();
            _hp = new SQLiteHelper(_cmd);
        }

        public void Dispose()
        {
            _cmd.Dispose();
            _conn.Close();
            _conn.Dispose();
        }

        public IEnumerable<String> TableList { get; private set; }

        #region --Code Generate--

        public string GetTableCode(System.Collections.IList tlist)
        {
            var ret = new StringBuilder();

            //Declare stmt
            ret.Append("//Declare--------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n");
            foreach(string tablename in tlist)
            {
                ret.Append("\tsqlite3_stmt *m_" + tablename + "_stmt;\n");
            }
            ret.Append("\n");

            //Initial stmt
            ret.Append("//Initial--------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n");
            foreach (string tablename in tlist)
            {
                var cols = _hp.GetColumnStatus(tablename).AsEnumerable().Select(t => t.Field<string>("name").ToString());
                ret.Append("\tres = sqlite3_prepare_v2(m_pDB,\"");
                AppendInsertSqlCmd(ret, tablename, cols);
                ret.Append("\", -1, &m_" + tablename + "_stmt, NULL);\n");
                ret.Append("\tif (res != SQLITE_OK)\n\t{\n\t\tm_" + tablename +"_stmt = NULL;\n\t}\n\n");
            }
            ret.Append("\n");

            //Release stmt
            ret.Append("//Release--------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n");
            foreach (string tablename in tlist)
            {
                ret.Append("\tif (m_" + tablename + "_stmt)\n\t{\n\t\tsqlite3_finalize(m_" + tablename + "_stmt);\n");
                ret.Append("\t\tm_" + tablename + "_stmt= NULL;\n\t}\n\n");
            }

            //Action
            ret.Append("//Release--------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n");
            foreach (string tablename in tlist)
            {

            }

            return ret.ToString();
            
        }

        private void AppendInsertSqlCmd(StringBuilder ret, string tablename, IEnumerable<string> cols)
        {
            ret.Append("INSERT INTO " + tablename + " (");
            foreach (var r in cols)
            {
                ret.Append(r + ", ");
            }
            ret.Remove(ret.Length - 2, 2);
            ret.Append(") VALUES(");
            foreach (var r in cols)
            {
                ret.Append(":" + r + ", ");
            }
            ret.Remove(ret.Length - 2, 2);
            ret.Append(");");
        }

    #endregion
}
}
