using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace BaseWorkService.Domain.Core.Facturas
{

    public sealed class Factura : BaseFactura
    {
        public bool IsDeleted { get; set; }
        public DateTime Created { get; set; }

        public DateTime? Updated { get; set; }
    }
}
