using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Net;
using Microsoft.Win32;
using System.Linq;

namespace Wox.Plugin.Doc
{
    public class DocSet
    {
        private static List<Doc> installedDocs = new List<Doc>();
        private string docsetPath;

        public static List<Doc> InstalledDocs
        {
            get { return installedDocs; }
        }

        /// <summary>
        /// Load all installed docs
        /// </summary>
        public void Load(string pluginDirectory)
        {
            docsetPath = Path.Combine(pluginDirectory, @"Docset");
            if (!Directory.Exists(docsetPath))
            {
                Directory.CreateDirectory(docsetPath);
            }

            foreach (string docPath in Directory.GetDirectories(docsetPath))
            {
                string docName = Path.GetFileNameWithoutExtension(docPath);
                string dbPath = Path.Combine(docPath, @"Contents\Resources\docSet.dsidx");
                installedDocs.Add(new Doc
                    {
                        Name = docName,
                        DBPath = dbPath,
                        DBType = CheckTableExists("searchIndex", dbPath) ? DocType.DASH : DocType.ZDASH,
                        IconPath = pluginDirectory + "\\Images\\icons\\" + docName.Replace(" ", "_") + ".png"
                    });
            }
        }

        private bool CheckTableExists(string table, string path)
        {
            string dbPath = "Data Source =" + path;
            SQLiteConnection conn = new SQLiteConnection(dbPath);
            conn.Open();
            string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name='" + table + "';";
            SQLiteCommand cmdQ = new SQLiteCommand(sql, conn);
            object obj = cmdQ.ExecuteScalar();
            conn.Close();
            return obj != null;
        }

        public List<Result> Query(string query)
        {
            List<Result> results = new List<Result>();
            foreach (Doc doc in InstalledDocs)
            {
                results.AddRange(QuerySqllite(doc, query));
            }
            return results.OrderBy(r => r.Title.Length).ToList();
        }

        private List<Result> QuerySqllite(Doc doc, string key)
        {
            string dbPath = "Data Source =" + doc.DBPath;
            SQLiteConnection conn = new SQLiteConnection(dbPath);
            conn.Open();
            string sql = GetQuerySqlByDocType(doc.DBType).Replace("{0}", key);
            SQLiteCommand cmdQ = new SQLiteCommand(sql, conn);
            SQLiteDataReader reader = cmdQ.ExecuteReader();

            List<Result> results = new List<Result>();
            while (reader.Read())
            {
                string name = reader.GetString(reader.GetOrdinal("name"));
                string docPath = reader.GetString(reader.GetOrdinal("path"));

                results.Add(new Result
                    {
                        Title = name,
                        SubTitle = doc.Name.Replace(".docset", ""),
                        IcoPath = doc.IconPath,
                        Action = (c) =>
                            {
                                string url = string.Format(@"{0}\{1}\Contents\Resources\Documents\{2}#{3}", docsetPath,
                                                           doc.Name+".docset", docPath, name);
                                string browser = GetDefaultBrowserPath();
                                Process.Start(browser, String.Format("\"file:///{0}\"", url));
                                return true;
                            }
                    });
            }

            conn.Close();

            return results;
        }

        private static string GetDefaultBrowserPath()
        {
            string key = @"HTTP\shell\open\command";
            using (RegistryKey registrykey = Registry.ClassesRoot.OpenSubKey(key, false))
            {
                if (registrykey != null) return ((string)registrykey.GetValue(null, null)).Split('"')[1];
            }
            return null;
        }

        private string GetQuerySqlByDocType(DocType type)
        {
            string sql = string.Empty;
            if (type == DocType.DASH)
            {
                sql = "select * from searchIndex where name like '%{0}%' order by name = '{0}' desc, name like '{0}%' desc limit 30";
            }
            if (type == DocType.ZDASH)
            {
                sql = @"select ztokenname as name, zpath as path from ztoken
join ztokenmetainformation on ztoken.zmetainformation = ztokenmetainformation.z_pk
join zfilepath on ztokenmetainformation.zfile = zfilepath.z_pk
where (ztokenname like '%{0}%') order by length(ztokenname), lower(ztokenname) asc, zpath asc limit 30";
            }

            return sql;
        }
    }
}