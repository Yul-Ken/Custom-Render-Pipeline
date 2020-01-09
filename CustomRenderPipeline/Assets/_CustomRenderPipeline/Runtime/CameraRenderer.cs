using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    ScriptableRenderContext context;

    Camera camera;

    //some commands have to be issued indirectly, via a separate command buffer
    const string bufferName = "Render Camera";

    CommandBuffer buffer = new CommandBuffer
    {
        name = bufferName
    };

    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    static ShaderTagId litShaderTagId = new ShaderTagId("CustomLit");



    /// <summary>
    /// Render content in a camera view
    /// </summary>
    /// <param name="context"></param>
    /// <param name="camera"></param>
    public void Render(ScriptableRenderContext context, Camera camera,
        bool useDynamicBatching, bool useGPUInstancing
        )
    {
        this.context = context;
        this.camera = camera;

        PrepareBuffer();
        PrepareForSceneWindow();

        if (!Cull())
        {
            return;
        }

        //---- Setup Camera Properties
        Setup();

        //---- render job ----
        //context are buffered (not draw before submitting)
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        //Draw Unsupported Meshes
        DrawUnsupportedShaders();
        DrawGizmos();

        Submit();
    }

   
    private void Setup()
    {
        context.SetupCameraProperties(camera);
        var flags = camera.clearFlags;
        // the skybox would be drawn at the beginning
        buffer.ClearRenderTarget(
            flags<=CameraClearFlags.Depth, // if nothing =>  not clear
            flags == CameraClearFlags.Color,  // if it is color =>  clear with color
            flags == CameraClearFlags.Color ?camera.backgroundColor.linear : Color.clear // transparent or color
            );
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
        
    }

    void Submit()
    {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }


    private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        context.DrawSkybox(camera);

        var sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };  //var sortingSettings = new SortingSettings(camera);
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings) {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing
        };
        drawingSettings.SetShaderPassName(1, litShaderTagId);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);


        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

        // Handle Transparent Object

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    CullingResults cullingResults;
    private bool Cull()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }
        return false;
    }
}