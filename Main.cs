using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using Microsoft.Win32;

namespace Wox.Plugin.Doc
{
    public class Main : IPlugin, ISettingProvider
    {
        private static DocSet docSet = new DocSet();

        public List<Result> Query(Query query)
        {
            if (string.IsNullOrEmpty(query.GetAllRemainingParameter().Trim()))
            {
                return new List<Result>();
            }

            return docSet.Query(query.GetAllRemainingParameter());
        }

        public void Init(PluginInitContext context)
        {
            docSet.Load(context.CurrentPluginMetadata.PluginDirectory);
        }

        public System.Windows.Controls.Control CreateSettingPanel()
        {
            return new SettingPanel();
        }
    }
}
