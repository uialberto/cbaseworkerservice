using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseWorkService.Domain.Entities.MongoDb
{
    public abstract class EntityMongoDb<TEntity> : EntityMongoDb
    {
        [BsonElement("_doc")]
        [BsonRequired]
        public TEntity Doc { get; set; }
    }
}
