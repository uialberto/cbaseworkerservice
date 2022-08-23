using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseWorkService.Domain.Core.Facturas
{
    public class BaseFactura
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string AppId { get; set; }
        IEnumerable<KeyValuePair<string, object>> Properties
        {
            get
            {
                if (Properties != null)
                {
                    yield return new KeyValuePair<string, object>("properties", Properties);
                }
            }
        }
    }
}
