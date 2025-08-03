using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UIPanel
{
    public class AddressableUIPanel : MonoBehaviour, IUIPanel
    {
        [SerializeField]
        private AssetReference _uiPanelAddress;

        #region IUIPanel interface implementations
        public PanelStatus Status => throw new System.NotImplementedException();

        public bool ShowFromStart => throw new System.NotImplementedException();

        public void Close()
        {
            throw new System.NotImplementedException();
        }

        public int GetPanelInstanceID()
        {
            throw new System.NotImplementedException();
        }

        public void Init(UIController controller)
        {
            throw new System.NotImplementedException();
        }

        public void Open()
        {
            throw new System.NotImplementedException();
        }

        public void SetClickBlockerVisible(bool visible)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}

