using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace DynamoDbTest;

public class SongProcessor
{
    private readonly AmazonDynamoDBClient _dbClient;
    private readonly AmazonDynamoDBStreamsClient _streamsClient;

    public SongProcessor(AmazonDynamoDBClient dbClient, AmazonDynamoDBStreamsClient streamsClient)
    {
        _dbClient = dbClient;
        _streamsClient = streamsClient;
    }

    public void Start()
    {
        Task.Run(() => RunAsync());
    }

    private async Task RunAsync()
    {
        var streamArn = (await _dbClient.DescribeTableAsync("Music")).Table.LatestStreamArn;
        
        var describeStreamResponse = await _streamsClient.DescribeStreamAsync(streamArn);
        var shards = describeStreamResponse.StreamDescription.Shards;

        foreach (var shard in shards)
        {
            var shardIteratorResponse = await _streamsClient.GetShardIteratorAsync(new GetShardIteratorRequest
            {
                StreamArn = streamArn,
                ShardId = shard.ShardId,
                ShardIteratorType = ShardIteratorType.TRIM_HORIZON
            });

            await ProcessShard(shardIteratorResponse.ShardIterator);
        }
    }

    private async Task ProcessShard(string currentShardIterator)
    {
        while (currentShardIterator != null)
        {
            var recordsResponse = await _streamsClient.GetRecordsAsync(currentShardIterator);
            currentShardIterator = recordsResponse.NextShardIterator;
            
            if (recordsResponse.Records.Any())
            {
                ProcessRecords(recordsResponse.Records);
            }
            else
            {
                await Task.Delay(100);
            }
        }
    }

    private void ProcessRecords(List<Record> records)
    {
        foreach (var record in records)
        {
            Console.WriteLine($"Background processing song {record.Dynamodb.Keys["SongTitle"].S}");
        }
    }
}