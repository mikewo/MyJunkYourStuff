using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyJunkYourStuff.Models
{
    public interface ILocationRepository
    {
        Task<Location> FindAsync(Guid id);
        Task AddAsync(Location location);
        Task DeleteAsync(Guid id);
        Task EditAsync(Location location);

        //NOTE: In production code there likely wouldn't be a Get ALL, or would be implemented with paging and/or better filters, or would
        //be scoped to a specific method as GetUpcomingLocationsAsync below is.
        Task<IEnumerable<Location>> GetAllLocationsAsync();

        Task<IEnumerable<Location>> GetUpcomingLocationsAsync(TimeSpan timeSpan, int maxResult = 5);
    }
}
