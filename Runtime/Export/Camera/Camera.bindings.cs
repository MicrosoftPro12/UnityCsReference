// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Bindings;
using UnityEngine.Rendering;
using uei = UnityEngine.Internal;

using OpaqueSortMode = UnityEngine.Rendering.OpaqueSortMode;
using CameraEvent = UnityEngine.Rendering.CameraEvent;
using CommandBuffer = UnityEngine.Rendering.CommandBuffer;
using ComputeQueueType = UnityEngine.Rendering.ComputeQueueType;

namespace UnityEngine
{
    [NativeHeader("Runtime/Camera/Camera.h")]
    [NativeHeader("Runtime/Camera/RenderManager.h")]
    [NativeHeader("Runtime/GfxDevice/GfxDeviceTypes.h")]
    [NativeHeader("Runtime/Graphics/RenderTexture.h")]
    [NativeHeader("Runtime/Graphics/CommandBuffer/RenderingCommandBuffer.h")]
    [NativeHeader("Runtime/Misc/GameObjectUtility.h")]
    [NativeHeader("Runtime/Shaders/Shader.h")]
    [UsedByNativeCode]
    [RequireComponent(typeof(Transform))]
    public sealed partial class Camera : Behaviour
    {
        /// <summary>
        /// The minimum allowed aperture.
        /// </summary>
        public const float kMinAperture = 0.7f;

        /// <summary>
        /// The maximum allowed aperture.
        /// </summary>
        public const float kMaxAperture = 32f;

        /// <summary>
        /// The minimum blade count for the aperture diaphragm.
        /// </summary>
        public const int kMinBladeCount = 3;

        /// <summary>
        /// The maximum blade count for the aperture diaphragm.
        /// </summary>
        public const int kMaxBladeCount = 11;

        public Camera() {}

        [NativeProperty("Near")] extern public float nearClipPlane { get; set; }
        [NativeProperty("Far")]  extern public float farClipPlane  { get; set; }
        [NativeProperty("VerticalFieldOfView")]  extern public float fieldOfView   { get; set; }

        extern public RenderingPath renderingPath { get; set; }
        extern public RenderingPath actualRenderingPath {[NativeName("CalculateRenderingPath")] get;  }

        extern public void Reset();
        extern public bool allowHDR { get; set; }
        extern public bool allowMSAA { get; set; }
        extern public bool allowDynamicResolution { get; set; }
        [NativeProperty("ForceIntoRT")] extern public bool forceIntoRenderTexture { get; set; }

        extern public float orthographicSize { get; set; }
        extern public bool  orthographic { get; set; }

        extern public OpaqueSortMode opaqueSortMode { get; set; }
        extern public TransparencySortMode transparencySortMode { get; set; }
        extern public Vector3 transparencySortAxis { get; set; }
        extern public void ResetTransparencySortSettings();

        extern public float depth { get; set; }
        extern public float aspect { get; set; }
        extern public void ResetAspect();

        extern public Vector3 velocity { get; }

        extern public int cullingMask { get; set; }
        extern public int eventMask { get; set; }
        public bool layerCullSpherical
        {
            get { return layerCullSphericalInternal; }
            set
            {
                if (GraphicsSettings.currentRenderPipeline != null)
                {
                    Debug.LogWarning("Your project uses a scriptable render pipeline. You can use Camera.layerCullSpherical only with the built-in renderer.");
                }

                layerCullSphericalInternal = value;
            }
        }
        [NativeProperty("LayerCullSpherical")]extern internal bool layerCullSphericalInternal { get; set; }
        extern public CameraType cameraType { get; set; }

        extern internal Material skyboxMaterial { get; }

        [NativeConditional("UNITY_EDITOR")]
        extern public ulong  overrideSceneCullingMask     { get; set; }

        [NativeConditional("UNITY_EDITOR")]
        extern internal ulong sceneCullingMask { get; }

        [NativeConditional("UNITY_EDITOR")]
        extern internal bool useInteractiveLightBakingData { get; set; }

        [FreeFunction("CameraScripting::GetLayerCullDistances", HasExplicitThis = true)] extern private float[] GetLayerCullDistances();
        [FreeFunction("CameraScripting::SetLayerCullDistances", HasExplicitThis = true)] extern private void SetLayerCullDistances([NotNull] float[] d);
        public float[] layerCullDistances
        {
            get { return GetLayerCullDistances(); }
            set
            {
                if (value.Length != 32)
                    throw new UnityException("Array needs to contain exactly 32 floats for layerCullDistances.");
                SetLayerCullDistances(value);
            }
        }

        [Obsolete("PreviewCullingLayer is obsolete. Use scene culling masks instead.", false)]
        internal static int PreviewCullingLayer { get { return 31; } } // Return 31 because this used to be the PreviewCullingLayer stored in kPreviewLayer in Camera.h

        extern public bool useOcclusionCulling { get; set; }
        extern public Matrix4x4 cullingMatrix { get; set; }
        extern public void ResetCullingMatrix();

        extern public Color backgroundColor { get; set; }
        extern public CameraClearFlags clearFlags { get; set; }

        extern public DepthTextureMode depthTextureMode { get; set; }
        extern public bool clearStencilAfterLightingPass { get; set; }

        extern public void SetReplacementShader(Shader shader, string replacementTag);
        extern public void ResetReplacementShader();

        internal enum ProjectionMatrixMode{ Explicit, Implicit, PhysicalPropertiesBased }

        extern internal ProjectionMatrixMode projectionMatrixMode { get; }

        public enum GateFitMode{ Vertical = 1 , Horizontal = 2, Fill = 3, Overscan = 4, None = 0 }
        extern public bool usePhysicalProperties { get; set; }


        extern public int iso  { get; set; }
        extern public float shutterSpeed  { get; set; }
        extern public float aperture  { get; set; }
        extern public float focusDistance  { get; set; }
        extern public float focalLength  { get; set; }
        extern public int bladeCount  { get; set; }
        extern public Vector2 curvature  { get; set; }
        extern public float barrelClipping  { get; set; }
        extern public float anamorphism  { get; set; }
        extern public Vector2 sensorSize  { get; set; }
        extern public Vector2 lensShift  { get; set; }
        extern public GateFitMode gateFit  { get; set; }
        public enum FieldOfViewAxis { Vertical, Horizontal }
        extern public float GetGateFittedFieldOfView();
        extern public Vector2 GetGateFittedLensShift();

        extern internal Vector3 GetLocalSpaceAim();
        [NativeProperty("NormalizedViewportRect")] extern public Rect rect      { get; set; }
        [NativeProperty("ScreenViewportRect")]     extern public Rect pixelRect { get; set; }

        extern public int pixelWidth  {[FreeFunction("CameraScripting::GetPixelWidth",  HasExplicitThis = true)] get; }
        extern public int pixelHeight {[FreeFunction("CameraScripting::GetPixelHeight", HasExplicitThis = true)] get; }

        extern public int scaledPixelWidth  {[FreeFunction("CameraScripting::GetScaledPixelWidth",  HasExplicitThis = true)] get; }
        extern public int scaledPixelHeight {[FreeFunction("CameraScripting::GetScaledPixelHeight", HasExplicitThis = true)] get; }

        extern public RenderTexture targetTexture { get; set; }
        extern public RenderTexture activeTexture {[NativeName("GetCurrentTargetTexture")] get; }
        extern public int targetDisplay { get; set; }

        [FreeFunction("CameraScripting::SetTargetBuffers",  HasExplicitThis = true)] extern private void SetTargetBuffersImpl(RenderBuffer color, RenderBuffer depth);
        public void SetTargetBuffers(RenderBuffer colorBuffer, RenderBuffer depthBuffer) { SetTargetBuffersImpl(colorBuffer, depthBuffer); }

        [FreeFunction("CameraScripting::SetTargetBuffers",  HasExplicitThis = true)] extern private void SetTargetBuffersMRTImpl(RenderBuffer[] color, RenderBuffer depth);
        public void SetTargetBuffers(RenderBuffer[] colorBuffer, RenderBuffer depthBuffer) { SetTargetBuffersMRTImpl(colorBuffer, depthBuffer); }

        extern internal string[] GetCameraBufferWarnings();


        extern public Matrix4x4 cameraToWorldMatrix { get; }
        extern public Matrix4x4 worldToCameraMatrix { get; set; }
        extern public Matrix4x4 projectionMatrix    { get; set; }
        extern public Matrix4x4 nonJitteredProjectionMatrix { get; set; }
        [NativeProperty("UseJitteredProjectionMatrixForTransparent")] extern public bool useJitteredProjectionMatrixForTransparentRendering { get; set; }
        extern public Matrix4x4 previousViewProjectionMatrix { get; }
        extern public void ResetWorldToCameraMatrix();
        extern public void ResetProjectionMatrix();

        [FreeFunction("CameraScripting::CalculateObliqueMatrix", HasExplicitThis = true)] extern public Matrix4x4 CalculateObliqueMatrix(Vector4 clipPlane);

        extern public Vector3 WorldToScreenPoint(Vector3 position, MonoOrStereoscopicEye eye);
        extern public Vector3 WorldToViewportPoint(Vector3 position, MonoOrStereoscopicEye eye);
        extern public Vector3 ViewportToWorldPoint(Vector3 position, MonoOrStereoscopicEye eye);
        extern public Vector3 ScreenToWorldPoint(Vector3 position, MonoOrStereoscopicEye eye);
        public Vector3 WorldToScreenPoint(Vector3 position) { return WorldToScreenPoint(position, MonoOrStereoscopicEye.Mono); }
        public Vector3 WorldToViewportPoint(Vector3 position) { return WorldToViewportPoint(position, MonoOrStereoscopicEye.Mono); }
        public Vector3 ViewportToWorldPoint(Vector3 position) { return ViewportToWorldPoint(position, MonoOrStereoscopicEye.Mono); }
        public Vector3 ScreenToWorldPoint(Vector3 position) { return ScreenToWorldPoint(position, MonoOrStereoscopicEye.Mono); }
        extern public Vector3 ScreenToViewportPoint(Vector3 position);

        extern public Vector3 ViewportToScreenPoint(Vector3 position);

        extern internal Vector2 GetFrustumPlaneSizeAt(float distance);

        extern private Ray ViewportPointToRay(Vector2 pos, MonoOrStereoscopicEye eye);
        public Ray ViewportPointToRay(Vector3 pos, MonoOrStereoscopicEye eye) { return ViewportPointToRay((Vector2)pos, eye); }
        public Ray ViewportPointToRay(Vector3 pos) { return ViewportPointToRay(pos, MonoOrStereoscopicEye.Mono); }

        extern private Ray ScreenPointToRay(Vector2 pos, MonoOrStereoscopicEye eye);
        public Ray ScreenPointToRay(Vector3 pos, MonoOrStereoscopicEye eye) { return ScreenPointToRay((Vector2)pos, eye); }
        public Ray ScreenPointToRay(Vector3 pos) { return ScreenPointToRay(pos, MonoOrStereoscopicEye.Mono); }

        [FreeFunction("CameraScripting::CalculateViewportRayVectors", HasExplicitThis = true)]
        extern private void CalculateFrustumCornersInternal(Rect viewport, float z, MonoOrStereoscopicEye eye, [Out] Vector3[] outCorners);

        public void CalculateFrustumCorners(Rect viewport, float z, MonoOrStereoscopicEye eye, Vector3[] outCorners)
        {
            if (outCorners == null)     throw new ArgumentNullException("outCorners");
            if (outCorners.Length < 4)  throw new ArgumentException("outCorners minimum size is 4", "outCorners");
            CalculateFrustumCornersInternal(viewport, z, eye, outCorners);
        }

        public struct GateFitParameters
        {
            public GateFitMode mode {get; set; }
            public float aspect {get; set; }

            public GateFitParameters(GateFitMode mode, float aspect)
            {
                this.mode = mode;
                this.aspect = aspect;
            }
        }

        [NativeName("CalculateProjectionMatrixFromPhysicalProperties")]
        extern private static void CalculateProjectionMatrixFromPhysicalPropertiesInternal(out Matrix4x4 output, float focalLength, Vector2 sensorSize, Vector2 lensShift, float nearClip, float farClip, float gateAspect, GateFitMode gateFitMode);

        public static void CalculateProjectionMatrixFromPhysicalProperties(out Matrix4x4 output, float focalLength, Vector2 sensorSize, Vector2 lensShift, float nearClip, float farClip, GateFitParameters gateFitParameters = default(GateFitParameters))
        {
            CalculateProjectionMatrixFromPhysicalPropertiesInternal(out output, focalLength, sensorSize, lensShift, nearClip, farClip, gateFitParameters.aspect, gateFitParameters.mode);
        }

        [NativeName("FocalLengthToFieldOfView_Safe")]
        extern public static float FocalLengthToFieldOfView(float focalLength, float sensorSize);

        [NativeName("FieldOfViewToFocalLength_Safe")]
        extern public static float FieldOfViewToFocalLength(float fieldOfView, float sensorSize);

        [NativeName("HorizontalToVerticalFieldOfView_Safe")]
        extern public static float HorizontalToVerticalFieldOfView(float horizontalFieldOfView, float aspectRatio);
        extern public static float VerticalToHorizontalFieldOfView(float verticalFieldOfView, float aspectRatio);

        extern public static Camera main {[FreeFunction("FindMainCamera")] get; }
        public static Camera current {
            get
            {
                return currentInternal;
            }
        }
        extern private static Camera currentInternal { [FreeFunction("GetCurrentCameraPPtr")] get; }

        extern public UnityEngine.SceneManagement.Scene scene
        {
            [FreeFunction("CameraScripting::GetScene", HasExplicitThis = true)] get;
            [FreeFunction("CameraScripting::SetScene", HasExplicitThis = true)] set;
        }


        public enum StereoscopicEye { Left, Right }
        public enum MonoOrStereoscopicEye { Left, Right, Mono }

        extern public bool stereoEnabled
        {
            [NativeMethod("GetStereoEnabledForBuiltInOrSRP")]
            get;
        }
        extern public float stereoSeparation  { get; set; }
        extern public float stereoConvergence { get; set; }
        extern public bool  areVRStereoViewMatricesWithinSingleCullTolerance {[NativeName("AreVRStereoViewMatricesWithinSingleCullTolerance")] get; }
        public StereoTargetEyeMask stereoTargetEye
        {
            get { return stereoTargetEyeInternal; }
            set
            {
                if (GraphicsSettings.currentRenderPipeline != null)
                {
                    Debug.LogWarning("Your project uses a scriptable render pipeline. You can use Camera.stereoTargetEye only with the built-in renderer.");
                }

                stereoTargetEyeInternal = value;
            }
        }
        [NativeProperty("StereoTargetEye")]extern internal StereoTargetEyeMask stereoTargetEyeInternal { get; set; }
        extern public MonoOrStereoscopicEye stereoActiveEye {[FreeFunction("CameraScripting::GetStereoActiveEye", HasExplicitThis = true)] get; }

        extern public Matrix4x4 GetStereoNonJitteredProjectionMatrix(StereoscopicEye eye);

        [FreeFunction("CameraScripting::GetStereoViewMatrix", HasExplicitThis = true)]
        extern public Matrix4x4 GetStereoViewMatrix(StereoscopicEye eye);
        extern public void CopyStereoDeviceProjectionMatrixToNonJittered(StereoscopicEye eye);

        [FreeFunction("CameraScripting::GetStereoProjectionMatrix", HasExplicitThis = true)]
        extern public Matrix4x4 GetStereoProjectionMatrix(StereoscopicEye eye);
        extern public void SetStereoProjectionMatrix(StereoscopicEye eye, Matrix4x4 matrix);
        extern public void ResetStereoProjectionMatrices();

        extern public void SetStereoViewMatrix(StereoscopicEye eye, Matrix4x4 matrix);
        extern public void ResetStereoViewMatrices();


        [FreeFunction("CameraScripting::GetAllCamerasCount")] extern private static int GetAllCamerasCount();
        [FreeFunction("CameraScripting::GetAllCameras")] extern private static int GetAllCamerasImpl([Out][NotNull] Camera[] cam);

        public static int allCamerasCount { get { return GetAllCamerasCount(); } }
        public static Camera[] allCameras
        {
            get { Camera[] cam = new Camera[allCamerasCount]; GetAllCamerasImpl(cam); return cam; }
        }
        public static int GetAllCameras(Camera[] cameras)
        {
            if (cameras == null)
                throw new NullReferenceException();

            if (cameras.Length < allCamerasCount)
                throw new ArgumentException("Passed in array to fill with cameras is to small to hold the number of cameras. Use Camera.allCamerasCount to get the needed size.");
            return GetAllCamerasImpl(cameras);
        }

        [FreeFunction("CameraScripting::RenderToCubemap", HasExplicitThis = true)] extern private bool RenderToCubemapImpl(Texture tex, [uei.DefaultValue("63")] int faceMask);
        public bool RenderToCubemap(Cubemap cubemap, int faceMask)          { return RenderToCubemapImpl(cubemap, faceMask); }
        public bool RenderToCubemap(Cubemap cubemap)                        { return RenderToCubemapImpl(cubemap, 63); }
        public bool RenderToCubemap(RenderTexture cubemap, int faceMask)    { return RenderToCubemapImpl(cubemap, faceMask); }
        public bool RenderToCubemap(RenderTexture cubemap)                  { return RenderToCubemapImpl(cubemap, 63); }

        public enum SceneViewFilterMode
        {
            Off = 0,
            ShowFiltered = 1
        }

        [NativeConditional("UNITY_EDITOR")]
        extern private int GetFilterMode();

        [NativeConditional("UNITY_EDITOR")]
        public SceneViewFilterMode sceneViewFilterMode
        {
            get
            {
                return (SceneViewFilterMode)GetFilterMode();
            }
        }

        [NativeConditional("UNITY_EDITOR")]
        extern public bool renderCloudsInSceneView { get; set; }


        // TODO: it should be collapsed with others
        [NativeName("RenderToCubemap")] extern private bool RenderToCubemapEyeImpl(RenderTexture cubemap, int faceMask, MonoOrStereoscopicEye stereoEye);
        public bool RenderToCubemap(RenderTexture cubemap, int faceMask, MonoOrStereoscopicEye stereoEye)
        {
            return RenderToCubemapEyeImpl(cubemap, faceMask, stereoEye);
        }

        [FreeFunction("CameraScripting::Render", HasExplicitThis = true)]            extern public void Render();
        [FreeFunction("CameraScripting::RenderWithShader", HasExplicitThis = true)]  extern public void RenderWithShader(Shader shader, string replacementTag);
        [FreeFunction("CameraScripting::RenderDontRestore", HasExplicitThis = true)] extern public void RenderDontRestore();

        public void SubmitRenderRequest<RequestData>(RequestData renderRequest)
        {
            if (renderRequest == null)
                throw new ArgumentException($"{nameof(SubmitRenderRequest)} is invoked with invalid renderRequests");

            if (renderRequest is ObjectIdRequest objectIdRequest)
            {
                if (objectIdRequest.destination.depthStencilFormat == Experimental.Rendering.GraphicsFormat.None)
                {
                    Debug.LogWarning("ObjectId Render Request submitted without a depth stencil, which can produce results that are not depth tested correctly");
                }
                if (GraphicsSettings.currentRenderPipeline == null || !RenderPipelineManager.currentPipeline.IsRenderRequestSupported(this, objectIdRequest))
                {
                    // If the render pipeline supports object id rendering let the pipeline handle it.
                    // Otherwise rely on the built-in support through "magic", which also works with both hdrp/urp shaders
                    HandleBuiltInObjectIDRenderRequest(objectIdRequest);
                    return;
                }
            }
            if (GraphicsSettings.currentRenderPipeline == null)
            {
                Debug.LogWarning("Trying to invoke 'SubmitRenderRequest' when no SRP is set. A scriptable render pipeline is needed for this function call");
                return;
            }
            SubmitRenderRequestsInternal(renderRequest);
        }

        void HandleBuiltInObjectIDRenderRequest(ObjectIdRequest renderRequest)
        {
            UnityEngine.Object[] objects;
            objects = SubmitBuiltInObjectIDRenderRequest(
                renderRequest.destination,
                renderRequest.mipLevel,
                renderRequest.face,
                renderRequest.slice);
            renderRequest.result = new ObjectIdResult(objects);
        }

        [FreeFunction("CameraScripting::SubmitRenderRequests", HasExplicitThis = true)]  extern private void SubmitRenderRequestsInternal(object requests);
        [FreeFunction("CameraScripting::SubmitBuiltInObjectIDRenderRequest", HasExplicitThis = true)] [NativeConditional("UNITY_EDITOR")] [return: Unmarshalled]
        extern private UnityEngine.Object[] SubmitBuiltInObjectIDRenderRequest(
            RenderTexture target,
            int mipLevel,
            CubemapFace cubemapFace,
            int depthSlice);
        [FreeFunction("CameraScripting::SetupCurrent")] extern public static void SetupCurrent(Camera cur);
        [FreeFunction("CameraScripting::CopyFrom", HasExplicitThis = true)] extern public void CopyFrom(Camera other);

        extern public int  commandBufferCount { get; }
        [NativeName("RemoveCommandBuffers")] extern void RemoveCommandBuffersImpl(CameraEvent evt);
        [NativeName("RemoveAllCommandBuffers")] extern void RemoveAllCommandBuffersImpl();

        public void RemoveCommandBuffers(CameraEvent evt)
        {
		    if(RenderPipelineManager.currentPipeline != null)
            {
                Debug.LogWarning("Your project uses a scriptable render pipeline. You can use Camera.RemoveCommandBuffers only with the built-in renderer.");
            }
            else
            {
			    m_NonSerializedVersion++;
                RemoveCommandBuffersImpl(evt);
            }
        }

        public void RemoveAllCommandBuffers()
        {
            if (RenderPipelineManager.currentPipeline != null)
            {
                Debug.LogWarning("Your project uses a scriptable render pipeline. You can use Camera.RemoveAllCommandBuffers only with the built-in renderer.");
            }
            else
            {
			    m_NonSerializedVersion++;
                RemoveAllCommandBuffersImpl();
            }
        }

        // in old bindings these functions code like this:
        //   self->AddCommandBuffer(evt, &*buffer);
        // this dereference generated null-ref exception (as opposed to "normal" argument-null exception)
        // we want to preserve this behaviour

        // extern public void AddCommandBuffer(CameraEvent evt, [NotNull] CommandBuffer buffer);
        // extern public void RemoveCommandBuffer(CameraEvent evt, [NotNull] CommandBuffer buffer);
        [NativeName("AddCommandBuffer")]      extern private void AddCommandBufferImpl(CameraEvent evt, [NotNull] CommandBuffer buffer);
        [NativeName("AddCommandBufferAsync")] extern private void AddCommandBufferAsyncImpl(CameraEvent evt, [NotNull] CommandBuffer buffer, ComputeQueueType queueType);
        [NativeName("RemoveCommandBuffer")]   extern private void RemoveCommandBufferImpl(CameraEvent evt, [NotNull] CommandBuffer buffer);

        public void AddCommandBuffer(CameraEvent evt, CommandBuffer buffer)
        {
            if (!Rendering.CameraEventUtils.IsValid(evt))
                throw new ArgumentException(string.Format(@"Invalid CameraEvent value ""{0}"".", (int)evt), "evt");
            if (buffer == null) throw new NullReferenceException("buffer is null");

            if (RenderPipelineManager.currentPipeline != null)
            {
                Debug.LogWarning("Your project uses a scriptable render pipeline. You can use Camera.AddCommandBuffer only with the built-in renderer.");
            }
            else
            {
                AddCommandBufferImpl(evt, buffer);
                m_NonSerializedVersion++;
            }
        }

        public void AddCommandBufferAsync(CameraEvent evt, CommandBuffer buffer, ComputeQueueType queueType)
        {
            if (!Rendering.CameraEventUtils.IsValid(evt))
                throw new ArgumentException(string.Format(@"Invalid CameraEvent value ""{0}"".", (int)evt), "evt");
            if (buffer == null) throw new NullReferenceException("buffer is null");

            if (RenderPipelineManager.currentPipeline != null)
            {
                Debug.LogWarning("Your project uses a scriptable render pipeline. You can use Camera.AddCommandBufferAsync only with the built-in renderer.");
            }
            else
            {
                AddCommandBufferAsyncImpl(evt, buffer, queueType);
                m_NonSerializedVersion++;
            }
        }

        public void RemoveCommandBuffer(CameraEvent evt, CommandBuffer buffer)
        {
            if (!Rendering.CameraEventUtils.IsValid(evt))
                throw new ArgumentException(string.Format(@"Invalid CameraEvent value ""{0}"".", (int)evt), "evt");
            if (buffer == null) throw new NullReferenceException("buffer is null");

            if (RenderPipelineManager.currentPipeline != null)
            {
                Debug.LogWarning("Your project uses a scriptable render pipeline. You can use Camera.RemoveCommandBuffer only with the built-in renderer.");
            }
            else
            {
                RemoveCommandBufferImpl(evt, buffer);
                m_NonSerializedVersion++;
            }
        }

        public UnityEngine.Rendering.CommandBuffer[] GetCommandBuffers(UnityEngine.Rendering.CameraEvent evt)
        {
            if(RenderPipelineManager.currentPipeline != null)
            {
                Debug.LogWarning("Your project uses a scriptable render pipeline. You can use Camera.GetCommandBuffers only with the built-in renderer.");
            }

            return GetCommandBuffersImpl(evt);
        }

        [FreeFunction("CameraScripting::GetCommandBuffers", HasExplicitThis = true)]
        extern internal UnityEngine.Rendering.CommandBuffer[] GetCommandBuffersImpl(UnityEngine.Rendering.CameraEvent evt);
	    internal uint m_NonSerializedVersion;
    }

    public partial class Camera
    {
        // called before a camera culls the scene.
        // void OnPreCull();

        // called before a camera starts rendering the scene.
        // void OnPreRender();

        // called after a camera has finished rendering the scene.
        // void OnPostRender();

        // called after all rendering is complete to render image
        // void OnRenderImage(RenderTexture source, RenderTexture destination);

        // called after camera has rendered the scene.
        // void OnRenderObject();

        // called once for each camera if the object is visible.
        // void OnWillRenderObject();

        public delegate void CameraCallback(Camera cam);

        public static CameraCallback onPreCull;
        public static CameraCallback onPreRender;
        public static CameraCallback onPostRender;

        [RequiredByNativeCode]
        private static void FireOnPreCull(Camera cam)
        {
            if (onPreCull != null)
                onPreCull(cam);
        }

        [RequiredByNativeCode]
        private static void FireOnPreRender(Camera cam)
        {
            if (onPreRender != null)
                onPreRender(cam);
        }

        [RequiredByNativeCode]
        private static void FireOnPostRender(Camera cam)
        {
            if (onPostRender != null)
                onPostRender(cam);
        }

        [RequiredByNativeCode]
        private static void BumpNonSerializedVersion(Camera cam)
        {
            cam.m_NonSerializedVersion++;
        }

        // These two empty internal methods (which will always be stripped) are required to make the EmptyBuildGotStrippedEnough test work.
        internal void OnlyUsedForTesting1()
        {
        }

        internal void OnlyUsedForTesting2()
        {
        }

        public unsafe bool TryGetCullingParameters(out Rendering.ScriptableCullingParameters cullingParameters)
        {
            return GetCullingParameters_Internal(this, false, out cullingParameters, sizeof(Rendering.ScriptableCullingParameters));
        }

        public unsafe bool TryGetCullingParameters(bool stereoAware, out Rendering.ScriptableCullingParameters cullingParameters)
        {
            return GetCullingParameters_Internal(this, stereoAware, out cullingParameters, sizeof(Rendering.ScriptableCullingParameters));
        }

        [NativeHeader("Runtime/Export/RenderPipeline/ScriptableRenderPipeline.bindings.h")]
        [FreeFunction("ScriptableRenderPipeline_Bindings::GetCullingParameters_Internal")]
        extern private static bool GetCullingParameters_Internal(Camera camera, bool stereoAware, out Rendering.ScriptableCullingParameters cullingParameters, int managedCullingParametersSize);
    }
}
