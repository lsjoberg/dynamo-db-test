using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;

namespace DynamoDbTest;

public class MusicRepository
{
    private readonly AmazonDynamoDBClient _dbClient;

    public MusicRepository(AmazonDynamoDBClient dbClient)
        => _dbClient = dbClient;
    
    public async Task<string> AddSong()
    {
        var song = $"Song {Guid.NewGuid().ToString()[..6]}";
        await _dbClient.PutItemAsync("Music",
            new Dictionary<string, AttributeValue>
            {
                { "Artist", new AttributeValue("The artist") },
                { "Album", new AttributeValue("The album") },
                { "SongTitle", new AttributeValue(song) }
            });

        return song;
    }

    public async Task<List<string>> GetAllSongs()
    {
        var scanResponse = await _dbClient.ScanAsync("Music", new List<string>());
        return scanResponse.Items.Select(item => item["SongTitle"].S).ToList();
    }
}