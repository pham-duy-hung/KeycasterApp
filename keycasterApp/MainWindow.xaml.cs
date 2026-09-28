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
        private DispatcherTimer _mouseHoldTimer;
        private DateTime _mouseDownTime;
        private bool _isMouseDown = false;
        private string _currentMouseButton = "";

        // Win32 API để cài đặt Click-Through (Nhấp xuyên qua cửa sổ)
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public MainWindow()
        {
            InitializeComponent();
            SetupHoldTimer();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Bật thuộc tính Click-Through
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);

            // Đặt vị trí cửa sổ ở góc dưới bên phải màn hình
            this.Left = SystemParameters.PrimaryScreenWidth - this.Width - 20;
            this.Top = SystemParameters.PrimaryScreenHeight - this.Height - 60;

            // Đăng ký Global Hook
            SubscribeGlobalHooks();
        }

        private void SubscribeGlobalHooks()
        {
            _globalHook = Hook.GlobalEvents();

            // Lắng nghe bàn phím
            _globalHook.KeyDown += OnKeyDown;

            // Lắng nghe chuột
            _globalHook.MouseDown += OnMouseDown;
            _globalHook.MouseUp += OnMouseUp;
        }

        #region Xử lý Bàn Phím
        private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MouseText.Text = ""; // Xóa text chuột nếu đang hiển thị phím
                KeyText.Text = e.KeyData.ToString();
                ShowOverlay();
            });
        }
        #endregion

        #region Xử lý Chuột & Giữ Chuột (Mouse Hold)
        private void SetupHoldTimer()
        {
            _mouseHoldTimer = new DispatcherTimer();
            _mouseHoldTimer.Interval = TimeSpan.FromMilliseconds(200); // Sau 200ms nhấn sẽ tính là "Giữ"
            _mouseHoldTimer.Tick += (s, e) =>
            {
                if (_isMouseDown)
                {
                    MouseText.Text = $"[HOLD {_currentMouseButton}]";
                    ShowOverlay(autoHide: false); // Giữ nguyên không ẩn khi đang hold
                }
            };
        }

        private void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _isMouseDown = true;
            _mouseDownTime = DateTime.Now;
            _currentMouseButton = e.Button.ToString().ToUpper();

            Dispatcher.Invoke(() =>
            {
                KeyText.Text = "";
                MouseText.Text = $"[{_currentMouseButton} CLICK]";
                ShowOverlay();
                _mouseHoldTimer.Start();
            });
        }

        private void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _isMouseDown = false;
            _mouseHoldTimer.Stop();

            Dispatcher.Invoke(() =>
            {
                TriggerFadeOut();
            });
        }
        #endregion

        #region Hiệu ứng UI (Animation)
        private void ShowOverlay(bool autoHide = true)
        {
            OverlayBorder.BeginAnimation(UIElement.OpacityProperty, null);
            OverlayBorder.Opacity = 1;

            if (autoHide)
            {
                TriggerFadeOut();
            }
        }

        private void TriggerFadeOut()
        {
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(800)),
                BeginTime = TimeSpan.FromMilliseconds(1000) // Đợi 1 giây rồi bắt đầu mờ dần
            };

            OverlayBorder.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
        #endregion

        protected override void OnClosed(EventArgs e)
        {
            // Hủy đăng ký hook khi đóng ứng dụng
            _globalHook.KeyDown -= OnKeyDown;
            _globalHook.MouseDown -= OnMouseDown;
            _globalHook.MouseUp -= OnMouseUp;
            _globalHook.Dispose();
            base.OnClosed(e);
        }
    }
}