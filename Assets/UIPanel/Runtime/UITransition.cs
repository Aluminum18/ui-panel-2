using System;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.Playables;

namespace UIPanel
{
    public class UITransition : MonoBehaviour
    {
        [SerializeField]
        private TransitionSetup[] _showTransition;
        [SerializeField]
        private TransitionSetup[] _hideTransition;

        [SerializeField]
        private UnityEvent _onStartShow;
        [SerializeField]
        private UnityEvent _onFinishShow;
        [SerializeField]
        private UnityEvent _onStartHide;
        [SerializeField]
        private UnityEvent _onFinishHide;

        private CanvasGroup _canvasGroup;
        private PlayableDirector _timelinePlayer;
        private bool _useFixedTimeScale = true;

        public void Init()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _timelinePlayer = GetComponent<PlayableDirector>();
        }

        public void PreshowSetSup()
        {
            for (int i = 0; i < _showTransition.Length; i++)
            {
                PreshowSetUp(_showTransition[i]);
            }
        }

        public async UniTask RunShowTransition()
        {
            PreshowSetSup();
            await UniTask.NextFrame();

            _onStartShow.Invoke();
            for (int i = 0; i < _showTransition.Length; i++)
            {
                var transition = _showTransition[i];
                float startAfter = transition.startAfterSecond;
                await UniTask.WaitForSeconds(startAfter, _useFixedTimeScale);
                await RunTranstion(transition);
            }

            _onFinishShow.Invoke();
        }

        public async UniTask RunHideTransition()
        {
            _onStartHide.Invoke();
            for (int i = 0; i < _hideTransition.Length; i++)
            {
                var transition = _hideTransition[i];
                float startAfter = transition.startAfterSecond;
                await UniTask.WaitForSeconds(startAfter, _useFixedTimeScale);
                await RunTranstion(transition);
            }

            _onFinishHide.Invoke();
        }

        private void PreshowSetUp(TransitionSetup setup)
        {
            switch (setup.transitionType)
            {
                case TransitionType.Fade:
                    {
                        if (_canvasGroup == null)
                        {
                            Debug.LogWarning("In order to use Fade Transition, add CanvasGroup Component to this object", this);
                            return;
                        }

                        _canvasGroup.alpha = setup.from.x;
                        break;
                    }
                case TransitionType.Move:
                    {
                        transform.localPosition = setup.from;
                        break;
                    }
                case TransitionType.Zoom:
                    {
                        transform.localScale = setup.from;
                        break;
                    }
                case TransitionType.Timeline:
                    {
                        if (_timelinePlayer == null)
                        {
                            Debug.LogError("Please attach PlayableDirector to use Timeline transition", this);
                            return;
                        }
                        _timelinePlayer.time = 0f;
                        _timelinePlayer.Stop();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        public async UniTask RunTranstion(TransitionSetup setup)
        {
            switch (setup.transitionType)
            {
                case TransitionType.Fade:
                    {
                        if (_canvasGroup == null)
                        {
                            Debug.LogWarning("In order to use Fade Transition, add CanvasGroup Component to this object", this);
                            return;
                        }

                        _canvasGroup.DOKill();
                        await _canvasGroup.DOFade(setup.to.x, setup.duration).SetEase(setup.ease).SetUpdate(_useFixedTimeScale);
                        break;
                    }
                case TransitionType.Move:
                    {
                        transform.DOKill();
                        await transform.DOLocalMove(setup.to, setup.duration).SetEase(setup.ease).SetUpdate(_useFixedTimeScale);
                        break;
                    }
                case TransitionType.Zoom:
                    {
                        transform.DOKill();
                        await transform.DOScale(setup.to.x, setup.duration).SetEase(setup.ease).SetUpdate(_useFixedTimeScale);
                        break;
                    }
                case TransitionType.Timeline:
                    {
                        _timelinePlayer.timeUpdateMode = _useFixedTimeScale ? DirectorUpdateMode.UnscaledGameTime : DirectorUpdateMode.GameTime;
                        _timelinePlayer.Play();
                        await UniTask.Delay(TimeSpan.FromSeconds(_timelinePlayer.duration), _useFixedTimeScale);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
    }

    public enum TransitionType
    {
        None = 0,
        Move = 1,
        Zoom = 2,
        Fade = 3,
        Timeline = 4
    }

    [System.Serializable]
    public struct TransitionSetup
    {
        public float startAfterSecond;
        public TransitionType transitionType;
        public Vector3 from;
        public Vector3 to;
        public float duration;
        public Ease ease;
    }

}