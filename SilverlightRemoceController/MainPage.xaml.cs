using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Threading;

using Microsoft.Silverlight.Windows.Platform;
using Microsoft.Silverlight.Windows.Taskbar;

using Stream = System.IO.Stream;

namespace SilverlightRemoteController
{
    public partial class MainPage : UserControl
    {
        DispatcherTimer timer = new DispatcherTimer();
        private int currentIndex;
        IList<FileInfo> files = new List<FileInfo>();
        List<ThumbbarButton> taskbarButtons = new List<ThumbbarButton>(4);
        private bool paused;

        public MainPage()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += TimerTick;

            CreateTaskbarButtons();

            WindowMessageInterceptor.Current.WindowMessage += WindowMessage;
            WindowMessageInterceptor.Current.CommandMessage += CommandMessage;
            WindowMessageInterceptor.Current.AddMessageIntercept(NativeMethods.WM_APPCOMMAND);
            WindowMessageInterceptor.Current.AddCommandMessageIntercept(NativeMethods.THBN_CLICKED);
        }

        private void CreateTaskbarButtons()
        {
            for (int i = 0; i < 4; i++)
            {
                var thumbbarButton = TaskbarButton.Current.CreateThumbbarButton((uint)(i + 1));
                thumbbarButton.Flags = THUMBBUTTONFLAGS.THBF_ENABLED;

                thumbbarButton.ImageDataType = ButtonImageDataType.PNG;
                StreamResourceInfo resourceInfo = Application.GetResourceStream(new Uri(string.Format("images/{0}.png", i), UriKind.Relative));
                using (var br = new BinaryReader(resourceInfo.Stream))
                {
                    thumbbarButton.Image = br.ReadBytes((int)resourceInfo.Stream.Length);
                }

                taskbarButtons.Add(thumbbarButton);
            }

            taskbarButtons[0].Tooltip = "First Image";
            taskbarButtons[1].Tooltip = "Previous Image";
            taskbarButtons[2].Tooltip = "Next Image";
            taskbarButtons[3].Tooltip = "Last Image";
            TaskbarButton.Current.ShowThumbbarButtons();
        }

        private void CommandMessage(object sender, CommandMessageEventArgs e)
        {
            if (e.NotifyCode != NativeMethods.THBN_CLICKED)
            {
                return;
            }

            switch (e.ControlID)
            {
                case 1:
                    FirstImage();
                    break;
                case 2:
                    PreviousImage();
                    break;
                case 3:
                    NextImage();
                    break;
                case 4:
                    LastImage();
                    break;
            }
        }

        private void WindowMessage(object sender, WindowMessageEventArgs e)
        {
            if (e.Message == NativeMethods.WM_APPCOMMAND)
            {
                int cmd = NativeMethods.GET_APPCOMMAND_LPARAM(e.lParam);
                switch (cmd)
                {
                    case NativeMethods.APPCOMMAND_MEDIA_PLAY:
                        ResumeSlideShow();
                        break;
                    case NativeMethods.APPCOMMAND_MEDIA_PAUSE:
                        PauseSlideShow();
                        break;
                    case NativeMethods.APPCOMMAND_MEDIA_PLAY_PAUSE:
                        PlayPauseSlideShow();
                        break;
                }
            }
        }

        private void SwitchImage()
        {
            currentIndex = (currentIndex + 1) % files.Count;

            FadeOutAnimation.Begin();
        }

        private void SetImageSource(int index)
        {
            using (Stream stream = files[index].OpenRead())
            {
                var bi = new BitmapImage();
                bi.SetSource(stream);
                theImage.Source = bi;
            }
            TaskbarButton.Current.SetProgressValue((ulong)index + 1, (ulong)files.Count);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            SwitchImage();
        }

        private void FadeOutCompleted(object sender, EventArgs e)
        {
            SetImageSource(currentIndex);
            FadeInAnimation.Begin();
        }

        private void BrowseButtonClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Images |*.jpg;*.jpeg;*.bmp;*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                files = openFileDialog.Files.ToList();
                SetImageSource(0);
                currentIndex = 0;
                timer.Start();
            }
        }


        private void PauseButtonClick(object sender, RoutedEventArgs e)
        {
            PauseSlideShow();
        }

        private void ResumeButtonClick(object sender, RoutedEventArgs e)
        {
            ResumeSlideShow();
        }

        private void PreviousButtonClick(object sender, RoutedEventArgs e)
        {
            PreviousImage();
        }

        private void NextButtonClick(object sender, RoutedEventArgs e)
        {
            NextImage();
        }


        private void SlideShowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                PreviousImage();
            }

            if (e.Key == Key.Right)
            {
                NextImage();
            }
        }


        private void PauseSlideShow()
        {
            pauseButton.IsEnabled = false;
            resumeButton.IsEnabled = true;

            timer.Stop();
            paused = true;

            TaskbarButton.Current.SetProgressState(TaskbarItemProgressState.Paused);
        }

        private void ResumeSlideShow()
        {
            pauseButton.IsEnabled = true;
            resumeButton.IsEnabled = false;

            SwitchImage();
            timer.Start();
            paused = false;

            TaskbarButton.Current.SetProgressState(TaskbarItemProgressState.Normal);
        }

        private void PlayPauseSlideShow()
        {
            if (paused)
            {
                ResumeSlideShow();
            }
            else
            {
                PauseSlideShow();
            }
        }

        private void PreviousImage()
        {
            currentIndex = currentIndex == 0 ? files.Count - 2 : currentIndex - 2;

            SwitchImage();

            if (!paused)
            {
                timer.Restart();
            }
        }

        private void NextImage()
        {
            SwitchImage();

            if (!paused)
            {
                timer.Restart();
            }
        }

        private void FirstImage()
        {
            currentIndex = -1;
            SwitchImage();
            timer.Restart();
        }

        private void LastImage()
        {
            currentIndex = files.Count - 2;
            SwitchImage();
            timer.Restart();
        }
    }

    static class TimerEx
    {
        public static void Restart(this DispatcherTimer timer)
        {
            timer.Stop();
            timer.Start();
        }
    }
}
