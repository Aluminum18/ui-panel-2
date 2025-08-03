
namespace UIPanel
{
    public interface IUIPanel
    {
        void Init(UIController controller);
        void Open();
        void Close();
        void SetClickBlockerVisible(bool visible);
        int GetPanelInstanceID();
        bool ShowFromStart { get; }
        PanelStatus Status { get; }
    }
}