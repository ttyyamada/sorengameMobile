using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepublicController : MonoBehaviour
{
    int idNumber;
    [SerializeField] int republicNumber;
    [SerializeField] RepublicController nextRepublicController;

    MainManager mainManager;
    Rigidbody2D thisRigidbody2D;
    PolygonCollider2D thisPolygonCollider2D;
    SpriteRenderer thisSpriteRenderer;

    float massMultiple = 10f;   //質量調整用
    int hitRepublicIDNumber = -1;   //衝突した相手のIDNumber
    bool isAwake = false;   //このRepublicでゲームオーバー判定する状態かどうか
    float timeElapsed = 0f; //Red Line接触時間カウント
    float timeOut = 3f; //Red Line接触時間のゲームオーバー閾値
    int alartFlashCount = 0;    //Red Lineオーバーの警告表示用

    int[] score = new int[]{1, 3, 6, 10, 15, 21, 28, 36, 45, 55, 66, 78, 91, 105, 120, 136};    //合体時の得点テーブル

    [SerializeField] GameObject effectExplosion;    //通常合体時に発生するエフェクト

    void OnTriggerStay2D(Collider2D other)  //ゲームオーバー判定
    {
        if(isAwake == false)    return;
        if(     mainManager.GetGameState() != MainManager.GameState.MOVE
            &&  mainManager.GetGameState() != MainManager.GameState.DROP) return;

        if(other.gameObject.tag == "Dead Line") //即時ゲームオーバーライン
        {
            if(mainManager.GetGameState() != MainManager.GameState.MOVE)    return;

            //ステート遷移
            mainManager.SetGameState(MainManager.GameState.GAMEOVER);
        }
        else if(other.gameObject.tag == "Red Line") //猶予のあるゲームオーバーライン
        {
            //カウントアップ
            timeElapsed += Time.deltaTime;

            //Republicの色を変化させ警告する
            alartFlashCount += 1;
            if(timeElapsed <= timeOut * 0.5f)
            {
                //ゆっくり警告
                thisSpriteRenderer.color = Color.white;
                if(alartFlashCount % 16 == 0 || alartFlashCount % 16 == 1 || alartFlashCount % 16 == 2)
                {
                    thisSpriteRenderer.color = Color.red;
                }
            }
            else
            {
                //はやく警告
                thisSpriteRenderer.color = Color.white;
                if(alartFlashCount % 8 == 0 || alartFlashCount % 8 == 1 || alartFlashCount % 8 == 2)
                {
                    thisSpriteRenderer.color = Color.red;
                }
            }

            //時間切れ判定
            if(timeElapsed >= timeOut)
            {
                //ステート遷移
                mainManager.SetGameState(MainManager.GameState.GAMEOVER);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)  //Red Line超えが解消されたときのリセット処理
    {
        if(isAwake == false)    return;
        if(other.gameObject.tag != "Red Line")  return;

        if(     mainManager.GetGameState() != MainManager.GameState.MOVE
            &&  mainManager.GetGameState() != MainManager.GameState.DROP) return;

        //カウンタの初期化
        timeElapsed = 0f;

        //色の初期化
        alartFlashCount = 0;
        thisSpriteRenderer.color = Color.white;
    }

    public int GetIDNmber()
    {
        return idNumber;
    }

    public int GetRepublicNmber()
    {
        return republicNumber;
    }

    

    public int GetHitRepublicIDNumber()
    {
        return hitRepublicIDNumber;
    }

    public void AddExplosionForce2D(Vector3 explosionOrigin, float explosionForce, float explosionRadius)
    {
        if(thisRigidbody2D == null) return;

        Vector3 direction = transform.position - explosionOrigin;
        float forceFalloff = 1 - (direction.magnitude / explosionRadius);

        thisRigidbody2D.AddForce(direction.normalized * (forceFalloff <= 0 ? 0 : explosionForce) * forceFalloff, ForceMode2D.Impulse);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------
    // 以下は3個以上同時合体時のバグを修正したもの
    //---------------------------------------------------------------------------------------------------------------------------------------------------
    public void SetRepublicInit()
    {
        mainManager = GameObject.Find("Main Manager").GetComponent<MainManager>();
        thisSpriteRenderer = GetComponent<SpriteRenderer>();

        idNumber = mainManager.GetMasterIDNumber(this);

        thisPolygonCollider2D = GetComponent<PolygonCollider2D>();
        thisPolygonCollider2D.enabled = true;

        thisRigidbody2D = GetComponent<Rigidbody2D>();
        thisRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        thisRigidbody2D.mass = thisRigidbody2D.mass * massMultiple;
    }

    public void SetExplosionEffect(Vector3 InputPosition)
    {
        Instantiate(effectExplosion, new Vector3(InputPosition.x, InputPosition.y, -5f), Quaternion.identity);
    }

    public void SetNextRepublic(Vector3 InputPosition)
    {
        RepublicController tmpRepublicController2 = Instantiate(nextRepublicController, InputPosition, Quaternion.identity);
        tmpRepublicController2.SetRepublicInit();
    }

    public int GetScore()
    {
        return score[republicNumber - 1];
    }
    
    void OnCollisionEnter2D(Collision2D collisionInfo)
    {
        if(isAwake == false)    //このRepublicでゲームオーバー判定しない状態の場合
        {
            if(collisionInfo.gameObject.tag != "Side Stage") isAwake = true; //左右のステージ以外に接触したらこのRepublicでゲームオーバー判定する状態に変える
        }

        if(collisionInfo.gameObject.tag != "Republic")  return;

        RepublicController tmpRepublicController = collisionInfo.gameObject.GetComponent<RepublicController>();

        if(republicNumber == tmpRepublicController.GetRepublicNmber())
        {
            if(!(tmpRepublicController.GetHitRepublicIDNumber() == -1 || tmpRepublicController.GetHitRepublicIDNumber() == idNumber)) return; //当たった相手の、衝突した相手のIDNumberが-1(未接触)か自分自身でない場合はリターン

            hitRepublicIDNumber = tmpRepublicController.GetIDNmber();

            //ヒットした相手のIDをリストに登録する
            mainManager.SetHitRepublicIDNumber(idNumber, hitRepublicIDNumber);
        }
    }
}
