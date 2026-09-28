using System.Windows.Input;

namespace keycasterApp
{
    public enum TargetActionType
    {
        LeftClick,
        RightClick,
        HoldLeft,
        KeyPress
    }

    public class TargetModel
    {
        public int OrderIndex { get; set; }
        public TargetActionType ActionType { get; set; } = TargetActionType.LeftClick;
        public Key KeyToPress { get; set; } = Key.A;
        public int DelayMs { get; set; } = 200; // Thời gian nghỉ sau khi thực hiện
    }
}