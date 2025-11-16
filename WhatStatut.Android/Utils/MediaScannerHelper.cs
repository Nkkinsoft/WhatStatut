using Android.Content;
using Android.Media;

namespace WhatStatut.Android.Utils;

public static class MediaScannerHelper
{
    /// <summary>
    /// Scan a file so it appears in the media gallery
    /// </summary>
    public static void ScanFile(Context context, string filePath)
    {
        try
        {
            MediaScannerConnection.ScanFile(
                context,
                new[] { filePath },
                null,
                null
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error scanning file {filePath}: {ex.Message}");
        }
    }
}
