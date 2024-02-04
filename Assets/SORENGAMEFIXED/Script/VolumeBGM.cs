using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeBGM : MonoBehaviour
{
    Slider thisSlider;

    // Start is called before the first frame update
    void Start()
    {
        thisSlider = GetComponent<Slider>();
        thisSlider.value = (float)AudioManager.bgmVolume;
    }

    public void SetBGMVolume(float value)
    {
        AudioManager.instance.SetBGMVolume(value);
    }
}
