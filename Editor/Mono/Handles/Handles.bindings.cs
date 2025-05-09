// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.Internal;
using UnityEngine.Rendering;

namespace UnityEditor
{
    [Flags]
    internal enum OutlineDrawMode
    {
        SelectionOutline = 1 << 0,
        SelectionWire = 1 << 1,
    }

    [NativeHeader("Editor/Src/Handles/Handles.bindings.h")]
    public sealed partial class Handles
    {
        // Are handles lit?
        [NativeProperty("handles::g_HandleLighting", true, TargetType.Field)]
        public static extern bool lighting { get; set; }

        // Colors of the handles
        [NativeProperty("handles::g_HandleColor", true, TargetType.Field)]
        public static extern Color color { get; set; }

        // ZTest of the handles
        [NativeProperty("handles::g_HandleZTest", true, TargetType.Field)]
        public static extern CompareFunction zTest { get; set; }

        // Matrix for all handle operations
        public static extern Matrix4x4 matrix
        {
            [FreeFunction("Internal_GetMatrix")] get;
            [FreeFunction("Internal_SetMatrix")] set;
        }

        [NativeProperty("handles::g_HandleInverseMatrix", true, TargetType.Field)]
        public static extern Matrix4x4 inverseMatrix { get; }

        [FreeFunction("handles::ClearHandles")]
        internal static extern void ClearHandles();

        [FreeFunction("Internal_DrawGizmos")]
        internal static extern void Internal_DoDrawGizmos([NotNull] Camera camera);

        [FreeFunction("Internal_IsCameraDrawModeSupported")]
        internal static extern bool IsCameraDrawModeSupported([NotNull] Camera camera, DrawCameraMode drawMode);

        [FreeFunction("Internal_IsCameraDrawModeEnabled")]
        internal static extern bool IsCameraDrawModeEnabled([NotNull] Camera camera, DrawCameraMode drawMode);

        [FreeFunction]
        static extern void Internal_DrawCameraWithGrid([NotNull] Camera cam, DrawCameraMode renderMode, ref DrawGridParameters gridParam, bool drawGizmos, bool drawSelection);

        [FreeFunction]
        internal static extern void Internal_DrawOutline(Color parentNodeColor, Color childNodeColor, EntityId submeshOutlineMaterialId, EntityId[] parentRenderers, EntityId[] childRenderers, OutlineDrawMode outlineMode, float parentOutlineAlpha = 0, float childOutlineAlpha = 0);

        [FreeFunction]
        static extern void Internal_DrawCamera([NotNull] Camera cam, DrawCameraMode renderMode, bool drawGizmos, bool drawSelection);

        [FreeFunction]
        static extern void Internal_DrawCameraWithFilter([NotNull] Camera cam, DrawCameraMode renderMode, bool drawGizmos, bool drawSelection, GameObject[] filter);

        [FreeFunction]
        internal static extern void Internal_FinishDrawingCamera([NotNull] Camera cam, [DefaultValue("true")] bool drawGizmos);
        static void Internal_FinishDrawingCamera(Camera cam) { Internal_FinishDrawingCamera(cam, true); }

        [FreeFunction]
        static extern void Internal_ClearCamera([NotNull] Camera cam);

        [FreeFunction]
        internal static extern void Internal_SetCurrentCamera(Camera cam);

        [FreeFunction("Internal_SetSceneViewColors")]
        internal static extern void SetSceneViewColors(Color wire, Color wireOverlay, Color selectedOutline, Color selectedChildrenOutline, Color selectedWire);

        [FreeFunction("Internal_SetSceneViewModeGIContributorsReceiversColors")]
        internal static extern void SetSceneViewModeGIContributorsReceiversColors(Color noContributeGI, Color receiveGILightmaps, Color receiveGILightProbesColor);

        [FreeFunction("Internal_EnableCameraFx")]
        internal static extern void EnableCameraFx([NotNull] Camera cam, bool fx);

        [FreeFunction("Internal_EnableCameraFlares")]
        internal static extern void EnableCameraFlares([NotNull] Camera cam, bool flares);

        [FreeFunction("Internal_EnableCameraSkybox")]
        internal static extern void EnableCameraSkybox([NotNull] Camera cam, bool skybox);

        // Setup viewport and stuff for a current camera.
        [FreeFunction]
        static extern void Internal_SetupCamera([NotNull] Camera cam);

        [FreeFunction]
        static extern void Internal_DrawAAPolyLine(Color[] colors, Vector3[] points, Color defaultColor, int actualNumberOfPoints, Texture2D texture, float width, Matrix4x4 toWorld);

        [FreeFunction]
        static extern void Internal_DrawAAConvexPolygon(Vector3[] points, Color defaultColor, int actualNumberOfPoints, Matrix4x4 toWorld);

        [FreeFunction]
        static extern void Internal_DrawBezier(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, Color color, Texture2D texture, float width, Matrix4x4 toWorld);

        [FreeFunction("Internal_SetDiscSectionPoints")]
        internal static extern void SetDiscSectionPoints(Vector3[] dest, Vector3 center, Vector3 normal, Vector3 from, float angle, float radius);

        [FreeFunction("Internal_EmitGUIGeometryForCamera")]
        internal static extern void EmitGUIGeometryForCamera([NotNull] Camera source, Camera dest);

        [FreeFunction("Internal_SetCameraFilterMode")]
        internal static extern void SetCameraFilterMode([NotNull] Camera camera, CameraFilterMode mode);

        [FreeFunction("Internal_GetCameraFilterMode")]
        internal static extern CameraFilterMode GetCameraFilterMode([NotNull] Camera camera);

        [FreeFunction("Internal_DrawCameraFade")]
        internal static extern void DrawCameraFade([NotNull] Camera camera, float fade);

        [FreeFunction]
        [return:Unmarshalled]
        static extern Vector3[] Internal_MakeBezierPoints(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, int division);
    }
}
