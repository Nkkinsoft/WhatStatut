using Android.Content;
using Android.OS;
using AndroidX.Core.Content;
using Java.IO;

namespace WhatStatut.Android.Utils;

public static class FileProviderHelper
{
    private const string Authority = "com.nkkinsoft.whatstatut.fileprovider";

    /// <summary>
    /// Get a content URI for a file using FileProvider
    /// </summary>
    public static global::Android.Net.Uri? GetUriForFile(Context context, string filePath)
    {
        try
        {
            var file = new Java.IO.File(filePath);
            return FileProvider.GetUriForFile(context, Authority, file);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting URI for file {filePath}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Copy a file to the app's external files directory
    /// </summary>
    public static string? CopyToExternalFiles(Context context, string sourcePath, string subDirectory)
    {
        try
        {
            var externalDir = context.GetExternalFilesDir(subDirectory);
            if (externalDir == null)
                return null;

            if (!externalDir.Exists())
                externalDir.Mkdirs();

            var fileName = Path.GetFileName(sourcePath);
            var destFile = new Java.IO.File(externalDir, fileName);
            
            using var inputStream = new FileInputStream(sourcePath);
            using var outputStream = new FileOutputStream(destFile);
            
            var buffer = new byte[8192];
            int bytesRead;
            while ((bytesRead = inputStream.Read(buffer)) > 0)
            {
                outputStream.Write(buffer, 0, bytesRead);
            }
            
            return destFile.AbsolutePath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error copying file {sourcePath}: {ex.Message}");
            return null;
        }
    }
}
