using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace FortNiteApp
{

    //swithc between obsolete JSON parsing:
    // using command at top.
    // Next() Method
    // GetValues() method

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int indx;
        public int e_indx;
        public int n_indx;
        public int boostcount = 0;
        public bool boost;
        public bool newStats = true;

        public const int GlowRaduis = 14;

        public string playerData;

        // Strings
        #region strings
        public const string kd = "K/d";
        //public const string disVal = "\"value\":\"";  //obsolete
        public string substr;
        public string dlString = "";
        public bool cleared = false;

        public string statsType = "Squad";
        //public string pIndex = "\"p9\""; // obsolete
        public string pIndex = "curr_p9";
        public int modeint = 0;
        public string name;
        #endregion

        // ints
        #region ints

        public double sq_score;
        public int sq_rating;
        public int trn_rating;
        public double ratingadd;
        public int sq_wins;
        public int sq_top3;
        public int sq_top5;
        public int sq_top6;
        public int sq_top10;
        public int sq_top12;
        public int sq_top25;
        public double sq_kd;
        public double sq_wr;
        public int sq_matches;
        public int sq_kills;
        public double sq_time;
        public double sq_KPMin;
        public double sq_KPG;

        int[] inc = new int[9];
        int[] total = new int[9];

        //int total_rate;
        int count = 0;

        #endregion 

        public System.Timers.Timer timer = new System.Timers.Timer();
        public System.Timers.Timer timerAni = new System.Timers.Timer();
        public int timercount = 0;

        public Storyboard sb, switchOn, switchOff;


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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWin_Loaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();
            button_tyty.Click += ButtonHandler;
            button_jojo.Click += ButtonHandler;
            button_bk.Click += ButtonHandler;
            button_ster.Click += ButtonHandler;
            button_mike.Click += ButtonHandler;

            button_tyty.MouseRightButtonDown += RightButtonHandler;
            button_jojo.MouseRightButtonDown += RightButtonHandler;
            button_bk.MouseRightButtonDown += RightButtonHandler;
            button_ster.MouseRightButtonDown += RightButtonHandler;
            button_mike.MouseRightButtonDown += RightButtonHandler;
            button_ex.MouseRightButtonDown += RightButtonHandler;


            timerAni.Elapsed += TimerAniTick;
            timerAni.Interval = 18;
            timerAni.Enabled = false;

            timer.Elapsed += TimerTick;
            timer.Interval = 1000;
            timer.Enabled = false;

            sb = this.FindResource("Light") as Storyboard;
            switchOff = this.FindResource("SwitchToOff") as Storyboard;
            switchOn = this.FindResource("SwitchToOn") as Storyboard;
        }

        private void TimerTick(Object source, System.Timers.ElapsedEventArgs e)
        {
            timercount++;
            if (timercount > 300)
            {
                this.Dispatcher.Invoke(() => { FindName(); textblock_time.Text = ""; });
                timercount = 0;
            }
            if (timercount > 29)
                this.Dispatcher.Invoke(() => { textblock_time.Text = "Auto Refresh in " + (300 - timercount) + " Seconds"; });
        }


        public void ZeroOut()
        {
            sq_rating = sq_wins = sq_top3 = sq_top5 = sq_top6 = sq_top10 = sq_top12 = sq_top25 = sq_matches = sq_kills = 0;
            sq_KPMin = sq_KPG = sq_score = sq_wr = sq_kd = sq_time = 0.0;
            CalcRating();
            SetValues();
            GlowControl(sq_rating);
            text_rating.Text = "0";
            trn_rating_text.Text = "TRN Rating: 0";
            text_rank.Text = "RANK";
            textblock_name.Text = "";
            text_rank.Foreground = new SolidColorBrush(Color.FromRgb(155, 155, 155));
            text_rating_glow.Visibility = Visibility.Hidden;
            text_rating.Visibility = Visibility.Visible;
        }

        //obsolete Next()
        //public void Next()
        //{
        //    try
        //    {
        //        if ((dlString.IndexOf("\"p10\"", indx, (dlString.IndexOf(disVal, indx) - indx)) != -1) ||
        //            (dlString.IndexOf("\"p9\"", indx, (dlString.IndexOf(disVal, indx) - indx)) != -1))
        //            throw new ArgumentOutOfRangeException();
        //
        //        indx = dlString.IndexOf(disVal, indx) + disVal.Length;
        //        e_indx = dlString.IndexOf("\"", indx);
        //        substr = dlString.Substring(indx, e_indx - indx);
        //    }
        //    catch (ArgumentOutOfRangeException e)
        //    {
        //        textBlock1.Text = "Player hasn't played enough games: " + inputName.Text;
        //        Console.WriteLine("caught {0}", e);
        //        ZeroOut();
        //    }
        //
        //}
        

        public void getValues()
        {
            try
            {
                // Parse json (requires .dll)
                var obj = JObject.Parse(playerData);
                
                trn_rating = (int)obj[pIndex][0]["value"];
                //sq_score = (int)obj[pIndex][1]["value"];
                sq_wins = (int)obj[pIndex][2]["value"];
                sq_top3 = (int)obj[pIndex][3]["value"];
                sq_top5 = (int)obj[pIndex][4]["value"];
                sq_top6 = (int)obj[pIndex][5]["value"];
                sq_top10 = (int)obj[pIndex][6]["value"];
                sq_top12 = (int)obj[pIndex][7]["value"];
                sq_top25 = (int)obj[pIndex][8]["value"];
                sq_kd = (double)obj[pIndex][9]["value"];
                sq_wr = (double)obj[pIndex][10]["value"];
                sq_matches = (int)obj[pIndex][11]["value"];
                sq_kills = (int)obj[pIndex][12]["value"];
                sq_KPG = (double)obj[pIndex][13]["value"];
                sq_time = (double)obj[pIndex][14]["value"];
                sq_KPMin = Math.Truncate(((sq_KPG / sq_time) * 60) * 100) / 100;
                //sq_KPMin = (double)obj[pIndex][14]["value"];
                sq_score = (double)obj[pIndex][15]["value"];
                

                // obsolete
                //Next();
                //sq_score = Convert.ToInt32(substr);
                //Next();
                //sq_wins = Convert.ToInt32(substr);
                //Next();
                //sq_top3 = Convert.ToInt32(substr);
                //Next();
                //sq_top5 = Convert.ToInt32(substr);
                //Next();
                //sq_top6 = Convert.ToInt32(substr);
                //Next();
                //sq_top10 = Convert.ToInt32(substr);
                //Next();
                //sq_top12 = Convert.ToInt32(substr);
                //Next();
                //sq_top25 = Convert.ToInt32(substr);
                //Next();
                //sq_kd = Convert.ToDouble(substr);
                //Next();
                //sq_wr = Convert.ToDouble(substr);
                //Next();
                //sq_matches = Convert.ToInt32(substr);
                //Next();
                //sq_kills = Convert.ToInt32(substr);
                //Next();
                //sq_time = Convert.ToInt32(substr);
                //Next();
                //sq_KPMin = Convert.ToDouble(substr);
                //Next();
                //sq_KPG = Convert.ToDouble(substr);
            }
            catch
            {
                textBlock1.Text = "Not Enough Games Played, Stats Inaccurate";
                ZeroOut();
            }
        }

        #region Glow Controls
        public void ShadowControl()
        {
            if (sq_rating > 2000)
            {
                text_rating.Visibility = Visibility.Hidden;
                text_rating_glow.Text = text_rating.Text;
                text_rating_glow.Visibility = Visibility.Visible;
            }
            else
            {
                text_rating.Visibility = Visibility.Visible;
                text_rating_glow.Visibility = Visibility.Hidden;
            }
        }

        public void ColorControl()
        {
            if (sq_rating < 1000)
            {
                text_rating.Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200));
            }
            else if (sq_rating < 1500)
            {
                text_rating.Foreground = new SolidColorBrush(Color.FromRgb(93, 255, 56));
            }
            else if (sq_rating < 2000)
            {
                text_rating.Foreground = new SolidColorBrush(Color.FromRgb(38, 198, 255));
            }
            else if (sq_rating < 2500)
            {
                text_rating.Foreground = new SolidColorBrush(Color.FromRgb(197, 35, 252));
            }
            else if (sq_rating < 3000)
            {
                text_rating.Foreground = new SolidColorBrush(Color.FromRgb(255, 182, 28));
            }
            else if (sq_rating < 3500)
            {
                text_rating.Foreground = new SolidColorBrush(Color.FromRgb(255, 231, 28));
            }

        }

        public void GlowControl(int sq_rating)
        {
            if (sq_rating < 1000)
            {
                UIElement uie = new UIElement
                {
                    Effect = new DropShadowEffect
                    {
                        Color = new Color { A = 255, R = 200, G = 200, B = 200 },
                        ShadowDepth = 0,
                        BlurRadius = GlowRaduis,
                        Opacity = 0
                    }
                };
                text_rank.Text = "PEON";
                text_rank.Foreground = new SolidColorBrush(Color.FromRgb(155, 155, 155));
                text_rank.Visibility = Visibility.Visible;
                text_rank_glow.Visibility = Visibility.Hidden;
                text_rating.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                text_rating.Effect = uie.Effect;
            }
            else if (sq_rating < 1500)
            {
                // Green
                UIElement uie = new UIElement
                {
                    Effect = new DropShadowEffect
                    {
                        Color = new Color { A = 255, R = 93, G = 255, B = 56 },
                        ShadowDepth = 0,
                        BlurRadius = GlowRaduis,
                        Opacity = 1
                    }
                };
                text_rank.Text = "UNCOMMON";
                text_rank.Foreground = new SolidColorBrush(Color.FromRgb(93, 255, 56));
                text_rank.Visibility = Visibility.Visible;
                text_rank_glow.Visibility = Visibility.Hidden;
                text_rating.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                text_rating.Effect = uie.Effect;
            }
            else if (sq_rating < 2000)
            {
                // Blue
                UIElement uie = new UIElement
                {
                    Effect = new DropShadowEffect
                    {
                        Color = new Color { A = 255, R = 38, G = 198, B = 255 },
                        ShadowDepth = 0,
                        BlurRadius = GlowRaduis,
                        Opacity = 1
                    }
                };
                text_rank.Text = "RARE";
                text_rank.Foreground = new SolidColorBrush(Color.FromRgb(38, 198, 255));
                text_rank.Visibility = Visibility.Visible;
                text_rank_glow.Visibility = Visibility.Hidden;
                text_rating.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                text_rating.Effect = uie.Effect;
            }
            else if (sq_rating < 2500)
            {
                //Purple
                UIElement uie = new UIElement
                {
                    Effect = new DropShadowEffect
                    {
                        Color = new Color { A = 255, R = 232, G = 158, B = 255 },
                        ShadowDepth = 0,
                        BlurRadius = GlowRaduis,
                        Opacity = 1
                    }
                };
                text_rank.Text = "EPIC";
                text_rank.Foreground = new SolidColorBrush(Color.FromRgb(232, 158, 255));
                text_rank.Visibility = Visibility.Visible;
                text_rank_glow.Visibility = Visibility.Hidden;
                text_rating.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                text_rating.Effect = uie.Effect;
            }
            else if (sq_rating < 3000)
            {
                //yellow
                UIElement uie = new UIElement
                {
                    Effect = new DropShadowEffect
                    {
                        Color = new Color { A = 255, R = 255, G = 235, B = 50 },
                        ShadowDepth = 0,
                        BlurRadius = GlowRaduis,
                        Opacity = 1
                    }
                };
                text_rank.Text = "LEGENDARY";
                text_rank.Foreground = new SolidColorBrush(Color.FromRgb(255, 235, 50));
                text_rank.Visibility = Visibility.Visible;
                text_rank_glow.Visibility = Visibility.Hidden;
                text_rating.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                text_rating.Effect = uie.Effect;
            }
            else if (sq_rating >= 3000)
            {
                //gold
                UIElement uie = new UIElement
                {
                    Effect = new DropShadowEffect
                    {
                        Color = new Color { A = 255, R = 255, G = 235, B = 50 },
                        ShadowDepth = 0,
                        BlurRadius = GlowRaduis,
                        Opacity = 1
                    }
                };
                text_rank.Visibility = Visibility.Hidden;
                text_rank_glow.Visibility = Visibility.Visible;
                text_rating.Foreground = new SolidColorBrush(Color.FromRgb(255, 245, 210));
                text_rating.Effect = uie.Effect;
            }
        }

        #endregion

        public void SetValues()
        {
            text_w.Text = sq_wins.ToString();
            text_kd.Text = sq_kd.ToString();
            text_wr.Text = sq_wr.ToString() + "%";
            text_m.Text = sq_matches.ToString();
            text_k.Text = sq_kills.ToString();
            text_km.Text = sq_KPMin.ToString();
            text_kg.Text = sq_KPG.ToString();
            text_s.Text = sq_score.ToString();
            textblock_name.Text = name;
            trn_rating_text.Text = "TRN Rating: " + trn_rating.ToString();
        }

        public void CalcRating()
        {
            ratingadd = (sq_kd * 100) / 2;
            ratingadd += (sq_KPG * 100) / 2;
            ratingadd += (sq_KPMin * 1000) / 2 / 4;
            if (modeint == 2)
                ratingadd += (ratingadd * (sq_wr / 10) * 0.86);
            else
                ratingadd = (ratingadd * (sq_wr / 10));


            sq_rating = Convert.ToInt32(ratingadd);

            if (boost)
            {
                if (inputName.Text == "puttzs")
                    sq_rating += 500;
                if (inputName.Text == "honkin")
                    sq_rating += 280;
            }



            //text_rating.Text = sq_rating.ToString();
            if (sq_rating != 0 && sq_rating != total[0])
                RatingAnimation();
            else
                text_rating.Text = sq_rating.ToString();


        }

        public void RatingAnimation()
        {
            inc[0] = (sq_rating - total[0]) / 50;
            count = 0;
            text_rating.Text = total[0].ToString();
            timerAni.Start();
        }

        public void TimerAniTick(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (count < 50)
            {
                total[0] += inc[0];
                this.Dispatcher.Invoke(() => {
                    text_rating.Text = total[0].ToString();
                    GlowControl(total[0]);
                });
                count++;
            }
            else
            {
                this.Dispatcher.Invoke(() => { text_rating.Text = sq_rating.ToString(); sb.Begin(); GlowControl(sq_rating); });
                count = 0;
                timerAni.Stop();
                total[0] = sq_rating;
            }

        }

        public void FindName()
        {
            Thread getThread = new Thread(() =>
            {
                string uri = "";
                WebClient client = new WebClient();
                //client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(GetMode);
                this.Dispatcher.Invoke(() => { uri = "https://fortnitetracker.com/profile/pc/" + inputName.Text; });
                try
                {
                    this.Dispatcher.Invoke(() => { textBlock1.Text = "Retrieving Stats.."; });
                    dlString = client.DownloadString(uri);
                    this.Dispatcher.Invoke(() => { GetMode(); });
                    
                }
                catch
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        textBlock1.Text = "No Connection";
                        ZeroOut();
                    });
                    return;
                }
                
            });
            getThread.Start();

            //GetMode();
        }
        public void GetMode()
        {
            if (dlString.IndexOf("Issue while trying to get your stats") != -1)
            {
                textBlock1.Text = statsType + " Stats Not Found for " + inputName.Text;
                ZeroOut();
            }
            else
            {
                try
                {
                    // what a fucking statement
                    // getting json from web source, starting at "playerData", to ";</script>"
                    playerData = dlString.Substring((dlString.IndexOf("var playerData = ") + 17),
                        (dlString.IndexOf(";</script>", dlString.IndexOf("var playerData = "))) - (dlString.IndexOf("var playerData = ") + 17));

                    name = dlString.Substring((dlString.IndexOf("\"View ") + 6), inputName.Text.Length);
                    textBlock1.Text = name + " " + statsType + " Stats";
                    indx = dlString.IndexOf("var playerData = ");
                    indx = dlString.IndexOf(pIndex, indx) + 6;
                    getValues();
                    SetValues();
                    CalcRating();
                    //text_rating.Text = sq_rating.ToString();
                    //ShadowControl();
                    //ColorControl();
                    //GlowControl();
                    timer.Enabled = true;

                }
                catch
                {
                    textBlock1.Text = "Error Getting Stats, Not Available";
                    ZeroOut();
                }
            }
        }

        private void button_type_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            modeint = Convert.ToInt16(btn.Uid);
            //modeint++;
            //if (modeint > 2)
            //    modeint = 0;
            Refresh();
            
        }

        public void Refresh()
        {
            switch (modeint)
            {
                case 0:
                    if (newStats)
                        pIndex = "curr_p9";
                    else
                        pIndex = "p9";
                    //pIndex = "\"p9\"";
                    statsType = "Squad";
                    textblock_rating.Text = statsType + " Rating";
                    GetMode();
                    break;

                case 1:
                    if (newStats)
                        pIndex = "curr_p10";
                    else
                        pIndex = "p10";
                    //pIndex = "\"p10\"";
                    statsType = "Duo";
                    textblock_rating.Text = statsType + " Rating";
                    GetMode();
                    break;

                case 2:
                    if (newStats)
                        pIndex = "curr_p2";
                    else
                        pIndex = "p2";
                    //pIndex = "\"p2\"";
                    statsType = "Solo";
                    textblock_rating.Text = statsType + " Rating";
                    GetMode();
                    break;
            }
        }


        #region UI Stuff

        // Some name error handling here as well
        public void ButtonHandler(object sender, EventArgs e)
        {
            Button button = sender as Button;
            switch (button.Name)
            {
                case "button_tyty":
                    inputName.Text = button.Uid;
                    FindName();
                    break;

                case "button_jojo":
                    inputName.Text = button.Uid;
                    FindName();
                    break;

                case "button_bk":
                    inputName.Text = button.Uid;
                    FindName();
                    break;

                case "button_ster":
                    inputName.Text = button.Uid;
                    FindName();
                    break;

                case "button_mike":
                    inputName.Text = button.Uid;
                    FindName();
                    break;

                case "button_ex":
                    inputName.Text = button.Uid;
                    FindName();
                    break;
            }
        }

        private void RightButtonHandler(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button;
            ChangeName changeWin = new ChangeName();
            changeWin.Top = this.Top + 50;
            changeWin.Left = this.Left + 50;
            changeWin.ShowDialog();

            if (changeWin.DialogResult.HasValue && changeWin.DialogResult.Value)
            {
                button.Uid = changeWin.inputName.Text;
                button.Content = changeWin.inputName.Text;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            timercount = 0;
            textblock_time.Text = "";
            FindName();
        }

        private void inputName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!cleared)
            {
                inputName.Text = "";
                inputName.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                cleared = true;
            }
            timer.Enabled = false;
            timercount = 0;
            textblock_time.Text = "";
        }

        private void inputName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                FindName();
            }
        }

        private void FortNiteApp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
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

        private void FortNiteApp_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.Height > 370 && this.Width > 690)
            {
                TextBlock_KD.FontSize = TextBlock_KpG.FontSize = TextBlock_KpM.FontSize = TextBlock_WR.FontSize =
                TextBlock_K.FontSize = TextBlock_Wins.FontSize = TextBlock_Matches.FontSize = TextBlock_Score.FontSize = 20;

                TextBlock_KD.Width = TextBlock_KpG.Width = TextBlock_KpM.Width = TextBlock_WR.Width =
                TextBlock_K.Width = TextBlock_Wins.Width = TextBlock_Matches.Width = TextBlock_Score.Width = 150;
                //////
                text_kd.FontSize = text_kg.FontSize = text_km.FontSize = text_wr.FontSize =
                text_k.FontSize = text_w.FontSize = text_m.FontSize = text_s.FontSize = 44;

                text_kd.Width = text_kg.Width = text_km.Width = text_wr.Width =
                text_k.Width = text_w.Width = text_m.Width = text_s.Width = 200;

                text_kd.Margin = text_kg.Margin = text_km.Margin = text_wr.Margin =
                text_k.Margin = text_w.Margin = text_m.Margin = text_s.Margin = new Thickness(10, 32, 0, 0);
            }

            else
            {

                TextBlock_KD.FontSize = TextBlock_KpG.FontSize = TextBlock_KpM.FontSize = TextBlock_WR.FontSize =
                TextBlock_K.FontSize = TextBlock_Wins.FontSize = TextBlock_Matches.FontSize = TextBlock_Score.FontSize = 12;

                text_kd.FontSize = text_kg.FontSize = text_km.FontSize = text_wr.FontSize =
                text_k.FontSize = text_w.FontSize = text_m.FontSize = text_s.FontSize = 18;

                text_kd.Margin = text_kg.Margin = text_km.Margin = text_wr.Margin =
                text_k.Margin = text_w.Margin = text_m.Margin = text_s.Margin = new Thickness(10, 29, 0, 0);
            }

        }

        private void rectangle1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (newStats)
            {
                newStats = false;
                switchOff.Begin();
                Refresh();
                textbox_statss.Text = "Overall";
            }
            else
            {
                newStats = true;
                switchOn.Begin();
                Refresh();
                textbox_statss.Text = "This Season";
            }
        }

        private void rect_boost_MouseDown(object sender, MouseButtonEventArgs e)
        {
            boostcount += 1;
            if (boostcount > 4)
            {
                boost = true;
                FindName();
            }
        }

        

        private void button_ex_Click(object sender, RoutedEventArgs e)
        {
            ChangeName changeWin = new ChangeName();
            changeWin.Top = this.Top + 50;
            changeWin.Left = this.Left + 50;
            changeWin.ShowDialog();

            if (changeWin.DialogResult.HasValue && changeWin.DialogResult.Value)
            {
                button_ex.Uid = changeWin.inputName.Text;
                button_ex.Content = changeWin.inputName.Text;
                button_ex.Click -= button_ex_Click;
                button_ex.Click += ButtonHandler;
            }
        }

        #endregion


    }
}
