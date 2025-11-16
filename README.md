# WhatStatut

A WhatsApp Android app for educational purposes.

## Features

### Messages (Anti-Delete via Notifications)

WhatStatut can capture WhatsApp messages from system notifications to provide an anti-delete feature. This allows you to see messages even if they are deleted by the sender.

#### What It Captures

- **Notifications only**: Messages are captured from Android system notifications
- **WhatsApp and WhatsApp Business**: Supports both `com.whatsapp` and `com.whatsapp.w4b` packages
- **Text messages**: Captures message text, sender name, chat title, and timestamp

#### Limitations

- **No media content**: Images, videos, voice messages, and other media are not captured
- **No past messages**: Only captures new messages that arrive after enabling the feature
- **No database decryption**: Does not access or decrypt WhatsApp's private databases
- **Notification-based only**: Only works when notifications are visible
- **Device-specific**: All data is stored locally on your device

#### Setup Instructions

1. **Grant Notification Access**:
   - Open WhatStatut
   - Tap the menu (three dots)
   - Select "Notification access"
   - Enable notification access for WhatStatut in Android settings

2. **Enable Capture**:
   - In WhatStatut, open the menu
   - Toggle "Capture messages" to ON
   - You'll see a confirmation message

3. **View Captured Messages**:
   - Open the menu
   - Select "Messages"
   - Browse all captured messages sorted by most recent

4. **Clear History**:
   - In the Messages screen, open the menu
   - Select "Clear all messages"

#### Privacy Notes

- **Local storage only**: All captured messages are stored in a local SQLite database on your device
- **No network calls**: No data is sent to any server or external service
- **User control**: You can disable capture and clear all stored messages at any time
- **Transparent**: The app shows a privacy notice on first launch explaining how the feature works
- **Educational use only**: This app is for educational purposes and is not intended for Play Store distribution

#### Technical Details

- **Minimum SDK**: API 24 (Android 7.0)
- **Target SDK**: API 34 (Android 14)
- **Database**: SQLite with deduplication using SHA-256 hash
- **Architecture**: .NET for Android (not MAUI)

## Building

```bash
cd WhatStatut
dotnet restore
dotnet build
```

## License

Educational use only. Not for distribution on Google Play Store.
 
