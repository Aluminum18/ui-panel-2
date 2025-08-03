using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIPanel
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class UIPanel : MonoBehaviour, IUIPanel
    {
        [Header("Config")]
        [SerializeField]
        private List<UIElement> _elements;
        [SerializeField]
        private Image _clickBlocker;
        public bool HasClickBlocker => _clickBlocker != null;
        [SerializeField]
        private bool _showFromStart;
        [SerializeField]
        private bool _refreshWhenReopen = false;
        [SerializeField]
        private bool _ignoreTimeScale = true;

        [Header("UnityEvents")]
        [SerializeField]
        private UnityEvent _onRefresh;
        [SerializeField]
        private UnityEvent _onStartShow;
        [SerializeField]
        private UnityEvent _onAllElementsShown;
        [SerializeField]
        private UnityEvent _onStartHide;
        [SerializeField]
        private UnityEvent _onAllElementsHided;

        [SerializeField]
        private PanelStatus _panelStatus;

        private UIController _uiController;
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private GraphicRaycaster _rayCaster;

        private float _clickBlockerOriginalAlpha;

        private UniTask[] _showTasks;
        private UniTask[] _hideTasks;

        #region IUIPanel interface implementations
        public void Init(UIController controller)
        {
            _canvas = GetComponent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _rayCaster = GetComponent<GraphicRaycaster>();
            _canvas.enabled = false;
            _canvasGroup.interactable = false;
            _rayCaster.enabled = false;

            _uiController = controller;

            if (!ValidateElements())
            {
                return;
            }

            for (int i = 0; i < _elements.Count; i++)
            {
                _elements[i].Init(_ignoreTimeScale);
            }

            _showTasks = new UniTask[_elements.Count];
            _hideTasks = new UniTask[_elements.Count];

            if (_clickBlocker == null)
            {
                return;
            }
            _clickBlockerOriginalAlpha = _clickBlocker.color.a;

            TrackGlobalClickBlocker();
        }

        public void Open()
        {
            Async_Open().Forget();
        }

        public void Close()
        {
            Async_Close().Forget();
        }

        public int GetPanelInstanceID()
        {
            return gameObject.GetInstanceID();
        }
        public bool ShowFromStart => _showFromStart;
        public PanelStatus Status => _panelStatus;

        public async UniTask Async_Open()
        {
            if (Status == PanelStatus.Opened || Status == PanelStatus.IsOpening)
            {
                if (_refreshWhenReopen)
                {
                    _onRefresh.Invoke();
                }
                return;
            }

            if (!ValidateElements())
            {
                return;
            }

            // Handle case: a Panel is asked to open while it is closing
            if (Status == PanelStatus.IsClosing)
            {
                ForcePanelToClosedState();
            }

            _canvas.enabled = true;
            _rayCaster.enabled = true;
            transform.SetAsLastSibling();

            _panelStatus = PanelStatus.IsOpening;

            SetClickBlockerVisible(true);

            _onStartShow.Invoke();
            for (int i = 0; i < _elements.Count; i++)
            {
                _showTasks[i] = _elements[i].Async_Show();
            }
            await UniTask.WhenAll(_showTasks);
            Array.Clear(_showTasks, 0, _showTasks.Length);

            _uiController.PushToStack(this);
            FinishOpenProcess();
        }
        public void SetClickBlockerVisible(bool visible)
        {
            if (_clickBlocker == null)
            {
                return;
            }
            Color color = _clickBlocker.color;
            color.a = visible ? _clickBlockerOriginalAlpha : 0f;
            _clickBlocker.color = color;

            if (!_uiController.IsGlobalUI)
            {
                return;
            }
            if (visible)
            {
                _uiController.NotifyGlobalClickerBlockerActive();
                return;
            }
            if (_uiController.ShowingPanelCount != 0)
            {
                return;
            }

            _uiController.NotifyAllGlobalClickBlockerInactive();
        }
        #endregion

        public async UniTask Async_Close()
        {
            if (Status == PanelStatus.Closed || Status == PanelStatus.IsClosing)
            {
                return;
            }

            if (!ValidateElements())
            {
                return;
            }

            // Handle case: a Panel is asked to close while it is opening
            if (Status == PanelStatus.IsOpening)
            {
                ForcePanelToOpenedState();
            }

            _panelStatus = PanelStatus.IsClosing;
            _onStartHide.Invoke();
            // panel is on top, pop it.
            // if it is not on top, just close and it will be pop later
            if (_uiController.IsOnTop(this))
            {
                _uiController.PopFromStack();
            }

            _canvasGroup.interactable = false;
            for (int i = 0; i < _elements.Count; i++)
            {
                _hideTasks[i] = _elements[i].Async_Hide();
            }
            await UniTask.WhenAll(_hideTasks);
            Array.Clear(_hideTasks, 0, _hideTasks.Length);

            FinishCloseProcess();
        }

        public void CloseAndOpenOther(UIPanel other)
        {
            Async_CloseAndOpenOther(other).Forget();
        }
        private async UniTaskVoid Async_CloseAndOpenOther(UIPanel other)
        {
            await Async_Close();
            other.Open();
        }

        public void CloseAllButInitPanels()
        {
            _uiController.CloseAllButInitPanels();
        }

        private void FinishOpenProcess()
        {
            _canvasGroup.interactable = true;
            _onAllElementsShown?.Invoke();
            _panelStatus = PanelStatus.Opened;
        }

        private void FinishCloseProcess()
        {
            _canvas.enabled = false;
            _rayCaster.enabled = false;
            _onAllElementsHided?.Invoke();
            _panelStatus = PanelStatus.Closed;
        }

        public void SetInteractable(bool interactable)
        {
            _canvasGroup.interactable = interactable;
        }

        public void SetMoveToFront()
        {
            transform.SetAsLastSibling();
        }

        private void ForcePanelToOpenedState()
        {
            _canvas.enabled = true;
            _rayCaster.enabled = true;
            transform.SetAsLastSibling();
            _panelStatus = PanelStatus.Opened;

            FinishOpenProcess();
        }

        private void ForcePanelToClosedState()
        {
            _canvasGroup.interactable = false;
            FinishCloseProcess();
        }

        private bool ValidateElements()
        {
            if (_uiController == null)
            {
                return false;
            }

            if (_elements.Count == 0)
            {
                Debug.LogWarning($"No element found in {gameObject.name}", this);
                return false;
            }

            return true;
        }

        private void TrackGlobalClickBlocker()
        {
            if (_uiController.IsGlobalUI)
            {
                return;
            }

            UIController.OnGlobalClickerBlockerActive += AdaptWithActivatedGlobalClickerBlocker;
            UIController.OnAllGlobalClickerBlockersInactive += ApdaptWithInactiveGlobalClickerBlocker;
        }

        private void OnDisable()
        {
            if (_uiController == null || _uiController.IsGlobalUI)
            {
                return;
            }

            UIController.OnGlobalClickerBlockerActive -= AdaptWithActivatedGlobalClickerBlocker;
            UIController.OnAllGlobalClickerBlockersInactive -= ApdaptWithInactiveGlobalClickerBlocker;
        }

        private void AdaptWithActivatedGlobalClickerBlocker()
        {
            if (!_uiController.IsOnTop(this))
            {
                return;
            }

            SetClickBlockerVisible(false);
        }

        private void ApdaptWithInactiveGlobalClickerBlocker()
        {
            if (!_uiController.IsOnTop(this))
            {
                return;
            }

            SetClickBlockerVisible(true);
        }      
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
