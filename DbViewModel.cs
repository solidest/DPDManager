using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
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
            SegTypeList = _hp.Select("select * from dic_segtype;").AsEnumerable().Select(t => new ISItem() { StrValue = t.Field<string>("segtypetoken").ToString(), IntValue = (int)t.Field<Int64>("segtypecode") });
            ValueTypeList = _hp.Select("select * from dic_propertyvaluetype;").AsEnumerable().Select(t => new ISItem() { StrValue = t.Field<string>("note").ToString(), IntValue = (int)t.Field<Int64>("pvaluetypecode") });
            PropertyList = _hp.Select("select * from dic_propertytoken;").AsEnumerable().Select(t => t.Field<string>("propertytoken").ToString());
            PValueList = _hp.Select("select * from dic_propertyvaluetoken;").AsEnumerable().Select(t => t.Field<string>("pvaluetoken").ToString());
        }

        private void InitialDb()
        {
            _conn = new SQLiteConnection(@"data source=C:\Kiyun\DPDManager\db\dpd_parser.db");
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

        public void Execute(string sql)
        {
            _hp.Execute(sql);
        }

        public IEnumerable<Rule> GetRuleList()
        {
            var ret = new List<Rule>();
            GetNessaryRule(ret);
            GetCompatibleRule(ret);
            GetForceRule(ret);
            GetTypeEnum(ret);
            GetTypeCompatible(ret);
            return ret;
        }

        private void GetTypeCompatible(List<Rule> ret)
        {
            var tb = _hp.Select("SELECT rowid, propertytoken, valuetype FROM rule_property_vtype;");
            var dic = new Dictionary<String, List<Int32>>();
            var dic2 = new Dictionary<String, List<Int32>>();

            foreach (DataRow r in tb.Rows)
            {
                List<Int32> plist = null;
                List<Int32> plist2 = null;
                Int32 vtype = Convert.ToInt32(r["valuetype"]);
                string prop = r["propertytoken"].ToString();
                if (dic.ContainsKey(prop))
                {
                    plist = dic[prop];
                    plist2 = dic2[prop];
                }
                else
                {
                    plist = new List<Int32>();
                    plist2 = new List<Int32>();
                    dic.Add(prop, plist);
                    dic2.Add(prop, plist2);
                }
                plist.Add(vtype);
                plist2.Add(Convert.ToInt32(r["rowid"]));
            }

            foreach (string key in dic.Keys)
            {
                string ruletext = "TypeCompatible: " + key + " = " + "(" + GetStrList(dic[key].Select( p=> GetValueTypeName(p))) + ")";
                ret.Add(new Rule() { TableName = "rule_property_vtype", RuleText = ruletext, RowList = dic2[key] });
            }
        }

        private void GetTypeEnum(List<Rule> ret)
        {
            var tb = _hp.Select("SELECT rowid, propertytoken, valuetoken FROM rule_property_vtoken;");
            var dic = new Dictionary<String, List<string>>();
            var dic2 = new Dictionary<String, List<Int32>>();

            foreach (DataRow r in tb.Rows)
            {
                List<String> plist = null;
                List<Int32> plist2 = null;
                string vtoken = r["valuetoken"].ToString();
                string prop = r["propertytoken"].ToString();
                if (dic.ContainsKey(prop))
                {
                    plist = dic[prop];
                    plist2 = dic2[prop];
                }
                else
                {
                    plist = new List<string>();
                    plist2 = new List<Int32>();
                    dic.Add(prop, plist);
                    dic2.Add(prop, plist2);
                }
                plist.Add(vtoken);
                plist2.Add(Convert.ToInt32(r["rowid"]));
            }

            foreach (string key in dic.Keys)
            {
                string ruletext = "TypeEnum: " + key + " = " + "(" + GetStrList(dic[key]) + ")";
                ret.Add(new Rule() { TableName = "rule_property_vtoken", RuleText = ruletext, RowList = dic2[key] });
            }
        }

        private void GetForceRule(List<Rule> ret)
        {
            var tb = _hp.Select("SELECT rowid, segtype, propertytoken, valuetype FROM rule_segtype_property_vtype;");
            foreach (DataRow r in tb.Rows)
            {
                var rl= new Rule();
                rl.RuleText = "Force: " + GetSegTypeName(Convert.ToInt32(r["segtype"])) + "." + r["propertytoken"].ToString() + " = " + GetValueTypeName(Convert.ToInt32(r["valuetype"]));
                rl.RowList = new List<int>();
                rl.RowList.Add(Convert.ToInt32(r["rowid"]));
                rl.TableName = "rule_segtype_property_vtype";
                ret.Add(rl);
            }
        }

        private void GetCompatibleRule(List<Rule> ret)
        {
            var tb = _hp.Select("SELECT rowid, segtype, propertytoken FROM rule_segtype_compatibleproperty; ");
            var dic = new Dictionary<Int32, List<string>>();
            var dic2 = new Dictionary<Int32, List<Int32>>();

            foreach (DataRow r in tb.Rows)
            {
                List<String> plist = null;
                List<Int32> plist2 = null;
                Int32 segtype = Convert.ToInt32(r["segtype"]);
                string prop = r["propertytoken"].ToString();
                if (dic.ContainsKey(segtype))
                {
                    plist = dic[segtype];
                    plist2 = dic2[segtype];
                }
                else
                {
                    plist = new List<string>();
                    plist2 = new List<Int32>();
                    dic.Add(segtype, plist);
                    dic2.Add(segtype, plist2);
                }
                plist.Add(prop);
                plist2.Add(Convert.ToInt32(r["rowid"]));
            }


            foreach (Int32 key in dic.Keys)
            {
                string ruletext = "Compatible: " + GetSegTypeName(key) + "." + "(" + GetStrList(dic[key]) + ")";
                ret.Add(new Rule() { TableName = "rule_segtype_compatibleproperty", RuleText = ruletext, RowList = dic2[key] });
            }

        }

        private void GetNessaryRule(List<Rule> ret)
        {
            var tb = _hp.Select("SELECT rowid, segtype, propertytoken FROM rule_segtype_necessaryproperty");
            var dic = new Dictionary<Int32, List<string>>();
            var dic2 = new Dictionary<Int32, List<Int32>>();

            foreach (DataRow r in tb.Rows)
            {
                List<String> plist = null;
                List<Int32> plist2 = null;
                Int32 segtype = Convert.ToInt32(r["segtype"]);
                string prop = r["propertytoken"].ToString();
                if (dic.ContainsKey(segtype))
                {
                    plist = dic[segtype];
                    plist2 = dic2[segtype];
                }
                else
                {
                    plist = new List<string>();
                    plist2 = new List<Int32>();
                    dic.Add(segtype, plist);
                    dic2.Add(segtype, plist2);
                }
                plist.Add(prop);
                plist2.Add(Convert.ToInt32(r["rowid"]));
            }

            
            foreach (Int32 key in dic.Keys)
            {
                string ruletext = "Necessary: " + GetSegTypeName(key) + "." + "(" + GetStrList(dic[key]) + ")";
                ret.Add(new Rule() { TableName = "rule_segtype_necessaryproperty", RuleText = ruletext, RowList = dic2[key] });
            }

        }

        public void RemoveRule(Rule rl)
        {
            foreach(Int32 id in rl.RowList)
            {
                string sql = "DELETE FROM " + rl.TableName + " WHERE rowid = " + id.ToString();
                _hp.Execute(sql);
            }
        }

        private string GetStrList(IEnumerable<string> l)
        {
            string ss = "";
            foreach(string s in l)
            {
                ss += s + ", ";
            }
            return ss.Substring(0, ss.Length - 2);
        }

        private string GetSegTypeName(Int32 id)
        {
            foreach(var it in SegTypeList)
            {
                if (it.IntValue == id) return it.StrValue;
            }
            return "";
        }

        private string GetValueTypeName(Int32 id)
        {
            foreach (var it in ValueTypeList)
            {
                if (it.IntValue == id) return it.StrValue;
            }
            return "";
        }

        public IEnumerable<String> TableList { get; private set; }
        public IEnumerable<ISItem> SegTypeList { get; private set; }
        public IEnumerable<String> PropertyList { get; private set; }
        public IEnumerable<String> PValueList { get; private set; }
        public IEnumerable<ISItem> ValueTypeList { get; private set; }

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
            ret.Append("\n");

            //Action
            ret.Append("//Insert--------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n");
            foreach (string tablename in tlist)
            {
                var cols = _hp.GetColumnStatus(tablename);
                int colindex = 0;
                foreach(DataRow r in cols.Rows)
                {
                    colindex += 1;
                    string colname = r["name"].ToString();
                    string coltype = r["type"].ToString();
                    if(coltype=="INTEGER")
                    {
                        ret.Append("\tsqlite3_bind_int(m_" + tablename + "_stmt, " + colindex.ToString() + ", " + colname + ");\n");
                    }
                    else if(coltype == "TEXT")
                    {
                        ret.Append("\tsqlite3_bind_text(m_" + tablename + "_stmt, " + colindex.ToString() + ", " + colname + "-1, SQLITE_STATIC);\n");
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
                ret.Append("\tint rc = sqlite3_step(m_" + tablename + "_stmt);\n\tif ((rc != SQLITE_DONE) && (rc != SQLITE_ROW))\n\t{\n\t\treturn -1;\n\t}\n");
                ret.Append("\tsqlite3_reset(m_" + tablename + "_stmt);\n\n");
            }
            ret.Append("\n");

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

        #region --Rule Manager--



        #endregion
    }

}
