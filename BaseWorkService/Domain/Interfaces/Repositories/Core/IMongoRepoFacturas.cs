using BaseWorkService.Domain.Core.Facturas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseWorkService.Domain.Interfaces.Repositories.Core
{
    public interface IMongoRepoFacturas
    {
        Task<Factura?> FindAsync(Guid id, CancellationToken ct = default);
    }
}
