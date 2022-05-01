using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MyCustomRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class FeatureSettings
    {
        // Public fields here will be displayed in the inspector
        public RenderPassEvent renderPass = RenderPassEvent.AfterRendering;
        public Material materialToBlit;
    }

    // MUST be named "settings" (lowercase) to be shown in the inspector
    public FeatureSettings settings = new FeatureSettings();

    private RenderTargetHandle _renderTargetHandle;
    private MyCustomRenderPass _myCustomRenderPass;
    
    public override void Create()
    {
        _myCustomRenderPass = new MyCustomRenderPass(
            "My Custom Render Pass",
            settings.renderPass,
            settings.materialToBlit
            );
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Gather up and pass any extra information our pass will need.
        // In this case we're getting the camera's color buffer target
        RenderTargetIdentifier cameraColorTargetIdent = renderer.cameraColorTarget;
        _myCustomRenderPass.Setup(cameraColorTargetIdent);

        // Ask the renderer to add our pass.
        // Could queue up multiple passes and/or pick passes to use
        renderer.EnqueuePass(_myCustomRenderPass);
    }
}