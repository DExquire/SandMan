using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    [SerializeField] private int FrameRange = 60;
    private int[] FpsBuffer;
    private int FpsBufferIndex;
    public int AverageFps {get; private set;}
    void Update()
    {
        //AverageFps = (int)(1f/Time.unscaledDeltaTime);
        if(FpsBuffer == null) //|| FpsBuffer.Length != FrameRange)
        {
            InitializeBuffer();
        }
        UpdateBuffer();
        CalculateFps();
    }

    private void InitializeBuffer()
    {
        if(FrameRange <= 0)
        {
            FrameRange = 1;
        }
        FpsBuffer = new int[FrameRange];
        FpsBufferIndex = 0;
    }

    private void UpdateBuffer()
    {
        FpsBuffer[FpsBufferIndex++] = (int)(1f/Time.unscaledDeltaTime);
        if(FpsBufferIndex >= FrameRange)
        {
            FpsBufferIndex = 0;
        }
    }

    private void CalculateFps()
    {
        int sum = 0;
        for(int i = 0; i < FrameRange; i++)
        {
            sum += FpsBuffer[i];
        }
        AverageFps = sum / FpsBuffer.Length;
    }
}
