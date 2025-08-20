
using Cysharp.Threading.Tasks;

namespace UIPanel
{
    public interface IUIPanel
    {
        void Init(UIController controller);
        void Open();
        UniTask Async_Open();
        void Close();
        UniTask Async_Close();
    }
}