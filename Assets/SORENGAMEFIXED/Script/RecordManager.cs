using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordManager : MonoBehaviour
{
    public static RecordManager instance;

    //レコード保持用
	public static int record = 0;

    void Awake()
	{
		//シングルトン処理
		if (instance != null)
		{
			Destroy(this.gameObject);
		}
		else if (instance == null)
		{
			instance = this;
		}
		DontDestroyOnLoad (this.gameObject);
	}
}
