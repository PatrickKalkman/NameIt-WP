using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Caliburn.Micro;

using Nokia.Music;
using Nokia.Music.Types;

using NetworkInterface = System.Net.NetworkInformation.NetworkInterface;

namespace NameIt.Nokia
{
    public class GenreRetriever
    {
        private MusicClient client;
        private readonly CachingService cachingService;
        private const string CacheKey = "NameItGenres";

        public GenreRetriever(CachingService cachingService)
        {
            this.cachingService = cachingService;
        }

        public async Task<List<Genre>> GetAll()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                bool isCacheAvailable = await cachingService.IsAvailable(CacheKey);
                if (!isCacheAvailable)
                {
                    client = IoC.Get<MusicClient>();
                    if (client != null)
                    {
                        ListResponse<Genre> genres = await client.GetGenresAsync();
                        cachingService.StoreCachedData(CacheKey, genres);
                    }
                    else
                    {
                        return new List<Genre>();
                    }
                }
                return await cachingService.LoadCachedData<List<Genre>>(CacheKey);
            }
            return new List<Genre>();
        }

        public async Task<Uri> GetTopSongFromGenre(string genreId)
        {
            if (!string.IsNullOrWhiteSpace(genreId))
            {
                ListResponse<Product> releases = await client.GetNewReleasesForGenreAsync(genreId, Category.Track, 0, 1);
                return client.GetTrackSampleUri(releases[0].Id);
            }
            return null;
        }

        public async Task<List<string>> GetGenreFirstLetters()
        {
            List<Genre> genres = await GetAll();
            List<string> firstLetters = genres.Select(genre => genre.Name.Substring(0, 1)).ToList();
            return firstLetters;
        }

        public void ClearCache()
        {
            cachingService.Clear(CacheKey);
        }
    }
}
