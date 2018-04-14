using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FortNiteApp
{
    /// <summary>
    /// Interaction logic for StatReport.xaml
    /// </summary>
    public partial class StatReport : Window
    {
        public StatReport()
        {
            InitializeComponent();
        }

        public double[] nums = new double[12];
        /* layout of nums[], related to label grid
        0   1   2   K/d
        3   4   5   kills/game
        6   7   8   winrate
        9  10  11   score/m
        */
        public int[] ints = new int[8];
        /* layout of ints[], at the bottom of the window
         *  0    4  kills
         *  1    5  wins
         *  2    6  matches
         *  3    7  score
         */

        public BrushConverter converter = new BrushConverter();

        public int season;
        public int gamemode;
        public string pIndex;
        public string player1_data;
        public string player2_data;
        public JObject parser;


        public int rank1, rank2;
        public Storyboard sb;
        public MainWindow parent;

        // Method executed when window is shown
        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            sb = (Storyboard)Resources["SlideIn"];
            RefreshAll();
        }

        private void RefreshAll()
        {
            EnableBlur();

            ParseData();
            CalcRating();
            SetValues();
            SetColors();
            SetColors_Bottom();
            SetStatInfo();
            //GlowControl(name1, rank1, 14);
            //GlowControl(name2, rank2, 14);
            ColorControl(rectrank1, rank1);
            ColorControl(rectrank2, rank2);

            sb.Begin();
        }

        private void CalcRating()
        {
            double rating;

            rating = (nums[0] * 100) / 2;
            rating += (nums[3] * 100) / 2;
            rating += (nums[9] * 1000) / 2 / 4 / 128;
            if (gamemode == 2)
                rating += (rating * (nums[6] / 10) * 0.86);
            else
                rating = (rating * (nums[6] / 10));

            rank1 = Convert.ToInt32(rating);

            rating = (nums[2] * 100) / 2;
            rating += (nums[5] * 100) / 2;
            rating += (nums[11] * 1000) / 2 / 4 / 128;
            if (gamemode == 2)
                rating += (rating * (nums[8] / 10) * 0.86);
            else
                rating = (rating * (nums[8] / 10));

            rank2 = Convert.ToInt32(rating);
        }

        private void ParseData()
        {
            pIndex = "";
            switch (season)
            {
                case 0:
                    break;
                case 1:
                    pIndex = "curr_";
                    break;
            }
            switch (gamemode)
            {
                case 0:
                    pIndex = pIndex + "p9";
                    break;
                case 1:
                    pIndex = pIndex + "p10";
                    break;
                case 2:
                    pIndex = pIndex + "p2";
                    break;
            }

            //player one stats
            parser = JObject.Parse(player1_data);

            nums[0] = (double)parser[pIndex][9]["value"];     //kd
            nums[3] = (double)parser[pIndex][13]["value"];    //kpg
            nums[6] = (double)parser[pIndex][10]["value"];    //winrate
            nums[9] = (double)parser[pIndex][14]["value"];    //score/min
            ints[0] = (int)parser[pIndex][12]["value"];       //kills
            ints[1] = (int)parser[pIndex][2]["value"];        //wins
            ints[2] = (int)parser[pIndex][11]["value"];       //macthes
            ints[3] = (int)parser[pIndex][1]["value"];        //score

            parser = JObject.Parse(player2_data);

            nums[2] = (double)parser[pIndex][9]["value"];
            nums[5] = (double)parser[pIndex][13]["value"];
            nums[8] = (double)parser[pIndex][10]["value"];
            nums[11] = (double)parser[pIndex][14]["value"];
            ints[4] = (int)parser[pIndex][12]["value"];
            ints[5] = (int)parser[pIndex][2]["value"];
            ints[6] = (int)parser[pIndex][11]["value"];
            ints[7] = (int)parser[pIndex][1]["value"];
        }

        private void SetValues()
        {


            text_kd_one.Text = nums[0].ToString();
            text_kd_two.Text = nums[2].ToString();

            text_kg_one.Text = nums[3].ToString();
            text_kg_two.Text = nums[5].ToString();

            text_wr_one.Text = nums[6].ToString() + "%";
            text_wr_two.Text = nums[8].ToString() + "%";

            text_sg_one.Text = nums[9].ToString();
            text_sg_two.Text = nums[11].ToString();

            text_kd_diff.Text = (nums[0] - nums[2]).ToString("n2");
            text_kg_diff.Text = (nums[3] - nums[5]).ToString("n2");
            text_wr_diff.Text = (nums[6] - nums[8]).ToString("n2");
            text_sg_diff.Text = (nums[9] - nums[11]).ToString("n2");

            rating1.Text = rank1.ToString();
            rating2.Text = rank2.ToString();

            int i = 0;
            foreach (TextBlock t in BottomGrid.Children)
            {
                if(t.Uid == "")
                {
                    t.Text = ints[i].ToString();
                    i++;
                }
            }

        }

        private void SetColors()
        {

            if (nums[0] < nums[2])
            {
                text_kd_two.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
                text_kd_one.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
            }
            else
            {
                text_kd_one.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
                text_kd_diff.Text = "+" + text_kd_diff.Text;
                text_kd_diff.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
                text_kd_two.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
            }

            if (nums[3] < nums[5])
            {
                text_kg_two.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
                text_kg_one.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
            }
            else
            {
                text_kg_one.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
                text_kg_diff.Text = "+" + text_kg_diff.Text;
                text_kg_diff.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
                text_kg_two.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
            }

            if (nums[6] < nums[8])
            {
                text_wr_two.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
                text_wr_one.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
            }
            else
            {
                text_wr_one.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
                text_wr_diff.Text = "+" + text_wr_diff.Text;
                text_wr_diff.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
                text_wr_two.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
            }

            if (nums[9] < nums[11])
            {
                text_sg_two.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
                text_sg_one.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
            }
            else
            {
                text_sg_one.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
                text_sg_diff.Text = "+" + text_sg_diff.Text;
                text_sg_diff.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
                text_sg_two.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
            }
        }

        private void SetColors_Bottom()
        {
            if (ints[0] < ints[4])
            {
                display_kills_Copy.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
                display_kills.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
            }
            else
            {
                display_kills.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
                display_kills_Copy.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
            }

            if (ints[1] < ints[5])
            {
                display_wins.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
                display_wins_Copy.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
            }
            else
            {
                display_wins_Copy.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
                display_wins.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
            }

            if (ints[2] < ints[6])
            {
                display_matches.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
                display_matches_Copy.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
            }
            else
            {
                display_matches_Copy.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
                display_matches.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
            }

            if (ints[3] < ints[7])
            {
                display_score.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
                display_score_Copy.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
            }
            else
            {
                display_score_Copy.Foreground = (Brush)converter.ConvertFromString("#FFFFFFFF");
                display_score.Foreground = (Brush)converter.ConvertFromString("#FF89BEFF");
            }
        }

        private void SetStatInfo()
        {
            if(season == 1)
            {
                select1.Fill = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
                select2.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
            if(season == 0)
            {
                select1.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                select2.Fill = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
            }

            if (gamemode == 0)
            {
                select3.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                select4.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                select5.Fill = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
            }
            if (gamemode == 1)
            {
                select3.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                select4.Fill = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
                select5.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
            if (gamemode == 2)
            {
                select3.Fill = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
                select4.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                select5.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
        }

        public void GlowControl(TextBlock inputTextBox, int inputRating, int radius)
        {
            if (inputRating < 1000)
            {
                UIElement uie = new UIElement
                {
                    Effect = new DropShadowEffect
                    {
                        Color = new Color { A = 255, R = 200, G = 200, B = 200 },
                        ShadowDepth = 0,
                        BlurRadius = radius,
                        Opacity = 0
                    }
                };

                inputTextBox.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                inputTextBox.Effect = uie.Effect;
            }
            else if (inputRating < 1500)
            {
                // Green
                UIElement uie = new UIElement
                {
                    Effect = new DropShadowEffect
                    {
                        Color = new Color { A = 255, R = 93, G = 255, B = 56 },
                        ShadowDepth = 0,
                        BlurRadius = radius,
                        Opacity = 1
                    }
                };

                inputTextBox.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                inputTextBox.Effect = uie.Effect;
            }
            else if (inputRating < 2000)
            {
                // Blue
                UIElement uie = new UIElement
                {
                    Effect = new DropShadowEffect
                    {
                        Color = new Color { A = 255, R = 38, G = 198, B = 255 },
                        ShadowDepth = 0,
                        BlurRadius = radius,
                        Opacity = 1
                    }
                };
                inputTextBox.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                inputTextBox.Effect = uie.Effect;
            }
            else if (inputRating < 2500)
            {
                //Purple
                UIElement uie = new UIElement
                {
                    Effect = new DropShadowEffect
                    {
                        Color = new Color { A = 255, R = 232, G = 158, B = 255 },
                        ShadowDepth = 0,
                        BlurRadius = radius,
                        Opacity = 1
                    }
                };

                inputTextBox.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                inputTextBox.Effect = uie.Effect;
            }
            else if (inputRating < 3000)
            {
                //yellow
                UIElement uie = new UIElement
                {
                    Effect = new DropShadowEffect
                    {
                        Color = new Color { A = 255, R = 255, G = 235, B = 50 },
                        ShadowDepth = 0,
                        BlurRadius = radius,
                        Opacity = 1
                    }
                };

                inputTextBox.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                inputTextBox.Effect = uie.Effect;
            }
            else if (inputRating >= 3000)
            {
                //gold
                UIElement uie = new UIElement
                {
                    Effect = new DropShadowEffect
                    {
                        Color = new Color { A = 255, R = 255, G = 235, B = 50 },
                        ShadowDepth = 0,
                        BlurRadius = radius,
                        Opacity = 1
                    }
                };
                inputTextBox.Foreground = new SolidColorBrush(Color.FromRgb(255, 245, 210));
                inputTextBox.Effect = uie.Effect;
            }
        }

        public void ColorControl(Rectangle rect, int inputRating)
        {
            if (inputRating < 1000)
            {
            }
            else if (inputRating < 1500)
            {
                rect.Fill = (Brush)converter.ConvertFromString("#36BB43");
            }
            else if (inputRating < 2000)
            {
                rect.Fill = (Brush)converter.ConvertFromString("#2582E0");
            }
            else if (inputRating < 2500)
            {
                rect.Fill = (Brush)converter.ConvertFromString("#C725E0");
            }
            else if (inputRating < 3000)
            {
                rect.Fill = (Brush)converter.ConvertFromString("#EEB800");
            }
            else if (inputRating >= 3000)
            {
                rect.Fill = (Brush)converter.ConvertFromString("#EEB800");
            }
        }

        private void ButtonClose_MouseEnter(object sender, MouseEventArgs e)
        {
            ButtonClose.Fill = new SolidColorBrush(Color.FromRgb(236, 80, 80));
        }

        private void ButtonClose_MouseLeave(object sender, MouseEventArgs e)
        {
            ButtonClose.Fill = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        }

        private void ButtonClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        #region Glass Stuff
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }

        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }
        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            var accentStructSize = Marshal.SizeOf(accent);
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }
        #endregion

        private void StatWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach(StatReport s in parent.reportWindow)
            {
                if (s.Uid == this.Uid)
                {
                    parent.reportWindow.Remove(s);
                    break;
                }
            }

        }

        private void BoxHoverHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            //Rectangle r = (Rectangle)sender;

            //if (r.IsMouseOver)
            //    r.Fill = (Brush)converter.ConvertFromString("#59919191");
            //else
            //    r.Fill = (Brush)converter.ConvertFromString("#59000000");
        }

        private void LeftSelects(object sender, DependencyPropertyChangedEventArgs e)
        {
            Rectangle r = (Rectangle)sender;

            if (r.IsMouseOver)
                r.Fill = new SolidColorBrush(Color.FromArgb(102, 150, 150, 150));
            else
                if(season.ToString() == r.Uid)
                    r.Fill = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
                else
                    r.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

        }

        private void RightSelects(object sender, DependencyPropertyChangedEventArgs e)
        {
            Rectangle r = (Rectangle)sender;

            if (r.IsMouseOver)
                r.Fill = new SolidColorBrush(Color.FromArgb(102, 150, 150, 150));
            else
                if (gamemode.ToString() == r.Uid)
                    r.Fill = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
                else
                    r.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

        private void LeftClick(object sender, MouseButtonEventArgs e)
        {
            Rectangle r = (Rectangle)sender;
            season = Convert.ToInt16(r.Uid);
            RefreshAll();
        }

        private void RightClicks(object sender, MouseButtonEventArgs e)
        {
            Rectangle r = (Rectangle)sender;
            gamemode = Convert.ToInt16(r.Uid);
            RefreshAll();
        }

        private void StatWin_Loaded(object sender, RoutedEventArgs e)
        {
             
        }
    }
}
