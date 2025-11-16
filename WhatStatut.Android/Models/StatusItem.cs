namespace WhatStatut.Android.Models;

public enum StatusType
{
    Image,
    Video
}

public class StatusItem
{
    public string Path { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public StatusType Type { get; set; }
    public DateTime LastModifiedUtc { get; set; }
}
