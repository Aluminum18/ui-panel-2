using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace UIPanel
{
    public abstract class BaseUIPanel : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField]
        protected bool _showFromStart;
        public bool ShowFromStart => _showFromStart;
        [SerializeField]
        protected bool _refreshWhenReopen = false;
        [SerializeField]
        protected bool _ignoreTimeScale = true;

        [Header("UnityEvents")]
        [SerializeField]
        protected UnityEvent _onRefresh;
        [SerializeField]
        protected UnityEvent _onStartShow;
        [SerializeField]
        protected UnityEvent _onAllElementsShown;
        [SerializeField]
        protected UnityEvent _onStartHide;
        [SerializeField]
        protected UnityEvent _onAllElementsHided;
        
        protected PanelStatus _panelStatus = PanelStatus.Closed;
        public PanelStatus Status => _panelStatus;

        protected UIController _uiController;

        public virtual void Init(UIController controller) { }
        public virtual void Open() { }
        public virtual UniTask Async_Open()
        {
            return default;
        }
        public virtual void Close() { }
        public virtual UniTask Async_Close()
        {
            return default;
        }
        public virtual int GetPanelInstanceID()
        {
            return gameObject.GetInstanceID();
        }
        public virtual void SetClickBlockerVisible(bool visible) { }
    }

    [System.Serializable]
    public enum PanelStatus
    {
        IsClosing = 0,
        Closed = 1,
        IsOpening = 2,
        Opened = 3,
    }
}
