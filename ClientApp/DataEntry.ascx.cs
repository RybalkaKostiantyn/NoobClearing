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
        public string Currency
        {
            get
            {
                Order order = Order.Get(OrderId, CultureID);
                var storeDetails = uStore.Common.BLL.Store.GetStoreDetails(order.StoreId);
                var currency = uStore.Common.BLL.Currency.Get(storeDetails.PrimaryCurrencyID);
                return Newtonsoft.Json.JsonConvert.SerializeObject(currency);
            }
        }

        public decimal OrderPrice
        {
            get
            {
                return GetOrderPrice().Total;
            }
        }

        public string Culture
        {
            get
            {
                uStoreAPI.Culture culture = uStoreAPI.Culture.GetCulture(CultureID);
                return Newtonsoft.Json.JsonConvert.SerializeObject(culture);
            }
        }

        public string DefaultClearingResult
        {
            get
            {
                XmlNode xmlNode = ClearingConfigXml.SelectSingleNode("//DefaultClearingResult");
                return xmlNode.InnerText;
            }
        }

        public string Markup
        {
            get
            {
                XmlNode xmlNode = ClearingConfigXml.SelectSingleNode("//Markup");
                return xmlNode.InnerText;
            }
        }

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

        public override ValidationResult Validate()
        {
            return new ValidationResult(true);
        }

        #region private

        private OrderPrice GetOrderPrice()
        {
            Order order = Order.Get(OrderId, CultureID);
            order.DeliveryTentatives = GetShipments();

            var couponCode = txtCouponCode != null ? txtCouponCode.Text : String.Empty;

            decimal discount = 0M;
            if (couponCode.Trim() != string.Empty)
            {
                try
                {
                    Coupon coupon = new Coupon(couponCode, base.CurrentUser.UserID, this.CultureID);
                    ValidationResult vr = coupon.Validate(this.OrderId);
                    if (vr.IsValid)
                    {
                        discount = coupon.CalculateDiscount(this.OrderId);
                        coupon.AssociateWithUser(base.CurrentUser.UserID);
                    }
                }
                catch { }
            }

            var orderPrice = uStore.Common.BLL.PricingModels.DefaultPricingModel.OrderPrice.Calculate(order, discount, this.CultureID);
            return orderPrice;
        }

        private TextBox txtCouponCode
        {
            get
            {
                TextBox txtCouponCode = GetControl(this, @"cphMainContent", @"txtCouponCode") as TextBox;
                return txtCouponCode;
            }
        }

        public static Control GetControl(Control context, params string[] ids)
        {
            try
            {
                if (context != null)
                {
                    var currentControl = context;
                    //search the top control in a sequence
                    while (true)
                    {
                        if (currentControl.FindControl(ids[0]) != null || currentControl.Parent == null)
                        {
                            break;
                        }
                        currentControl = currentControl.Parent;
                    }
                    //get end control in the sequence of controls
                    foreach (string id in ids)
                    {
                        currentControl = currentControl.FindControl(id);
                    }
                    return currentControl;
                }
            }
            catch (Exception exc)
            {
                throw exc;
            }
            return null;
        }

        private List<DeliveryTentative> GetShipments()
        {
            try
            {
                return PageContext.PageDataParams["OrderShipments"] as List<DeliveryTentative>;
            }
            catch (Exception exc)
            {
                Logger.WriteError(exc.ToString());
                return new List<DeliveryTentative>();
            }
        }

        #endregion
    }
}