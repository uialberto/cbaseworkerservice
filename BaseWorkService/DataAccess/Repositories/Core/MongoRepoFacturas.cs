using BaseWorkService.Domain.Core.Facturas;
using BaseWorkService.Domain.Interfaces.Repositories.Core;
using BaseWorkService.Helpers;
using BaseWorkService.UnitOfWork.MongoDb;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace BaseWorkService.DataAccess.Repositories.Core
{

    public sealed class MongoRepoFacturas : MongoDbRepository<Factura>, IMongoRepoFacturas
    {
        private readonly FacturaOptions options;
        private readonly ILogger<MongoRepoFacturas> log;

        static MongoRepoFacturas()
        {
            BsonClassMap.RegisterClassMap<BaseFactura>(cm =>
            {
                cm.AutoMap();
                cm.MapIdProperty(x => x.Id).SetSerializer(new GuidSerializer(BsonType.String));
            });

        }

        public MongoRepoFacturas(IMongoDatabase database, IOptions<FacturaOptions> options,ILogger<MongoRepoFacturas> log): base(database)
        {
            this.options = options.Value;
            this.log = log;
        }

        protected override string CollectionName()
        {
            return "Facturas";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<Factura> collection,
            CancellationToken ct = default)
        {
            await Collection.Indexes.CreateOneAsync(
                new CreateIndexModel<Factura>(
                    IndexKeys
                        .Ascending(x => x.Updated)
                        .Ascending(x => x.IsDeleted)
                        .Descending(x => x.Created)),
                null, ct);

            await Collection.Indexes.CreateOneAsync(
                new CreateIndexModel<Factura>(IndexKeys.Ascending(x => x.Created)), null, ct);

            await Collection.Indexes.CreateOneAsync(
                new CreateIndexModel<Factura>(IndexKeys.Descending(x => x.Created),
                    new CreateIndexOptions
                    {
                        ExpireAfter = options.RetentionTime
                    }),
                null, ct);
        }
        public async Task<Factura?> FindAsync(Guid id, CancellationToken ct = default)
        {

            var entity = await Collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
            return entity;

        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await Collection.UpdateOneAsync(x => x.Id == id, Update.Set(x => x.IsDeleted, true), cancellationToken: ct);
        }

        public async Task InsertAsync(Factura notification, CancellationToken ct = default)
        {

            try
            {
                await Collection.InsertOneAsync(notification, null, ct);
                await CleanupAsync(notification, ct);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                //throw new UniqueConstraintException();
            }

        }

        private async Task CleanupAsync(Factura notification,CancellationToken ct)
        {
            if (options.MaxItemsPerUser <= 0 || options.MaxItemsPerUser >= int.MaxValue)
            {
                return;
            }
            try
            {
                var filter = BuildFilter(notification);

                var oldNotifications = Collection.Find(filter).Skip(options.MaxItemsPerUser).SortBy(x => x.Created).Only(x => x.Id).ToAsyncEnumerable(ct);

                await foreach (var batch in oldNotifications.Chunk(5000, ct))
                {
                    var ids = batch.Select(x => x["_id"].AsString!);
                    await Collection.DeleteManyAsync(Filter.In("_id", ids), ct);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to cleanup notifications.");
            }
        }

        private static FilterDefinition<Factura> BuildFilter(Factura notification)
        {
            var filters = new List<FilterDefinition<Factura>>
            {
                Filter.Eq(x => x.AppId, notification.AppId),
                Filter.Eq(x => x.UserId, notification.UserId)
            };

            return Filter.And(filters);
        }
    }

}
