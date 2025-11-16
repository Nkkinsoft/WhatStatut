using WhatStatut.Android.Models;

namespace WhatStatut.Android.Repositories;

public class StatusRepository
{
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly string[] VideoExtensions = { ".mp4" };

    private static readonly string[] StatusPaths = 
    {
        "/storage/emulated/0/Android/media/com.whatsapp/WhatsApp/Media/.Statuses",
        "/storage/emulated/0/WhatsApp/Media/.Statuses", // legacy
        "/storage/emulated/0/Android/media/com.whatsapp.w4b/WhatsApp Business/Media/.Statuses"
    };

    public List<StatusItem> GetStatusItems()
    {
        var items = new List<StatusItem>();

        foreach (var path in StatusPaths)
        {
            if (!Directory.Exists(path))
                continue;

            try
            {
                var files = Directory.GetFiles(path);
                
                foreach (var file in files)
                {
                    var extension = Path.GetExtension(file).ToLowerInvariant();
                    StatusType? type = null;

                    if (ImageExtensions.Contains(extension))
                        type = StatusType.Image;
                    else if (VideoExtensions.Contains(extension))
                        type = StatusType.Video;

                    if (type.HasValue)
                    {
                        var fileInfo = new FileInfo(file);
                        items.Add(new StatusItem
                        {
                            Path = file,
                            FileName = Path.GetFileName(file),
                            Type = type.Value,
                            LastModifiedUtc = fileInfo.LastWriteTimeUtc
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with other paths
                System.Diagnostics.Debug.WriteLine($"Error reading {path}: {ex.Message}");
            }
        }

        // Sort by most recent first
        return items.OrderByDescending(x => x.LastModifiedUtc).ToList();
    }
}
