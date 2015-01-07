using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace DocumentDbBootstrapper
{
    class Program
    {
        private static DocumentClient _client;
        private static readonly string DatabaseId = ConfigurationManager.AppSettings["databaseId"];
        private static readonly string Collection = ConfigurationManager.AppSettings["collectionId"];
        private static readonly string Endpoint = ConfigurationManager.AppSettings["endpointUrl"];
        private static readonly string AuthorizationKey = ConfigurationManager.AppSettings["authKey"];

        static void Main(string[] args)
        {
            try
            {
                using (_client = new DocumentClient(new Uri(Endpoint), AuthorizationKey))
                {
                    InitializeDatabase(DatabaseId, Collection).Wait();
                }
            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message, baseException.Message);
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
        }

        private static async Task InitializeDatabase(string databaseId, string collectionId)
        {
            //Get, or Create, the Database
            Database database = await GetOrCreateDatabaseAsync(databaseId);

            //Get, or Create, the Document Collection
            DocumentCollection collection = await GetOrCreateCollectionAsync(database.SelfLink, collectionId);

            //Create documents needed for query samples
            await CreateDocuments(collection.SelfLink);
        }

        private static async Task<DocumentCollection> GetOrCreateCollectionAsync(string selfLink, string collectionId)
        {
            Console.WriteLine("Creating collection . . . .");

            DocumentCollection collection = _client.CreateDocumentCollectionQuery(selfLink)
                .Where(c => c.Id == collectionId)
                .AsEnumerable()
                .FirstOrDefault();

            if (collection == null)
            {
                if (collection == null)
                {
                    // Define a custom index policy to enable efficient queries against the StartTime (DateEpoch custom type).
                    // See http://blogs.msdn.com/b/documentdb/archive/2014/11/18/working-with-dates-in-azure-documentdb.aspx


                    collection = new DocumentCollection { Id = collectionId };

                    // Set the default IncludePath to set all the properites in the document to have a Hash index.
                    collection.IndexingPolicy.IncludedPaths.Add(new IndexingPath
                    {
                        IndexType = IndexType.Hash,
                        Path = "/"
                    });

                    // Define an additional path for StartTime that will handle range based queries
                    collection.IndexingPolicy.IncludedPaths.Add(new IndexingPath
                    {
                        IndexType = IndexType.Range,
                        Path = "/\"startTime\"/\"epoch\"/?",
                        NumericPrecision = 7
                    });

                    collection = await _client.CreateDocumentCollectionAsync(selfLink, collection);
                }
            }

            return collection;
        }

        private static async Task<Database> GetOrCreateDatabaseAsync(string databaseId)
        {
            Console.WriteLine("Creating database . . . ");

            Database db = _client.CreateDatabaseQuery()
                .Where(d => d.Id == databaseId)
                .AsEnumerable()
                .FirstOrDefault();

            if (db == null)
            {
                db = await _client.CreateDatabaseAsync(new Database { Id = databaseId });
            }

            return db;
        }

        private static async Task CreateDocuments(string colSelfLink)
        {
            Console.WriteLine("Creating test documents . . . ");

            IEnumerable<Location> locations = new[]
            {
                new Location { Id=Guid.NewGuid(), Title="Furniture", Description="We have furniture of all types and shapes.", Address="123 AnyStreet, Bardstown, KY", JunkerName="Bob", StartTime = new DateEpoch {Date = new DateTime(2015, 1, 9, 8, 0, 0)}},
                new Location { Id=Guid.NewGuid(), Title="Board Games!", Description="Selling an array of different board games.", Address="2354 Jefferson Street, Chicago, IL", JunkerName="Dave", StartTime = new DateEpoch {Date = new DateTime(2015, 1, 10, 10, 0, 0)}},
                new Location { Id=Guid.NewGuid(), Title="Legos", Description="Yes, we know it's inconcievable to want to part with Legos, but they must go!", Address="95434 Ryan Circle, Sandusky, OH", JunkerName="William", StartTime =new DateEpoch{ Date = new DateTime(2015, 1, 14, 9, 0, 0)}},
                new Location { Id=Guid.NewGuid(), Title="Garage Sale", Description="The entire garage and everything in it. Bring a BIG truck.", Address="293 Striker Park, LaCenter, KY", JunkerName="Steven", StartTime = new DateEpoch {Date = new DateTime(2015, 2, 1, 9, 0, 0)}},
                new Location { Id=Guid.NewGuid(), Title="Office Closeout", Description="Selling computers, monitors, printers, office furniture and even the cube walls (slightly burnt).", Address="232 Initech Way, Los Angeles, CA", JunkerName="Bob", StartTime = new DateEpoch {Date = new DateTime(2015, 1, 27, 11, 0, 0)}},
                new Location { Id=Guid.NewGuid(), Title="Movie Buff's Dream", Description="Selling movie props and sets, including things from Star Wars, the LOTR movies and the Princess Bride.", Address="One Studios Way, Hollywood, CA", JunkerName="Bob", StartTime = new DateEpoch {Date = new DateTime(2015, 2, 2, 9, 0, 0)}},
                new Location { Id=Guid.NewGuid(), Title="Transparent Aluminum Aquarium", Description="Great for whales, needs slight repair.", Address="Pier 121, San Franciso, CA", JunkerName="Scotty", StartTime = new DateEpoch {Date = new DateTime(2015, 1, 26, 8, 0, 0)}},
                new Location { Id=Guid.NewGuid(), Title="Black Mask", Description="A fashionable black mask. Cover the eyes and the bridge of the nose. Terribly comfortable, but slightly used.", Address="244 Storm Castle Way, Columbus, OH", JunkerName="Wesley", StartTime = new DateEpoch {Date = new DateTime(2015, 1, 29, 8, 0, 0)}},
            };

            foreach (var l in locations)
            {
                var doc = await _client.CreateDocumentAsync(colSelfLink, l);
            }
        }
    }
}
