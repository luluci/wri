using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wri.Interface.Office
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class Office
    {
        public ClosedXMLIf ClosedXML { get; set; }
        public Office()
        {
            ClosedXML = new ClosedXMLIf();
        }
    }
}
