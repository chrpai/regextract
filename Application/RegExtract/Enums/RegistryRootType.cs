using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegExtract.Enums
{
    enum RegistryRootType
    {
        HKMU = -1,
        HKCR = 0,
        HKCU = 1,
        HKLM = 2,
        HKU = 3
    }
}
