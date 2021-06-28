using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLogic
{
    public partial class Installer : Component
    {
        public Installer()
        {
            InitializeComponent();
        }

        public Installer(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }
}
