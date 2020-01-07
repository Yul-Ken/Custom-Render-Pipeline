using System;
using UnityEngine;
using UnityEngine.Rendering;
public class CustomRenderPipeline : RenderPipeline
{

    public CustomRenderPipeline()
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
    }

    CameraRenderer cameraRenderer = new CameraRenderer();

    //Each frame Unity invokes Render on the RP instance
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            var camera = cameras[i];
            cameraRenderer.Render(context, camera);
        }
    }
}
