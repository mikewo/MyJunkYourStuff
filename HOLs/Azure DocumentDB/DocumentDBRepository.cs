using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace MyJunkYourStuff.Models
{
    public class DocumentDBRepository : ILocationRepository
    {
        private static DocumentClient _client;

        private static readonly string _databaseId;
        private static readonly string _collectionId;
        private static readonly string _endpoint;
        private static readonly string _authorizationKey;

        private static Database _database;
        private static DocumentCollection _collection;


        private static Database Database
        {
            get { return _database ?? (_database = GetDatabase()); }
        }

        private static DocumentCollection Collection
        {
            get { return _collection ?? (_collection = GetCollection(Database.SelfLink)); }
        }

        private static DocumentClient Client
        {
            get
            {
                if (_client == null)
                {
                    Uri endpointUrl = new Uri(_endpoint);
                    _client = new DocumentClient(endpointUrl, _authorizationKey);
                }

                return _client;
            }
        }

        static DocumentDBRepository()
        {
            _endpoint = ConfigurationManager.AppSettings["endpointUrl"];
            if (string.IsNullOrEmpty(_endpoint))
            {
                throw new ConfigurationErrorsException("You must supply a value for the database endpoint URL in the web.config under the appsettings key 'endpointUrl'.");
            }

            _authorizationKey = ConfigurationManager.AppSettings["authKey"];
            if (string.IsNullOrEmpty(_authorizationKey))
            {
                throw new ConfigurationErrorsException("You must supply a value for the database authentication key in the web.config under the appsettings key 'authKey'.");
            }

            _databaseId = ConfigurationManager.AppSettings["databaseId"];
            if (string.IsNullOrEmpty(_databaseId))
            {
                throw new ConfigurationErrorsException("You must supply a name for the database in the web.config under the appsettings key 'databaseId'.");
            }

            _collectionId = ConfigurationManager.AppSettings["collectionId"];
            if (string.IsNullOrEmpty(_collectionId))
            {
                throw new ConfigurationErrorsException("You must supply a name for the database collection in the web.config under the appsettings key 'collectionId'.");
            }
        }

        private static DocumentCollection GetCollection(string selfLink)
        {
            DocumentCollection docCollection = Client.CreateDocumentCollectionQuery(selfLink)
                .Where(c => c.Id == _collectionId)
                .AsEnumerable()
                .FirstOrDefault();

            return docCollection;
        }

        private static Database GetDatabase()
        {
            Database db = Client.CreateDatabaseQuery()
                    .Where(d => d.Id == _databaseId)
                    .AsEnumerable()
                    .FirstOrDefault();

            return db;
        }

        public Task<Location> FindAsync(Guid id)
        {
            // NOTE: Same result . . . different syntax.

            //var loc = Client.CreateDocumentQuery<Location>(Collection.SelfLink)
            //    .Where(d => d.Id == id)
            //    .AsEnumerable()
            //    .FirstOrDefault();

            string query = string.Format("SELECT * FROM Junk j WHERE j.id = '{0}'", id);

            var loc = Client.CreateDocumentQuery<Location>(Collection.DocumentsLink, query)
                .AsEnumerable()
                .FirstOrDefault();

            return Task.FromResult(loc);
        }

        public async Task AddAsync(Location location)
        {
            ResourceResponse<Document> doc = await Client.CreateDocumentAsync(Collection.DocumentsLink, location);

            // Measure the performance (request units) of the write operation.
            Trace.TraceInformation("Insert of documnet consumed [{0}] request units.", doc.RequestCharge);
        }

        public async Task DeleteAsync(Guid id)
        {
            Document doc = GetDocument(id);
            ResourceResponse<Document> response = await Client.DeleteDocumentAsync(doc.SelfLink);
        }

        private Document GetDocument(Guid? id)
        {
            var doc = Client.CreateDocumentQuery(Collection.DocumentsLink)
                .Where(d => d.Id == id.ToString())
                .AsEnumerable()
                .FirstOrDefault();

            return doc;
        }

        public async Task EditAsync(Location location)
        {
            Document doc = GetDocument(location.Id);

            if (doc != null)
            {
                ResourceResponse<Document> response = await Client.ReplaceDocumentAsync(doc.SelfLink, location);
            }
        }

        public async Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            // NOTE: Statement below is used to show one way to execute a query and 
            //       also get details about DocumentDB resource usage.
            IDocumentQuery<Document> q = Client.CreateDocumentQuery(Collection.DocumentsLink).AsDocumentQuery();
            while (q.HasMoreResults)
            {
                FeedResponse<dynamic> response = await q.ExecuteNextAsync<dynamic>();
                Trace.TraceInformation("Query consumed [{0}] request units.", response.RequestCharge);
            }


            // NOTE: Currently executing query is not asynchronous!
            IEnumerable<Location> locations = (from l in Client.CreateDocumentQuery<Location>(Collection.DocumentsLink)
                select l).AsEnumerable();

            // NOTE: Following statement included only to show a different approach.
            IQueryable<Location> locationsQuery = Client.CreateDocumentQuery<Location>(Collection.DocumentsLink, "SELECT * FROM Junk");
            IEnumerable<Location> locationsList = locationsQuery.AsEnumerable();

            return locations;
        }

        public async Task<IEnumerable<Location>> GetUpcomingLocationsAsync(TimeSpan timeSpan, int maxResult = 5)
        {
            var queryOptions = new FeedOptions {MaxItemCount = maxResult};

            var locationQuery = Client.CreateDocumentQuery<Location>(Collection.SelfLink, queryOptions)
                .Where(l => l.StartTime.Epoch > DateTime.Now.ToEpoch() && l.StartTime.Epoch <= DateTime.Now.Add(timeSpan).ToEpoch())
                .AsDocumentQuery();
            var response = await locationQuery.ExecuteNextAsync<Location>();

            // NOTE: DocumentDb does not currently support OrderBy in a LINQ query. Need to use a stored procedure. 
            //       See example at https://code.msdn.microsoft.com/Azure-DocumentDB-NET-Code-6b3da8af#content

            var locations = response.OrderBy(x => x.StartTime.Date);
            return locations;
        }
    }
}