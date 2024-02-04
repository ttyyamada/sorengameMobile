using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSE : MonoBehaviour
{
    Slider thisSlider;

    // Start is called before the first frame update
    void Start()
    {
        thisSlider = GetComponent<Slider>();
        thisSlider.value = (float)AudioManager.seVolume;
    }

    public void SetSEVolume(float value)
    {
        AudioManager.instance.SetSEVolume(value);
    }
}
