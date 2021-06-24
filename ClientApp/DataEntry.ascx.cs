using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using uStore.Common.BLL;
using uStore.Common.BLL.Clearing;
using uStore.Common.BLL.Coupons;
using uStore.Common.BLL.Delivery.Shipping;
using uStore.Common.BLL.PricingModels.DefaultPricingModel;
using XPI.Log;
using XPI.WebInfrastructure.Page;

namespace ClientApp
{
    public partial class DataEntry : ClearingUserDataBase
    {
        public override XmlDocument UserData
        {
            get
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml("<UserData/>");
                if (String.IsNullOrWhiteSpace(hfDefaultClearingResult.Value) == false)
                {
                    byte[] data = Convert.FromBase64String(hfDefaultClearingResult.Value);
                    document.DocumentElement.AppendChild(document.CreateElement("ClearingResult")).InnerText = Encoding.UTF8.GetString(data);
                }
                return document;
            }
        }
    }
}