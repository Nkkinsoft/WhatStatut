using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using WhatStatut.Services;

namespace WhatStatut;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    private const string PrefsName = "WhatStatutPrefs";
    private const string PrefCaptureEnabled = "capture_enabled";
    private const string PrefPrivacyShown = "privacy_shown";

    private ISharedPreferences? _prefs;
    private IMenuItem? _captureToggleItem;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Set our view from the "main" layout resource
        SetContentView(Resource.Layout.activity_main);

        _prefs = GetSharedPreferences(PrefsName, FileCreationMode.Private);

        // Show privacy dialog on first launch
        ShowPrivacyDialogIfNeeded();
    }

    protected override void OnResume()
    {
        base.OnResume();
        
        // Update menu state
        InvalidateOptionsMenu();
    }

    private void ShowPrivacyDialogIfNeeded()
    {
        if (_prefs == null) return;

        var privacyShown = _prefs.GetBoolean(PrefPrivacyShown, false);
        if (!privacyShown)
        {
            ShowPrivacyDialog();
            _prefs.Edit()?.PutBoolean(PrefPrivacyShown, true)?.Apply();
        }
    }

    private void ShowPrivacyDialog()
    {
        var builder = new AlertDialog.Builder(this);
        builder.SetTitle(Resource.String.privacy_dialog_title);
        builder.SetMessage(Resource.String.privacy_dialog_message);
        builder.SetPositiveButton(Resource.String.privacy_dialog_ok, (sender, args) => { });
        builder.SetCancelable(false);
        builder.Show();
    }

    public override bool OnCreateOptionsMenu(IMenu? menu)
    {
        MenuInflater.Inflate(Resource.Menu.main_menu, menu);
        
        _captureToggleItem = menu?.FindItem(Resource.Id.menu_capture_toggle);
        
        if (_captureToggleItem != null && _prefs != null)
        {
            var isEnabled = _prefs.GetBoolean(PrefCaptureEnabled, false);
            _captureToggleItem.SetChecked(isEnabled);
        }

        return true;
    }

    public override bool OnOptionsItemSelected(IMenuItem item)
    {
        switch (item.ItemId)
        {
            case Resource.Id.menu_messages:
                OpenMessagesActivity();
                return true;

            case Resource.Id.menu_capture_toggle:
                ToggleCapture(item);
                return true;

            case Resource.Id.menu_notification_access:
                OpenNotificationSettings();
                return true;

            default:
                return base.OnOptionsItemSelected(item);
        }
    }

    private void OpenMessagesActivity()
    {
        var intent = new Intent(this, typeof(MessagesActivity));
        StartActivity(intent);
    }

    private void ToggleCapture(IMenuItem item)
    {
        if (_prefs == null) return;

        var currentState = _prefs.GetBoolean(PrefCaptureEnabled, false);
        var newState = !currentState;

        // Check if notification access is granted
        if (newState && !MessagesNotificationListener.IsNotificationAccessGranted(this))
        {
            Toast.MakeText(this, Resource.String.notification_access_required, ToastLength.Long)?.Show();
            OpenNotificationSettings();
            return;
        }

        _prefs.Edit()?.PutBoolean(PrefCaptureEnabled, newState)?.Apply();
        item.SetChecked(newState);

        var messageRes = newState ? Resource.String.capture_enabled : Resource.String.capture_disabled;
        Toast.MakeText(this, messageRes, ToastLength.Short)?.Show();
    }

    private void OpenNotificationSettings()
    {
        MessagesNotificationListener.RequestNotificationAccess(this);
    }
}
