# WhatStatut

A .NET for Android educational application for viewing locally cached WhatsApp status media files.

## ⚠️ Important Disclaimers

- **Educational Use Only**: This project is created for educational purposes to demonstrate .NET for Android development, file system access, and media handling.
- **Local Files Only**: This app does NOT use WhatsApp APIs, does NOT intercept network traffic, and does NOT access WhatsApp's internal data. It only reads media files that WhatsApp has already cached on the device's storage.
- **Not for Distribution**: This app is NOT intended for Google Play Store distribution or commercial use.
- **Privacy**: All operations are performed locally on the device. No data is transmitted to external servers.

## Features

- **Status Discovery**: Automatically finds WhatsApp and WhatsApp Business status media (images and videos) from local cache directories
- **Grid View**: Displays statuses in a 3-column grid with thumbnails
- **Preview**: View full-size images and videos using the device's default media viewer
- **Save Copy**: Save status media to your device's Pictures (images) or Movies (videos) folder
- **Share**: Share status media with other apps using Android's share functionality
- **Safe Media Handling**: Uses bitmap downsampling to prevent out-of-memory errors
- **FileProvider Integration**: Uses content URIs instead of file:// URIs for secure sharing (Android 7.0+)
- **Scoped Storage Compliance**: Requests only necessary permissions based on Android API level

## Permissions Rationale

The app requests the following permissions:

### Android 13+ (API 33+)
- `READ_MEDIA_IMAGES`: To read image files from WhatsApp status cache
- `READ_MEDIA_VIDEO`: To read video files from WhatsApp status cache

### Android 12 and below (API ≤ 32)
- `READ_EXTERNAL_STORAGE`: To read media files from external storage

**Note**: The app does NOT request `MANAGE_EXTERNAL_STORAGE` to comply with scoped storage best practices.

## Status File Locations

The app scans the following directories for status media:

1. `/storage/emulated/0/Android/media/com.whatsapp/WhatsApp/Media/.Statuses` (WhatsApp - Modern)
2. `/storage/emulated/0/WhatsApp/Media/.Statuses` (WhatsApp - Legacy)
3. `/storage/emulated/0/Android/media/com.whatsapp.w4b/WhatsApp Business/Media/.Statuses` (WhatsApp Business)

## Supported Media Types

- **Images**: .jpg, .jpeg, .png, .webp
- **Videos**: .mp4

## Build Instructions

### Prerequisites

1. **.NET SDK 9.0 or later**
   ```bash
   dotnet --version
   ```

2. **.NET Android workload**
   ```bash
   dotnet workload install android
   ```

3. **Android SDK** (automatically installed with the workload, or use Android Studio)

### Building the App

1. **Clone the repository**
   ```bash
   git clone https://github.com/Nkkinsoft/WhatStatut.git
   cd WhatStatut
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build WhatStatut.Android/WhatStatut.Android.csproj
   ```

4. **Install to connected device/emulator**
   ```bash
   dotnet build WhatStatut.Android/WhatStatut.Android.csproj -t:Install
   ```

   Or to run directly:
   ```bash
   dotnet build WhatStatut.Android/WhatStatut.Android.csproj -t:Run
   ```

### Building APK

To create an APK for testing:

```bash
dotnet publish WhatStatut.Android/WhatStatut.Android.csproj -c Release -f net8.0-android
```

The APK will be located in `WhatStatut.Android/bin/Release/net8.0-android/publish/`.

## Architecture Overview

### Project Structure

```
WhatStatut.Android/
├── Adapters/           # RecyclerView adapters
│   └── StatusAdapter.cs
├── Features/           # Future feature stubs (OCR, Encryption)
│   ├── OcrPipeline.cs
│   └── EncryptionManager.cs
├── Models/            # Data models
│   └── StatusItem.cs
├── Repositories/      # Data access layer
│   └── StatusRepository.cs
├── Resources/         # Android resources (layouts, drawables, etc.)
├── Services/          # Background services (stubs)
│   └── StatusMonitorService.cs
├── Utils/            # Utility classes
│   ├── BitmapUtils.cs
│   ├── FileProviderHelper.cs
│   ├── MediaScannerHelper.cs
│   └── ThumbnailCache.cs
└── MainActivity.cs    # Main activity
```

### Key Components

1. **StatusRepository**: Scans WhatsApp status directories and returns a list of `StatusItem` objects
2. **StatusAdapter**: RecyclerView adapter that displays status thumbnails in a grid
3. **BitmapUtils**: Safe bitmap decoding with downsampling to prevent OOM errors
4. **FileProviderHelper**: Manages content URI generation and file copying
5. **MainActivity**: Handles permissions, UI, and user interactions

### Data Flow

1. App starts → Request permissions
2. Permissions granted → `StatusRepository.GetStatusItems()`
3. Repository scans directories → Returns list of `StatusItem`
4. `StatusAdapter` displays items in RecyclerView grid
5. User clicks item → Action dialog (Preview/Save/Share)
6. Action selected → Uses `FileProviderHelper` to handle file operations

## Stretch Features (Not Implemented)

The following features are stubbed out with TODO comments for future implementation:

1. **FileObserver Service**: Auto-refresh when new statuses appear
2. **LRU Thumbnail Cache**: In-memory cache for decoded bitmaps
3. **OCR Pipeline**: Extract text from status images
4. **Encryption**: Save statuses with password protection

## Testing on Emulator vs Real Device

- **Emulator**: The app will build and run, but will show "No statuses found" since emulators don't have WhatsApp installed
- **Real Device**: Install WhatsApp, view some statuses (they cache for ~24 hours), then run the app to see them in the grid

## Known Limitations

- Status media is only available while cached by WhatsApp (typically 24 hours)
- Cannot access statuses that WhatsApp has not cached locally
- Requires WhatsApp to be installed on the device
- Does not support other media types (GIFs, audio)

## Technical Details

- **Target Framework**: net9.0-android
- **Minimum SDK**: Android 7.0 (API 24)
- **Target SDK**: Android 14 (API 34)
- **UI Library**: AndroidX (RecyclerView, CardView, AppCompat)
- **Language**: C# 12 (latest)
- **Nullable Reference Types**: Enabled

## Development Tools

- **Visual Studio 2022** (Windows/Mac)
- **Visual Studio Code** with C# extension
- **JetBrains Rider**
- **Command Line** (.NET CLI)

## License

This project is provided as-is for educational purposes. Use responsibly and respect privacy.

## Contributing

This is an educational project. Feel free to fork and experiment, but remember the ethical considerations of accessing media content.

## Troubleshooting

### "No statuses found"
- Ensure WhatsApp is installed
- View some WhatsApp statuses (they cache for ~24 hours)
- Check that storage permissions are granted

### Build errors
- Ensure .NET 8.0 SDK is installed
- Run `dotnet workload install android`
- Clear bin/obj folders and rebuild

### Runtime crashes
- Check Android version compatibility
- Verify permissions are granted
- Check logcat for detailed error messages

## Contact

For issues or questions about this educational project, please open an issue on GitHub.
 
