using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace MuzU_Studio.util
{
    internal static class PageExtensions
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.RegisterAttached("Title", typeof(string),
                typeof(PageExtensions),
                new PropertyMetadata(string.Empty, OnTitlePropertyChanged));

        public static string GetTitle(DependencyObject d)
        {
            return d.GetValue(TitleProperty).ToString();
        }

        public static void SetTitle(DependencyObject d, string value)
        {
            d.SetValue(TitleProperty, value);
        }

        private static void OnTitlePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string title = e.NewValue.ToString();
            var window = ApplicationView.GetForCurrentView();
            window.Title = title;
        }
    }
}
