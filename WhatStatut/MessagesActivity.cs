using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using WhatStatut.Data;
using WhatStatut.Models;
using WhatStatut.Services;

namespace WhatStatut;

[Activity(Label = "Messages")]
public class MessagesActivity : Activity
{
    private RecyclerView? _recyclerView;
    private TextView? _emptyStateText;
    private MessagesAdapter? _adapter;
    private MessagesRepository? _repository;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_messages);

        _recyclerView = FindViewById<RecyclerView>(Resource.Id.messagesRecyclerView);
        _emptyStateText = FindViewById<TextView>(Resource.Id.emptyStateText);

        // Initialize repository
        var dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "messages.db");
        _repository = new MessagesRepository(dbPath);

        // Setup RecyclerView
        if (_recyclerView != null)
        {
            _recyclerView.SetLayoutManager(new LinearLayoutManager(this));
            _adapter = new MessagesAdapter();
            _recyclerView.SetAdapter(_adapter);
        }

        LoadMessages();
    }

    protected override void OnResume()
    {
        base.OnResume();
        LoadMessages();
    }

    private async void LoadMessages()
    {
        if (_repository == null) return;

        try
        {
            var messages = await _repository.QueryLatestAsync(100);
            
            if (_adapter != null)
            {
                _adapter.UpdateMessages(messages);
            }

            // Show/hide empty state
            if (_recyclerView != null && _emptyStateText != null)
            {
                if (messages.Count == 0)
                {
                    _recyclerView.Visibility = ViewStates.Gone;
                    _emptyStateText.Visibility = ViewStates.Visible;
                }
                else
                {
                    _recyclerView.Visibility = ViewStates.Visible;
                    _emptyStateText.Visibility = ViewStates.Gone;
                }
            }
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("MessagesActivity", $"Error loading messages: {ex.Message}");
        }
    }

    public override bool OnCreateOptionsMenu(IMenu? menu)
    {
        MenuInflater.Inflate(Resource.Menu.messages_menu, menu);
        return true;
    }

    public override bool OnOptionsItemSelected(IMenuItem item)
    {
        if (item.ItemId == Resource.Id.menu_clear_all)
        {
            ClearAllMessages();
            return true;
        }

        return base.OnOptionsItemSelected(item);
    }

    private async void ClearAllMessages()
    {
        if (_repository == null) return;

        try
        {
            await _repository.ClearAllAsync();
            Toast.MakeText(this, Resource.String.messages_cleared, ToastLength.Short)?.Show();
            LoadMessages();
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("MessagesActivity", $"Error clearing messages: {ex.Message}");
        }
    }
}

public class MessagesAdapter : RecyclerView.Adapter
{
    private List<MessageEntry> _messages = new List<MessageEntry>();

    public void UpdateMessages(List<MessageEntry> messages)
    {
        _messages = messages;
        NotifyDataSetChanged();
    }

    public override int ItemCount => _messages.Count;

    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
    {
        if (holder is MessageViewHolder viewHolder)
        {
            viewHolder.Bind(_messages[position]);
        }
    }

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        var view = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.message_row, parent, false);
        return new MessageViewHolder(view!);
    }
}

public class MessageViewHolder : RecyclerView.ViewHolder
{
    private readonly TextView _chatTitle;
    private readonly TextView _packageBadge;
    private readonly TextView _sender;
    private readonly TextView _messageText;
    private readonly TextView _timestamp;

    public MessageViewHolder(View itemView) : base(itemView)
    {
        _chatTitle = itemView.FindViewById<TextView>(Resource.Id.chatTitle)!;
        _packageBadge = itemView.FindViewById<TextView>(Resource.Id.packageBadge)!;
        _sender = itemView.FindViewById<TextView>(Resource.Id.sender)!;
        _messageText = itemView.FindViewById<TextView>(Resource.Id.messageText)!;
        _timestamp = itemView.FindViewById<TextView>(Resource.Id.timestamp)!;
    }

    public void Bind(MessageEntry message)
    {
        _chatTitle.Text = message.ChatTitle;
        _messageText.Text = message.Text;

        // Set package badge
        if (message.Package == "com.whatsapp.w4b")
        {
            _packageBadge.Text = "Business";
            _packageBadge.SetBackgroundColor(Android.Graphics.Color.ParseColor("#1976D2"));
        }
        else
        {
            _packageBadge.Text = "WhatsApp";
            _packageBadge.SetBackgroundColor(Android.Graphics.Color.ParseColor("#25D366"));
        }

        // Show sender if available
        if (!string.IsNullOrEmpty(message.Sender))
        {
            _sender.Text = message.Sender;
            _sender.Visibility = ViewStates.Visible;
        }
        else
        {
            _sender.Visibility = ViewStates.Gone;
        }

        // Format timestamp
        var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(message.PostedAt).ToLocalTime();
        var now = DateTimeOffset.Now;
        
        if (dateTime.Date == now.Date)
        {
            _timestamp.Text = dateTime.ToString("HH:mm");
        }
        else if (dateTime.Date == now.AddDays(-1).Date)
        {
            _timestamp.Text = $"Yesterday {dateTime:HH:mm}";
        }
        else if (dateTime.Year == now.Year)
        {
            _timestamp.Text = dateTime.ToString("MMM dd HH:mm");
        }
        else
        {
            _timestamp.Text = dateTime.ToString("MMM dd yyyy HH:mm");
        }
    }
}
