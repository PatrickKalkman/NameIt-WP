using System;
using System.IO;
using System.Threading.Tasks;

using Windows.Storage;

using Newtonsoft.Json;

namespace NameIt.Nokia
{
    public class CachingService
    {
        private const string CacheFileNameFormat = "Cache_{0}_Data.json";

        public async Task<bool> IsAvailable(string key)
        {
            return await ContainsFileAsync(GenerateCacheFileName(key));
        }

        private async Task<bool> ContainsFileAsync(string filename)
        {
            try
            {
                await ApplicationData.Current.LocalFolder.GetFileAsync(filename);
                //no exception means file exists
                return true;
            }
            catch (FileNotFoundException ex)
            {
                //find out through exception 
                return false;
            }
        }

        public async Task<T> LoadCachedData<T>(string key) where T : class
        {
            bool isAvailable = await IsAvailable(key);
            if (isAvailable)
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile cacheStorageFile = await localFolder.GetFileAsync(GenerateCacheFileName(key));
                using (Stream cacheStream = await cacheStorageFile.OpenStreamForReadAsync())
                {
                    using (var reader = new StreamReader(cacheStream))
                    {
                        string cacheDataString = await reader.ReadToEndAsync();
                        T cacheData = await JsonConvert.DeserializeObjectAsync<T>(cacheDataString);
                        return cacheData;
                    }
                }
            }
            return null;
        }

        public async void StoreCachedData<T>(string key, T dataToCache) where T : class
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile cacheStorageFile = await localFolder.CreateFileAsync(GenerateCacheFileName(key), CreationCollisionOption.ReplaceExisting);
            using (Stream cacheStream = await cacheStorageFile.OpenStreamForWriteAsync())
            {
                using (var writer = new StreamWriter(cacheStream))
                {
                    string cacheDataString = JsonConvert.SerializeObject(dataToCache);
                    await writer.WriteAsync(cacheDataString);
                }
            }
        }

        private static string GenerateCacheFileName(string key)
        {
            return string.Format(CacheFileNameFormat, key);
        }

        public async void Clear(string cacheKey)
        {
            if (await ContainsFileAsync(GenerateCacheFileName(cacheKey)))
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile cacheStorageFile = await localFolder.GetFileAsync(GenerateCacheFileName(cacheKey));
                await cacheStorageFile.DeleteAsync();
            }
        }
    }
}
