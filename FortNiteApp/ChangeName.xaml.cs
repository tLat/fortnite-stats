using System;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FortNiteApp
{
    /// <summary>
    /// Interaction logic for ChangeName.xaml
    /// </summary>
    public partial class ChangeName : Window
    {
        private bool keydown = false;

        public ChangeName()
        {
            InitializeComponent();
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

        private void inputName_GotFocus(object sender, RoutedEventArgs e)
        {
            //inputName.Clear();
        }

        private void inputName_KeyDown(object sender, KeyEventArgs e)
        {
            if (!keydown)
            {
                inputName.Clear();
                keydown = true;
            }
            if (e.Key == Key.Return)
            {
                DialogResult = true;
                this.Close();
            }
        }

        private void changeWin_Loaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();
            inputName.Focus();
        }

        private void ButtonClose_MouseEnter(object sender, MouseEventArgs e)
        {
            ButtonClose.Fill = new SolidColorBrush(Color.FromRgb(236, 80, 80));
        }

        private void ButtonClose_MouseLeave(object sender, MouseEventArgs e)
        {
            ButtonClose.Fill = new SolidColorBrush(Color.FromRgb(199, 199, 199));
        }

        private void ButtonClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void changeWin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void inputName_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!keydown)
            {
                inputName.Clear();
                keydown = true;
            }
        }

        private void inputName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!keydown)
            {
                inputName.Clear();
                keydown = true;
            }
        }
    }
}
