using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SLua;

namespace UGUI.Tween
{
    [CustomLuaClass]
    public class UIPlayTween : MonoBehaviour
    {
        public bool includeChildren = false;
        List<ITween> EnumAllTween()
        {
            List<ITween> tweeners = new List<ITween>();
            ITween[] tweens = transform.GetComponents<ITween>();
            foreach(var t in tweens)
            {
                if(t.enabled)
                {
                    tweeners.Add(t);
                }
            }
            if(includeChildren)
            {
                tweens = transform.GetComponentsInChildren<ITween>(false);
                foreach (var t in tweens)
                {
                    if (t.enabled)
                    {
                        tweeners.Add(t);
                    }
                }
            }

            return tweeners;
        }

        void Start()
        {

        }

        public void Play(bool restart = true)
        {
            List<ITween> tweeners = EnumAllTween();
            foreach(var t in tweeners)
            {
                if (restart)
                    t.RePlay();
                else
                    t.Play();
            }
        }

        [ContextMenu("Play All Tween")]
        void ResetPlay()
        {
            List<ITween> tweeners = EnumAllTween();
            foreach (var t in tweeners)
            {
                t.RePlay();
            }
        }
    }
}
