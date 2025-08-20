using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIPanel
{
    public class UIController : MonoBehaviour
    {
        [Header("Configs")]
        [SerializeField]
        private bool _isGlobalUI = false;
        public bool IsGlobalUI => _isGlobalUI;
        [SerializeField]
        private bool _showInitUIFromStart = true;
        [SerializeField]
        private List<BaseUIPanel> _UIPanels;

        private Dictionary<UIPanel, AddressableUIPanel> _addressableUIs = new();

        // Store UI panel following opening order
        private Stack<UIPanel> _panelStack = new();
        public int ShowingPanelCount => _panelStack.Count;

        public static event Action OnGlobalClickerBlockerActive;
        public static event Action OnAllGlobalClickerBlockersInactive;

        public void ShowInitPanels()
        {
            InitAllUIPanels().Forget();
        }

        public void CloseTopPanel()
        {
            if (_panelStack.Count == 0)
            {
                return;
            }

            ClosePanelIfOpened(_panelStack.Peek());
        }

        public void CloseTopPanelExceptInitPanel()
        {
            if (_panelStack.Count == 0)
            {
                return;
            }

            var topPanel = _panelStack.Peek();
            if (topPanel.ShowFromStart)
            {
                return;
            }

            ClosePanelIfOpened(topPanel);
        }

        public void CloseAllButInitPanels()
        {
            while (_panelStack.Count > 0)
            {
                var panel = _panelStack.Peek();
                if (panel.ShowFromStart)
                {
                    return;
                }

                panel.Close();
            }
        }

        public bool IsOnTop(BaseUIPanel panel)
        {
            if (_panelStack.Count == 0)
            {
                return false;
            }

            var topPanel = _panelStack.Peek();

            return panel.GetPanelInstanceID() == topPanel.GetPanelInstanceID();
        }

        internal void ClosePanelIfOpened(BaseUIPanel panel)
        {
            if (panel.Status != PanelStatus.Opened)
            {
                return;
            }

            panel.Close();
        }

        internal void PushToStack(UIPanel panel)
        {
            if (0 < _panelStack.Count)
            {
                var recentPanel = _panelStack.Peek();
                recentPanel.SetClickBlockerVisible(false);
            }

            _panelStack.Push(panel);
        }

        internal void PopFromStack()
        {
            if (_panelStack.Count == 0)
            {
                return;
            }

            var popPanel = _panelStack.Pop();
            HandleClosingAddressablePanel(popPanel);

            if (_panelStack.Count == 0)
            {
                return;
            }

            var previous = _panelStack.Peek();
            if (previous == null)
            {
                goto PopNext;
            }
            if (previous.Status == PanelStatus.Opened || previous.Status == PanelStatus.IsOpening)
            {
                previous.SetClickBlockerVisible(true);
                return;
            }

            PopNext:
            PopFromStack();
        }

        internal void NotifyGlobalClickerBlockerActive()
        {
            OnGlobalClickerBlockerActive?.Invoke();
        }

        internal void NotifyAllGlobalClickBlockerInactive()
        {
            OnAllGlobalClickerBlockersInactive?.Invoke();
        }

        internal void RegisterAddressableUIPanel(UIPanel uiPanel, AddressableUIPanel aaPanel)
        {
            if (_addressableUIs.ContainsKey(uiPanel))
            {
                return;
            }

            _addressableUIs.Add(uiPanel, aaPanel);
            uiPanel.Init(this);
        }

        private void HandleClosingAddressablePanel(UIPanel uiPanel)
        {
            if (uiPanel == null)
            {
                return;
            }

            _addressableUIs.TryGetValue(uiPanel, out var aaPanel);
            if (aaPanel == null)
            {
                return;
            }

            _addressableUIs.Remove(uiPanel);
            aaPanel.PostCloseProcess().Forget();
        }

        private void Start()
        {
            if (!_showInitUIFromStart)
            {
                return;
            }

            InitAllUIPanels().Forget();
        }

        private async UniTaskVoid InitAllUIPanels()
        {
            // the init frame handles very heavy logic, showing animation from beginning often causes lagging
            await UniTask.DelayFrame(2);

            for (int i = 0; i < _UIPanels.Count; i++)
            {
                var panel = _UIPanels[i];
                if (panel == null)
                {
                    Debug.LogError($"Panel at index [{i}] is null");
                    continue;
                }

                panel.Init(this);

                if (panel.ShowFromStart)
                {
                    panel.Open();
                }
            }
        }
    }

}
