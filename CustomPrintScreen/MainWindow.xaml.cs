﻿using System.Windows;
using Forms = System.Windows.Forms;
using System.Windows.Controls;
using System;
using System.Windows.Interop;
using System.Windows.Media;

namespace CustomPrintScreen
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Determines if the printscreen is simple or advanced.
        /// If is simple, than user click the screen and that's all.
        /// If is advanced than user can crop or save every image of prt sc
        /// </summary>

        public MainWindow()
        {
            InitializeComponent();
            //this.SourceInitialized += Window1_SourceInitialized;
        }

        /// <summary>
        /// Function draws the application basing on the screens
        /// </summary>
        public void DrawApplication()
        {
            Handler.ShotTime = DateTime.Now.Year.ToString() + DateTime.Now.Month + DateTime.Now.Day + DateTime.Now.Hour + DateTime.Now.Minute;
            int ScreensAmount = Forms.Screen.AllScreens.Length;

            // if user has one screen
            if (ScreensAmount == 1)
            {
                // in advanced mode user will be able to crop and save
                if (Handler.AdvancedMode)
                {
                    OpenCropWindow(0);
                }
                // other way the screen will be simply saved
                else
                {
                    Handler.SaveScreen(0);
                    return;
                }
            }
            else
            {
                for (int i = 0; i < ScreensAmount; i++)
                {
                    // dump will be used for onclick events in buttons
                    int dump = i;

                    if (Handler.AdvancedMode)
                    {
                        // Structure
                        // Every screenshot will be saved as grid's background and to the grid
                        // will be added a stackpanel which will hold two buttons for cropping
                        // and saving

                        #region Create grid

                        double ratio = Handler.Bitmaps[i].Height * 1.0d / Handler.Bitmaps[i].Width * 1.0d;

                        Grid grid = new Grid()
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            Width = SystemParameters.PrimaryScreenWidth * 0.9d / ScreensAmount*1.0d,
                            Margin = new Thickness(8, 0, 0, 0)
                        };

                        grid.Height = grid.Width * ratio;
                        grid.Background = new ImageBrush(Handler.BitmapToImageSource(Handler.Bitmaps[i]));
                        imgs.Children.Add(grid);

                        #endregion

                        #region Create a StackPanel

                        StackPanel sp = new StackPanel()
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Orientation = Orientation.Horizontal
                            
                        };

                        grid.Children.Add(sp);
                        #endregion

                        #region Create buttons for cropping and saving and add them to StackPanel

                        Button[] imgbuttons = new Button[2];
                        string[] imgspaths = new string[] { "crop.png", "save.png" };

                        for (int j = 0; j < 2; j++)
                        {
                            imgbuttons[j] = new Button()
                            {
                                Width = 100,
                                Height = 100,
                                Background = new SolidColorBrush(Color.FromArgb(130, 255, 255, 255)),
                                Margin = new Thickness(4, 0, 0, 0),
                                BorderThickness = new Thickness(4, 4, 4, 4),
                                Content = new Image()
                                {
                                    Source = Handler.LoadImage(imgspaths[j])
                                }
                            };

                            sp.Children.Add(imgbuttons[j]);
                        }

                        imgbuttons[0].Click += (sender, e) => { OpenCropWindow(dump); };

                        imgbuttons[1].Click += (sender, e) =>
                        {
                            Handler.SaveScreen(dump);
                        };

                        #endregion
                    }
                    else
                    {
                        // Stucture
                        // Every screenshot will be a buttons's content.
                        // By pressing the button you save the image

                        Button btn = new Button()
                        {
                            Margin = new Thickness(8, 0, 0, 0),
                            BorderThickness = new Thickness(4, 4, 4, 4),
                            Content = new Image()
                            {
                                Width = SystemParameters.PrimaryScreenWidth * 0.9f / ScreensAmount,
                                Source = Handler.BitmapToImageSource(Handler.Bitmaps[i])
                            }
                        };
                        btn.Click += (sender, e) =>
                        {
                            Handler.SaveScreen(dump);
                            Hide();
                        };

                        imgs.Children.Add(btn);
                    }
                }

                Application.Current.MainWindow.Show();
            }
        }

        void OpenCropWindow(int id)
        {
            CropWindow cropwd = new CropWindow();
            cropwd.Id = id;
            cropwd.Show();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            //If the Height has changed then calc half of the the offset to move the form
            if (sizeInfo.HeightChanged == true)
            {
                this.Top += (sizeInfo.PreviousSize.Height - sizeInfo.NewSize.Height) / 2;
            }

            //If the Width has changed then calc half of the the offset to move the form
            if (sizeInfo.WidthChanged == true)
            {
                this.Left += (sizeInfo.PreviousSize.Width - sizeInfo.NewSize.Width) / 2;
            }
        }

        /*private void Window1_SourceInitialized(object sender, EventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MOVE = 0xF010;

            switch (msg)
            {
                case WM_SYSCOMMAND:
                    int command = wParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE)
                    {
                        handled = true;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }
        */
    }
}
