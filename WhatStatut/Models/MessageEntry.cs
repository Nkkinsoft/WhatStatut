using SQLite;

namespace WhatStatut.Models;

[Table("messages")]
public class MessageEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(50)]
    public string Package { get; set; } = string.Empty;

    [MaxLength(255)]
    public string ChatTitle { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Sender { get; set; }

    public string Text { get; set; } = string.Empty;

    public long PostedAt { get; set; }

    public bool IsGroup { get; set; }

    [MaxLength(64), Indexed(Unique = true)]
    public string Hash { get; set; } = string.Empty;
}
