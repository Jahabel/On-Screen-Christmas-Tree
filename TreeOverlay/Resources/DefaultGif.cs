using System;
using System.IO;

namespace TreeOverlay.Resources;

public static class DefaultGif
{
    private const string Base64Gif = "R0lGODlhAQABAIAAAAUEBAAAACwAAAAAAQABAAACAkQBADs=";

    public static void WriteTo(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var bytes = Convert.FromBase64String(Base64Gif);
        File.WriteAllBytes(path, bytes);
    }
}
