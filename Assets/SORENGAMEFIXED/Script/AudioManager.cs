using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {

	public static AudioManager instance;
	
	//効果音用
	public AudioSource[] audioSource;
	float[] defaultVolume;

	public static int PlayingBGMNumber = -1;

    public static int bgmVolume = 3;
    public static int seVolume = 3;

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

		Init();
	}

	void Init()
	{
		//効果音関連
        AudioSource[] audioSources = gameObject.GetComponents<AudioSource>();
		audioSource = new AudioSource[audioSources.Length];
        for(int i = 0; i < audioSource.Length; i++)
		{
			audioSource[i] = audioSources[i];
		}

		//ボリューム初期値の保持
		defaultVolume = new float[audioSource.Length];
		for(int i = 0; i < audioSource.Length; i++)
		{
			defaultVolume[i] = audioSource[i].volume;
		}

		//初期音量設定
		audioSource[0].volume = defaultVolume[0] * 0.125f * (float)bgmVolume;
		audioSource[1].volume = defaultVolume[1] * 0.125f * (float)bgmVolume;

		audioSource[2].volume = defaultVolume[2] * 0.125f * (float)seVolume;
		audioSource[3].volume = defaultVolume[3] * 0.125f * (float)seVolume;
		audioSource[4].volume = defaultVolume[4] * 0.125f * (float)seVolume;
		audioSource[5].volume = defaultVolume[5] * 0.125f * (float)seVolume;
	}

	//------------------------------------------------------------------------------------------------------------------------
	public void PlayBGM(int inputNumber)
	{
		if(audioSource[inputNumber].isPlaying == false)
		{
			//BGM鳴らす
			audioSource[inputNumber].time = 0f;
			audioSource[inputNumber].Play();
			PlayingBGMNumber = inputNumber;
		}
	}

	public void StopBGM(int inputNumber)
	{
		if(audioSource[inputNumber].isPlaying == true)
		{
			audioSource[inputNumber].Stop();
		}
	}

	public void PlaySound(int inputNumber)
	{
		if(audioSource[inputNumber].isPlaying == true)
		{
			audioSource[inputNumber].Stop();
		}

		//効果音鳴らす
		audioSource[inputNumber].PlayOneShot(audioSource[inputNumber].clip);
	}

	//スライダー用------------------------------------------------------------------------------------------------------------
	public void SetBGMVolume(float value)	//BGMの音量をスライダーの値にセットする
	{
		audioSource[0].volume = defaultVolume[0] * 0.125f * value;
		audioSource[1].volume = defaultVolume[1] * 0.125f * value;

		bgmVolume = (int)value;
	}

	public void SetSEVolume(float value)	//SEの音量をスライダーの値にセットする
	{
		audioSource[2].volume = defaultVolume[2] * 0.125f * value;
		audioSource[3].volume = defaultVolume[3] * 0.125f * value;
		audioSource[4].volume = defaultVolume[4] * 0.125f * value;
		audioSource[5].volume = defaultVolume[5] * 0.125f * value;

		seVolume = (int)value;
	}
	
}
