using SQLite;
using WhatStatut.Models;
using System.Security.Cryptography;
using System.Text;

namespace WhatStatut.Data;

public class MessagesRepository
{
    private readonly SQLiteAsyncConnection _database;

    public MessagesRepository(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        _database.CreateTableAsync<MessageEntry>().Wait();
    }

    public async Task<bool> InsertIfNewAsync(MessageEntry message)
    {
        // Generate hash for deduplication
        message.Hash = GenerateHash(message.Package, message.PostedAt, message.ChatTitle, message.Text);

        try
        {
            // Check if message with this hash already exists
            var existing = await _database.Table<MessageEntry>()
                .Where(m => m.Hash == message.Hash)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                return false; // Already exists
            }

            await _database.InsertAsync(message);
            return true;
        }
        catch (SQLiteException)
        {
            // Unique constraint violation - message already exists
            return false;
        }
    }

    public async Task<List<MessageEntry>> QueryLatestAsync(int limit = 100)
    {
        return await _database.Table<MessageEntry>()
            .OrderByDescending(m => m.PostedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> ClearAllAsync()
    {
        return await _database.DeleteAllAsync<MessageEntry>();
    }

    private string GenerateHash(string package, long postedAt, string chatTitle, string text)
    {
        var input = $"{package}|{postedAt}|{chatTitle}|{text}";
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
