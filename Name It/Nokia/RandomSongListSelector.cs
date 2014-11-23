using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Nokia.Music;
using System;

using Nokia.Music.Types;

namespace NameIt.Nokia
{
    public class RandomSongListSelector
    {
        private readonly Regex myRegex = new Regex(@"\(.*?\)", RegexOptions.None);

        private readonly MusicClient client;
        private readonly Random random = new Random((int)DateTime.Now.TimeOfDay.TotalMilliseconds); 

        public RandomSongListSelector(MusicClient client)
        {
            this.client = client;
        }

        public async Task<List<Song>> SelectRandomSongList(string genreId, int numberOfSongsToSelect)
        {
            var songs = new List<Song>();
            ListResponse<Artist> artists = await client.GetTopArtistsForGenreAsync(genreId, 0, 50);

            if (artists.Count != 0)
            {
                // Select one song per artist.
                for (int songIndex = 0; songIndex < numberOfSongsToSelect; songIndex++)
                {
                    Artist selectedArtist;
                    do
                    {
                        int selectedArtistIndex = random.Next(0, 49);
                        selectedArtist = artists[selectedArtistIndex];
                    }
                    while (IsArtistAlreadySelected(selectedArtist.Name, songs));

                    Song song = await SelectSongFromArtist(selectedArtist, genreId);
                    if (song != null)
                    {
                        songs.Add(song);
                    }
                }
            }
            return songs;
        }

        public bool IsArtistAlreadySelected(string artistName, List<Song> songs)
        {
            return songs.Any(s => s.ArtistName == artistName);
        }

        private async Task<Song> SelectSongFromArtist(Artist artist, string genreId)
        {
            ListResponse<MusicItem> songs = await client.SearchAsync(artist.Name, Category.Track, genreId, null, null);
            if (songs.TotalResults.HasValue && songs.TotalResults.Value > 0)
            {
                Song song = new Song();
                song.ArtistName = myRegex.Replace(artist.Name, string.Empty).Trim();
                song.SongName = myRegex.Replace(songs[0].Name, string.Empty).Trim();
                song.SongUri = client.GetTrackSampleUri(songs[0].Id);
                return song;
            }
            return null;
        }
    }
}
