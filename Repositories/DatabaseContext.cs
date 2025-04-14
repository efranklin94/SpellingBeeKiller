using DomainModels.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using StackExchange.Redis;
using System.Linq.Expressions;

namespace Repositories
{
    public class DatabaseContext : IDatabaseContext
    {
        private readonly string VersionsHash;
        private readonly ILogger<DatabaseContext> logger;

        private readonly MongoClient client;

        // Databases
        private readonly IMongoDatabase GameDb;
        private readonly IMongoDatabase UsersDb;
        private readonly IDatabase RedisDb;
        private readonly IMongoDatabase TempDb;

        #region Collection Names
        private readonly string UsersCollectionName = "Users";
        private const string ClanCollectionName = "Clans";
        private const string ClanChatsCollectionName = "ClanChats";
        private const string ClanFinishedItemRequestCollectionName = "ClanFinishedItemRequests";
        private const string FinishedClanTournamentStatsCollectionName = "FinishedClanTournamentStats";
        private const string ViolationsCollectionName = "Violations";
        #endregion

        #region IMongoCollections
        #endregion

        #region Game Data Collections
        #endregion

        #region Users Data Collections
        public virtual IMongoCollection<User> UsersCollection { get; set; }
        #endregion

        public DatabaseContext(IConfiguration configuration, IRedisConnection redisConnection, ILogger<DatabaseContext> logger)
        {
            this.RedisDb = redisConnection.GetRedisDb();

            MongoClientSettings settings = MongoClientSettings.FromConnectionString(configuration.GetConnectionString("MongoDbConnection"));
            settings.WaitQueueTimeout = TimeSpan.FromMinutes(10);
            settings.MaxConnectionIdleTime = TimeSpan.FromMinutes(3);
            settings.ReadPreference = ReadPreference.PrimaryPreferred; // change this only if you are ready for 1 sec delay between write and reads

            this.client = new MongoClient(settings);

            // Databases
            GameDb = client.GetDatabase(nameof(GameDb));
            UsersDb = client.GetDatabase(nameof(UsersDb));
            TempDb = client.GetDatabase(nameof(TempDb));

            #region Game Db Collections
            #endregion

            #region Users Db Collections
            UsersCollection = UsersDb.GetCollection<User>(UsersCollectionName);
            #endregion

            #region Temp Db Collections
            #endregion

            VersionsHash = configuration.GetSection("ProjectSettings")["VersionsHash"]!;
            this.logger = logger;
        }

        public MongoClient GetMongoClient()
        {
            return client;
        }

        public async Task<string> ReplaceDataItemsAsync<T>(IMongoCollection<T> mongoCollection, List<T> newItems, double? version = null, IMongoDatabase db = null!)
        {
            var collectionName = mongoCollection.CollectionNamespace.CollectionName;

            RedisValue serverVersionRedisValue = await RedisDb.HashGetAsync(VersionsHash, collectionName);

            if (version.HasValue && serverVersionRedisValue.HasValue)
            {
                // get previous version
                var serverVersion = double.Parse(serverVersionRedisValue!);

                // find if we should check for version
                if (version <= serverVersion)
                {
                    return "New Version should be greater than old version";
                }
            }

            try
            {
                if (db == null)
                {
                    db = GameDb;
                }
                // replace the db
                await db.DropCollectionAsync(collectionName);

                if (newItems.Count != 0)
                {
                    await mongoCollection.InsertManyAsync(newItems);
                }

                if (version.HasValue)
                {
                    await RedisDb.HashSetAsync(VersionsHash, collectionName, version.ToString());
                }

                return null!;
            }
            catch (Exception e)
            {
                logger.LogCritical($"Failed to replace {collectionName} Db : {e}");
                return $"Failed to replace {collectionName}";
            }
        }

        public async Task<double> GetRepositoryVersionAsync<T>(IMongoCollection<T> repository)
        {
            return (double)await RedisDb.HashGetAsync(VersionsHash, repository.CollectionNamespace.CollectionName);
        }

        #region Indexes
        public void CreateIndexForCollectionsJob()
        {
            #region User
            CreateUniqueIndexForField<User>(UsersDb, x => x.Username, UsersCollectionName);

            CreateHashedIndexForField<User>(UsersDb, x => x.ActiveDeviceId, UsersCollectionName);

            // TODO CreateUniqueIndexForFieldWithPartialFilter<User>(UsersDb, x => x.StoreTournamentUserIdentifier, UsersCollectionName, @"{ ""StoreTournamentUserIdentifier"" : {""$type"" : ""string"" } }");

            CreateUniqueIndexForFieldWithPartialFilterAndIgnoreCase<User>(UsersDb, x => x.Email!, UsersCollectionName, @"{ ""Email"" : { ""$exists"" : true, ""$gt"" : ""0"", ""$type"" : ""string"" } }");
            // TODO CreateUniqueIndexForFieldWithPartialFilterAndIgnoreCase<User>(UsersDb, x => x.FacebookRecoveryId, UsersCollectionName, @"{ ""FacebookRecoveryId"" : { ""$exists"" : true, ""$gt"" : ""0"", ""$type"" : ""string"" } }");
            // TODO CreateUniqueIndexForFieldWithPartialFilterAndIgnoreCase<User>(UsersDb, x => x.AppleRecoveryId, UsersCollectionName, @"{ ""AppleRecoveryId"" : { ""$exists"" : true, ""$gt"" : ""0"", ""$type"" : ""string"" } }");
            // TODO CreateUniqueIndexForFieldWithPartialFilterAndIgnoreCase<User>(UsersDb, x => x.GoogleRecoveryId, UsersCollectionName, @"{ ""GoogleRecoveryId"" : { ""$exists"" : true, ""$gt"" : ""0"", ""$type"" : ""string"" } }");
            #endregion
        }

        private void CreateUniqueIndexForField<T>(IMongoDatabase db, Expression<Func<T, object>> field, string collectionName)
        {
            var options = new CreateIndexOptions() { Unique = true };
            var indexDefinition = new IndexKeysDefinitionBuilder<T>().Ascending(field);
            var indexModel = new CreateIndexModel<T>(indexDefinition, options);
            db.GetCollection<T>(collectionName).Indexes.CreateOne(indexModel);
        }

        private void CreateHashedIndexForField<T>(IMongoDatabase db, Expression<Func<T, object>> field, string collectionName)
        {
            var options = new CreateIndexOptions();
            var indexDefinition = new IndexKeysDefinitionBuilder<T>().Hashed(field);
            var indexModel = new CreateIndexModel<T>(indexDefinition, options);
            db.GetCollection<T>(collectionName).Indexes.CreateOne(indexModel);
        }

        private void CreateUniqueIndexForFieldWithPartialFilterAndIgnoreCase<T>(IMongoDatabase db, Expression<Func<T, object>> field,
            string collectionName, string partialFilter)
        {
            var options = new CreateIndexOptions<T>()
            {
                Unique = true,
                PartialFilterExpression = partialFilter,
                Collation = new Collation("en", strength: CollationStrength.Secondary)
            };
            var indexDefinition = new IndexKeysDefinitionBuilder<T>().Ascending(field);
            var indexModel = new CreateIndexModel<T>(indexDefinition, options);
            db.GetCollection<T>(collectionName).Indexes.CreateOne(indexModel);
        }
        #endregion

        public IMongoDatabase GetTempDatabase()
        {
            return TempDb;
        }
    }
}
