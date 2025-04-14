using DomainModels.Models;
using MongoDB.Driver;

namespace Repositories
{
    public interface IDatabaseContext
    {
        #region Collections
        IMongoCollection<User> UsersCollection { get; }
        #endregion

        #region Methods
        void CreateIndexForCollectionsJob();
        MongoClient GetMongoClient();
        Task<double> GetRepositoryVersionAsync<T>(IMongoCollection<T> repository);
        Task<string> ReplaceDataItemsAsync<T>(IMongoCollection<T> mongoCollection, List<T> newItems,
            double? version = null, IMongoDatabase db = null!);
        IMongoDatabase GetTempDatabase();
        #endregion
    }
}
