using Android.App;
using Android.Content;
using Android.OS;
using Android.Service.Notification;
using WhatStatut.Data;
using WhatStatut.Models;

namespace WhatStatut.Services;

[Service(
    Label = "WhatsApp Message Capture",
    Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE",
    Exported = true)]
[IntentFilter(new[] { "android.service.notification.NotificationListenerService" })]
public class MessagesNotificationListener : NotificationListenerService
{
    private const string WhatsAppPackage = "com.whatsapp";
    private const string WhatsAppBusinessPackage = "com.whatsapp.w4b";
    private const string PrefsName = "WhatStatutPrefs";
    private const string PrefCaptureEnabled = "capture_enabled";

    private MessagesRepository? _repository;

    public override void OnCreate()
    {
        base.OnCreate();
        InitializeRepository();
    }

    private void InitializeRepository()
    {
        try
        {
            var dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "messages.db");
            _repository = new MessagesRepository(dbPath);
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("MessagesNotificationListener", $"Failed to initialize repository: {ex.Message}");
        }
    }

    public override void OnNotificationPosted(StatusBarNotification? sbn)
    {
        base.OnNotificationPosted(sbn);

        if (sbn == null) return;

        if (_repository == null)
        {
            InitializeRepository();
        }

        // Check if capture is enabled
        if (!IsCaptureEnabled())
        {
            return;
        }

        var packageName = sbn.PackageName;

        // Filter for WhatsApp packages only
        if (packageName != WhatsAppPackage && packageName != WhatsAppBusinessPackage)
        {
            return;
        }

        // Process notification in background thread
        Task.Run(async () =>
        {
            try
            {
                await ProcessNotificationAsync(sbn, packageName);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("MessagesNotificationListener", $"Error processing notification: {ex.Message}");
            }
        });
    }

    private async Task ProcessNotificationAsync(StatusBarNotification sbn, string packageName)
    {
        if (_repository == null) return;

        var notification = sbn.Notification;
        if (notification?.Extras == null) return;

        // Skip summary notifications
        var isGroupSummary = (notification.Flags & NotificationFlags.GroupSummary) != 0;
        if (isGroupSummary)
        {
            return;
        }

        var extras = notification.Extras;
        var title = extras.GetString(Notification.ExtraTitle) ?? string.Empty;
        var text = extras.GetString(Notification.ExtraText) ?? string.Empty;
        var subText = extras.GetString(Notification.ExtraSubText);
        var infoText = extras.GetString(Notification.ExtraInfoText);
        var postTime = sbn.PostTime;

        // Skip if no meaningful content
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        // Try to extract EXTRA_TEXT_LINES for multiple messages
        var messagesExtracted = false;
        var textLines = extras.GetCharSequenceArray(Notification.ExtraTextLines);
        
        if (textLines != null && textLines.Length > 0)
        {
            // Process each line as a potential message
            foreach (var line in textLines)
            {
                if (line == null) continue;
                
                var lineText = line.ToString();
                if (string.IsNullOrWhiteSpace(lineText)) continue;

                // Try to parse sender from text for group chats (format: "Sender: Message")
                string? sender = null;
                var messageText = lineText;
                var isGroup = false;

                var colonIndex = lineText.IndexOf(": ");
                if (colonIndex > 0 && colonIndex < 50) // Reasonable sender name length
                {
                    sender = lineText.Substring(0, colonIndex);
                    messageText = lineText.Substring(colonIndex + 2);
                    isGroup = true;
                }

                var entry = new MessageEntry
                {
                    Package = packageName,
                    ChatTitle = title,
                    Sender = sender,
                    Text = messageText,
                    PostedAt = postTime,
                    IsGroup = isGroup
                };

                await _repository.InsertIfNewAsync(entry);
                messagesExtracted = true;
            }
        }

        // Fallback: if textLines didn't work, use basic extraction
        if (!messagesExtracted && !string.IsNullOrWhiteSpace(text))
        {
            // Try to parse sender from text for group chats (format: "Sender: Message")
            string? sender = null;
            var messageText = text;
            var isGroup = false;

            var colonIndex = text.IndexOf(": ");
            if (colonIndex > 0 && colonIndex < 50) // Reasonable sender name length
            {
                sender = text.Substring(0, colonIndex);
                messageText = text.Substring(colonIndex + 2);
                isGroup = true;
            }

            var entry = new MessageEntry
            {
                Package = packageName,
                ChatTitle = title,
                Sender = sender,
                Text = messageText,
                PostedAt = postTime,
                IsGroup = isGroup
            };

            await _repository.InsertIfNewAsync(entry);
        }
    }

    private bool IsCaptureEnabled()
    {
        try
        {
            var prefs = Application.Context.GetSharedPreferences(PrefsName, FileCreationMode.Private);
            return prefs?.GetBoolean(PrefCaptureEnabled, false) ?? false;
        }
        catch
        {
            return false;
        }
    }

    public static void RequestNotificationAccess(Context context)
    {
        var intent = new Intent("android.settings.ACTION_NOTIFICATION_LISTENER_SETTINGS");
        intent.AddFlags(ActivityFlags.NewTask);
        context.StartActivity(intent);
    }

    public static bool IsNotificationAccessGranted(Context context)
    {
        try
        {
            var enabledListeners = Android.Provider.Settings.Secure.GetString(
                context.ContentResolver,
                "enabled_notification_listeners");

            if (!string.IsNullOrEmpty(enabledListeners))
            {
                var packageName = context.PackageName;
                if (!string.IsNullOrEmpty(packageName))
                {
                    return enabledListeners.Contains(packageName);
                }
            }
        }
        catch
        {
            // Ignore
        }

        return false;
    }
}
