using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseWorkService.Domain.Core.Facturas
{

    public sealed class FacturaOptions 
    {
        public TimeSpan RetentionTime { get; init; } = TimeSpan.FromDays(20);

        public int MaxItemsPerUser { get; init; } = 5000;

        public IEnumerable<(string errorMessage, string pathError)> Validate()
        {
            if (RetentionTime <= TimeSpan.Zero)
            {
                yield return new ("Retention time must be greater than zero.", nameof(RetentionTime));
            }
        }
    }
}
