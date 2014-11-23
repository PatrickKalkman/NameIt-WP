using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NameIt.Nokia;

namespace NameIt.Common
{
    public class LeaderboardManager
    {
        private const string ScoreKey = "Scores";
        private readonly CachingService cachingService;

        private List<GenreScoreHistory> scoresPerGenre;

        public LeaderboardManager(CachingService cachingService)
        {
            this.cachingService = cachingService;
        }

        public async void StoreScore(string genreId, int score)
        {
            if (scoresPerGenre == null)
            {
                scoresPerGenre = await GetScores();
            }

            scoresPerGenre.Add(new GenreScoreHistory()
            {
                DateTime = DateTime.Now,
                GenreId = genreId,
                Score = score
            });

            this.cachingService.StoreCachedData(ScoreKey, scoresPerGenre);
        }

        public async Task<List<GenreScoreHistory>> GetScores()
        {
            scoresPerGenre = await this.cachingService.LoadCachedData<List<GenreScoreHistory>>(ScoreKey);
            if (scoresPerGenre == null)
            {
                scoresPerGenre = new List<GenreScoreHistory>();
            }
            return scoresPerGenre;
        }

        public async Task<GenreScoreHistory> GetHighScore()
        {
            GenreScoreHistory highScore = null;
            List<GenreScoreHistory> scores = await GetScores();
            if (scores.Count > 0)
            {
                int maxScore = 0;
                foreach (GenreScoreHistory scoreHistory in scores)
                {
                    if (scoreHistory.Score > maxScore)
                    {
                        maxScore = scoreHistory.Score;
                        highScore = scoreHistory;
                    }
                }
            }
            return highScore;
        }

        public void Clear()
        {
            this.cachingService.Clear(ScoreKey);
            scoresPerGenre = new List<GenreScoreHistory>();
        }
    }
}
