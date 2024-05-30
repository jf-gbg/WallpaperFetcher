using System.Runtime.InteropServices;
using HtmlAgilityPack;
using Microsoft.Win32;

namespace WallpaperFetcher;

class Program
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    private const int SPI_SETDESKWALLPAPER = 20;
    private const int SPIF_UPDATEINFILE = 0x01;
    private const int SPIF_SENDCHANGE = 0x02;

    static async Task Main()
    {
        var imageUrl = await GetApodImageUrlAsync();
        var localPath = Path.Combine(Path.GetTempPath(), "wallpaper.jpg");

        try
        {
            await DownloadImageAsync(imageUrl, localPath);
            SetWallpaper(localPath);
            SetWallpaperStyle("0", "0");
            Console.WriteLine("Wallpaper updated successfully");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static async Task DownloadImageAsync(string url, string localPath)
    {
        using HttpClient client = new HttpClient();
        var imageBytes = await client.GetByteArrayAsync(url);
        await File.WriteAllBytesAsync(localPath, imageBytes);
    }

    private static async Task<string> GetApodImageUrlAsync()
    {
        var pageUrl = "https://apod.nasa.gov/apod/astropix.html";
        var client = new HttpClient();
        var html = await client.GetStringAsync(pageUrl);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var imageNode = doc.DocumentNode.SelectSingleNode("//img");
        var imageUrl = "https://apod.nasa.gov/apod/" + imageNode.GetAttributeValue("src", "");

        return imageUrl;
    }
    
    private static void SetWallpaper(string filePath)
    {
        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, filePath, SPIF_UPDATEINFILE | SPIF_SENDCHANGE);
    }

    private static void SetWallpaperStyle(string style, string tile)
    {
        const string key = @"control Panel\Desktop";
        using var regKey = Registry.CurrentUser.OpenSubKey(key, true);
        regKey.SetValue(@"WallpaperStyle", style);
        regKey.SetValue(@"TileWallpaper", tile);
    }
}