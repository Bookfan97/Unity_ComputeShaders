﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

public class QuadParticles : MonoBehaviour
{

    private Vector2 cursorPos;

    // struct
    struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;
        public float life;
    }

    const int SIZE_PARTICLE = 7 * sizeof(float);

    public int particleCount = 10000;
    public Material material;
    public ComputeShader shader;
    [Range(0.01f, 1.0f)]
    public float quadSize = 0.1f;

    int numParticles;
    int numVerticesInMesh;
    int kernelID;
    ComputeBuffer particleBuffer;
    
    int groupSizeX; 
    
    // Use this for initialization
    void Start()
    {
        Init();
    }

    void Init()
    {
        // find the id of the kernel
        kernelID = shader.FindKernel("CSMain");

        uint threadsX;
        shader.GetKernelThreadGroupSizes(kernelID, out threadsX, out _, out _);
        groupSizeX = Mathf.CeilToInt((float)particleCount / (float)threadsX);
        numParticles = groupSizeX * (int)threadsX;

        // initialize the particles
        Particle[] particleArray = new Particle[numParticles];

        int numVertices = numParticles * 6;
        
        Vector3 pos = new Vector3();
        
        for (int i = 0; i < numParticles; i++)
        {
            pos.Set(Random.value * 2 - 1.0f, Random.value * 2 - 1.0f, Random.value * 2 - 1.0f);
            pos.Normalize();
            pos *= Random.value;
            pos *= 0.5f;

            particleArray[i].position.Set(pos.x, pos.y, pos.z + 3);
            particleArray[i].velocity.Set(0,0,0);
          
            // Initial life value
            particleArray[i].life = Random.value * 5.0f + 1.0f;
        }

        // create compute buffers
        particleBuffer = new ComputeBuffer(numParticles, SIZE_PARTICLE);
        particleBuffer.SetData(particleArray);
        
        // bind the compute buffers to the shader and the compute shader
        shader.SetBuffer(kernelID, "particleBuffer", particleBuffer);
    }

    void OnRenderObject()
    {
        material.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Points, 1, numParticles);
    }

    void OnDestroy()
    {
        if (particleBuffer != null){
            particleBuffer.Release();
        }
    }

    // Update is called once per frame
    void Update()
    {
        float[] mousePosition2D = { cursorPos.x, cursorPos.y };

        // Send datas to the compute shader
        shader.SetFloat("deltaTime", Time.deltaTime);
        shader.SetFloats("mousePosition", mousePosition2D);

        // Update the Particles
        shader.Dispatch(kernelID, groupSizeX, 1, 1);
    }

    void OnGUI()
    {
        Vector3 p = new Vector3();
        Camera c = Camera.main;
        Event e = Event.current;
        Vector2 mousePos = new Vector2();

        // Get the mouse position from Event.
        // Note that the y position from Event is inverted.
        mousePos.x = e.mousePosition.x;
        mousePos.y = c.pixelHeight - e.mousePosition.y;

        p = c.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, c.nearClipPlane + 14));

        cursorPos.x = p.x;
        cursorPos.y = p.y;
        
    }
}
