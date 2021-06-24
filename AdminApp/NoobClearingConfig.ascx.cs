using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;
using uStore.Common.BLL;
using uStore.Common.BLL.Clearing;
using uStore.Common.BLL.Datasource;
using uStoreAPI.PluginBase.Clearing;

namespace AdminApp
{
    public partial class NoobClearingConfig : ClearingPluginConfigurationBase
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        public override XmlDocument GetConfigurationData()
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml("<Config/>");

            XmlNode customHTML_PropertyDefault = document.DocumentElement.AppendChild(document.CreateElement("DefaultClearingResult"));
            customHTML_PropertyDefault.InnerText = DefaultClearingResult.Value;

            XmlNode customHTML_Markup = document.DocumentElement.AppendChild(document.CreateElement("Markup"));
            customHTML_Markup.InnerText = Markup.Value;

            return document;
        }

        public override void LoadSavedData(XmlDocument pSavedData)
        {
            // CustomHTML_Markup
            XmlNode customHTML_Markup = pSavedData.SelectSingleNode("//Markup");
            if (customHTML_Markup != null)
            {
                Markup.Value = customHTML_Markup.InnerText;
            }
        }
    }
}