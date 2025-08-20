using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UIPanel
{
    public class AddressableUIPanel : BaseUIPanel
    {
        [SerializeField]
        private GameObject _loadingPanel;
        [SerializeField]
        private AssetReference _uiPanelAddress;

        private UIPanel _panelInstance;

        public override void Init(UIController controller)
        {
            _uiController = controller;
        }

        public override void Open()
        {
            Async_Open().Forget();
        }

        public override async UniTask Async_Open()
        {
            _panelStatus = PanelStatus.IsOpening;
            transform.SetAsLastSibling();

            if (_panelInstance == null)
            {
                _loadingPanel.SetActive(true);
                _panelInstance = (await _uiPanelAddress.InstantiateAsync(transform, false)).GetComponent<UIPanel>();
                _uiController.RegisterAddressableUIPanel(_panelInstance, this);
                _loadingPanel.SetActive(false);
            }

            await _panelInstance.Async_Open();
            _panelStatus = PanelStatus.Opened;
        }

        public override void Close()
        {
            Async_Close().Forget();
        }

        public override async UniTask Async_Close()
        {
            if (_panelInstance == null)
            {
                return;
            }

            _panelStatus = PanelStatus.IsClosing;
            await _panelInstance.Async_Close();
            transform.SetAsFirstSibling();
            _panelStatus = PanelStatus.Closed;
        }

        public override void SetClickBlockerVisible(bool visible)
        {
            if (_panelInstance == null)
            {
                return;
            }
            _panelInstance.SetClickBlockerVisible(visible);
        }

        internal async UniTaskVoid PostCloseProcess()
        {
            if (_panelInstance == null)
            {
                return;
            }

            _panelStatus = PanelStatus.IsClosing;
            await UniTask.WaitUntil(() => _panelInstance.Status == PanelStatus.Closed);
            _uiPanelAddress.ReleaseInstance(_panelInstance.gameObject);
            _panelInstance = null;
            _panelStatus = PanelStatus.Closed;
        }

        private enum ReleasePanelMode
        {
            ReleaseAfterClosing = 0,
            ControllerHandle = 1
        }
    }
}

