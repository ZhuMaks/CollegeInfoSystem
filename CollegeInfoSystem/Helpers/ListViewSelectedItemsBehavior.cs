using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace CollegeInfoSystem.Helpers;

public static class ListViewSelectedItemsBehavior
{
    public static readonly DependencyProperty BindableSelectedItemsProperty =
        DependencyProperty.RegisterAttached(
            "BindableSelectedItems",
            typeof(IList),
            typeof(ListViewSelectedItemsBehavior),
            new PropertyMetadata(null, OnBindableSelectedItemsChanged));

    public static IList GetBindableSelectedItems(DependencyObject obj)
        => (IList)obj.GetValue(BindableSelectedItemsProperty);

    public static void SetBindableSelectedItems(DependencyObject obj, IList value)
        => obj.SetValue(BindableSelectedItemsProperty, value);

    private static void OnBindableSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListView listView) return;

        listView.SelectionChanged -= ListView_SelectionChanged;
        listView.SelectionChanged += ListView_SelectionChanged;

        if (e.NewValue is IList newList)
        {
            newList.Clear();
            foreach (var item in listView.SelectedItems)
                newList.Add(item);
        }
    }

    private static void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListView listView) return;

        var selectedItems = GetBindableSelectedItems(listView);
        if (selectedItems == null) return;

        foreach (var item in e.RemovedItems)
            selectedItems.Remove(item);

        foreach (var item in e.AddedItems)
            if (!selectedItems.Contains(item))
                selectedItems.Add(item);
    }
}
