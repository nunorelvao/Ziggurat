﻿using DotNetCore.CAP;
using MongoDB.Driver;
using Ziggurat;
using Ziggurat.MongoDB;

public class Consumer : ICapSubscribe
{
    private readonly IConsumerService<MyMessage> _service;

    public Consumer(IConsumerService<MyMessage> service)
    {
        _service = service;
    }

    [CapSubscribe("message", Group = "sample.rabbitmq.mongodb")]
    public async Task ConsumeMessage(MyMessage message)
    {
        await _service.ProcessMessageAsync(message);
    }
}

public class ConsumerService : IConsumerService<MyMessage>
{
    private readonly ILogger<ConsumerService> _logger;
    private readonly MongoClient _client;

    public ConsumerService(ILogger<ConsumerService> logger, MongoClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task ProcessMessageAsync(MyMessage message)
    {
        // IO
        var databaseName = "test";
        _logger.LogInformation(message.Text);
        
        using var session = _client.StartIdempotentTransaction(message, databaseName);
        var collection = _client.GetDatabase(databaseName).GetCollection<MyMessage>("test.collection");

        await collection.InsertOneAsync(session, message);
    }
}