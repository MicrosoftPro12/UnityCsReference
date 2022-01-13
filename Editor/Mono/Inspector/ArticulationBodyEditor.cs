// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using UnityEditor.AnimatedValues;
using UnityEngine;

namespace UnityEditor
{
    [CustomEditor(typeof(ArticulationBody))]
    [CanEditMultipleObjects]
    internal class ArticulationBodyEditor : Editor
    {
        SerializedProperty m_Mass;
        SerializedProperty m_Immovable;
        SerializedProperty m_UseGravity;

        SerializedProperty m_LinearDamping;
        SerializedProperty m_AngularDamping;
        SerializedProperty m_JointFriction;

        SerializedProperty m_CollisionDetectionMode;

        SerializedProperty m_ParentAnchorPosition;
        SerializedProperty m_ParentAnchorRotation;
        SerializedProperty m_AnchorPosition;
        SerializedProperty m_AnchorRotation;
        SerializedProperty m_ComputeParentAnchor;

        SerializedProperty m_LinearX;
        SerializedProperty m_LinearY;
        SerializedProperty m_LinearZ;
        SerializedProperty m_SwingY;
        SerializedProperty m_SwingZ;
        SerializedProperty m_Twist;

        SerializedProperty m_ArticulationJointType;

        SerializedProperty m_XDrive;
        SerializedProperty m_YDrive;
        SerializedProperty m_ZDrive;

        readonly AnimBool m_ShowInfo = new AnimBool();
        bool m_RequiresConstantRepaint;
        SavedBool m_ShowInfoFoldout;

        private const float GizmoLinearSize = 0.5f;
        private const float CapScale = 0.03f;

        private class Styles
        {
            public static GUIContent mass = EditorGUIUtility.TrTextContent("Mass", "Mass of this articulation body");
            public static GUIContent immovable = EditorGUIUtility.TrTextContent("Immovable", "Is this articulation body immovable by forces and torques? Only applies to the root body of the articulation.");
            public static GUIContent useGravity = EditorGUIUtility.TrTextContent("Use Gravity", "Controls whether gravity affects this articulation body.");

            public static GUIContent collisionDetectionMode = EditorGUIUtility.TrTextContent("Collision Detection", "The method to use to detect collisions for child colliders: discrete (default) or various modes of continuous collision detection that can help solving fast moving object issues.");

            public static GUIContent linearDamping = EditorGUIUtility.TrTextContent("Linear Damping", "Damping factor that affects how this body resists linear motion.");
            public static GUIContent angularDamping = EditorGUIUtility.TrTextContent("Angular Damping", "Damping factor that affects how this body resists rotations.");
            public static GUIContent jointFriction = EditorGUIUtility.TrTextContent("Joint Friction", "Amount of friction that is applied as a result of connected bodies moving relative to this body.");

            public static GUIContent computeParentAnchor = EditorGUIUtility.TrTextContent("Compute Parent Anchor", "Controls whether to set the anchor relative to the parent to be the same as the anchor relative to this body.");
            public static GUIContent anchorPosition = EditorGUIUtility.TrTextContent("Anchor Position", "Position of the anchor relative to this body.");
            public static GUIContent parentAnchorPosition = EditorGUIUtility.TrTextContent("Parent Anchor Position", "Position of the anchor relative to the parent body.");
            public static GUIContent anchorRotation =  EditorGUIUtility.TrTextContent("Anchor Rotation", "Rotation of the anchor relative to this body.");
            public static GUIContent parentAnchorRotation = EditorGUIUtility.TrTextContent("Parent Anchor Rotation", "Rotation of the anchor relative to the parent body.");

            public static GUIContent prismaticAxis = EditorGUIUtility.TrTextContent("Axis", "The only axis the joint allows linear motion along.");
            public static GUIContent unlockedMotionType = EditorGUIUtility.TrTextContent("Motion", "Controls whether the motion is free or limited.");

            public static GUIContent lowerLimit = EditorGUIUtility.TrTextContent("Lower limit", "Limit the minimum linear or angular coordinate this drive allows.");
            public static GUIContent upperLimit = EditorGUIUtility.TrTextContent("Upper limit", "Limit the maximum linear or angular coordinate this drive allows.");

            public static GUIContent stiffness = EditorGUIUtility.TrTextContent("Stiffness", "The stiffness of the spring connected to this drive.");
            public static GUIContent damping = EditorGUIUtility.TrTextContent("Damping", "The damping of the spring attached to this drive.");
            public static GUIContent forceLimit = EditorGUIUtility.TrTextContent("Force Limit", "The maximum force this drive can apply to a body.");
            public static GUIContent target = EditorGUIUtility.TrTextContent("Target", "The target value for the drive to try reaching.");
            public static GUIContent targetVelocity = EditorGUIUtility.TrTextContent("Target Velocity", "The target velocity for the drive to try reaching.");
        }

        internal enum NonLockedMotion
        {
            Free = 2,
            Limited = 1
        }

        internal enum LinearDof
        {
            X,
            Y,
            Z
        }

        public void OnEnable()
        {
            m_Mass = serializedObject.FindProperty("m_Mass");
            m_Immovable = serializedObject.FindProperty("m_Immovable");
            m_UseGravity = serializedObject.FindProperty("m_UseGravity");

            m_CollisionDetectionMode = serializedObject.FindProperty("m_CollisionDetectionMode");

            m_LinearDamping = serializedObject.FindProperty("m_LinearDamping");
            m_AngularDamping = serializedObject.FindProperty("m_AngularDamping");
            m_JointFriction = serializedObject.FindProperty("m_JointFriction");

            m_ParentAnchorPosition = serializedObject.FindProperty("m_ParentAnchorPosition");
            m_ParentAnchorRotation = serializedObject.FindProperty("m_ParentAnchorRotation");
            m_AnchorPosition = serializedObject.FindProperty("m_AnchorPosition");
            m_AnchorRotation = serializedObject.FindProperty("m_AnchorRotation");
            m_ComputeParentAnchor = serializedObject.FindProperty("m_ComputeParentAnchor");
            m_ArticulationJointType = serializedObject.FindProperty("m_ArticulationJointType");

            m_LinearX = serializedObject.FindProperty("m_LinearX");
            m_LinearY = serializedObject.FindProperty("m_LinearY");
            m_LinearZ = serializedObject.FindProperty("m_LinearZ");
            m_SwingY = serializedObject.FindProperty("m_SwingY");
            m_SwingZ = serializedObject.FindProperty("m_SwingZ");
            m_Twist = serializedObject.FindProperty("m_Twist");

            m_XDrive = serializedObject.FindProperty("m_XDrive");
            m_YDrive = serializedObject.FindProperty("m_YDrive");
            m_ZDrive = serializedObject.FindProperty("m_ZDrive");

            // Info foldout
            m_ShowInfo.valueChanged.AddListener(Repaint);

            m_RequiresConstantRepaint = false;
            m_ShowInfoFoldout = new SavedBool($"{target.GetType()}.ShowFoldout", false);
            m_ShowInfo.value = m_ShowInfoFoldout.value;
        }

        public void OnDisable()
        {
            m_ShowInfo.valueChanged.RemoveListener(Repaint);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ArticulationBody body = (ArticulationBody)target;

            EditorGUILayout.PropertyField(m_Mass, Styles.mass);

            EditorGUILayout.PropertyField(m_UseGravity, Styles.useGravity);
            if (body.isRoot)
            {
                EditorGUILayout.PropertyField(m_Immovable, Styles.immovable);
                if (!m_Immovable.boolValue)
                {
                    EditorGUILayout.PropertyField(m_LinearDamping, Styles.linearDamping);
                    EditorGUILayout.PropertyField(m_AngularDamping, Styles.angularDamping);
                }

                EditorGUILayout.PropertyField(m_CollisionDetectionMode, Styles.collisionDetectionMode);

                EditorGUILayout.HelpBox("This is the root body of the articulation.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.PropertyField(m_LinearDamping, Styles.linearDamping);
                EditorGUILayout.PropertyField(m_AngularDamping, Styles.angularDamping);
                EditorGUILayout.PropertyField(m_JointFriction, Styles.jointFriction);

                EditorGUILayout.PropertyField(m_CollisionDetectionMode, Styles.collisionDetectionMode);
                EditorGUILayout.PropertyField(m_ComputeParentAnchor, Styles.computeParentAnchor);

                // Show anchor edit fields and set to joint if changed
                // The reason we have change checks here is because in AwakeFromLoad we won't overwrite anchors
                // If we were to do that, simulation would drift caused by anchors reset relative to current poses
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_AnchorPosition, Styles.anchorPosition);
                EditorGUILayout.PropertyField(m_AnchorRotation, Styles.anchorRotation);
                if (EditorGUI.EndChangeCheck())
                {
                    body.anchorPosition = m_AnchorPosition.vector3Value;
                    body.anchorRotation = m_AnchorRotation.quaternionValue;

                    if (m_ComputeParentAnchor.boolValue)
                    {
                        // setting child anchors in this mode will also change the parent ones
                        // lets fetch them back otherwise ApplyModifiedProperties overwrites them
                        m_ParentAnchorPosition.vector3Value = body.parentAnchorPosition;
                        m_ParentAnchorRotation.quaternionValue = body.parentAnchorRotation;
                    }
                }

                // parent anchors
                if (!m_ComputeParentAnchor.boolValue)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(m_ParentAnchorPosition, Styles.parentAnchorPosition);
                    EditorGUILayout.PropertyField(m_ParentAnchorRotation, Styles.parentAnchorRotation);

                    if (EditorGUI.EndChangeCheck())
                    {
                        body.parentAnchorPosition = m_ParentAnchorPosition.vector3Value;
                        body.parentAnchorRotation = m_ParentAnchorRotation.quaternionValue;
                    }
                }

                if (GUILayout.Button("Snap anchor to closest contact"))
                {
                    //GetComponentInParent does not look whether returned ArticulationBody is enabled, so implemented appropriate function.
                    ArticulationBody parentBody = FindEnabledParentArticulationBody(body);

                    Undo.RecordObject(body, "Changing anchor position/rotation to match closest contact.");
                    Vector3 com = parentBody.worldCenterOfMass;
                    Vector3 closestOnSurface = body.GetClosestPoint(com);
                    body.anchorPosition = body.transform.InverseTransformPoint(closestOnSurface);
                    body.anchorRotation = Quaternion.FromToRotation(Vector3.right,
                        body.transform.InverseTransformDirection(com - closestOnSurface).normalized);
                }

                EditorGUILayout.PropertyField(m_ArticulationJointType); // the tooltip for this is still in the header

                switch (body.jointType)
                {
                    case ArticulationJointType.FixedJoint:
                        break;

                    case ArticulationJointType.PrismaticJoint:
                        // work out joint settings
                        LinearDof linearDof = LinearDof.X; // x by default
                        ArticulationDofLock dofLockSetting = ArticulationDofLock.FreeMotion;

                        if (body.linearLockX != ArticulationDofLock.LockedMotion)
                        {
                            linearDof = LinearDof.X;
                            dofLockSetting = body.linearLockX;
                        }
                        else if (body.linearLockY != ArticulationDofLock.LockedMotion)
                        {
                            linearDof = LinearDof.Y;
                            dofLockSetting = body.linearLockY;
                        }
                        else if (body.linearLockZ != ArticulationDofLock.LockedMotion)
                        {
                            linearDof = LinearDof.Z;
                            dofLockSetting = body.linearLockZ;
                        }

                        int dofCount = 0;

                        if (body.linearLockX != ArticulationDofLock.LockedMotion) dofCount++;
                        if (body.linearLockY != ArticulationDofLock.LockedMotion) dofCount++;
                        if (body.linearLockZ != ArticulationDofLock.LockedMotion) dofCount++;

                        bool overrideDof = (dofCount != 1);

                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.Space();
                        linearDof = (LinearDof)EditorGUILayout.EnumPopup(Styles.prismaticAxis, linearDof);
                        NonLockedMotion motion = (dofLockSetting == ArticulationDofLock.FreeMotion) ? NonLockedMotion.Free : NonLockedMotion.Limited;
                        motion = (NonLockedMotion)EditorGUILayout.EnumPopup(Styles.unlockedMotionType, motion);
                        if (EditorGUI.EndChangeCheck() || overrideDof)
                        {
                            m_LinearX.enumValueIndex = (linearDof == LinearDof.X) ? (int)motion : 0;
                            m_LinearY.enumValueIndex = (linearDof == LinearDof.Y) ? (int)motion : 0;
                            m_LinearZ.enumValueIndex = (linearDof == LinearDof.Z) ? (int)motion : 0;
                        }

                        EditorGUILayout.Space();

                        DoDriveInspector(m_YDrive, (ArticulationDofLock)m_LinearY.enumValueIndex);
                        DoDriveInspector(m_ZDrive, (ArticulationDofLock)m_LinearZ.enumValueIndex);
                        DoDriveInspector(m_XDrive, (ArticulationDofLock)m_LinearX.enumValueIndex);

                        break;

                    case ArticulationJointType.RevoluteJoint:
                        ArticulationDofLock serialisedDofLock = (ArticulationDofLock)m_Twist.enumValueIndex;

                        NonLockedMotion revoluteMotion = serialisedDofLock == ArticulationDofLock.LimitedMotion
                            ? NonLockedMotion.Limited
                            : NonLockedMotion.Free;

                        EditorGUILayout.Space();
                        motion = (NonLockedMotion)EditorGUILayout.EnumPopup(Styles.unlockedMotionType, revoluteMotion);

                        ArticulationDofLock newDofLock = (ArticulationDofLock)motion;

                        if (newDofLock != serialisedDofLock)
                        {
                            m_Twist.enumValueIndex = (int)newDofLock;
                        }

                        EditorGUILayout.Space();
                        DoDriveInspector(m_XDrive, newDofLock);

                        break;

                    case ArticulationJointType.SphericalJoint:
                        EditorGUILayout.Space();
                        // here we just need to make sure we disable getting into an invalid configuration
                        SphericalJointMotionSetting(m_SwingY);
                        SphericalJointMotionSetting(m_SwingZ);
                        SphericalJointMotionSetting(m_Twist);

                        EditorGUILayout.Space();

                        DoDriveInspector(m_YDrive, body.swingYLock);
                        DoDriveInspector(m_ZDrive, body.swingZLock);
                        DoDriveInspector(m_XDrive, body.twistLock);

                        break;
                }
            }
            serializedObject.ApplyModifiedProperties();
            ShowBodyInfoProperties();
        }

        private void SphericalJointMotionSetting(SerializedProperty axis)
        {
            EditorGUI.BeginChangeCheck();
            int oldSetting = axis.enumValueIndex; // assume it was correct before changing
            EditorGUILayout.PropertyField(axis);
            if (EditorGUI.EndChangeCheck())
            {
                if (m_SwingY.enumValueIndex == 0 && m_SwingZ.enumValueIndex == 0 && m_Twist.enumValueIndex == 0)
                {
                    axis.enumValueIndex = oldSetting;
                }
            }
        }

        protected virtual void OnSceneGUI()
        {
            ArticulationBody body = (ArticulationBody)target;

            if (body.isRoot)
                return;

            ArticulationBody parentBody = FindEnabledParentArticulationBody(body);

            {
                Vector3 localAnchorT = body.anchorPosition;
                Quaternion localAnchorR = body.anchorRotation;

                EditorGUI.BeginChangeCheck();

                DisplayProperAnchorHandle(body, ref localAnchorT, ref localAnchorR);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Changing Articulation body anchor position/rotation");
                    m_AnchorPosition.vector3Value = localAnchorT;
                    m_AnchorRotation.quaternionValue = localAnchorR;

                    body.anchorPosition = m_AnchorPosition.vector3Value;
                    body.anchorRotation = m_AnchorRotation.quaternionValue;
                }
            }

            if (!m_ComputeParentAnchor.boolValue)
            {
                Vector3 localAnchorT = body.parentAnchorPosition;
                Quaternion localAnchorR = body.parentAnchorRotation;

                EditorGUI.BeginChangeCheck();

                DisplayProperAnchorHandle(parentBody, ref localAnchorT, ref localAnchorR);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Changing Articulation body parent anchor position/rotation");
                    m_ParentAnchorPosition.vector3Value = localAnchorT;
                    m_ParentAnchorRotation.quaternionValue = localAnchorR;

                    body.parentAnchorPosition = m_ParentAnchorPosition.vector3Value;
                    body.parentAnchorRotation = m_ParentAnchorRotation.quaternionValue;
                }
            }

            DisplayJointLimits(body);
        }

        private void DisplayJointLimits(ArticulationBody body)
        {
            ArticulationBody parentBody = FindEnabledParentArticulationBody(body);

            Matrix4x4 parentAnchorSpace = Matrix4x4.TRS(parentBody.transform.TransformPoint(body.parentAnchorPosition), parentBody.transform.rotation * body.parentAnchorRotation, Vector3.one);

            // this is root body no joint limits
            if (parentBody == null)
                return;

            if (body.jointType == ArticulationJointType.PrismaticJoint)
            {
                ShowPrismaticLimits(body, parentBody, parentAnchorSpace);
                return;
            }

            if (body.jointType == ArticulationJointType.RevoluteJoint)
            {
                ShowRevoluteLimits(body, parentBody, parentAnchorSpace);
                return;
            }

            if (body.jointType == ArticulationJointType.SphericalJoint)
            {
                ShowSphericalLimits(body, parentBody, parentAnchorSpace);
                return;
            }
        }

        private void ShowPrismaticLimits(ArticulationBody body, ArticulationBody parentBody, Matrix4x4 parentAnchorSpace)
        {
            // if prismatic and unlocked - nothing to visualise
            if (body.linearLockX == ArticulationDofLock.FreeMotion || body.linearLockY == ArticulationDofLock.FreeMotion || body.linearLockZ == ArticulationDofLock.FreeMotion)
                return;

            float dashSize = 5;

            // compute the primary axis of the prismatic
            Vector3 primaryAxis = Vector3.zero;
            ArticulationDrive drive = body.xDrive;

            if (body.linearLockX == ArticulationDofLock.LimitedMotion)
            {
                primaryAxis = Vector3.right;
                drive = body.xDrive;
            }
            else if (body.linearLockY == ArticulationDofLock.LimitedMotion)
            {
                primaryAxis = Vector3.up;
                drive = body.yDrive;
            }
            else if (body.linearLockZ == ArticulationDofLock.LimitedMotion)
            {
                primaryAxis = Vector3.forward;
                drive = body.zDrive;
            }

            // now show the valid movement along the axis as well as limits
            using (new Handles.DrawingScope(parentAnchorSpace))
            {
                Vector3 lowerPoint = primaryAxis * drive.lowerLimit;
                Vector3 upperPoint = primaryAxis * drive.upperLimit;

                Quaternion orientation = Quaternion.LookRotation(primaryAxis);

                Handles.color = Color.red;
                Handles.CylinderHandleCap(0, lowerPoint, orientation, CapScale, EventType.Repaint);

                Handles.color = Color.green;
                Handles.CylinderHandleCap(0, upperPoint, orientation, CapScale, EventType.Repaint);

                Handles.color = Color.white;
                Handles.DrawDottedLine(lowerPoint, upperPoint, dashSize);
            }
        }

        private void ShowAngularLimitSpan(bool freeMotion, ArticulationBody body, ArticulationBody parentBody, Color color, Vector3 axis, Matrix4x4 space, ArticulationDrive drive, Vector3 zeroDirection)
        {
            // check if it's a free span - show only a solid disc in this case
            if (freeMotion)
            {
                using (new Handles.DrawingScope(space))
                {
                    color.a = 0.3f;
                    Handles.color = color;
                    Handles.DrawSolidDisc(Vector3.zero, axis, GizmoLinearSize);
                }

                return;
            }

            // here we know the angle is limited - show a span
            float totalAngle = drive.upperLimit - drive.lowerLimit;

            Quaternion zeroPose = Quaternion.FromToRotation(Vector3.forward, Vector3.Cross(axis, zeroDirection));

            Quaternion lowerRotation = Quaternion.AngleAxis(drive.lowerLimit, axis);
            Quaternion upperRotation = Quaternion.AngleAxis(drive.upperLimit, axis);

            Vector3 from = lowerRotation * zeroDirection;
            Vector3 to = upperRotation * zeroDirection;

            // Nb: Cylinder cap is oriented along Z
            using (new Handles.DrawingScope(space))
            {
                color.a = 0.3f;
                Handles.color = color;
                Handles.DrawSolidArc(Vector3.zero, axis, from, totalAngle, GizmoLinearSize);

                Handles.color = Color.red;
                Handles.CylinderHandleCap(0, from * GizmoLinearSize, lowerRotation * zeroPose, CapScale, EventType.Repaint);

                Handles.color = Color.green;
                Handles.CylinderHandleCap(0, to * GizmoLinearSize, upperRotation * zeroPose, CapScale, EventType.Repaint);
            }
        }

        private void ShowRevoluteLimits(ArticulationBody body, ArticulationBody parentBody, Matrix4x4 parentAnchorSpace)
        {
            bool free = (body.twistLock == ArticulationDofLock.FreeMotion);

            ShowAngularLimitSpan(free, body, parentBody, Color.red, Vector3.right, parentAnchorSpace, body.xDrive, Vector3.forward);
        }

        // this is a variant that draws cross
        private void ShowSphericalLimits(ArticulationBody body, ArticulationBody parentBody, Matrix4x4 parentAnchorSpace)
        {
            // swing z
            if (body.swingZLock != ArticulationDofLock.LockedMotion)
            {
                bool free = (body.swingZLock == ArticulationDofLock.FreeMotion);
                ShowAngularLimitSpan(free, body, parentBody, Color.blue, Vector3.forward, parentAnchorSpace, body.zDrive, Vector3.right);
            }

            // swing y
            if (body.swingYLock != ArticulationDofLock.LockedMotion)
            {
                bool free = (body.swingYLock == ArticulationDofLock.FreeMotion);
                ShowAngularLimitSpan(free, body, parentBody, Color.green, Vector3.up, parentAnchorSpace, body.yDrive, Vector3.right);
            }

            // twist
            if (body.twistLock != ArticulationDofLock.LockedMotion)
            {
                bool free = (body.twistLock == ArticulationDofLock.FreeMotion);
                ShowAngularLimitSpan(free, body, parentBody, Color.red, Vector3.right, parentAnchorSpace, body.xDrive, Vector3.forward);
            }
        }

        private void DisplayProperAnchorHandle(ArticulationBody body, ref Vector3 anchorPos, ref Quaternion anchorRot)
        {
            float handleScaling = 0.5f;

            var bodySpace = Matrix4x4.TRS(body.transform.position, body.transform.rotation, Vector3.one * handleScaling);

            // Need to pre-scale in body space because our handle matrix is scaled, we will remove scaling after reading back
            anchorPos *= (1 / handleScaling);

            using (new Handles.DrawingScope(bodySpace))
            {
                if (Tools.current == Tool.Move)
                {
                    anchorPos = Handles.PositionHandle(anchorPos, anchorRot);
                }

                if (Tools.current == Tool.Rotate)
                {
                    anchorRot = Handles.RotationHandle(anchorRot, anchorPos);
                }

                if (Tools.current == Tool.Transform)
                {
                    Handles.TransformHandle(ref anchorPos, ref anchorRot);
                }
            }

            // Don't forget to remove scaling
            anchorPos *= handleScaling;
        }

        private void DoDriveInspector(SerializedProperty drive, ArticulationDofLock dofLock)
        {
            // If lockedMotion - don't render any drive inspector fields
            if (dofLock == ArticulationDofLock.LockedMotion)
                return;

            EditorGUILayout.LabelField(drive.displayName);

            EditorGUI.indentLevel++;

            // Display limit fields only if drive is LimitedMotion
            if (dofLock == ArticulationDofLock.LimitedMotion)
            {
                EditorGUILayout.PropertyField(drive.FindPropertyRelative("lowerLimit"), Styles.lowerLimit);
                EditorGUILayout.PropertyField(drive.FindPropertyRelative("upperLimit"), Styles.upperLimit);
            }

            // Always display fields
            EditorGUILayout.PropertyField(drive.FindPropertyRelative("stiffness"), Styles.stiffness);
            EditorGUILayout.PropertyField(drive.FindPropertyRelative("damping"), Styles.damping);
            EditorGUILayout.PropertyField(drive.FindPropertyRelative("forceLimit"), Styles.forceLimit);
            EditorGUILayout.PropertyField(drive.FindPropertyRelative("target"), Styles.target);
            EditorGUILayout.PropertyField(drive.FindPropertyRelative("targetVelocity"), Styles.targetVelocity);

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        private void ShowBodyInfoProperties()
        {
            m_RequiresConstantRepaint = false;

            m_ShowInfoFoldout.value = m_ShowInfo.target = EditorGUILayout.Foldout(m_ShowInfo.target, "Info", true);
            if (EditorGUILayout.BeginFadeGroup(m_ShowInfo.faded))
            {
                if (targets.Length == 1)
                {
                    var body = targets[0] as ArticulationBody;
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.FloatField("Speed", body.velocity.magnitude);
                    EditorGUILayout.Vector3Field("Velocity", body.velocity);
                    EditorGUILayout.Vector3Field("Angular Velocity", body.angularVelocity);
                    EditorGUILayout.Vector3Field("Inertia Tensor", body.inertiaTensor);
                    EditorGUILayout.Vector3Field("Inertia Tensor Rotation", body.inertiaTensorRotation.eulerAngles);
                    EditorGUILayout.Vector3Field("Local Center of Mass", body.centerOfMass);
                    EditorGUILayout.Vector3Field("World Center of Mass", body.worldCenterOfMass);
                    EditorGUILayout.LabelField("Sleep State", body.IsSleeping() ? "Asleep" : "Awake");
                    EditorGUILayout.IntField("Body Index", body.index);
                    EditorGUI.EndDisabledGroup();

                    // We need to repaint as some of the above properties can change without causing a repaint.
                    if (EditorApplication.isPlaying)
                        m_RequiresConstantRepaint = true;
                }
                else
                {
                    EditorGUILayout.HelpBox("Cannot show Info properties when multiple bodies are selected.", MessageType.Info);
                }
            }
            EditorGUILayout.EndFadeGroup();
        }

        public override bool RequiresConstantRepaint()
        {
            return m_RequiresConstantRepaint;
        }

        private ArticulationBody FindEnabledParentArticulationBody(ArticulationBody body)
        {
            ArticulationBody parent = body.transform.parent.GetComponentInParent<ArticulationBody>();
            while (parent && !parent.enabled)
            {
                parent = parent.transform.parent.GetComponentInParent<ArticulationBody>();
            }

            return parent;
        }
    }
}
