using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uStoreInstallerCommonLogic
{
    public class MultiStoreException : Exception
    {
        public MultiStoreException(string message) : base(message) { }
    }
}
