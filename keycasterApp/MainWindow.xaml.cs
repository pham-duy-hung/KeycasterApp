using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Gma.System.MouseKeyHook;

namespace keycasterApp
{
    public partial class MainWindow : Window
    {
        private List<TargetCircleWindow> _targetList = new List<TargetCircleWindow>();
        private bool _isSequenceRunning = false;

        private const int HOTKEY_ID = 9000;
        private const uint VK_F6 = 0x75;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            HwndSource source = HwndSource.FromHwnd(hwnd);
            source.AddHook(HwndHook);

            NativeMethods.RegisterHotKey(hwnd, HOTKEY_ID, 0, VK_F6);
        }

        // --- Thêm / Xóa Ô Target ---
        private void BtnAddTarget_Click(object sender, RoutedEventArgs e)
        {
            int nextIndex = _targetList.Count + 1;
            TargetCircleWindow target = new TargetCircleWindow(nextIndex);

            target.Left = SystemParameters.PrimaryScreenWidth / 2 + (nextIndex * 20);
            target.Top = SystemParameters.PrimaryScreenHeight / 2;

            target.Show();
            _targetList.Add(target);
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void BtnRemoveTarget_Click(object sender, RoutedEventArgs e)
        {
            if (_targetList.Count > 0)
            {
                var lastTarget = _targetList[_targetList.Count - 1];
                lastTarget.Close();
                _targetList.Remove(lastTarget);
            }
        }

        // --- Bật / Dừng Vòng lặp sequence ---
        private void ToggleSequence()
        {
            if (_isSequenceRunning)
            {
                _isSequenceRunning = false;
                BtnToggleAutoClick.Content = "BẬT CHẠY TỰ ĐỘNG (F6)";
            }
            else
            {
                if (_targetList.Count == 0)
                {
                    MessageBox.Show("Hãy ấn '+ Thêm Ô' để tạo ít nhất 1 điểm click!", "Thông báo");
                    return;
                }

                _isSequenceRunning = true;
                BtnToggleAutoClick.Content = "ĐANG CHẠY... (ẤN F6 ĐỂ DỪNG)";

                // Khởi chạy Task bất đồng bộ để không làm treo giao diện
                Task.Run(() => ExecuteSequenceLoop());
            }
        }

        private async Task ExecuteSequenceLoop()
        {
            while (_isSequenceRunning)
            {
                for (int i = 0; i < _targetList.Count; i++)
                {
                    if (!_isSequenceRunning) break;

                    var target = _targetList[i];
                    TargetModel model = target.Model;

                    // Lấy tọa độ tâm của ô hình tròn thời gian thực trên màn hình
                    int centerX = 0;
                    int centerY = 0;

                    Dispatcher.Invoke(() =>
                    {
                        centerX = (int)(target.Left + target.Width / 2);
                        centerY = (int)(target.Top + target.Height / 2);
                    });

                    // 1. Di chuyển con trỏ chuột đến tâm ô hình tròn
                    NativeMethods.SetCursorPos(centerX, centerY);
                    await Task.Delay(20);

                    // 2. Thực hiện hành động theo cấu hình
                    switch (model.ActionType)
                    {
                        case TargetActionType.LeftClick:
                            NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN | NativeMethods.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                            break;

                        case TargetActionType.RightClick:
                            NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_RIGHTDOWN | NativeMethods.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                            break;

                        case TargetActionType.HoldLeft:
                            NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                            break;

                        case TargetActionType.KeyPress:
                            // Đổi phím WPF sang Virtual Key Win32
                            byte vkCode = (byte)KeyInterop.VirtualKeyFromKey(model.KeyToPress);
                            NativeMethods.keybd_event(vkCode, 0, 0, 0); // Key Down
                            NativeMethods.keybd_event(vkCode, 0, NativeMethods.KEYEVENTF_KEYUP, 0); // Key Up
                            break;
                    }

                    // 3. Đợi Delay của ô đó trước khi qua ô tiếp theo
                    await Task.Delay(model.DelayMs);
                }
            }
        }

        private void BtnToggleAutoClick_Click(object sender, RoutedEventArgs e)
        {
            ToggleSequence();
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                ToggleSequence();
                handled = true;
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            foreach (var target in _targetList)
            {
                target.Close();
            }
            base.OnClosed(e);
        }
    }
}