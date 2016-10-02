using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Mosaic.Base;

namespace Mosaic.Controls
{
    public class Thumbnail : FrameworkElement
    {
        public Thumbnail()
        {
            this.LayoutUpdated += Thumbnail_LayoutUpdated;
            this.Unloaded += Thumbnail_Unloaded;
        }

        public static DependencyProperty SourceProperty;
        public static DependencyProperty ClientAreaOnlyProperty;

        static Thumbnail()
        {
            SourceProperty = DependencyProperty.Register(
                "Source",
                typeof(IntPtr),
                typeof(Thumbnail),
                new FrameworkPropertyMetadata(
                    IntPtr.Zero,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    delegate(DependencyObject obj, DependencyPropertyChangedEventArgs args)
                    {
                        ((Thumbnail)obj).InitializeThumbnail((IntPtr)args.NewValue);
                    }));

            ClientAreaOnlyProperty = DependencyProperty.Register(
                "ClientAreaOnly",
                typeof(bool),
                typeof(Thumbnail),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    delegate(DependencyObject obj, DependencyPropertyChangedEventArgs args)
                    {
                        ((Thumbnail)obj).UpdateThumbnail();
                    }));

            OpacityProperty.OverrideMetadata(
                typeof(Thumbnail),
                new FrameworkPropertyMetadata(
                    1.0,
                    FrameworkPropertyMetadataOptions.Inherits,
                    delegate(DependencyObject obj, DependencyPropertyChangedEventArgs args)
                    {
                        ((Thumbnail)obj).UpdateThumbnail();
                    }));
        }

        public IntPtr Source
        {
            get { return (IntPtr)this.GetValue(SourceProperty); }
            set { this.SetValue(SourceProperty, value); }
        }

        public bool ClientAreaOnly
        {
            get { return (bool)this.GetValue(ClientAreaOnlyProperty); }
            set { this.SetValue(ClientAreaOnlyProperty, value); }
        }

        public new double Opacity
        {
            get { return (double)this.GetValue(OpacityProperty); }
            set { this.SetValue(OpacityProperty, value); }
        }

        private HwndSource target;
        private IntPtr thumb;

        public void InitializeThumbnail(IntPtr source)
        {
            if (IntPtr.Zero != thumb)
            {
                // release the old thumbnail
                ReleaseThumbnail();
            }

            if (IntPtr.Zero != source)
            {
                // find our parent hwnd
                target = (HwndSource)HwndSource.FromVisual(this);

                // if we have one, we can attempt to register the thumbnail
                if (target != null && 0 == WinAPI.DwmRegisterThumbnail(target.Handle, source, out this.thumb))
                {
                    WinAPI.ThumbnailProperties props = new WinAPI.ThumbnailProperties();
                    props.Visible = false;
                    props.SourceClientAreaOnly = this.ClientAreaOnly;
                    props.Opacity = 255;//(byte)(255 * this.Opacity);
                    props.Flags = WinAPI.ThumbnailFlags.Visible | WinAPI.ThumbnailFlags.SourceClientAreaOnly
                        | WinAPI.ThumbnailFlags.Opacity;
                    WinAPI.DwmUpdateThumbnailProperties(thumb, ref props);
                }
            }
        }

        private void ReleaseThumbnail()
        {
            WinAPI.DwmUnregisterThumbnail(thumb);
            this.thumb = IntPtr.Zero;
            this.target = null;
        }

        private void UpdateThumbnail()
        {
            if (IntPtr.Zero != thumb)
            {
                WinAPI.ThumbnailProperties props = new WinAPI.ThumbnailProperties();
                props.SourceClientAreaOnly = this.ClientAreaOnly;
                props.Opacity = 255;//(byte)(255 * this.Opacity);
                props.Flags = WinAPI.ThumbnailFlags.SourceClientAreaOnly | WinAPI.ThumbnailFlags.Opacity;
                WinAPI.DwmUpdateThumbnailProperties(thumb, ref props);
            }
        }

        private void Thumbnail_Unloaded(object sender, RoutedEventArgs e)
        {
            ReleaseThumbnail();
        }

        // this is where the magic happens
        private void Thumbnail_LayoutUpdated(object sender, EventArgs e)
        {
            if (IntPtr.Zero == thumb)
            {
                InitializeThumbnail(this.Source);
            }

            if (IntPtr.Zero != thumb)
            {
                if (!target.RootVisual.IsAncestorOf(this))
                {
                    //we are no longer in the visual tree
                    ReleaseThumbnail();
                    return;
                }

                GeneralTransform transform = TransformToAncestor(target.RootVisual);
                Point a = transform.Transform(new Point(0, 0));
                Point b = transform.Transform(new Point(this.ActualWidth, this.ActualHeight));

                WinAPI.ThumbnailProperties props = new WinAPI.ThumbnailProperties();
                props.Visible = true;
                props.Opacity = 255;
                props.Destination = new WinAPI.Rect(
                    (int)Math.Ceiling(a.X), (int)Math.Ceiling(a.Y),
                    (int)Math.Ceiling(b.X), (int)Math.Ceiling(b.Y));
                props.Flags = WinAPI.ThumbnailFlags.Visible | WinAPI.ThumbnailFlags.RectDetination;
                WinAPI.DwmUpdateThumbnailProperties(thumb, ref props);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            WinAPI.Size size;
            WinAPI.DwmQueryThumbnailSourceSize(this.thumb, out size);

            double scale = 1;

            // our preferred size is the thumbnail source size
            // if less space is available, we scale appropriately
            if (size.Width > availableSize.Width)
                scale = availableSize.Width / size.Width;
            if (size.Height > availableSize.Height)
                scale = Math.Min(scale, availableSize.Height / size.Height);

            return new Size(size.Width * scale, size.Height * scale); ;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            WinAPI.Size size;
            WinAPI.DwmQueryThumbnailSourceSize(this.thumb, out size);

            // scale to fit whatever size we were allocated
            double scale = finalSize.Width / size.Width;
            scale = Math.Min(scale, finalSize.Height / size.Height);

            return new Size(size.Width * scale, size.Height * scale);
        }
    }
}
