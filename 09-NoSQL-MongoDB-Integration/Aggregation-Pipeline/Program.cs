using JoinCollectionsAggregationPipeline;
using JoinCollectionsAggregationPipeline.Models;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Testcontainers.MongoDb;

//await using var mongoDbContainer = new MongoDbBuilder().Build();
//await mongoDbContainer.StartAsync();
//var mongoClient = new MongoClient(mongoDbContainer.GetConnectionString());

var mongoClient = new MongoClient("mongodb://localhost:27017");
var repository = new StudentRepository(mongoClient);

//Seed Data
var database = mongoClient.GetDatabase(DatabaseConfiguration.DatabaseName);
await MongoHelper.AddSeedData(database);

var users = await repository.GetAllUsers();
foreach (var user in users)
{
    var json = user.ToJson(new JsonWriterSettings { Indent = true });
    Console.WriteLine(json);
}

//await mongoDbContainer.StopAsync();