using UnityEngine;
using UnityEngine.UI;

namespace SpaceBangBang
{
    public class HPUI : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField]
        Slider hpSlider;
        [SerializeField]
        PlayerStat playerStat;

        public void Init(PlayerStat stat)
        {
            playerStat = stat;
        }

        // Update is called once per frame
        void Update()
        {
            if (playerStat.MaxHP != 0)
            {
                hpSlider.value = playerStat.CurrentHP / (float)playerStat.MaxHP;
            }
        }
    }
}