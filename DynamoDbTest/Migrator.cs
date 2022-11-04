using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace DynamoDbTest;

public class Migrator
{
    private readonly AmazonDynamoDBClient _dbClient;

    public Migrator(AmazonDynamoDBClient dbClient)
        => _dbClient = dbClient;
    
    private async Task CreateTable()
    {
        var createTableRequest = new CreateTableRequest("Music",
            new List<KeySchemaElement>
            {
                new KeySchemaElement("Artist", KeyType.HASH),
                new KeySchemaElement("SongTitle", KeyType.RANGE),
            },
            new List<AttributeDefinition>
            {
                new AttributeDefinition("Artist", ScalarAttributeType.S),
                new AttributeDefinition("SongTitle", ScalarAttributeType.S)

            },
            new ProvisionedThroughput(10, 5))
        {
            StreamSpecification = new StreamSpecification
            {
                StreamEnabled = true,
                StreamViewType = StreamViewType.NEW_AND_OLD_IMAGES
            }
        };
        
        await _dbClient.CreateTableAsync(createTableRequest);
    }

    private async Task DeleteTable()
    {
        try
        {
            await _dbClient.DeleteTableAsync("Music");
            Console.WriteLine("Table deleted");
        }
        catch(ResourceNotFoundException)
        {
            Console.WriteLine("Table does not exist");
        }
    }

    private async Task WaitForTableToExist()
    {
        while (true)
        {
            try
            {
                var describeTableResponse = await _dbClient.DescribeTableAsync("Music");
                if (describeTableResponse.Table.TableStatus == TableStatus.ACTIVE)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine("Table not ready yet");
                await Task.Delay(500);
            }
        }
    }

    public async Task EnsureTableExists(bool forceRecreation = false)
    {
        if (forceRecreation)
            await DeleteTable();
        
        try
        {
            await CreateTable();
            await WaitForTableToExist();
            Console.WriteLine("Table created");
        }
        catch(ResourceInUseException)
        {
            Console.WriteLine("Table already exists");
        }
    }
}