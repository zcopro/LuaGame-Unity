using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace UGUI.Tween
{
    public class ITween : MonoBehaviour
    {
        public bool autoPlay = true;
        public float duration = 1.0f;
        public float delay = 0;

        protected Tweener tween;
        protected bool bLastPlaying = false;

        public enum PlayType
        {
            Once,
            PingPong,
            Loop,
        }
        public delegate void VoidDelegate(GameObject go);
        public VoidDelegate onTweenFinish;
        public VoidDelegate onStepTweenFinish;

        public PlayType playType = PlayType.Once;
        
        public void Play()
        {
            tween.Play();
            bLastPlaying = true;
        }

        public void RePlay()
        {
            tween.Restart();
            bLastPlaying = true;
        }

        public void Rewind(bool includeDelay = true)
        {
            tween.Rewind(includeDelay);
            bLastPlaying = true;
        }

        public void Stop()
        {
            bLastPlaying = false;
        }

        public bool IsPlaying
        {
            get { return tween != null && tween.IsPlaying(); }
        }

        protected RectTransform rectTransform { get { return gameObject.GetComponent<RectTransform>(); } }

        protected void OnFinished()
        {
            if (null != onTweenFinish)
                onTweenFinish(this.gameObject);
        }

        protected void OnStepFinished()
        {
            if (null != onStepTweenFinish)
                onStepTweenFinish(this.gameObject);
        }


        protected virtual void UpdateTweenInfo() { }

        void Awake()
        {
            UpdateTweenInfo();

            if(autoPlay)
                Play();
        }

        void OnDestroy()
        {
            bLastPlaying = false;
        }

        void OnDisable()
        {
            tween.Pause();
        }
        void OnEnable()
        {
            if(bLastPlaying)
                tween.Play();
        }
    }
}
