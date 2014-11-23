using System;
using System.Linq;

using Microsoft.Phone.Shell;

namespace NameIt.Common
{
    public class FlipTileCreator
    {
        public void CreateTile(string content, string wideContent)
        {
            var navigationUri = CreateNavigationUri();

            FlipTileData tileData = CreateTileData(content, wideContent);

            ShellTile tile = ShellTile.ActiveTiles.FirstOrDefault(t => t.NavigationUri == navigationUri);

            if (tile == null)
            {
                ShellTile.Create(navigationUri, tileData, true);
            }
            else
            {
                tile.Update(tileData);
            }
        }

        public void UpdateDefaultTile(string content, string wideContent)
        {
            FlipTileData tileData = CreateTileData(content, wideContent);
            ShellTile tile = ShellTile.ActiveTiles.FirstOrDefault();
            if (tile != null)
            {
                tile.Update(tileData);
            }
        }

        public void UpdateTile(string content, string wideContent)
        {
            var navigationUri = CreateNavigationUri();

            FlipTileData tileData = CreateTileData(content, wideContent);

            ShellTile tile = ShellTile.ActiveTiles.FirstOrDefault(t => t.NavigationUri == navigationUri);
            if (tile != null)
            {
                tile.Update(tileData);
            }
        }

        private static Uri CreateNavigationUri()
        {
            var navigationUri = new Uri("/Views/MainPage.xaml", UriKind.Relative);
            return navigationUri;
        }

        private static FlipTileData CreateTileData(string content, string wideContent)
        {
            var tileData = new FlipTileData()
            {
                Title = "Name It",
                BackTitle = "High scores",
                BackContent = content,
                WideBackContent = wideContent,
                WideBackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileLarge.png", UriKind.Relative),
                WideBackBackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileLargeBack.png", UriKind.Relative),
                BackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileMedium.png", UriKind.Relative),
                BackBackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileMediumBack.png", UriKind.Relative),
                SmallBackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileSmall.png", UriKind.Relative)
            };
            return tileData;
        }
    }
}
