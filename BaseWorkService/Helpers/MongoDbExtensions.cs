using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace BaseWorkService.Helpers
{
    public abstract record QueryBase
    {
        public int Skip { get; set; }

        public int Take { get; set; } = 20;

        public bool TotalNeeded { get; set; } = true;

        public bool ShouldQueryTotal(ICollection result)
        {
            return TotalNeeded && (result.Count >= Take || Skip > 0);
        }
    }
    public static class MongoDbExtensions
    {
        public static IFindFluent<TDocument, TDocument> Page<TDocument>(this IFindFluent<TDocument, TDocument> find, QueryBase query)
        {
            return find.Skip(query.Skip).Limit(query.Take);
        }

        public static Task<List<TDocument>> ToListAsync<TDocument>(this IFindFluent<TDocument, TDocument> find, QueryBase query,
            CancellationToken ct)
        {
            return find.Skip(query.Skip).Limit(query.Take).ToListAsync(ct);
        }

        public static IFindFluent<TDocument, BsonDocument> Only<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object?>> include)
        {
            return find.Project<BsonDocument>(Builders<TDocument>.Projection.Include(include));
        }

        public static IFindFluent<TDocument, BsonDocument> Only<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object?>> include1,
            Expression<Func<TDocument, object?>> include2)
        {
            return find.Project<BsonDocument>(Builders<TDocument>.Projection.Include(include1).Include(include2));
        }

        public static IFindFluent<TDocument, BsonDocument> Only<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object?>> include1,
            Expression<Func<TDocument, object?>> include2,
            Expression<Func<TDocument, object?>> include3)
        {
            return find.Project<BsonDocument>(Builders<TDocument>.Projection.Include(include1).Include(include2).Include(include3));
        }

        public static IFindFluent<TDocument, TDocument> Not<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object?>> exclude)
        {
            return find.Project<TDocument>(Builders<TDocument>.Projection.Exclude(exclude));
        }

        public static IFindFluent<TDocument, TDocument> Not<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object?>> exclude1,
            Expression<Func<TDocument, object?>> exclude2)
        {
            return find.Project<TDocument>(Builders<TDocument>.Projection.Exclude(exclude1).Exclude(exclude2));
        }

        public static IFindFluent<TDocument, TDocument> Not<TDocument>(this IFindFluent<TDocument, TDocument> find,
            Expression<Func<TDocument, object?>> exclude1,
            Expression<Func<TDocument, object?>> exclude2,
            Expression<Func<TDocument, object?>> exclude3)
        {
            return find.Project<TDocument>(Builders<TDocument>.Projection.Exclude(exclude1).Exclude(exclude2).Exclude(exclude3));
        }

        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IFindFluent<T, T> find,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var cursor = await find.ToCursorAsync(ct);

            while (await cursor.MoveNextAsync(ct))
            {
                foreach (var item in cursor.Current)
                {
                    ct.ThrowIfCancellationRequested();

                    yield return item;
                }
            }
        }

        public static async IAsyncEnumerable<TProjection> ToAsyncEnumerable<TDocument, TProjection>(this IFindFluent<TDocument, TProjection> find,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var cursor = await find.ToCursorAsync(ct);

            while (await cursor.MoveNextAsync(ct))
            {
                foreach (var item in cursor.Current)
                {
                    ct.ThrowIfCancellationRequested();

                    yield return item;
                }
            }
        }

        public static FilterDefinition<T> RawNe<T>(this FilterDefinitionBuilder<T> builder, string field, BsonValue value)
        {
            var filter = new BsonDocument
            {
                [field] = new BsonDocument
                {
                    ["$ne"] = value
                }
            };

            return new RawFilterDefinition<T>(filter);
        }

        public static FilterDefinition<T> Eq<T>(this FilterDefinitionBuilder<T> builder, string field, BsonValue value)
        {
            var filter = new BsonDocument
            {
                [field] = new BsonDocument
                {
                    ["$eq"] = value
                }
            };

            return new RawFilterDefinition<T>(filter);
        }

        private sealed class RawFilterDefinition<T> : FilterDefinition<T>
        {
            private readonly BsonDocument document;

            public RawFilterDefinition(BsonDocument document)
            {
                this.document = document;
            }

            public override BsonDocument Render(IBsonSerializer<T> documentSerializer, IBsonSerializerRegistry serializerRegistry, LinqProvider linqProvider)
            {
                return document;
            }
        }
    }
}
