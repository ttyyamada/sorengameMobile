using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

//unityroom向けオンラインランキング用
//unityroom向けオンラインランキングの実装方法はこちらを参照してください
//https://help.unityroom.com/how-to-implement-scoreboard
//
//using unityroom.Api;

public class MainManager : MonoBehaviour
{
    [SerializeField] RepublicController[] republicControllerPrefabs;

    int masterIDNumber = 0;
    int nextRepublic;

    RepublicController nowRepublicController;
    RepublicController nextRepublicController;

    Vector3 mouseWorldPos;
    float nowRepublicPosition;
    float nowRepublicAutoBaseLimit = 3.2f;
    float nowRepublicAutoLimit = 3.2f;

    float dropTimeElapsed = 0f;
    float dropTimeOut = 1f; //落としてから次のRepublicを落とせるようになるまでの時間

    [SerializeField] TMPro.TMP_Text[] scoreTMPText;
    [SerializeField] TMPro.TMP_Text[] recordTMPText;
    int score = 0;

    [SerializeField] GameObject gameOverUI;

    public struct RepublicPiece
    {
        public RepublicController republicController;
        public int masterIDNumber;
        public int hitRepublicIDNumber; //-100:ヒットチェック済み, -1:未ヒット, 0以上:ヒットした相手のIDNumber
    }

    List<RepublicPiece> republicPieceList = new List<RepublicPiece>();

    public enum GameState
    {
        MOVE,
        DROP,
        GAMEOVER,
        STOP,
    }
    GameState gameState = GameState.MOVE;
    GameState oldGameState;

    //鎌と槌の演出用
    [SerializeField] EffectSonic effectSonic;
    [SerializeField] Image bgImage;
    [SerializeField] Image fadeImage;
    [SerializeField] RectTransform hammerRectTransform;
    [SerializeField] RectTransform sickleRectTransform;
    [SerializeField] Image redImage;
    [SerializeField] CanvasGroup hammerAndSickleCanvasGroup;
    [SerializeField] Image flagImage;
    [SerializeField] RepublicController sorenRepublicController;
    int makeSorenCount = 0;

    

    //---------------------------------------------------------------------------------------------------------------------------------------------------
    void Start()
    {
        DOTween.Init();

        //時間開始
        Time.timeScale = 1;

        //BGM停止
        AudioManager.instance.StopBGM(0);
        AudioManager.instance.StopBGM(1);

        //BGM再生
        AudioManager.instance.PlayBGM(0);

        //レコード表示
        for(int i = 0; i < recordTMPText.Length; i++)
        {
            recordTMPText[i].SetText(RecordManager.record.ToString());
        }

        nextRepublic = GetRandomNext();
        CreateNextRepublic(nextRepublic);

        nowRepublicController = nextRepublicController;
        nowRepublicAutoLimit = nowRepublicAutoBaseLimit - nowRepublicController.gameObject.GetComponent<SpriteRenderer>().bounds.size.x * 0.5f;

        nextRepublic = GetRandomNext();
        CreateNextRepublic(nextRepublic);
    }

    void Update()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        nowRepublicPosition = mouseWorldPos.x;
        if(nowRepublicPosition > nowRepublicAutoLimit)    nowRepublicPosition = nowRepublicAutoLimit;
        if(nowRepublicPosition < -nowRepublicAutoLimit)    nowRepublicPosition = -nowRepublicAutoLimit;

        switch(gameState)
        {
            case GameState.MOVE:
                MoveState();
                break;

            case GameState.DROP:
                DropState();
                break;

            case GameState.GAMEOVER:
                GameOverState();
                break;

            case GameState.STOP:
                //何もしない
                break;
        }
    }

    void MoveState()
    {
        nowRepublicController.transform.position = new Vector3(nowRepublicPosition, 4.25f, 0f);

        if(     Input.GetMouseButtonDown(0)
            &&  mouseWorldPos.x > -4f
            &&  mouseWorldPos.x < 4f)   //マウスクリックした場合
        {   
            //SE再生
            AudioManager.instance.PlaySound(2);

            //物理演算を有効化
            nowRepublicController.SetRepublicInit();

            //ステート遷移
            gameState = GameState.DROP;
        }
    }

    void DropState()
    {
        dropTimeElapsed += Time.deltaTime;
        if(dropTimeElapsed >= dropTimeOut)
        {
            dropTimeElapsed = 0f;

            nowRepublicController = nextRepublicController;
            nowRepublicAutoLimit = nowRepublicAutoBaseLimit - nowRepublicController.gameObject.GetComponent<SpriteRenderer>().bounds.size.x * 0.5f;

            nextRepublic = GetRandomNext();
            CreateNextRepublic(nextRepublic);

            //ステート遷移
            gameState = GameState.MOVE;
        }
    }

    void GameOverState()
    {
        //時間停止
        Time.timeScale = 0;

        //ゲームオーバー画面表示
        gameOverUI.SetActive(true);

        //レコード更新処理
        if(score > RecordManager.record)    RecordManager.record = score;

        //ステート遷移
        gameState = GameState.STOP;

        //ハイスコアのオンラインランキング更新
        //unityroom向けオンラインランキングの実装方法はこちらを参照してください
        //https://help.unityroom.com/how-to-implement-scoreboard
        //
        //オンラインランキング更新
        //UnityroomApiClient.Instance.SendScore(1, (float)score, ScoreboardWriteMode.HighScoreDesc);
    }

    public void StartHammerAndSickle(Vector3 inputPosition, int inputID0, int inputID1)  //このゲーム中でソ連が初めて完成した時の演出開始
    {
        //ソ連建国カウントアップ
        AddMakeSorenCount();

        if(makeSorenCount >= 2) return;

        //ステート遷移
        oldGameState = gameState;
        gameState = GameState.STOP;

        //時間停止
        Time.timeScale = 0;

        //BGM停止
        AudioManager.instance.StopBGM(0);

        //衝撃波エフェクト表示
        Instantiate(effectSonic, inputPosition, Quaternion.identity);

        //背景画像を暗くする
        bgImage.DOColor(new Color(0.2f, 0.2f, 0.2f), 0.5f).SetUpdate(true);

        //フェードイメージ
        fadeImage.DOFade(1f, 0.75f).SetDelay(1f).SetUpdate(true);

        //ハンマー移動
        hammerRectTransform.DOAnchorPos(Vector2.zero, 3f).SetEase(Ease.OutQuad).SetDelay(2.25f).SetUpdate(true);

        //カマ移動
        sickleRectTransform.DOAnchorPos(Vector2.zero, 3f).SetEase(Ease.OutQuad).SetDelay(2.25f).SetUpdate(true).OnComplete(SorenBGMPlay);

        DOVirtual.DelayedCall
        (
            1f,   // 遅延させる（待機する）時間
            () =>
            {
                //SE再生
                AudioManager.instance.PlaySound(5);
            }
        );

        //Republic消去処理
        DOVirtual.DelayedCall
        (
            5.5f,   // 遅延させる（待機する）時間
            () =>
            {
                RepublicPiece tmpRepublicPiece0 = republicPieceList.Find(where => where.masterIDNumber == inputID0);
                RepublicPiece tmpRepublicPiece1 = republicPieceList.Find(where => where.masterIDNumber == inputID1);

                DeleteRepublicControllerFromList(inputID0);
                DeleteRepublicControllerFromList(inputID1);

                Destroy(tmpRepublicPiece0.republicController.gameObject);
                Destroy(tmpRepublicPiece1.republicController.gameObject);

                //合体生成
                RepublicController tmpRepublicController2 = Instantiate(sorenRepublicController, inputPosition, Quaternion.identity);
                tmpRepublicController2.SetRepublicInit();

                //スコア加算
                AddScore(120);
            }
        );
    }

    void SorenBGMPlay()
    {
        //BGM再生
        AudioManager.instance.PlayBGM(1);

        DOVirtual.DelayedCall
        (
            0.25f,   // 遅延させる（待機する）時間
            () =>
            {
                //赤背景表示
                redImage.gameObject.SetActive(true);
            }
        );

        hammerAndSickleCanvasGroup.DOFade(0f, 1f).SetDelay(4.2f).SetUpdate(true).OnComplete(ResumeGame);

        //背景差し替え
        flagImage.gameObject.SetActive(true);
        bgImage.gameObject.SetActive(false);
    }

    void ResumeGame()
    {
        //ゲームに戻る処理
        hammerAndSickleCanvasGroup.gameObject.SetActive(false);

        //時間進行
        Time.timeScale = 1;

        //ステート遷移
        gameState = oldGameState;
    }

    int GetRandomNext()
    {
        //return 2;
        return Random.Range(0, republicControllerPrefabs.Length);
    }

    void CreateNextRepublic(int inputIndex)
    {
        nextRepublicController = Instantiate(republicControllerPrefabs[inputIndex], new Vector3(6.25f, 1.5f, 0f), Quaternion.identity);
    }

    public int GetMasterIDNumber(RepublicController inputRepublicController)
    {
        masterIDNumber += 1;

        //リストに登録
        RepublicPiece tmpRepublicPiece = new RepublicPiece();
        tmpRepublicPiece.republicController = inputRepublicController;
        tmpRepublicPiece.masterIDNumber = masterIDNumber;

        republicPieceList.Add(tmpRepublicPiece);

        return masterIDNumber;
    }

    public GameState GetGameState()
    {
        return gameState;
    }

    public void SetGameState(GameState inputGameState)
    {
        gameState = inputGameState;
    }

    public void AddScore(int inputScore)
    {
        score += inputScore;

        for(int i = 0; i < scoreTMPText.Length; i++)
        {
            scoreTMPText[i].SetText(score.ToString());
        }
    }

    public void DeleteRepublicControllerFromList(int inputMasterIDNumber)
    {
        republicPieceList.RemoveAll(where => where.masterIDNumber == inputMasterIDNumber);
    }

    public void SetExplosion(Vector3 explosionOrigin, float explosionForce, float explosionRadius, int exclusionID1, int exclusionID2)
    {
        for(int i = 0; i < republicPieceList.Count; i++)
        {
            if(republicPieceList[i].masterIDNumber != exclusionID1 && republicPieceList[i].masterIDNumber != exclusionID2)
            {
                republicPieceList[i].republicController.AddExplosionForce2D(explosionOrigin, explosionForce, explosionRadius);
            }
        }
    }

    public int GetMakeSorenCount()
    {
        return makeSorenCount;
    }

    public void AddMakeSorenCount()
    {
        makeSorenCount += 1;

        //ソ連建国数のオンラインランキング更新
        //unityroom向けオンラインランキングの実装方法はこちらを参照してください
        //https://help.unityroom.com/how-to-implement-scoreboard
        //
        //UnityroomApiClient.Instance.SendScore(2, (float)makeSovietCount, ScoreboardWriteMode.HighScoreDesc);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------
    // 以下は3個以上同時合体時のバグを修正したもの
    //---------------------------------------------------------------------------------------------------------------------------------------------------
    
    public void SetHitRepublicIDNumber(int inputIDNumber, int inputHitRepublicIDNumber)
    {
        for(int i = 0; i < republicPieceList.Count; i++)
        {
            if(republicPieceList[i].masterIDNumber == inputIDNumber)
            {
                RepublicPiece tmpRepublicPiece = republicPieceList[i];
                tmpRepublicPiece.hitRepublicIDNumber = inputHitRepublicIDNumber;
                republicPieceList[i] = tmpRepublicPiece;
                break;
            }
        }
    }

    void FixedUpdate()
    {
        List<int> tmpDeleteIDNumber = new List<int>();  //削除リスト

        for(int i = 0; i < republicPieceList.Count; i++)
        {
            if(republicPieceList[i].hitRepublicIDNumber >= 0)
            {
                bool tmpIsFindPair = false;
                for(int j = 0; j < republicPieceList.Count; j++)
                {
                    if(     republicPieceList[j].masterIDNumber == republicPieceList[i].hitRepublicIDNumber
                        &&  republicPieceList[j].hitRepublicIDNumber >= 0)
                    {
                        RepublicPiece tmpRepublicPiece;
                        tmpRepublicPiece = republicPieceList[i];
                        tmpRepublicPiece.hitRepublicIDNumber = -100;
                        republicPieceList[i] = tmpRepublicPiece;

                        tmpRepublicPiece = republicPieceList[j];
                        tmpRepublicPiece.hitRepublicIDNumber = -100;
                        republicPieceList[j] = tmpRepublicPiece;

                        //削除リストに追加
                        tmpDeleteIDNumber.Add(republicPieceList[i].masterIDNumber);
                        tmpDeleteIDNumber.Add(republicPieceList[j].masterIDNumber);

                        tmpIsFindPair = true;

                        //共和国の合体処理
                        Vector3 tmpPosition = (republicPieceList[i].republicController.gameObject.transform.position + republicPieceList[j].republicController.gameObject.transform.position) * 0.5f;    //2Republicの中間座標を算出

                        //出来たのが1個目のソ連の場合
                        if(republicPieceList[i].republicController.GetRepublicNmber() == 15 && GetMakeSorenCount() == 0)
                        {
                            StartHammerAndSickle(tmpPosition, republicPieceList[i].masterIDNumber, republicPieceList[j].masterIDNumber);
                            return;
                        }

                        //2個目以降のソ連か、ソ連以外の場合
                        AudioManager.instance.PlaySound(3); //SE再生

                        //ソ連ができたらカウントアップ
                        if(republicPieceList[i].republicController.GetRepublicNmber() == 15)    AddMakeSorenCount();

                        //合体エフェクト生成
                        republicPieceList[i].republicController.SetExplosionEffect(tmpPosition);

                        //衝撃力生成
                        SetExplosion(this.transform.position, 450f, 2f, republicPieceList[i].masterIDNumber, republicPieceList[j].masterIDNumber);

                        if(republicPieceList[i].republicController.GetRepublicNmber() != 16)    //ソ連以外の場合は合体Republic生成。ソ連はただ消えるだけ…
                        {
                            //合体Republic生成
                            republicPieceList[i].republicController.SetNextRepublic(tmpPosition);
                        }

                        //スコア加算
                        AddScore(republicPieceList[i].republicController.GetScore());
                        
                        break;
                    }
                }

                if(tmpIsFindPair == false)
                {
                    RepublicPiece tmpRepublicPiece;
                    tmpRepublicPiece = republicPieceList[i];
                    tmpRepublicPiece.hitRepublicIDNumber = -1;
                    republicPieceList[i] = tmpRepublicPiece;
                }
            }
        }

        //MainManagerのリストから削除とGameObjectの破棄
        for(int i = 0; i < tmpDeleteIDNumber.Count; i++)
        {
            for(int j = 0; j < republicPieceList.Count; j++)
            {
                if(republicPieceList[j].masterIDNumber == tmpDeleteIDNumber[i])
                {
                    Destroy(republicPieceList[j].republicController.gameObject);
                    break;
                }
            }
            
            DeleteRepublicControllerFromList(tmpDeleteIDNumber[i]);
        }
    }
}
