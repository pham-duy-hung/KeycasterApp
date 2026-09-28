using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace keycasterApp
{
    public partial class TargetConfigWindow : Window
    {
        public TargetModel Model { get; private set; }
        private Key _selectedKey = Key.A;

        public TargetConfigWindow(TargetModel model)
        {
            InitializeComponent();
            Model = model;

            CboType.SelectedIndex = (int)Model.ActionType;
            TxtDelay.Text = Model.DelayMs.ToString();
            _selectedKey = Model.KeyToPress;
            TxtKey.Text = _selectedKey.ToString();
        }

        private void CboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PanelKey != null)
            {
                PanelKey.Visibility = (CboType.SelectedIndex == 3) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void TxtKey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            _selectedKey = (e.Key == Key.System) ? e.SystemKey : e.Key;
            TxtKey.Text = _selectedKey.ToString();
        }
        private void TxtKey_KeyDown(object sender, KeyEventArgs e)
        {
            // Cập nhật phím được nhấn vào TextBox
            if (sender is TextBox textBox)
            {
                textBox.Text = e.Key.ToString();
                e.Handled = true;
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            Model.ActionType = (TargetActionType)CboType.SelectedIndex;
            Model.KeyToPress = _selectedKey;

            if (int.TryParse(TxtDelay.Text, out int delay))
                Model.DelayMs = delay;

            this.DialogResult = true;
            this.Close();
        }
    }
}