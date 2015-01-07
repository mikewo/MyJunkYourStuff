using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyJunkYourStuff.Models
{
    public class LocationInMemoryRepository : ILocationRepository
    {
        private List<Location> _locations;

        /// <summary>
        /// Initializes a new instance of the LocationInMemoryRepository class.
        /// </summary>
        public LocationInMemoryRepository()
        {
            _locations = new List<Location>()
            {
                new Location() { Id=Guid.NewGuid(), Title="Furniture", Description="We have furniture of all types and shapes.", Address="123 AnyStreet, Bardstown, KY", JunkerName="Bob", StartTime = new DateTime(2015, 1, 9, 8, 0, 0)},
                new Location() { Id=Guid.NewGuid(), Title="Board Games!", Description="Selling an array of different board games.", Address="2354 Jefferson Street, Chicago, IL", JunkerName="Dave", StartTime = new DateTime(2015, 1, 10, 10, 0, 0)},
                new Location() { Id=Guid.NewGuid(), Title="Legos", Description="Yes, we know it's inconcievable to want to part with Legos, but they must go!", Address="95434 Ryan Circle, Sandusky, OH", JunkerName="William", StartTime = new DateTime(2015, 1, 14, 9, 0, 0)},
                new Location() { Id=Guid.NewGuid(), Title="Garage Sale", Description="The entire garage and everything in it. Bring a BIG truck.", Address="293 Striker Park, LaCenter, KY", JunkerName="Steven", StartTime = new DateTime(2015, 2, 1, 9, 0, 0)},
                new Location() { Id=Guid.NewGuid(), Title="Office Closeout", Description="Selling computers, monitors, printers, office furniture and even the cube walls (slightly burnt).", Address="232 Initech Way, Los Angeles, CA", JunkerName="Bob", StartTime = new DateTime(2015, 1, 27, 11, 0, 0)},
                new Location() { Id=Guid.NewGuid(), Title="Movie Buff's Dream", Description="Selling movie props and sets, including things from Star Wars, the LOTR movies and the Princess Bride.", Address="One Studios Way, Hollywood, CA", JunkerName="Bob", StartTime = new DateTime(2015, 2, 2, 9, 0, 0)},
                new Location() { Id=Guid.NewGuid(), Title="Transparent Aluminum Aquarium", Description="Great for whales, needs slight repair.", Address="Pier 121, San Franciso, CA", JunkerName="Scotty", StartTime = new DateTime(2015, 1, 26, 8, 0, 0)},
                new Location() { Id=Guid.NewGuid(), Title="Black Mask", Description="A fashionable black mask. Cover the eyes and the bridge of the nose. Terribly comfortable, but slightly used.", Address="244 Storm Castle Way, Columbus, OH", JunkerName="Wesley", StartTime = new DateTime(2015, 1, 29, 8, 0, 0)},
            };
        }

        public Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            return Task.FromResult(_locations.AsEnumerable());
        }

        public Task<IEnumerable<Location>> GetUpcomingLocationsAsync(TimeSpan timeSpan, int maxResult = 5)
        {
            var locations = _locations.Where(l => l.StartTime > DateTime.Now && l.StartTime <= DateTime.Now.Add(timeSpan))
                .OrderBy(x => x.StartTime)
                .Take(maxResult);

            return Task.FromResult(locations);
        }

        public Task<Location> FindAsync(Guid id)
        {
            var location = _locations.Where(x => x.Id == id).FirstOrDefault();
            return Task.FromResult(location);
        }


        public async Task AddAsync(Location location)
        {
            var duplicationCheck = await FindAsync(location.Id);
            if (duplicationCheck != null)
            {
                throw new ApplicationException("Duplicate Location Id was found.");
            }

            _locations.Add(location);
        }

        public async Task DeleteAsync(Guid id)
        {
            Location locationToRemove = await FindAsync(id);
            _locations.Remove(locationToRemove);

        }

        public async Task EditAsync(Location location)
        {
            Location locationToEdit = await FindAsync(location.Id);
            if (locationToEdit != null)
            {
                _locations[_locations.IndexOf(locationToEdit)] = location;
            }

        }

    }

    
}