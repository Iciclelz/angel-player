/*
    Copyright (C) 2016 Ryukuo

    This file is part of Angel Player.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Xml;

namespace Angel_Player.Windows
{
    /// <summary>
    /// Interaction logic for PlaylistWindow.xaml
    /// </summary>
    public partial class PlaylistWindow : MetroWindow
    {
        private string XmlPath;
        public bool IsCloseable;
        private PlayerWindow playerWindow;
        public PlaylistWindow(PlayerWindow pWnd)
        {
            InitializeComponent();
            IsCloseable = false;
            playerWindow = pWnd;

            XmlPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Angel Player", "Playlist.xml");
            if (!Directory.Exists(Path.GetDirectoryName(XmlPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(XmlPath));
            }

            
        }

        private void addMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "All media files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                addToPlayList(openFileDialog.FileName);
            }
        }

        public void addToPlayList(string filePath)
        {
            addToPlayList(filePath, false);
        }

        public void addToPlayList(string filePath, bool b)
        {
            if (File.Exists(filePath) && !ListBoxItemExist(filePath))
            {
                listBox.Items.Add(new CheckedListItem(filePath, b));
            }
        }
        
        public bool ListBoxItemExist(string filePath)
        {
            foreach (CheckedListItem cli in listBox.Items)
            {
                if (cli.Tag.Equals(filePath))
                {
                    return true;
                }
            }
            return false;
        }

        private void removeMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (listBox.SelectedItem != null && (listBox.SelectedItem as CheckedListItem).Visibility == Visibility.Visible)
            {
                listBox.Items.Remove(listBox.SelectedItem);
            }
        }

        private void clearMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            switch (MessageBox.Show(this, "Are you sure you would like to clear your entire playlist?", "Angel Player", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.None))
            {
                case MessageBoxResult.Yes:
                    listBox.Items.Clear();
                    break;
            }

        }

        private int BoolToInt(bool b)
        {
            if (b)
            {
                return 1;
            }
            return 0;
        }

        private bool IntToBool(int i)
        {
            if (i > 0)
            {
                return true;
            }
            return false;
        }

        public void saveXml()
        {
            using (XmlWriter writer = XmlWriter.Create(XmlPath))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Playlist");

                foreach (CheckedListItem cli in listBox.Items)
                {
                    writer.WriteStartElement("PlaylistItem");

                    writer.WriteAttributeString("IsChecked", BoolToInt(cli.IsChecked).ToString());
                    writer.WriteString(cli.Tag);

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        public void loadXml()
        {
            if (File.Exists(XmlPath))
            {
                using (XmlReader reader = XmlReader.Create(XmlPath))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "PlaylistItem":
                                    {
                                        bool IsChecked = IntToBool(Int32.Parse(reader["IsChecked"]));
                                        reader.Read();
                                        addToPlayList(reader.Value.Trim(), IsChecked);
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsCloseable)
            {
                e.Cancel = true;
                return;
            }

            saveXml();
        }

        private void listBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (listBox.SelectedItem != null && (listBox.SelectedItem as CheckedListItem).Visibility == Visibility.Visible)
            {
                playerWindow.Play((listBox.SelectedItem as CheckedListItem).Tag);
            }
        }

        private void mainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            loadXml();
        }

        private void grid_Drop(object sender, System.Windows.DragEventArgs e)
        {
            String[] fileNames = (String[])e.Data.GetData(DataFormats.FileDrop, true);
            if (fileNames.Length > 0)
            {
                foreach (string fileName in fileNames)
                {
                    addToPlayList(fileName);
                }
                
            }
            e.Handled = true;
        }

        private void playMediaMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedItem != null && (listBox.SelectedItem as CheckedListItem).Visibility == Visibility.Visible)
            {
                playerWindow.Play((listBox.SelectedItem as CheckedListItem).Tag);
            }
        }

        public List<CheckedListItem> GetAllCheckedItems()
        {
            List<CheckedListItem> checkedListItems = new List<CheckedListItem>();
            foreach (CheckedListItem cli in listBox.Items)
            {
                if (cli.IsChecked)
                {
                    checkedListItems.Add(cli);
                }
            }
            return checkedListItems;
        }

        public String GetNextMedia(String currentMedia)
        {
            List<CheckedListItem> checkedListItems = GetAllCheckedItems();

            if (checkedListItems.Count > 0)
            {
                for (int i = 0; i < checkedListItems.Count; i++)
                {
                    if (checkedListItems[i].Tag.Equals(currentMedia))
                    {
                        if (i == (checkedListItems.Count - 1))
                        {
                            return checkedListItems[0].Tag;    
                        }
                        else
                        {
                            return checkedListItems[i + 1].Tag;
                        }
                    }
                }
            }
            if (listBox.Items.Count > 0)
            {
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    if ((listBox.Items[i] as CheckedListItem).Tag.Equals(currentMedia))
                    {
                        if (i == (listBox.Items.Count - 1))
                        {
                            return (listBox.Items[0] as CheckedListItem).Tag;    
                        }
                        else
                        {
                            return (listBox.Items[i + 1] as CheckedListItem).Tag;
                        }
                    }
                }
                return (listBox.Items[0] as CheckedListItem).Tag;
            }

            return null;
        }

        public String GetLastMedia(String currentMedia)
        {
            List<CheckedListItem> checkedListItems = GetAllCheckedItems();

            if (checkedListItems.Count > 0)
            {
                for (int i = 0; i < checkedListItems.Count; i++)
                {
                    if (checkedListItems[i].Tag.Equals(currentMedia))
                    {
                        if (i > 0)
                        {
                            return checkedListItems[i - 1].Tag;
                        }
                        else
                        {
                            return checkedListItems[checkedListItems.Count - 1].Tag;
                        }
                    }
                }
            }
            if (listBox.Items.Count > 0)
            {
                for (int i = 0; i < listBox.Items.Count; i++)
                {
                    if ((listBox.Items[i] as CheckedListItem).Tag.Equals(currentMedia))
                    {
                        if (i > 0)
                        {
                            return (listBox.Items[i - 1] as CheckedListItem).Tag;
                        }
                        else
                        {
                            return (listBox.Items[listBox.Items.Count - 1] as CheckedListItem).Tag;
                        }
                    }
                }
                return (listBox.Items[0] as CheckedListItem).Tag;
            }
            return null;
        }
        private void moveUpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if ((listBox.SelectedItem as CheckedListItem).Visibility != Visibility.Visible)
            {
                return;
            }
            try {
                int i = listBox.Items.IndexOf(listBox.SelectedItem);
                if (i > 0)
                {
                    CheckedListItem cli = listBox.Items[i - 1] as CheckedListItem;
                    listBox.Items[i - 1] = listBox.Items[i];
                    listBox.Items[i] = cli;
                    listBox.SelectedItem = listBox.Items[i - 1];
                }
            }
            catch { }
        }

        private void moveDownMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if ((listBox.SelectedItem as CheckedListItem).Visibility != Visibility.Visible)
            {
                return;
            }
            try {
                int i = listBox.Items.IndexOf(listBox.SelectedItem);
                if (i < listBox.Items.Count)
                {
                    CheckedListItem cli = listBox.Items[i + 1] as CheckedListItem;
                    listBox.Items[i + 1] = listBox.Items[i];
                    listBox.Items[i] = cli;
                    listBox.SelectedItem = listBox.Items[i + 1];
                }
            }
            catch { }
        }

        private void sortMenuItem_Click(object sender, RoutedEventArgs e)
        {
            listBox.Items.SortDescriptions.Clear();
            listBox.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            listBox.Items.Refresh();
        }

        private void searchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(searchTextBox.Text.Trim()))
            {
                foreach (CheckedListItem cli in listBox.Items)
                {
                    cli.Visibility = Visibility.Visible;
                }
            }
            else
            {
                foreach (CheckedListItem cli in listBox.Items)
                {
                    if (cli.Name.ToLower().Contains(searchTextBox.Text.Trim().ToLower()))
                    {
                        cli.Visibility = Visibility.Visible;
                        listBox.ScrollIntoView(cli);
                    }
                    else
                    {
                        cli.Visibility = Visibility.Collapsed;
                    }

                }
            }  
        }

        private void openContainingFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedItem != null && (listBox.SelectedItem as CheckedListItem).Visibility == Visibility.Visible)
            {
                Process.Start(Path.GetDirectoryName((listBox.SelectedItem as CheckedListItem).Tag));
            }
        }

        private async void renameMediaMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CheckedListItem selectedItem = listBox.SelectedItem as CheckedListItem;
            string renamedFile = await this.ShowInputAsync("Angel Player - Rename Media", "What would you like to rename " + selectedItem.Name + " to?");
            if (!String.IsNullOrEmpty(renamedFile))
            {
                string renamedPath = Path.Combine(Path.GetDirectoryName(selectedItem.Tag), renamedFile);
                File.Move(selectedItem.Tag, renamedPath);
                selectedItem.Tag = renamedPath;
                selectedItem.Name = Path.GetFileName(selectedItem.Tag);
                listBox.Items.Refresh();
            }
            
        }

        private void deleteMediaMenuItem_Click(object sender, RoutedEventArgs e)
        {

            if (listBox.SelectedItem != null && (listBox.SelectedItem as CheckedListItem).Visibility == Visibility.Visible)
            {
                CheckedListItem selectedItem = listBox.SelectedItem as CheckedListItem;
                switch (MessageBox.Show(this, "Are you sure you would like to delete '" + (listBox.SelectedItem as CheckedListItem).Name + "' from your disk?", "Angel Player", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.None))
                {
                    case MessageBoxResult.Yes:
                        File.Delete(selectedItem.Tag);
                        listBox.Items.Remove(selectedItem);
                        break;
                }
            }
        }

        private void listBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            fullPathStatusBarItem.Content = (listBox.SelectedItem as CheckedListItem).Tag;
        }
    }
    public class CheckedListItem : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public bool IsChecked { get; set; }
        public string Tag { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private Visibility _Visibility;
        public Visibility Visibility { get { return _Visibility; } set { _Visibility = value; if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs("Visibility")); } } }

        public CheckedListItem()
        {
            Visibility = Visibility.Visible;
        }

        public CheckedListItem(string Tag, bool IsChecked) : this()
        {
            this.Name = Path.GetFileName(Tag);
            this.IsChecked = IsChecked;
            this.Tag = Tag;
        }

        public CheckedListItem(string Name, bool IsChecked, string Tag) : this()
        {
            this.Name = Name;
            this.IsChecked = IsChecked;
            this.Tag = Tag;
        }
    }
}
