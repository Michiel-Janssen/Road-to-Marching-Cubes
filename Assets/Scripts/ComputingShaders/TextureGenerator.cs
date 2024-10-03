using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGenerator : MonoBehaviour
{
    public ComputeShader TextureShader;
    private RenderTexture _rTexture;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_rTexture == null)
        {
            /* Visualize compute shader using rendertexture */
            _rTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _rTexture.enableRandomWrite = true;
            _rTexture.Create();
        }
        /* Kernels can be used with their respective number in the line
         * For example:
         * #pragma kernel CSMain
         * #pragma kernel Random
         * 
         * Random would be 1, CSMain would be 0
         * Using Findkernel tho gives back an int so its safer.
         */
        int kernel = TextureShader.FindKernel("CSMain");
        TextureShader.SetTexture(kernel, "Result", _rTexture);

        /* Screen dimensions, divide by 8 because we are using [8, 8, 1] (threads per workgroup)
         * (1920 * 8 * 907 * 8 =) 111452160 threads.
         * So we need to divide by 8 to get the amount of workgroups (every workgroup has 8 threads).
         * The workgroup needs to be an integer so ceil it so you have enough space (better more than less)
         */
        int workgroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int workgroupsY = Mathf.CeilToInt(Screen.height / 8.0f);

        /* Set the computing shader to the texture */
        TextureShader.Dispatch(kernel, workgroupsX, workgroupsY, 1);
        Graphics.Blit(_rTexture, destination);
    }
}
