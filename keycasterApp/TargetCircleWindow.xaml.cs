using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace keycasterApp
{
    public partial class TargetCircleWindow : Window
    {
        public TargetModel Model { get; private set; }

        public TargetCircleWindow(int index)
        {
            InitializeComponent();
            Model = new TargetModel { OrderIndex = index };
            UpdateUI();
        }

        public void UpdateUI()
        {
            TxtIndex.Text = Model.OrderIndex.ToString();

            // Đổi màu vòng tròn tùy thuộc loại thao tác
            if (Model.ActionType == TargetActionType.KeyPress)
                CircleBorder.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC00BCD4")); // Xanh ngọc cho Phím
            else
                CircleBorder.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCFF3366")); // Hồng cho Chuột
        }

        // Cho phép kéo thả ô hình tròn đi khắp màn hình
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    this.DragMove();
                }
                catch (InvalidOperationException)
                {
                    // Bỏ qua nếu Windows chưa sẵn sàng xử lý DragMove
                }
            }
        }

        // Nhấp đôi để mở bảng Edit cấu hình
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TargetConfigWindow configWindow = new TargetConfigWindow(Model);
            configWindow.Owner = this;
            if (configWindow.ShowDialog() == true)
            {
                UpdateUI();
            }
        }
    }
}