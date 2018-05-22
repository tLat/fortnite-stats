using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
        public bool boost;
        public bool newStats = true;
        public bool comparingMode = false;
        public bool cleared = false;
        public bool compareButtonSelect = false;

        // Strings
        #region strings

        public string playerData;
        public string playerData_compare;
        public const string kd = "K/d";
        //public const string disVal = "\"value\":\"";  //obsolete
        public string substr;
        public string dlString = "";
        public string dlString_compare = "";
        public string statsType = "Squad";
        //public string pIndex = "\"p9\""; // obsolete
        public string pIndex = "curr_p9";

        public string name;
        public string nameData;
        public string nameData_compare;
        //public string recentStats, recentStatsCompare;

        public string compareName;
        #endregion

        // ints
        #region numbers

        public const int GlowRaduis = 14;

        public int indx;
        public int e_indx;
        public int n_indx;
        public int boostcount = 0;
        public int modeint = 0;


        public int sq_score, cm_score;
        public int sq_rating, cm_rating;
        public int trn_rating, cm_trn_rating;
        public double ratingadd;
        public int sq_wins, cm_wins;
        public int sq_top3, cm_top3;
        public int sq_top5, cm_top5;
        public int sq_top6, cm_top6;
        public int sq_top10, cm_top10;
        public int sq_top12, cm_top12;
        public int sq_top25, cm_top25;
        public double sq_kd, cm_kd;
        public double sq_wr, cm_wr;
        public int sq_matches, cm_matches;
        public int sq_kills, cm_kills;
        public double sq_time, cm_time;
        public double sq_KPMin, cm_KPMin;
        public double sq_KPG, cm_KPG;

        int[] inc = new int[9];
        int[] total = new int[9];

        //int total_rate;
        int count = 0;
        public double[] nums = new double[12];
        public int[] ints = new int[8];

        public List<StatReport> reportWindow = new List<StatReport>();
        public bool windowFound = false;



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

            obj_x1.Visibility = obj_x2.Visibility = obj_back.Visibility = Visibility.Hidden;
        }

        private void TimerTick(Object source, System.Timers.ElapsedEventArgs e)
        {
            //if (!comparingMode)
            //{
                timercount++;
                if (timercount > 300)
                {
                    this.Dispatcher.Invoke(() => { FindName(); textblock_time.Text = ""; });
                    timercount = 0;
                }
                if (timercount > 29)
                    this.Dispatcher.Invoke(() => { textblock_time.Text = "Auto Refresh in " + (300 - timercount) + " Seconds"; });
            //}
        }


        public void ZeroOut()
        {
            sq_score = sq_rating = sq_wins = sq_top3 = sq_top5 = sq_top6 = sq_top10 = sq_top12 = sq_top25 = sq_matches = sq_kills = 0;
            sq_KPMin = sq_KPG = sq_wr = sq_kd = sq_time = 0.0;
            CalcRating();
            SetValues();
            GlowControl(sq_rating);
            text_rating.Text = "0";
            trn_rating_text.Text = "0";
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
            if (comparingMode)
                textblock_name.Text = name + " to";
            trn_rating_text.Text = trn_rating.ToString();
        }

        public void GetValues()
        {
            try
            {
                // Parse json (requires Newtonsoft.Json.dll)
                var obj = JObject.Parse(playerData);

                trn_rating = (int)obj[pIndex][0]["value"];
                sq_score = (int)obj[pIndex][1]["value"];
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
                //sq_time = (double)obj[pIndex][14]["value"];
                sq_KPMin = (double)obj[pIndex][14]["value"];


                if (comparingMode)
                {
                    obj = JObject.Parse(playerData_compare);

                    cm_trn_rating = (int)obj[pIndex][0]["value"];
                    cm_score = (int)obj[pIndex][1]["value"];
                    cm_wins = (int)obj[pIndex][2]["value"];
                    cm_top3 = (int)obj[pIndex][3]["value"];
                    cm_top5 = (int)obj[pIndex][4]["value"];
                    cm_top6 = (int)obj[pIndex][5]["value"];
                    cm_top10 = (int)obj[pIndex][6]["value"];
                    cm_top12 = (int)obj[pIndex][7]["value"];
                    cm_top25 = (int)obj[pIndex][8]["value"];
                    cm_kd = (double)obj[pIndex][9]["value"];
                    cm_wr = (double)obj[pIndex][10]["value"];
                    cm_matches = (int)obj[pIndex][11]["value"];
                    cm_kills = (int)obj[pIndex][12]["value"];
                    cm_KPG = (double)obj[pIndex][13]["value"];
                    //cm_time = (double)obj[pIndex][14]["value"];
                    cm_KPMin = (double)obj[pIndex][14]["value"];
                    /*
                    0   1   2
                    3   4   5
                    6   7   8
                    9  10  11
                    */
                    nums[0] = sq_kd;
                    nums[2] = cm_kd;

                    nums[3] = sq_KPG;
                    nums[5] = cm_KPG;

                    nums[6] = sq_wr;
                    nums[8] = cm_wr;

                    nums[9] = sq_KPMin;
                    nums[11] = cm_KPMin;


                    ints[0] = sq_kills;
                    ints[4] = cm_kills;

                    ints[1] = sq_wins;
                    ints[5] = cm_wins;

                    ints[2] = sq_matches;
                    ints[6] = cm_matches;

                    ints[3] = sq_score;
                    ints[7] = cm_score;

                    CalcRating();
                    CalcRating_compare();
                    CreateStatWin();
                }


                // obsolete, keep to in case to revert back to original json parsing
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

        public void CalcRating()
        {
            ratingadd = (sq_kd * 100) / 2;
            ratingadd += (sq_KPG * 100) / 2;
            ratingadd += (sq_KPMin * 1000) / 2 / 4 / 128;
            if (modeint == 2)
                ratingadd += (ratingadd * (sq_wr / 10) * 0.86);
            else
                ratingadd = (ratingadd * (sq_wr / 10));


            sq_rating = Convert.ToInt32(ratingadd);

            //nope
            //if (boost)
            //{
            //    if (inputName.Text == "puttzs")
            //        sq_rating += 500;
            //    if (inputName.Text == "honkin")
            //        sq_rating += 280;
            //}

            //text_rating.Text = sq_rating.ToString();
            if (sq_rating != 0 && sq_rating != total[0])
                RatingAnimation();
            else
                text_rating.Text = sq_rating.ToString();
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

        public void CalcRating_compare()
        {
                ratingadd = (cm_kd* 100) / 2;
                ratingadd += (cm_KPG* 100) / 2;
                ratingadd += (cm_KPMin* 1000) / 2 / 4 / 128;
                if (modeint == 2)
                    ratingadd += (ratingadd* (cm_wr / 10) * 0.86);
                else
                    ratingadd = (ratingadd* (cm_wr / 10));

                cm_rating = Convert.ToInt32(ratingadd);
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
                this.Dispatcher.Invoke(() =>
                {
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
                        textBlock1.Text = "No Connection or User Doesn't Exist";
                        ZeroOut();
                    });
                    return;
                }

            });
            getThread.Start();

            //GetMode();
        }

        public void FindNameCompare()
        {
            Thread getThread = new Thread(() =>
            {
                string uri = "";
                WebClient client = new WebClient();
                //client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(GetMode);
                this.Dispatcher.Invoke(() => { uri = "https://fortnitetracker.com/profile/pc/" + compareName; });
                try
                {
                    this.Dispatcher.Invoke(() => { textBlock1.Text = "Getting Compare Stats.."; });
                    dlString_compare = client.DownloadString(uri);
                    this.Dispatcher.Invoke(() => { GetMode_compare(); });

                }
                catch
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        textBlock1.Text = "No Connection or User Doesn't Exist";
                        //ZeroOut();
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
                // if this string is found, the user doesn't exist or no stats for user
                textBlock1.Text = statsType + " Stats Not Found for " + inputName.Text;
                ZeroOut();
            }
            else
            {
                try
                {
                    
                    playerData = GrabJSONfromString(dlString, "var imp_playerData = ");
                    nameData = GrabJSONfromString(dlString, "var imp_accountInfo = ");
                    //recentStats = GrabJSONfromString(dlString, "var imp_rollingStats = ");

                    // new way to get name
                    var obj = JObject.Parse(nameData);
                    name = (string)obj["nickname"];
                    Console.WriteLine("name from nameJson: " + name);
                    //

                    textBlock1.Text = name + " " + statsType + " Stats";
                    //indx = dlString.IndexOf("var playerData = ");
                    //indx = dlString.IndexOf(pIndex, indx) + 6;
                    GetValues();
                    SetValues();
                    CalcRating();
                    timer.Enabled = true;

                }
                catch
                {
                    textBlock1.Text = "Error Getting Stats, Not Available";
                    ZeroOut();
                }
            }
        }

        public void GetMode_compare()
        {
            if (dlString_compare.IndexOf("Issue while trying to get your stats") != -1)
            {
                // if this string is found, the user doesn't exist or no stats for user
                textBlock1.Text = statsType + " Stats Not Found for " + inputName.Text;
                //ZeroOut();
            }
            else
            {
                try
                {
                    // what a fucking statement
                    // getting json from web source, starting at "playerData", to ";</script>"
                    playerData_compare = GrabJSONfromString(dlString_compare, "var imp_playerData = ");
                    nameData_compare = GrabJSONfromString(dlString_compare, "var imp_accountInfo = ");
                    //recentStatsCompare = GrabJSONfromString(dlString_compare, "var imp_rollingStats = ");

                    // new way to get name
                    var obj = JObject.Parse(nameData);
                    name = (string)obj["nickname"];
                    Console.WriteLine("name from nameJson: " + name);
                    //

                    textBlock1.Text = name + " " + statsType + " Stats";
                    //indx = dlString_compare.IndexOf("var playerData = ");
                    //indx = dlString_compare.IndexOf(pIndex, indx) + 6;
                    CalcRating();
                    GetValues();
                    //SetValues();
                    
                    //timer.Enabled = true;

                }
                catch
                {
                    textBlock1.Text = "Error Getting Stats, Not Available";
                    //ZeroOut();
                }
            }
        }

        public string GrabJSONfromString(string s, string datatype)
        {
            // getting json from web source, starting at "playerData", to ";</script>"
            string output;

            output = s.Substring((s.IndexOf(datatype) + datatype.Length),
                        (s.IndexOf(";</script>", s.IndexOf(datatype))) - (s.IndexOf(datatype) + datatype.Length));

            return output;
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

        private void CompareSetUp()
        {
            comparingMode = true;
            obj_x1.Visibility = obj_x2.Visibility = obj_back.Visibility = Visibility.Visible;
            Console.WriteLine("input " + compareName);

            textblock_name.FontSize = 18;
            textblock_name.Text = name + " to";
            textblock_name_compare.Text = compareName;
            FindNameCompare();
        }

        public void CreateStatWin()
        {
            String newUid = name + compareName;

            foreach (var s in reportWindow)
            {
                if(s.Uid == newUid)
                {
                    //s.Hide();
                    s.player1_data = playerData;
                    s.player2_data = playerData_compare;
                    //s.player1recent = recentStats;
                    //s.player2recent = recentStatsCompare;
                    s.rank1 = sq_rating;
                    s.rank2 = cm_rating;
                    s.gamemode = modeint;
                    s.season = Convert.ToInt32(newStats);
                    s.RefreshAll();
                    windowFound = true;
                    Console.WriteLine("Stat window updated. ");
                    break;
                }
            }
            if (!windowFound)
            {
                StatReport reportWin = new StatReport();
                reportWin.Uid = newUid;
                reportWin.parent = this;
                reportWin.player1_data = playerData;
                reportWin.player2_data = playerData_compare;
                //reportWin.player1recent = recentStats;
                //reportWin.player2recent = recentStatsCompare;
                reportWin.rank1 = sq_rating;
                reportWin.rank2 = cm_rating;
                reportWin.name1.Text = name;
                reportWin.name2.Text = compareName;
                reportWin.gamemode = modeint;
                reportWin.season = Convert.ToInt32(newStats);
                if (reportWindow.Count == 0)
                {
                    reportWin.Top = this.Top - 100;
                    reportWin.Left = this.Left + 250;
                }
                else
                {
                    reportWin.Top = reportWindow[reportWindow.Count - 1].Top + 50;
                    reportWin.Left = reportWindow[reportWindow.Count - 1].Left + 100;
                }
                
                reportWin.Show();
                reportWindow.Add(reportWin);
                Console.Write("New window created. ");
                Console.WriteLine(reportWindow.Count);
            }
            windowFound = false;
        }

        private void FortNiteApp_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #region UI Stuff

        // Some name error handling here as well

        private void button_compare_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            comparingMode = true;
            compareButtonSelect = true;
            button_compare.Content = "CLICK NAME";
        }

        private void Rectangle_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (StatReport s in reportWindow)
                s.Focus();
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        public void ButtonHandler(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (!compareButtonSelect)
            {
                inputName.Text = button.Uid;
                FindName();
            }
            else
            {
                compareName = button.Uid;
                compareButtonSelect = false;
                button_compare.Content = "COMPARE";
                CompareSetUp();
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

        private void button_compare_Click(object sender, RoutedEventArgs e)
        {
            ChangeName changeWin = new ChangeName();
            changeWin.Top = this.Top + 50;
            changeWin.Left = this.Left + 50;
            changeWin.header.Text = "Compare To ";
            changeWin.ShowDialog();

            if (changeWin.DialogResult.HasValue && changeWin.DialogResult.Value)
            {
                compareName = changeWin.inputName.Text;
                CompareSetUp();
            }
        }

        private void obj_back_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (comparingMode)
            {
                if (obj_back.IsMouseOver)
                {
                    obj_back.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
                else
                {
                    obj_back.Fill = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
                }
            }
        }

        private void obj_back_MouseDown(object sender, MouseButtonEventArgs e)
        {
            obj_x1.Visibility = obj_x2.Visibility = obj_back.Visibility = Visibility.Hidden;
            obj_back.Fill = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            textblock_name.FontSize = 24;
            textblock_name.Text = name;
            textblock_name_compare.Text = "";
            comparingMode = false;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            timercount = 0;
            textblock_time.Text = "";
            if(!compareButtonSelect)
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
            Application.Current.Shutdown();
        }

        private void FortNiteApp_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.Height > 370 && this.Width > 690)
            {
                TextBlock_KD.FontSize = TextBlock_KpG.FontSize = TextBlock_KpM.FontSize = TextBlock_WR.FontSize =
                TextBlock_K.FontSize = TextBlock_Wins.FontSize = TextBlock_Matches.FontSize = TextBlock_Score.FontSize = 16;

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

        private void ButtonMini_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void ButtonMini_MouseEnter(object sender, MouseEventArgs e)
        {
            ButtonMini.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }
        private void ButtonMini_MouseLeave(object sender, MouseEventArgs e)
        {
            ButtonMini.Fill = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        }

        #endregion


    }
}
