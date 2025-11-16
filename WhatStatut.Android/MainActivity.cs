using Android;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using AndroidX.RecyclerView.Widget;
using WhatStatut.Android.Adapters;
using WhatStatut.Android.Models;
using WhatStatut.Android.Repositories;
using WhatStatut.Android.Utils;

namespace WhatStatut.Android;

[Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@style/AppTheme")]
public class MainActivity : AppCompatActivity
{
    private const int PermissionRequestCode = 100;
    
    private RecyclerView? _recyclerView;
    private StatusAdapter? _adapter;
    private StatusRepository? _repository;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_main);

        _repository = new StatusRepository();
        _adapter = new StatusAdapter();
        _adapter.ItemClick += OnStatusItemClick;

        _recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
        if (_recyclerView != null)
        {
            _recyclerView.SetLayoutManager(new GridLayoutManager(this, 3));
            _recyclerView.SetAdapter(_adapter);
        }

        CheckAndRequestPermissions();
    }

    private void CheckAndRequestPermissions()
    {
        var permissionsToRequest = new List<string>();

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu) // API 33+
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadMediaImages) != Permission.Granted)
                permissionsToRequest.Add(Manifest.Permission.ReadMediaImages);
            
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadMediaVideo) != Permission.Granted)
                permissionsToRequest.Add(Manifest.Permission.ReadMediaVideo);
        }
        else
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != Permission.Granted)
                permissionsToRequest.Add(Manifest.Permission.ReadExternalStorage);
        }

        if (permissionsToRequest.Count > 0)
        {
            ActivityCompat.RequestPermissions(this, permissionsToRequest.ToArray(), PermissionRequestCode);
        }
        else
        {
            LoadStatuses();
        }
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        if (requestCode == PermissionRequestCode)
        {
            bool allGranted = grantResults.Length > 0 && grantResults.All(r => r == Permission.Granted);
            
            if (allGranted)
            {
                LoadStatuses();
            }
            else
            {
                Toast.MakeText(this, Resource.String.permission_denied, ToastLength.Long)?.Show();
            }
        }
    }

    private void LoadStatuses()
    {
        if (_repository == null || _adapter == null)
            return;

        var items = _repository.GetStatusItems();
        _adapter.SetItems(items);

        if (items.Count == 0)
        {
            Toast.MakeText(this, Resource.String.no_statuses_found, ToastLength.Short)?.Show();
        }
    }

    private void OnStatusItemClick(object? sender, StatusItem item)
    {
        var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
        builder.SetTitle(Resource.String.choose_action);
        builder.SetItems(new[]
        {
            GetString(Resource.String.action_preview),
            GetString(Resource.String.action_save),
            GetString(Resource.String.action_share)
        }, (s, args) =>
        {
            switch (args.Which)
            {
                case 0:
                    PreviewItem(item);
                    break;
                case 1:
                    SaveItem(item);
                    break;
                case 2:
                    ShareItem(item);
                    break;
            }
        });
        builder.Show();
    }

    private void PreviewItem(StatusItem item)
    {
        var uri = FileProviderHelper.GetUriForFile(this, item.Path);
        if (uri == null)
            return;

        var intent = new Intent(Intent.ActionView);
        intent.SetDataAndType(uri, item.Type == StatusType.Image ? "image/*" : "video/*");
        intent.AddFlags(ActivityFlags.GrantReadUriPermission);

        try
        {
            StartActivity(intent);
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, $"Cannot preview: {ex.Message}", ToastLength.Short)?.Show();
        }
    }

    private void SaveItem(StatusItem item)
    {
        var subDirectory = item.Type == StatusType.Image 
            ? global::Android.OS.Environment.DirectoryPictures 
            : global::Android.OS.Environment.DirectoryMovies;

        var savedPath = FileProviderHelper.CopyToExternalFiles(this, item.Path, subDirectory);
        
        if (savedPath != null)
        {
            MediaScannerHelper.ScanFile(this, savedPath);
            
            var message = string.Format(GetString(Resource.String.saved_successfully) ?? "", 
                Path.GetDirectoryName(savedPath));
            Toast.MakeText(this, message, ToastLength.Short)?.Show();
        }
        else
        {
            Toast.MakeText(this, Resource.String.save_failed, ToastLength.Short)?.Show();
        }
    }

    private void ShareItem(StatusItem item)
    {
        var uri = FileProviderHelper.GetUriForFile(this, item.Path);
        if (uri == null)
            return;

        var intent = new Intent(Intent.ActionSend);
        intent.SetType(item.Type == StatusType.Image ? "image/*" : "video/*");
        intent.PutExtra(Intent.ExtraStream, uri);
        intent.AddFlags(ActivityFlags.GrantReadUriPermission);

        try
        {
            StartActivity(Intent.CreateChooser(intent, "Share via"));
        }
        catch (Exception ex)
        {
            Toast.MakeText(this, $"Cannot share: {ex.Message}", ToastLength.Short)?.Show();
        }
    }

    protected override void OnDestroy()
    {
        if (_adapter != null)
        {
            _adapter.ItemClick -= OnStatusItemClick;
        }
        base.OnDestroy();
    }
}
