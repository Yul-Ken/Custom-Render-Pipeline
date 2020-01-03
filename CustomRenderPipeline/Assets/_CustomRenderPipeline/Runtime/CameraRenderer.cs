using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraRenderer
{
    ScriptableRenderContext context;

    Camera camera;

    /// <summary>
    /// Render content in a camera view
    /// </summary>
    /// <param name="context"></param>
    /// <param name="camera"></param>
    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;

        //---- Setup Camera Properties
        Setup();

        //---- render job ----

        //context are buffered (not draw before submitting)
        DrawVisibleGeometry();
        Submit();
    }


    private void Setup()
    {
        context.SetupCameraProperties(camera);
    }


    private void DrawVisibleGeometry()
    {
        context.DrawSkybox(camera);
    }

    void Submit()
    {
        context.Submit();
    }
}