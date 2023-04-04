using DG.Tweening;
using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace SpaceBangBang
{
    public class SceneManagerEx : MonoBehaviour
    {
        public Action<string, string> OnSceneLoaded = null;

        public void LoadScene(AsyncOperation ao, string prevscene, string nextscene)
        {
            ao.allowSceneActivation = false;

            var endload = this.FixedUpdateAsObservable().Where(_ => ao.progress >= 0.89);
            endload.First().
                Subscribe(_ => OnSceneLoaded?.Invoke(prevscene, nextscene));

            endload
                .Delay(TimeSpan.FromSeconds(1.7f))
                .Subscribe(_ => ao.allowSceneActivation = true);
        }
    }
}