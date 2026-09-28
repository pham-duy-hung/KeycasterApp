using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Gma.System.MouseKeyHook;

namespace keycasterApp
{
    public partial class MainWindow : Window
    {
        private IKeyboardMouseEvents _globalHook;
        private DispatcherTimer _autoClickTimer;
        private bool _isAutoClickRunning = false;

        // Mã ID phím tắt F6 (Virtual Key Code F6 = 0x75)
        private const int HOTKEY_ID = 9000;
        private const uint VK_F6 = 0x75;

        public MainWindow()
        {
            InitializeComponent();
            SetupAutoClickTimer();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Lấy Handle cửa sổ để đăng ký Hotkey Win32
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            HwndSource source = HwndSource.FromHwnd(hwnd);
            source.AddHook(HwndHook);

            // Đăng ký phím F6 làm Global Hotkey
            NativeMethods.RegisterHotKey(hwnd, HOTKEY_ID, 0, VK_F6);

            // Đăng ký Keycaster Global Hook
            SubscribeGlobalHooks();
        }

        #region AutoClicker Engine & Timer
        private void SetupAutoClickTimer()
        {
            _autoClickTimer = new DispatcherTimer();
            _autoClickTimer.Tick += AutoClickTimer_Tick;
        }

        private void ToggleAutoClick()
        {
            if (_isAutoClickRunning)
            {
                // DỪNG AUTOCLICK
                _autoClickTimer.Stop();
                _isAutoClickRunning = false;

                // Thả chuột ra nếu đang ở chế độ Hold
                NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

                BtnToggleAutoClick.Content = "BẬT AUTOCLICK (F6)";
                BtnToggleAutoClick.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 122, 204));
            }
            else
            {
                // BẮT ĐẦU AUTOCLICK
                if (int.TryParse(TxtInterval.Text, out int interval) && interval > 0)
                {
                    _autoClickTimer.Interval = TimeSpan.FromMilliseconds(interval);
                    _autoClickTimer.Start();
                    _isAutoClickRunning = true;

                    BtnToggleAutoClick.Content = "ĐANG CHẠY... (ẤN F6 ĐỂ DỪNG)";
                    BtnToggleAutoClick.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 53, 69));
                }
                else
                {
                    MessageBox.Show("Vui lòng nhập khoảng thời gian (Interval ms) hợp lệ!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void AutoClickTimer_Tick(object sender, EventArgs e)
        {
            int selectedIndex = CboClickType.SelectedIndex;

            switch (selectedIndex)
            {
                case 0: // Click chuột trái
                    NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN | NativeMethods.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    break;

                case 1: // Click chuột phải
                    NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_RIGHTDOWN | NativeMethods.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    break;

                case 2: // Giữ chuột trái (Hold)
                    NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    break;
            }
        }

        private void BtnToggleAutoClick_Click(object sender, RoutedEventArgs e)
        {
            ToggleAutoClick();
        }

        // Lắng nghe thông điệp Hotkey từ hệ thống Windows
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                ToggleAutoClick();
                handled = true;
            }
            return IntPtr.Zero;
        }
        #endregion

        #region Keycaster Overlay Engine
        private void SubscribeGlobalHooks()
        {
            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    MouseText.Text = "";
                    KeyText.Text = e.KeyData.ToString();
                    ShowOverlay();
                });
            };

            _globalHook.MouseDown += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    KeyText.Text = "";
                    MouseText.Text = $"[{e.Button.ToString().ToUpper()}]";
                    ShowOverlay();
                });
            };
        }

        private void ShowOverlay()
        {
            OverlayBorder.BeginAnimation(UIElement.OpacityProperty, null);
            OverlayBorder.Opacity = 1;

            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(600)),
                BeginTime = TimeSpan.FromMilliseconds(800)
            };
            OverlayBorder.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
        #endregion

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            NativeMethods.UnregisterHotKey(hwnd, HOTKEY_ID);

            if (_globalHook != null)
            {
                _globalHook.Dispose();
            }
            base.OnClosed(e);
        }
    }
}