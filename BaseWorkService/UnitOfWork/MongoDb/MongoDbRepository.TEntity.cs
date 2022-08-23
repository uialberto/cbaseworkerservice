﻿using BaseWorkService.Helpers;
using MongoDB.Driver;
using System.Globalization;

namespace BaseWorkService.UnitOfWork.MongoDb
{
    public abstract class MongoDbRepository<TEntity>
    {
        private const string CollectionFormat = "{0}Set";

        protected static readonly UpdateOptions Upsert = new UpdateOptions { IsUpsert = true };
        protected static readonly ReplaceOptions UpsertReplace = new ReplaceOptions { IsUpsert = true };
        protected static readonly SortDefinitionBuilder<TEntity> Sort = Builders<TEntity>.Sort;
        protected static readonly UpdateDefinitionBuilder<TEntity> Update = Builders<TEntity>.Update;
        protected static readonly FilterDefinitionBuilder<TEntity> Filter = Builders<TEntity>.Filter;
        protected static readonly IndexKeysDefinitionBuilder<TEntity> IndexKeys = Builders<TEntity>.IndexKeys;
        protected static readonly ProjectionDefinitionBuilder<TEntity> Projection = Builders<TEntity>.Projection;

        private readonly IMongoDatabase mongoDatabase;
        private IMongoCollection<TEntity> mongoCollection;

        public IMongoCollection<TEntity> Collection
        {
            get
            {
                if (mongoCollection == null)
                {
                    ThrowExceptionHelper.InvalidOperationException("Inicializacion de Coleccion Indefinida.");
                    return default!;
                }

                return mongoCollection;
            }
        }

        protected IMongoDatabase Database
        {
            get => mongoDatabase;
        }

        protected MongoDbRepository(IMongoDatabase database)
        {
            mongoDatabase = database;
        }

        protected virtual MongoCollectionSettings CollectionSettings()
        {
            return new MongoCollectionSettings();
        }

        protected virtual string CollectionName()
        {
            return string.Format(CultureInfo.InvariantCulture, CollectionFormat, typeof(TEntity).Name);
        }

        protected virtual Task SetupCollectionAsync(IMongoCollection<TEntity> collection, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public virtual async Task ClearAsync()
        {
            try
            {
                await Database.DropCollectionAsync(CollectionName());
            }
            catch (MongoCommandException ex)
            {
                if (ex.Code != 26)
                {
                    throw;
                }
            }

            await InitializeAsync(default);
        }

        public async Task InitializeAsync(CancellationToken ct)
        {
            CreateCollection();
            await SetupCollectionAsync(Collection, ct);
        }

        private void CreateCollection()
        {
            mongoCollection = mongoDatabase.GetCollection<TEntity>(CollectionName(), CollectionSettings() ?? new MongoCollectionSettings());
        }
    }
}
