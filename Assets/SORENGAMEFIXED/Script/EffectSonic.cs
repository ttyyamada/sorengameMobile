using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EffectSonic : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        //SE再生
        AudioManager.instance.PlaySound(4);

        transform.DOScale(new Vector3(5f, 5f, 1f), 1f).SetEase(Ease.OutQuint).SetDelay(0.25f).SetUpdate(true);
        spriteRenderer.DOFade(0f, 1.1f).SetEase(Ease.OutQuint).SetDelay(0.25f).SetUpdate(true).OnComplete(DestroyGameObject);
    }

    void DestroyGameObject()
    {
        Destroy(this.gameObject);
    }


}
