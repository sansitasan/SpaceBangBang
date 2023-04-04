using System.Collections;
using UnityEngine;
using Photon.Pun;
using System;

namespace SpaceBangBang
{
    public class LazerGun : Gun
    {
        [SerializeField]
        protected AudioClip chargeSound;

        [SerializeField]
        protected ParticleSystem chargeParticle;
        [SerializeField]
        protected ParticleSystem[] lazerParticles;
        private int _lazerParticlesIndex;

        private Coroutine _chargeCoroutine;

        private bool _isCharging;

        private int LazerParticlesIndex { get => _lazerParticlesIndex; set { _lazerParticlesIndex = value; if (_lazerParticlesIndex >= lazerParticles.Length) _lazerParticlesIndex = 0; } }

        protected override void Init()
        {
            base.Init();
        }

        public override bool Shot(bool hasEffect = true)
        {
            if (CanShot)
            {
                CanShot = false;

                if (!_isCharging)
                {
                    if (_chargeCoroutine != null) StopCoroutine(_chargeCoroutine);
                    _chargeCoroutine = StartCoroutine(ChargeGun());
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void SparyShot()
        {
            StartCoroutine(ShotEightDirectionCoroutine());
        }

        private IEnumerator ShotEightDirectionCoroutine()
        {
            isEightDirectionShot = true;

            if (!_isCharging)
            {
                if (_chargeCoroutine != null) StopCoroutine(_chargeCoroutine);
                yield return _chargeCoroutine = StartCoroutine(ChargeGun());
            }

            isEightDirectionShot = false;
        }

        IEnumerator ChargeGun() // �� ��¡
        {
            photonView.RPC("ChargeEffectRPC", RpcTarget.All);

            _isCharging = true;
            yield return new WaitForSeconds(_weaponStat.ChargingTime);
            yield return StartCoroutine(FireLazer());
            _isCharging = false;
        }

        IEnumerator FireLazer() // Raycast �߻�� �浹ü üũ
        {
            int layerMask = ((1 << LayerMask.NameToLayer("Hole")) | (1 << LayerMask.NameToLayer("Guard")) | (1 << LayerMask.NameToLayer("Ignore Raycast")));
            layerMask = ~layerMask;

            if (isEightDirectionShot)
            {
                float initialAngle = transform.rotation.eulerAngles.z - 180;
                float temp = initialAngle - 45;

                for (int i = 0; i < 8; i++)
                {
                    temp += 45;
                    if (temp > 180)
                    {
                        temp -= 360;
                    }
                    photonView.RPC("RotateGunRPC", RpcTarget.All, temp);
                    RaycastHit2D hit = Physics2D.Raycast(bulletHole.position, -bulletHole.transform.right, 500, layerMask);

                    if (hit)
                    {
                        photonView.RPC("DrawLineRPC", RpcTarget.All, bulletHole.position, hit.point, i == 0);
                        if (hit.transform.CompareTag("Player"))
                        {
                            hit.transform.GetComponent<Player>().Hit();
                        }
                    }
                }

                photonView.RPC("RotateGunRPC", RpcTarget.All, initialAngle);
                isEightDirectionShot = false;
            }
            else
            {
                RaycastHit2D hit = Physics2D.Raycast(bulletHole.position, -bulletHole.transform.right, 500, layerMask);

                if (hit)
                {
                    photonView.RPC("DrawLineRPC", RpcTarget.All, bulletHole.position, hit.point, true);
                    if (hit.transform.CompareTag("Player"))
                    {
                        hit.transform.GetComponent<Player>().Hit();
                    }
                    yield return GameManager.Instance.Wfs(5);
                }
            }
        }

        [PunRPC]
        protected void DrawLineRPC(Vector3 startPos, Vector2 endPos, bool hasEffect) // ������ �׸���.
        {
            muzzleFlashParticle.Emit(1);
            if (hasEffect)
            {
                ShakeCamera();
                audioSource.PlayOneShot(shotSound);
            }

            float distance = Vector2.Distance(startPos, endPos);

            lazerParticles[LazerParticlesIndex].transform.localScale = new Vector3(1, distance + 0.6f, 1);
            lazerParticles[LazerParticlesIndex].Play();
            StartCoroutine(DrawLineCoroutine(LazerParticlesIndex++));
        }

        [PunRPC]
        protected void ChargeEffectRPC() // ��¡�� �� ������ ����Ʈ ��� �Լ�
        {
            audioSource.PlayOneShot(chargeSound);
            chargeParticle.Emit(1);
        }

        IEnumerator DrawLineCoroutine(int index) // ������ ��ƼŬ�� ������� ���� �������� ���� ����ٴ��� �ʰ� �ϵ��� �θ� ���� ����.
        {
            Vector3 tempPos = lazerParticles[index].transform.localPosition;
            Quaternion tempRot = lazerParticles[index].transform.localRotation;
            Quaternion tempWorldRot = lazerParticles[index].transform.rotation;
            float tempScale = lazerParticles[index].transform.localScale.y;
            lazerParticles[index].transform.SetParent(null);
            lazerParticles[index].transform.localScale = new Vector3(lazerParticles[index].transform.localScale.x, Math.Sign(lazerParticles[index].transform.localScale.y) * tempScale, lazerParticles[index].transform.localScale.z);
            while (lazerParticles[index].isPlaying)
            {
                yield return null;
            }
            lazerParticles[index].transform.SetParent(transform.GetChild(0));
            lazerParticles[index].transform.localPosition = tempPos;
            lazerParticles[index].transform.localRotation = tempRot;
        }
    }
}