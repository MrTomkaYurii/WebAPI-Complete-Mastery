using MongoDB.Bson;
using MongoDB.Driver;

namespace JoinCollectionsAggregationPipeline.Models;

public static class MongoHelper
{
    public static async Task AddSeedData(IMongoDatabase database)
    {
        var courseCollection = database.GetCollection<BsonDocument>("Courses");
        var studentCollection = database.GetCollection<BsonDocument>("Students");

        // IDs курсів
        var course1Id = new ObjectId("655e134180c300fcdd067d24");
        var course2Id = new ObjectId("655e134180c300fcdd067d25");

        // Вставка курсів, якщо вони не існують
        var courseFilter = Builders<BsonDocument>.Filter.In("_id", new[] { course1Id, course2Id });
        if (!await courseCollection.Find(courseFilter).AnyAsync())
        {
            await courseCollection.InsertManyAsync(new List<BsonDocument>
            {
                new BsonDocument { { "_id", course1Id }, { "Name", "Networks" }, { "Code", "ECEN 474" } },
                new BsonDocument { { "_id", course2Id }, { "Name", "Power Systems" }, { "Code", "ECEN 485" } }
            });
            Console.WriteLine("Courses inserted.");
        }
        else
        {
            Console.WriteLine("Courses already exist, skipping insertion.");
        }

        // ID студента
        var studentId = new ObjectId("656623db682962fa62ad75ba");

        // Вставка студента, якщо його не існує
        var studentFilter = Builders<BsonDocument>.Filter.Eq("_id", studentId);
        if (!await studentCollection.Find(studentFilter).AnyAsync())
        {
            await studentCollection.InsertOneAsync(new BsonDocument
            {
                { "_id", studentId },
                { "FirstName", "John" },
                { "LastName", "Doe" },
                { "Major", "Electrical Engineering" },
                { "Courses", new BsonArray { course1Id, course2Id } }
            });
            Console.WriteLine("Student inserted.");
        }
        else
        {
            Console.WriteLine("Student already exists, skipping insertion.");
        }

    }
}
