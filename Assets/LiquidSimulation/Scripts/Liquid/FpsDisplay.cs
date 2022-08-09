using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FpsCounter))]
public class FpsDisplay : MonoBehaviour
{
    [SerializeField] private Text FpsValue;
    private FpsCounter _fpsCounter;

    private void OnEnable() 
    {
        _fpsCounter = GetComponent<FpsCounter>();

    }

    private void Update() 
    {
        FpsValue.text = _fpsCounter.AverageFps.ToString();
    }
}
