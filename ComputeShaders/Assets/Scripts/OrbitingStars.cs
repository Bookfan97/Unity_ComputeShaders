using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitingStars : MonoBehaviour
{
    public int starCount = 17;
    public ComputeShader shader;

    public GameObject prefab;

    int kernelHandle;
    uint threadGroupSizeX;
    int groupSizeX;
    
    Transform[] stars;
    ComputeBuffer resultBuffer;
    Vector3[] output;
    
    void Start()
    {
        kernelHandle = shader.FindKernel("OrbitingStars");
        shader.GetKernelThreadGroupSizes(kernelHandle, out threadGroupSizeX, out _, out _);
        groupSizeX = (int)((starCount + threadGroupSizeX - 1) / threadGroupSizeX);
        resultBuffer = new ComputeBuffer(starCount, sizeof(float)*3);
        shader.SetBuffer(kernelHandle, "Result", resultBuffer);
        output = new Vector3[starCount];
        stars = new Transform[starCount];
        for (int i = 0; i < starCount; i++)
        {
            stars[i] = Instantiate(prefab, transform).transform;
        }
    }

    void Update()
    {
        shader.SetFloat("time", Time.time);
        shader.Dispatch(kernelHandle, groupSizeX, 1,1);
        resultBuffer.GetData(output);
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].localPosition = output[i];
        }
    }

    private void OnDestroy()
    {
        resultBuffer.Dispose();
    }
}
