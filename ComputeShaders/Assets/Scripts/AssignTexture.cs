using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignTexture : MonoBehaviour
{
    public ComputeShader shader;
    public int textureResolution = 256;
    private Renderer _renderer;
    private RenderTexture outputTexture;
    private int kernelHandle;
    
    // Start is called before the first frame update
    void Start()
    {
        outputTexture = new RenderTexture(textureResolution, textureResolution, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();

        _renderer = GetComponent<Renderer>();
        _renderer.enabled = true;
        
        InitShader();
    }

    private void DispachShader(int x, int y)
    {
        shader.Dispatch(kernelHandle, x, y, 1);
    }
    
    private void InitShader()
    {
        kernelHandle = shader.FindKernel("CSMain");
        shader.SetTexture(kernelHandle, "Result", outputTexture);
        _renderer.material.SetTexture("_MainTex", outputTexture);
        
        DispachShader(textureResolution/16, textureResolution/16);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.U))
        {
            DispachShader(textureResolution/8, textureResolution/8);
        }
    }
}
