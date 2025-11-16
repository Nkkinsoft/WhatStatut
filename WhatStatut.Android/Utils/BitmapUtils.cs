using Android.Graphics;

namespace WhatStatut.Android.Utils;

public static class BitmapUtils
{
    /// <summary>
    /// Safely decode a bitmap with downsampling to avoid OutOfMemoryError
    /// </summary>
    public static Bitmap? DecodeSampledBitmap(string path, int reqWidth, int reqHeight)
    {
        try
        {
            // First decode with inJustDecodeBounds=true to check dimensions
            var options = new BitmapFactory.Options
            {
                InJustDecodeBounds = true
            };
            BitmapFactory.DecodeFile(path, options);

            // Calculate inSampleSize
            options.InSampleSize = CalculateInSampleSize(options, reqWidth, reqHeight);

            // Decode bitmap with inSampleSize set
            options.InJustDecodeBounds = false;
            return BitmapFactory.DecodeFile(path, options);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error decoding bitmap {path}: {ex.Message}");
            return null;
        }
    }

    private static int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
    {
        // Raw height and width of image
        int height = options.OutHeight;
        int width = options.OutWidth;
        int inSampleSize = 1;

        if (height > reqHeight || width > reqWidth)
        {
            int halfHeight = height / 2;
            int halfWidth = width / 2;

            // Calculate the largest inSampleSize value that is a power of 2 and keeps both
            // height and width larger than the requested height and width.
            while ((halfHeight / inSampleSize) >= reqHeight && (halfWidth / inSampleSize) >= reqWidth)
            {
                inSampleSize *= 2;
            }
        }

        return inSampleSize;
    }

    /// <summary>
    /// Generate a thumbnail from a video file
    /// </summary>
    public static Bitmap? GetVideoThumbnail(string videoPath)
    {
        try
        {
            var retriever = new global::Android.Media.MediaMetadataRetriever();
            retriever.SetDataSource(videoPath);
            return retriever.GetFrameAtTime(0);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting video thumbnail {videoPath}: {ex.Message}");
            return null;
        }
    }
}
