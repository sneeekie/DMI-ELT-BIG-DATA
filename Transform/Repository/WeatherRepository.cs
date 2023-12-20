using Transform.Entities;
using Transform.IRepository;

namespace Transform.Repository
{
    public class WeatherRepository : BaseRepository<WeatherDataModel>, IWeatherRepository
    {
        public WeatherRepository(EfDbContext efDbContext)
            : base(efDbContext)
        { }
    }
}