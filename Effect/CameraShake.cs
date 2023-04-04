using Cinemachine;
using DG.Tweening;
using UnityEngine;

namespace SpaceBangBang
{
    public class CameraShake : MonoBehaviour
    {
        [SerializeField]
        private float shakeTime;
        [SerializeField]
        private float intensityRate;
        private float shakeTimer;

        public static CameraShake Instance { get; private set; }

        private CinemachineBasicMultiChannelPerlin CM;

        Sequence cameraShakeSequence;

        private void Awake()
        {
            Instance = this;
            CM = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        private void Start()
        {
            cameraShakeSequence = DOTween.Sequence()
                .SetAutoKill(false)
                .Append(DOTween.To(() => CM.m_AmplitudeGain, x => CM.m_AmplitudeGain = x, 0, 0.1f));
        }

        public void ShakeCamera(float intensity)
        {
            cameraShakeSequence.Pause();
            CM.m_AmplitudeGain = intensity * intensityRate;
            shakeTimer = shakeTime;
        }

        private void Update()
        {
            if (shakeTimer > 0)
            {
                shakeTimer -= Time.deltaTime;
                if (shakeTimer <= 0f)
                {
                    cameraShakeSequence.Restart();
                    shakeTimer = 0f;
                }
            }
        }
    }
}