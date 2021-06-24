using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using uStore.Common.BLL;
using uStore.Common.BLL.Clearing;

namespace LogicBase
{
    public class NoobClearing : ClearingLogicBase
    {
        public override ValidationResult DoClearing(Currency currency, decimal amount, XmlDocument userData, out XmlDocument resultData)
        {
            ValidationResult result = new ValidationResult(true);

            resultData = new XmlDocument();
            resultData.LoadXml("<ResultData/>");

            return result;
        }
    }
}