using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

internal class MyCustomRenderPass : ScriptableRenderPass
{
  // used to label this pass in Unity's Frame Debug utility
  private readonly string _profilerTag;

  private readonly Material _materialToBlit;
  private RenderTargetIdentifier _cameraColorTargetIdent;
  private RenderTargetHandle _tempTexture;

  public MyCustomRenderPass(string profilerTag,
    RenderPassEvent renderPassEvent, Material materialToBlit)
  {
    _profilerTag = profilerTag;
    this.renderPassEvent = renderPassEvent;
    _materialToBlit = materialToBlit;
  }

  // This isn't part of the ScriptableRenderPass class and is our own addition.
  // For this custom pass we need the camera's color target, so that gets passed in.
  public void Setup(RenderTargetIdentifier cameraColorTargetIdent)
  {
    _cameraColorTargetIdent = cameraColorTargetIdent;
  }

  // called each frame before Execute, use it to set up things the pass will need
  public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
  {
    // create a temporary render texture that matches the camera
    cmd.GetTemporaryRT(_tempTexture.id, cameraTextureDescriptor);
  }

  // Execute is called for every eligible camera every frame. It's not called at the moment that
  // rendering is actually taking place, so don't directly execute rendering commands here.
  // Instead use the methods on ScriptableRenderContext to set up instructions.
  // RenderingData provides a bunch of (not very well documented) information about the scene
  // and what's being rendered.
  public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
  {
    
    // fetch a command buffer to use
    CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);
    cmd.Clear();

    context.InvokeOnRenderObjectCallback();
    
    Camera cam = renderingData.cameraData.camera; 
    cmd.SetGlobalMatrix("cam_frustum", FrustumCorners(cam)); 
    cmd.SetGlobalVector("cam_position", cam.transform.position);
    
    // the actual content of our custom render pass!
    // we apply our material while blitting to a temporary texture
    cmd.Blit(_cameraColorTargetIdent, _tempTexture.Identifier(), _materialToBlit, 0);

    // ...then blit it back again 
    cmd.Blit(_tempTexture.Identifier(), _cameraColorTargetIdent);
    
    // don't forget to tell ScriptableRenderContext to actually execute the commands
    context.ExecuteCommandBuffer(cmd);

    // tidy up after ourselves
    cmd.Clear();
    CommandBufferPool.Release(cmd);
  }
  
  // called after Execute, use it to clean up anything allocated in Configure
  public override void FrameCleanup(CommandBuffer cmd)
  {
    cmd.ReleaseTemporaryRT(_tempTexture.id);
  }
  
  private static Matrix4x4 FrustumCorners(Camera cam)
  {
    Transform camTransform = cam.transform;

    Vector3[] frustumCorners = new Vector3[4];
    cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1),
      cam.farClipPlane, cam.stereoActiveEye, frustumCorners);

    Matrix4x4 frustumVectorsArray = Matrix4x4.identity;
    frustumVectorsArray.SetRow(0, camTransform.TransformVector(frustumCorners[0]));
    frustumVectorsArray.SetRow(1, camTransform.TransformVector(frustumCorners[1]));
    frustumVectorsArray.SetRow(2, camTransform.TransformVector(frustumCorners[3]));
    frustumVectorsArray.SetRow(3, camTransform.TransformVector(frustumCorners[2]));

    return frustumVectorsArray;
  }
}
