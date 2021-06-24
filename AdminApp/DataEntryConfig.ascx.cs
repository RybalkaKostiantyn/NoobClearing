using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using uStore.Common.BLL;
using uStoreAPI.PluginBase.Clearing;

namespace AdminApp
{
    public partial class DataEntryConfig : ClearingPluginConfigurationBase
    {
        public override XmlDocument GetConfigurationData()
        {
            return new XmlDocument();
        }

        public override void LoadSavedData(XmlDocument pSavedData)
        {

        }

        public override ValidationResult ValidateControl()
        {
            return new ValidationResult(true);
        }
    }
}