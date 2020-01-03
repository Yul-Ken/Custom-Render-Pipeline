using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraRenderer
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

    static ShaderTagId[] legacyShaderTagIds = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };
    static Material errorMaterial;

    /// <summary>
    /// Render content in a camera view
    /// </summary>
    /// <param name="context"></param>
    /// <param name="camera"></param>
    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;


        if (!Cull())
        {
            return;
        }

        //---- Setup Camera Properties
        Setup();

        //---- render job ----
        //context are buffered (not draw before submitting)
        DrawVisibleGeometry();
        //Draw Unsupported Meshes
        DrawUnsupportedShaders();

        Submit();
    }

    private void DrawUnsupportedShaders()
    {
        if(errorMaterial==null)
        {
            errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }


        var drawSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera))
        {
            overrideMaterial = errorMaterial
        };

        for (int i = 1; i < legacyShaderTagIds.Length; i++)
        {
            drawSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
        }
        var filteringSettings = FilteringSettings.defaultValue;
        context.DrawRenderers(cullingResults, ref drawSettings, ref filteringSettings); 
    }

    private void Setup()
    {
        context.SetupCameraProperties(camera);
        buffer.ClearRenderTarget(true, true, Color.clear);
        buffer.BeginSample(bufferName);
        ExecuteBuffer();
        
    }

    void Submit()
    {
        buffer.EndSample(bufferName);
        ExecuteBuffer();
        context.Submit();
    }


    private void DrawVisibleGeometry()
    {
        context.DrawSkybox(camera);

        var sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };  //var sortingSettings = new SortingSettings(camera);
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
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