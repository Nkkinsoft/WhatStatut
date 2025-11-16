using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using WhatStatut.Android.Models;
using WhatStatut.Android.Utils;

namespace WhatStatut.Android.Adapters;

public class StatusAdapter : RecyclerView.Adapter
{
    private List<StatusItem> _items = new();
    public event EventHandler<StatusItem>? ItemClick;

    public void SetItems(List<StatusItem> items)
    {
        _items = items;
        NotifyDataSetChanged();
    }

    public override int ItemCount => _items.Count;

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        var view = LayoutInflater.From(parent.Context)!
            .Inflate(Resource.Layout.status_item, parent, false)!;
        return new StatusViewHolder(view, OnItemClick);
    }

    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
    {
        if (holder is StatusViewHolder viewHolder)
        {
            viewHolder.Bind(_items[position]);
        }
    }

    private void OnItemClick(int position)
    {
        if (position >= 0 && position < _items.Count)
        {
            ItemClick?.Invoke(this, _items[position]);
        }
    }

    private class StatusViewHolder : RecyclerView.ViewHolder
    {
        private readonly ImageView _imageView;
        private readonly ImageView _playIcon;
        private readonly Action<int> _clickListener;

        public StatusViewHolder(View itemView, Action<int> clickListener) : base(itemView)
        {
            _imageView = itemView.FindViewById<ImageView>(Resource.Id.imageView)!;
            _playIcon = itemView.FindViewById<ImageView>(Resource.Id.playIcon)!;
            _clickListener = clickListener;

            itemView.Click += (s, e) => _clickListener(BindingAdapterPosition);
        }

        public void Bind(StatusItem item)
        {
            // Clear previous image
            _imageView.SetImageBitmap(null);
            _playIcon.Visibility = ViewStates.Gone;

            // Load thumbnail
            Bitmap? bitmap = null;
            if (item.Type == StatusType.Image)
            {
                bitmap = BitmapUtils.DecodeSampledBitmap(item.Path, 200, 200);
            }
            else if (item.Type == StatusType.Video)
            {
                bitmap = BitmapUtils.GetVideoThumbnail(item.Path);
                _playIcon.Visibility = ViewStates.Visible;
            }

            if (bitmap != null)
            {
                _imageView.SetImageBitmap(bitmap);
            }
        }
    }
}
