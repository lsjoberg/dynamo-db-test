using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using DynamoDbTest;

var credentials = new BasicAWSCredentials("dummy", "dummy");

var clientConfig = new AmazonDynamoDBConfig
{
    RegionEndpoint = RegionEndpoint.EUNorth1,
    ServiceURL = "http://localhost:4566"
};
var dbClient = new AmazonDynamoDBClient(credentials, clientConfig);

var migrator = new Migrator(dbClient);
await migrator.EnsureTableExists(true);

var musicRepository = new MusicRepository(dbClient);

var streamsConfig = new AmazonDynamoDBStreamsConfig
{
    RegionEndpoint = RegionEndpoint.EUNorth1,
    ServiceURL = "http://localhost:4566"
};

var streamsClient = new AmazonDynamoDBStreamsClient(credentials, streamsConfig);
var songProcessor = new SongProcessor(dbClient, streamsClient);
songProcessor.Start();

while (true)
{
    Console.WriteLine("a to add song, l to list all songs");
    var input = Console.ReadKey();
    Console.WriteLine();
    switch (input.KeyChar)
    {
        case 'a': 
            var addedSong = await musicRepository.AddSong();
            Console.WriteLine($"{addedSong} added");
            break;
        case 'l':
            (await musicRepository.GetAllSongs()).ForEach(Console.WriteLine);
            break;
    }
}