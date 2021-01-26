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
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace Angel_Player.Windows
{
    /// <summary>
    /// Interaction logic for PlayerWindow.xaml
    /// </summary>
    public partial class PlayerWindow : MetroWindow
    {
        private bool userIsDraggingSlider;
        private DispatcherTimer timer;
        private DispatcherTimer doubleClickTimer;
        private DispatcherTimer visibilityTimer;
        private bool musicPlaying;
        private PlaylistWindow playlistWindow;
        private bool audioVideoLoop;
        public PlayerWindow()
        {
            InitializeComponent();
            userIsDraggingSlider = false;
            musicPlaying = false;
            playlistWindow = new PlaylistWindow(this);
            playlistWindow.Show();
            playlistWindow.Hide();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

            doubleClickTimer = new DispatcherTimer();
            doubleClickTimer.Interval = TimeSpan.FromMilliseconds(System.Windows.Forms.SystemInformation.DoubleClickTime);
            doubleClickTimer.Tick += (s, e) => doubleClickTimer.Stop();

            visibilityTimer = new DispatcherTimer();
            visibilityTimer.Interval = TimeSpan.FromMilliseconds(1);
            visibilityTimer.Tick += visibilityTimer_Tick;
            visibilityTimer.Start();

            SetMp3VideoSettings();
            audioMediaElement.Play();
            audioVideoLoop = true;
        }

        public PlayerWindow(String[] args) : this()
        {
            if (args.Length > 0)
            {
                mediaElement.Source = new Uri(args[0].ToString());
                setContentLabel(args[0].ToString());
                mediaElement.Play();
                playPauseBrush.Visual = Resources["appbar_control_pause"] as Visual;
            }
        }

        private void SetMp3VideoSettings()
        {
            
            int i = 3;
            if (!Int32.TryParse(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings")), out i))
            {
                i = 3;
            }

            switch (i)
            {
                case 1:
                    audioMediaElement.Source = new Uri("Resources/480p.mp4", UriKind.Relative);
                    break;
                case 2:
                    audioMediaElement.Source = new Uri("Resources/720p.mp4", UriKind.Relative);
                    break;
                case 3:
                    audioMediaElement.Source = new Uri("Resources/1080p.mp4", UriKind.Relative);
                    break;
                case 4:
                    audioMediaElement.Source = new Uri("Resources/1080d.mp4", UriKind.Relative);
                    break;
            }
        }

        private void visibilityTimer_Tick(object sender, EventArgs e)
        {
            
            try
            {
                if (mediaElement.HasVideo)
                {
                    mediaElement.Visibility = Visibility.Visible;
                    titleLabel.Visibility = Visibility.Collapsed;
                    mediaLabel.Visibility = Visibility.Collapsed;
                    audioMediaElement.Visibility = Visibility.Collapsed;
                }
                else
                {
                    mediaElement.Visibility = Visibility.Collapsed;
                    titleLabel.Visibility = Visibility.Visible;
                    audioMediaElement.Visibility = Visibility.Visible;
                    if (!String.IsNullOrEmpty(titleLabel.Content.ToString())) {
                        mediaLabel.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        mediaLabel.Visibility = Visibility.Collapsed;
                    }

                }
            }
            catch { }

            try
            {
                if (playlistWindow.IsActive)
                {
                    audioMediaElement.Pause();
                    audioVideoLoop = false;
                }
                else
                {
                    if (!audioVideoLoop)
                    {
                        audioMediaElement.Play();
                    }
                    if (audioMediaElement.Position == audioMediaElement.NaturalDuration.TimeSpan)
                    {
                        audioMediaElement.Position = TimeSpan.FromSeconds(0);
                    }
                }
                
            }
            catch { }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            try {
                if ((mediaElement.Source != null) && (mediaElement.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
                {
                    slider.Minimum = 0;
                    slider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                    slider.Value = mediaElement.Position.TotalSeconds;
                    currentSecondLabel.Content = mediaElement.Position.ToString(@"hh\:mm\:ss");
                    totalSecondsLabel.Content = mediaElement.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss");
                }

                if (currentSecondLabel.Content.ToString().Equals(totalSecondsLabel.Content.ToString()))
                {
                    if (!totalSecondsLabel.Content.ToString().Equals("00:00:00"))
                    {
                        if (repeatMenuItem.IsChecked)
                        {
                            slider.Value = 0;
                            mediaElement.Position = TimeSpan.FromSeconds(0);
                        }
                        else
                        {
                            fastForwardRectangle_MouseLeftButtonDown(this, null);
                        }
                    }
                }
            }
            catch { }
        }

        private void openMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "All media files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true) {
                mediaElement.Source = new Uri(openFileDialog.FileName);
                setContentLabel(openFileDialog.FileName);
                mediaElement.Play();
                playPauseBrush.Visual = Resources["appbar_control_pause"] as Visual;
            }      
        }

        private void playMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            mediaElement.Play();
            playPauseBrush.Visual = Resources["appbar_control_pause"] as Visual;
            musicPlaying = true;
        }

        private void pauseMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            mediaElement.Pause();
            playPauseBrush.Visual = Resources["appbar_control_play"] as Visual;
            musicPlaying = false;
        }

        private void stopMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            mediaElement.Stop();
            playPauseBrush.Visual = Resources["appbar_control_play"] as Visual;
            musicPlaying = false;
        }

        private void slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            currentSecondLabel.Content = TimeSpan.FromSeconds(slider.Value).ToString(@"hh\:mm\:ss");
        }

        private void slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mediaElement.Position = TimeSpan.FromSeconds(slider.Value);
        }

        private void playPauseRectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try {
                if (musicPlaying)
                {
                    pauseMenuItem_Click(this, e);
                }
                else
                {
                    playMenuItem_Click(this, e);                  
                }
            }
            catch { }
        }

        private void repeatRectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (repeatBrush.Opacity == 0.5)
            {   
                repeatMenuItem.IsChecked = true;
            } 
            else
            {
                repeatMenuItem.IsChecked = false;
            }
        }

        private void repeatMenuItem_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                repeatBrush.Opacity = 1;
            }
            catch { }
        }

        private void repeatMenuItem_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                repeatBrush.Opacity = 0.5;
            }
            catch { }
        }

        private void stopRectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            stopMenuItem_Click(this, e);
        }

        private void mediaElement_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!doubleClickTimer.IsEnabled)
            {
                doubleClickTimer.Start();
            }
            else
            {
                fullScreenMenuItem_Click(this, e);
            }
        }

        public static readonly DependencyProperty ToggleFullScreenProperty = DependencyProperty.Register("ToggleFullScreen", typeof(bool), typeof(PlayerWindow), new PropertyMetadata(default(bool), ToggleFullScreenPropertyChangedCallback));

        private static void ToggleFullScreenPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            MetroWindow metroWindow = (MetroWindow)dependencyObject;
            if (e.OldValue != e.NewValue)
            {
                if ((bool)e.NewValue)
                {
                    metroWindow.UseNoneWindowStyle = true;
                    metroWindow.ShowTitleBar = false;
                    metroWindow.IgnoreTaskbarOnMaximize = true;
                    metroWindow.WindowState = WindowState.Maximized;
                    
                }
                else
                {
                    metroWindow.UseNoneWindowStyle = false;
                    metroWindow.ShowTitleBar = true;
                    metroWindow.IgnoreTaskbarOnMaximize = false;
                    metroWindow.WindowState = WindowState.Normal;
                    
                }
            }
        }

        public bool ToggleFullScreen
        {
            get { return (bool)GetValue(ToggleFullScreenProperty); }
            set { SetValue(ToggleFullScreenProperty, value); }
        }

        private void fullScreenMenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ToggleFullScreen == true)
            {
                hideDockMenuItem.IsChecked = false;
                ToggleFullScreen = false;
                return;
            }
            hideDockMenuItem.IsChecked = true;
            ToggleFullScreen = true;
        }

        private void hideDockMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            statusBar.Visibility = Visibility.Collapsed;
            mediaElement.Margin = new Thickness(0, 0, 0, 0);
        }

        private void hideDockMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            statusBar.Visibility = Visibility.Visible;
            mediaElement.Margin = new Thickness(0, 0, 0, 60);
        }

        private void escapeCommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            hideDockMenuItem.IsChecked = false;
            ToggleFullScreen = false;
        }

        private void grid_Drop(object sender, DragEventArgs e)
        {
            String[] fileName = (String[])e.Data.GetData(DataFormats.FileDrop, true);
            if (fileName.Length > 0)
            {
                mediaElement.Source = new Uri(fileName[0].ToString());
                setContentLabel(fileName[0].ToString());
                mediaElement.Play();
                playPauseBrush.Visual = Resources["appbar_control_pause"] as Visual;
            }
            e.Handled = true;
        }

        public void Play(string filePath)
        {
            try {
                mediaElement.Source = new Uri(filePath);
                setContentLabel(filePath);
                mediaElement.Play();
                playPauseBrush.Visual = Resources["appbar_control_pause"] as Visual;
            }
            catch { }
        }

        private void fastForwardRectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { Play(playlistWindow.GetNextMedia(mediaElement.Source.LocalPath)); }
            catch { }
        }

        private void rewindRectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try { Play(playlistWindow.GetLastMedia(mediaElement.Source.LocalPath)); }
            catch { }
        }

        private void addToPlaylistMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try { playlistWindow.addToPlayList(mediaElement.Source.LocalPath); }
            catch { }
        }

        private void showPlaylistMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            playlistWindow.Show();
            playlistWindow.WindowState = WindowState.Maximized;
        }

        private void showPlaylistMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            playlistWindow.Hide();
        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            playlistWindow.IsCloseable = true;
            playlistWindow.Close();
        }

        private void playlistCommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (showPlaylistMenuItem.IsChecked == true)
            {
                showPlaylistMenuItem.IsChecked = false;
                return;
            }
            showPlaylistMenuItem.IsChecked = true;
        }

        private void setContentLabel(string path)
        {
            titleLabel.Content = Path.GetFileNameWithoutExtension(path);
        }

        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }

        private void mainWindow_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (ToggleFullScreen)
            {
                if (Height - e.GetPosition(this).Y <= 60)
                {
                    hideDockMenuItem_Unchecked(this, e);
                }
                else
                {
                    hideDockMenuItem_Checked(this, e);
                }
            }
        }

        private void soundRectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (soundBrush.Visual == Resources["appbar_sound_0"] as Visual)
            {
                mediaElement.Volume = 0.33;
                soundBrush.Visual = Resources["appbar_sound_1"] as Visual;
            }
            else if (soundBrush.Visual == Resources["appbar_sound_1"] as Visual)
            {
                mediaElement.Volume = 0.66;
                soundBrush.Visual = Resources["appbar_sound_2"] as Visual;
            }
            else if (soundBrush.Visual == Resources["appbar_sound_2"] as Visual)
            {
                mediaElement.Volume = 1;
                soundBrush.Visual = Resources["appbar_sound_3"] as Visual;
            }
            else if (soundBrush.Visual == Resources["appbar_sound_3"] as Visual)
            {
                mediaElement.Volume = 0;
                soundBrush.Visual = Resources["appbar_sound_0"] as Visual;
            }
        }

        private void speedRectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (speedBrush.Opacity == 0.5)
            {
                speedBrush.Opacity = 1;
                speedNumericUpDown.Visibility = Visibility.Visible;
            }
            else
            {
                speedBrush.Opacity = 0.5;
                speedNumericUpDown.Visibility = Visibility.Collapsed;
            }
        }

        private void speedNumericUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            try {
                if (speedNumericUpDown.Value != null)
                {
                    mediaElement.SpeedRatio = (double)speedNumericUpDown.Value;
                }
            }
            catch { }
        }

    }
}
