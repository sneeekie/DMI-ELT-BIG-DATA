using System.Collections.Concurrent;
using MongoDB.Bson;

namespace Transform.Services;

public class RawDMIDataStorageService
{
    public ConcurrentQueue<BsonDocument> Items { get; init; }
    
    public RawDMIDataStorageService()
    {
        Items = [];
    }
}