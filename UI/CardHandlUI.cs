using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBangBang
{

public class CardHandlUI : MonoBehaviour
{
    private List<Button> hand;
    [SerializeField]
    private Transform Content;
    Button buttonPrefab;
    public Player _p;
    [SerializeField]
    Transform myCardLeft;
    [SerializeField]
    Transform myCardRight;
    [SerializeField]
    Transform cardSpawnPoint;

    private Transform originCard;

    public void Init(Player p)
    {
        hand = new List<Button>();
        _p = p;
        p.Clear -= Clear;
        p.Clear += Clear;
        buttonPrefab = Instantiate(Resources.Load<Button>("Prefabs/UI/Button"));
        originCard = buttonPrefab.transform;
    }
    
    public void AddCardToHand(ICard card)
    {
        var button = CardObjectPool.GetObject();
        //레이어 순서를 맞춰준다.
        
        button.transform.position = cardSpawnPoint.position;
        button.transform.SetParent(Content);
        button.transform.SetAsFirstSibling();
        //button.onClick.AddListener(() => TaskOnClick(button));
        button.image.sprite = CardsManager.Instance.GetSprite(card);
        hand.Add(button);
        CardAlignment();
    }
    
    public void RemoveCardFromHand(int index)
    {
        if(hand.Count == 0)
            return;
        hand[0].transform.DOKill(hand[0].transform);
        hand[0].transform.DOMove(hand[0].transform.position + Vector3.down*300f, 0.1f);
        hand[index].onClick.RemoveAllListeners();
        hand[index].animator.SetBool("Hide", true);
        hand[0].transform.localScale = originCard.localScale;
        //애니메이션이 끝나면 CardObjectPool에 반환
        float time = hand[index].animator.GetCurrentAnimatorStateInfo(0).length;
        StartCoroutine(ReturnToPool(hand[index], time));
        hand.RemoveAt(index);
        CardAlignment();
    }
    
    IEnumerator ReturnToPool(Button button, float time)
    {
        yield return new WaitForSeconds(time);
        button.animator.Rebind();
        CardObjectPool.ReturnObject(button);
    }

    public void TaskOnClick(Button button)
    {
        _p.UseCard(hand.FindIndex(x => x == button));
    }

    void CardAlignment()
    {
        List<PRS> cardPRSs = RoundAlignment(myCardLeft, myCardRight, 5f, Vector3.one);

        for (int i = 1; i < hand.Count; i++)
        {
            hand[i].transform.DOMove(cardPRSs[i].pos, 0.5f);
            hand[i].transform.DORotateQuaternion(cardPRSs[i].rot, 0.5f);
            hand[i].transform.DOScale(cardPRSs[i].scale, 0.5f);
        }

        if (hand.Count != 0)
        {
            DOTween.Sequence().Append(hand[0].transform.DORotateQuaternion(Quaternion.Euler(0, 0, 0), 0.5f))
                .Join(hand[0].transform.DOMove(cardPRSs[0].pos + new Vector3(-1, 2, 0) * 30f, 0.5f))
                .Append(hand[0].transform.DOShakePosition(0.5f, 10f, 10, 90f, false, true));
        }
    }

    List<PRS> RoundAlignment(Transform leftTr, Transform rightTr, float height, Vector3 scale)
    {
        float[] objLerps = new float[hand.Count];
        List<PRS> prsList = new List<PRS>();
        switch (hand.Count)
        {
            case 1:
                objLerps = new float[] { 0.5f };break;
            case 2:
                objLerps = new float[] { 0.27f, 0.73f };break;
            case 3:
                objLerps = new float[] { 0.1f, 0.5f, 0.9f };break;
            default:
                float interval = 1f/(hand.Count-1);
                for(int i = 0; i<hand.Count; i++)
                    objLerps[i] = interval*i;
                break;
        }
        
        for (int i = 0; i < hand.Count; i++)
        {
            var targetPos = Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);
            var targetRot = Quaternion.identity;
            if (hand.Count >= 4)
            {
                float curve = Mathf.Sqrt(Mathf.Pow(height,2) - Mathf.Pow(objLerps[i] - 0.5f,2));
                curve = height >=0 ? curve : -curve;
                targetPos.y += curve;
                targetRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, objLerps[i]);
            }
            prsList.Add(new PRS(targetPos, targetRot, scale));
        }

        return prsList;
    }

    private void Clear(Player p)
    {
        _p.Clear -= Clear;
        gameObject.SetActive(false);
    }
}

public class PRS
{
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;
    
    public PRS(Vector3 pos, Quaternion rot, Vector3 scale)
    {
        this.pos = pos;
        this.rot = rot;
        this.scale = scale;
    }
}}