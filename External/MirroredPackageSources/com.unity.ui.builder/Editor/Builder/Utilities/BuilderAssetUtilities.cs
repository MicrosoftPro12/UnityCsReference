using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Unity.UI.Builder
{
    internal static class BuilderAssetUtilities
    {
        public static string GetResourcesPathForAsset(Object asset)
        {
            var assetPath = AssetDatabase.GetAssetPath(asset);
            return GetResourcesPathForAsset(assetPath);
        }

        public static string GetResourcesPathForAsset(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
                return null;

            // Start by trying to find a "Resources" folder in the middle of the path. 
            var resourcesFolder = "/Resources/";
            var lastResourcesFolderIndex = assetPath.LastIndexOf(resourcesFolder, StringComparison.Ordinal);
            // Otherwise check if the "Resources" path is at the start.
            if (lastResourcesFolderIndex < 0)
            {
                if (assetPath.StartsWith("Resources/"))
                {
                    lastResourcesFolderIndex = 0;
                    resourcesFolder = "Resources/";
                }
                else return null;
            }
            
            var lastResourcesSubstring = lastResourcesFolderIndex + resourcesFolder.Length;
            assetPath = assetPath.Substring(lastResourcesSubstring);
            var lastExtDot = assetPath.LastIndexOf(".", StringComparison.Ordinal);

            if (lastExtDot == -1)
                return null;

            assetPath = assetPath.Substring(0, lastExtDot);

            return assetPath;
        }

        public static bool IsBuiltinPath(string assetPath)
        {
            return assetPath == "Resources/unity_builtin_extra";
        }

        public static bool ValidateAsset(VisualTreeAsset asset, string path)
        {
            string errorMessage = null;

            string errorTitle = null;

            if (asset == null)
            {
                if (string.IsNullOrEmpty(path))
                    path = "<unspecified>";

                if (path.StartsWith("Packages/"))
                    errorMessage = $"The asset at path {path} is not a UXML Document.\nNote, for assets inside Packages folder, the folder name for the package needs to match the actual official package name (ie. com.example instead of Example).";
                else
                    errorMessage = $"The asset at path {path} is not a UXML Document.";
                errorTitle = "Invalid Asset Type";
            }
            else if (asset.importedWithErrors)
            {
                if (string.IsNullOrEmpty(path))
                    path = AssetDatabase.GetAssetPath(asset);

                if (string.IsNullOrEmpty(path))
                    path = "<unspecified>";

                errorMessage = string.Format(BuilderConstants.InvalidUXMLDialogMessage, path);
                errorTitle = BuilderConstants.InvalidUXMLDialogTitle;
            }

            if (errorMessage != null)
            {
                BuilderDialogsUtility.DisplayDialog(errorTitle, errorMessage, "Ok");
                Debug.LogError(errorMessage);
                return false;
            }

            return true;
        }

        public static bool AddStyleSheetToAsset(
            BuilderDocument document, string ussPath)
        {
            var styleSheet = BuilderPackageUtilities.LoadAssetAtPath<StyleSheet>(ussPath);

            string errorMessage = null;
            string errorTitle = null;

            if (styleSheet == null)
            {
                if (ussPath.StartsWith("Packages/"))
                    errorMessage = $"Asset at path {ussPath} is not a StyleSheet.\nNote, for assets inside Packages folder, the folder name for the package needs to match the actual official package name (ie. com.example instead of Example).";
                else
                    errorMessage = $"Asset at path {ussPath} is not a StyleSheet.";
                errorTitle = "Invalid Asset Type";
            }
            else if (styleSheet.importedWithErrors)
            {
                errorMessage = string.Format(BuilderConstants.InvalidUSSDialogMessage, ussPath);
                errorTitle = BuilderConstants.InvalidUSSDialogTitle;
            }

            if (errorMessage != null)
            {
                BuilderDialogsUtility.DisplayDialog(errorTitle, errorMessage, "Ok");
                Debug.LogError(errorMessage);
                return false;
            }

            Undo.RegisterCompleteObjectUndo(
                document.visualTreeAsset, "Add StyleSheet to UXML");

            document.AddStyleSheetToDocument(styleSheet, ussPath);
            return true;
        }

        public static void RemoveStyleSheetFromAsset(
            BuilderDocument document, int ussIndex)
        {
            Undo.RegisterCompleteObjectUndo(
                document.visualTreeAsset, "Remove StyleSheet from UXML");

            document.RemoveStyleSheetFromDocument(ussIndex);
        }

        public static void RemoveStyleSheetsFromAsset(
            BuilderDocument document, int[] indexes)
        {
            Undo.RegisterCompleteObjectUndo(
                document.visualTreeAsset, "Remove StyleSheets from UXML");

            foreach (var index in indexes)
            {
                document.RemoveStyleSheetFromDocument(index);
            }
        }

        public static void ReorderStyleSheetsInAsset(
            BuilderDocument document, VisualElement styleSheetsContainerElement)
        {
            Undo.RegisterCompleteObjectUndo(
                document.visualTreeAsset, "Reorder StyleSheets in UXML");

            var reorderedUSSList = new List<StyleSheet>();
            foreach (var ussElement in styleSheetsContainerElement.Children())
                reorderedUSSList.Add(ussElement.GetStyleSheet());

            var openUXMLFile = document.activeOpenUXMLFile;
            openUXMLFile.openUSSFiles.Sort((left, right) =>
            {
                var leftOrder = reorderedUSSList.IndexOf(left.styleSheet);
                var rightOrder = reorderedUSSList.IndexOf(right.styleSheet);
                return leftOrder.CompareTo(rightOrder);
            });
        }

        public static VisualElementAsset AddElementToAsset(
            BuilderDocument document, VisualElement ve, int index = -1)
        {
            Undo.RegisterCompleteObjectUndo(
                document.visualTreeAsset, BuilderConstants.CreateUIElementUndoMessage);

            var veParent = ve.parent;
            VisualElementAsset veaParent = null;

            /* If the current parent element is linked to a VisualTreeAsset, it could mean
             that our parent is the TemplateContainer belonging to our parent document and the
             current open document is a sub-document opened in-place. In such a case, we don't
             want to use our parent's VisualElementAsset, as that belongs to our parent document.
             So instead, we just use no parent, indicating that we are adding this new element
             to the root of our document.*/
            if (veParent != null && veParent.GetVisualTreeAsset() != document.visualTreeAsset)
                veaParent = veParent.GetVisualElementAsset();

            if (veaParent == null)
                veaParent = document.visualTreeAsset.GetRootUXMLElement(); // UXML Root Element

            var vea = document.visualTreeAsset.AddElement(veaParent, ve);

            if (index >= 0)
                document.visualTreeAsset.ReparentElement(vea, veaParent, index);

            return vea;
        }

        public static VisualElementAsset AddElementToAsset(
            BuilderDocument document, VisualElement ve,
            Func<VisualTreeAsset, VisualElementAsset, VisualElement, VisualElementAsset> makeVisualElementAsset,
            int index = -1, bool registerUndo = true)
        {
            if (registerUndo)
            {
                Undo.RegisterCompleteObjectUndo(
                    document.visualTreeAsset, BuilderConstants.CreateUIElementUndoMessage);
            }

            var veParent = ve.parent;
            VisualElementAsset veaParent = null;

            /* If the current parent element is linked to a VisualTreeAsset, it could mean
             that our parent is the TemplateContainer belonging to our parent document and the
             current open document is a sub-document opened in-place. In such a case, we don't
             want to use our parent's VisualElementAsset, as that belongs to our parent document.
             So instead, we just use no parent, indicating that we are adding this new element
             to the root of our document.*/
            if (veParent != null && veParent.GetVisualTreeAsset() != document.visualTreeAsset)
                veaParent = veParent.GetVisualElementAsset();

            if (veaParent == null)
                veaParent = document.visualTreeAsset.GetRootUXMLElement(); // UXML Root Element

            var vea = makeVisualElementAsset(document.visualTreeAsset, veaParent, ve);
            ve.SetVisualElementAsset(vea);
            ve.SetProperty(BuilderConstants.ElementLinkedBelongingVisualTreeAssetVEPropertyName, document.visualTreeAsset);

            if (index >= 0)
                document.visualTreeAsset.ReparentElement(vea, veaParent, index);

            return vea;
        }

        public static void SortElementsByTheirVisualElementInAsset(VisualElement parentVE)
        {
            var parentVEA = parentVE.GetVisualElementAsset();
            if (parentVEA == null)
                return;

            if (parentVE.childCount <= 1)
                return;

            var correctOrderForElementAssets = new List<VisualElementAsset>();
            var correctOrdersInDocument = new List<int>();
            foreach (var ve in parentVE.Children())
            {
                var vea = ve.GetVisualElementAsset();
                if (vea == null)
                    continue;

                correctOrderForElementAssets.Add(vea);
                correctOrdersInDocument.Add(vea.orderInDocument);
            }

            if (correctOrderForElementAssets.Count <= 1)
                return;

            correctOrdersInDocument.Sort();

            for (int i = 0; i < correctOrderForElementAssets.Count; ++i)
                correctOrderForElementAssets[i].orderInDocument = correctOrdersInDocument[i];
        }

        public static void ReparentElementInAsset(
            BuilderDocument document, VisualElement veToReparent, VisualElement newParent, int index = -1, bool undo = true)
        {
            var veaToReparent = veToReparent.GetVisualElementAsset();
            if (veaToReparent == null)
                return;

            if (undo)
                Undo.RegisterCompleteObjectUndo(
                    document.visualTreeAsset, BuilderConstants.ReparentUIElementUndoMessage);

            VisualElementAsset veaNewParent = null;
            if (newParent != null)
                veaNewParent = newParent.GetVisualElementAsset();

            if (veaNewParent == null)
                veaNewParent = document.visualTreeAsset.GetRootUXMLElement(); // UXML Root Element

            document.visualTreeAsset.ReparentElement(veaToReparent, veaNewParent, index);
        }

        public static void ApplyAttributeOverridesToTreeAsset(List<TemplateAsset.AttributeOverride> attributeOverrides, VisualTreeAsset visualTreeAsset)
        {
            foreach (var attributeOverride in attributeOverrides)
            {
                var overwrittenElements = visualTreeAsset.FindElementsByName(attributeOverride.m_ElementName);

                foreach (var overwrittenElement in overwrittenElements)
                {
                    overwrittenElement.SetAttribute(attributeOverride.m_AttributeName, attributeOverride.m_Value);
                }
            }
        }

        public static void CopyAttributeOverridesToChildTemplateAssets(List<TemplateAsset.AttributeOverride> attributeOverrides, VisualTreeAsset visualTreeAsset)
        {
            foreach (var templateAsset in visualTreeAsset.templateAssets)
            {
                foreach (var attributeOverride in attributeOverrides)
                {
                    templateAsset.SetAttributeOverride(attributeOverride.m_ElementName, attributeOverride.m_AttributeName, attributeOverride.m_Value);
                }
            }
        }

        public static void AddStyleSheetsFromTreeAsset(VisualElementAsset visualElementAsset, VisualTreeAsset visualTreeAsset)
        {
            foreach (var styleSheet in visualTreeAsset.stylesheets)
            {
                var styleSheetPath = AssetDatabase.GetAssetPath(styleSheet);

                visualElementAsset.AddStyleSheet(styleSheet);
                visualElementAsset.AddStyleSheetPath(styleSheetPath);
            }
        }

        public static void DeleteElementFromAsset(BuilderDocument document, VisualElement ve, bool registerUndo = true)
        {
            var vea = ve.GetVisualElementAsset();
            if (vea == null)
                return;

            if (registerUndo)
            {
                Undo.RegisterCompleteObjectUndo(
                    document.visualTreeAsset, BuilderConstants.DeleteUIElementUndoMessage);
            }

            foreach (var child in ve.Children())
            {
                DeleteElementFromAsset(document, child, false);
            }

            document.visualTreeAsset.RemoveElement(vea);
        }

        public static void TransferAssetToAsset(
            BuilderDocument document, VisualElementAsset parent, VisualTreeAsset otherVta, bool registerUndo = true)
        {
            if (registerUndo)
            {
                Undo.RegisterCompleteObjectUndo(
                    document.visualTreeAsset, BuilderConstants.CreateUIElementUndoMessage);
            }

            document.visualTreeAsset.Swallow(parent, otherVta);
        }

        public static void TransferAssetToAsset(
            BuilderDocument document, StyleSheet styleSheet, StyleSheet otherStyleSheet)
        {
            Undo.RegisterCompleteObjectUndo(
                styleSheet, BuilderConstants.AddNewSelectorUndoMessage);

            styleSheet.Swallow(otherStyleSheet);
        }

        public static void AddStyleClassToElementInAsset(BuilderDocument document, VisualElement ve, string className)
        {
            Undo.RegisterCompleteObjectUndo(
                document.visualTreeAsset, BuilderConstants.AddStyleClassUndoMessage);

            var vea = ve.GetVisualElementAsset();
            vea.AddStyleClass(className);
        }

        public static void RemoveStyleClassFromElementInAsset(BuilderDocument document, VisualElement ve, string className)
        {
            Undo.RegisterCompleteObjectUndo(
                document.visualTreeAsset, BuilderConstants.RemoveStyleClassUndoMessage);

            var vea = ve.GetVisualElementAsset();
            vea.RemoveStyleClass(className);
        }

        public static void AddStyleComplexSelectorToSelection(StyleSheet styleSheet, StyleComplexSelector scs)
        {
            var selectionProp = styleSheet.AddProperty(
                scs,
                BuilderConstants.SelectedStyleRulePropertyName,
                BuilderConstants.ChangeSelectionUndoMessage);

            // Need to add at least one dummy value because lots of code will die
            // if it encounters a style property with no values.
            styleSheet.AddValue(
                selectionProp, 42.0f, BuilderConstants.ChangeSelectionUndoMessage);
        }

        public static void AddElementToSelectionInAsset(BuilderDocument document, VisualElement ve)
        {
            if (BuilderSharedStyles.IsStyleSheetElement(ve))
            {
                var styleSheet = ve.GetStyleSheet();
                styleSheet.AddSelector(
                    BuilderConstants.SelectedStyleSheetSelectorName,
                    BuilderConstants.ChangeSelectionUndoMessage);
            }
            else if (BuilderSharedStyles.IsSelectorElement(ve))
            {
                var styleSheet = ve.GetClosestStyleSheet();
                var scs = ve.GetStyleComplexSelector();
                AddStyleComplexSelectorToSelection(styleSheet, scs);
            }
            else if (BuilderSharedStyles.IsDocumentElement(ve))
            {
                Undo.RegisterCompleteObjectUndo(
                    document.visualTreeAsset, BuilderConstants.ChangeSelectionUndoMessage);

                var vta = ve.GetVisualTreeAsset();
                var vtaRoot = vta.GetRootUXMLElement();
                vta.AddElement(vtaRoot, BuilderConstants.SelectedVisualTreeAssetSpecialElementTypeName);
            }
            else if (ve.GetVisualElementAsset() != null)
            {
                Undo.IncrementCurrentGroup();
                Undo.RegisterCompleteObjectUndo(
                    document.visualTreeAsset, BuilderConstants.ChangeSelectionUndoMessage);

                var vea = ve.GetVisualElementAsset();
                vea.Select();
            }
        }

        public static void RemoveElementFromSelectionInAsset(BuilderDocument document, VisualElement ve)
        {
            if (BuilderSharedStyles.IsStyleSheetElement(ve))
            {
                var styleSheet = ve.GetStyleSheet();
                styleSheet.RemoveSelector(
                    BuilderConstants.SelectedStyleSheetSelectorName,
                    BuilderConstants.ChangeSelectionUndoMessage);
            }
            else if (BuilderSharedStyles.IsSelectorElement(ve))
            {
                var styleSheet = ve.GetClosestStyleSheet();
                var scs = ve.GetStyleComplexSelector();
                styleSheet.RemoveProperty(
                    scs,
                    BuilderConstants.SelectedStyleRulePropertyName,
                    BuilderConstants.ChangeSelectionUndoMessage);
            }
            else if (BuilderSharedStyles.IsDocumentElement(ve))
            {
                Undo.RegisterCompleteObjectUndo(
                    document.visualTreeAsset, BuilderConstants.ChangeSelectionUndoMessage);

                var vta = ve.GetVisualTreeAsset();
                var selectedElement = vta.FindElementByType(BuilderConstants.SelectedVisualTreeAssetSpecialElementTypeName);
                vta.RemoveElement(selectedElement);
            }
            else if (ve.GetVisualElementAsset() != null)
            {
                Undo.IncrementCurrentGroup();
                Undo.RegisterCompleteObjectUndo(
                    document.visualTreeAsset, BuilderConstants.ChangeSelectionUndoMessage);

                var vea = ve.GetVisualElementAsset();
                vea.Deselect();
            }
        }

        public static string GetVisualTreeAssetAssetName(VisualTreeAsset visualTreeAsset, bool hasUnsavedChanges) =>
            GetAssetName(visualTreeAsset, BuilderConstants.UxmlExtension, hasUnsavedChanges);

        public static string GetStyleSheetAssetName(StyleSheet styleSheet, bool hasUnsavedChanges) =>
            GetAssetName(styleSheet, BuilderConstants.UssExtension, hasUnsavedChanges);

        public static string GetAssetName(ScriptableObject asset, string extension, bool hasUnsavedChanges)
        {
            if (asset == null)
            {
                if (extension == BuilderConstants.UxmlExtension)
                    return BuilderConstants.ToolbarUnsavedFileDisplayText + extension;
                else
                    return string.Empty;
            }

            var assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
                return BuilderConstants.ToolbarUnsavedFileDisplayText + extension;

            return Path.GetFileName(assetPath) + (hasUnsavedChanges ? BuilderConstants.ToolbarUnsavedFileSuffix : "");
        }

        public static TemplateContainer GetVisualElementRootTemplate(VisualElement visualElement)
        {
            TemplateContainer templateContainerParent = null;
            var parent = visualElement.parent;

            while (parent != null)
            {
                if (parent is TemplateContainer && parent.GetVisualElementAsset() != null)
                {
                    templateContainerParent = parent as TemplateContainer;
                    break;
                }

                parent = parent.parent;
            }

            return templateContainerParent;
        }

        public static bool HasAttributeOverrideInRootTemplate(VisualElement visualElement, string attributeName)
        {
            var templateContainer = GetVisualElementRootTemplate(visualElement);
            var templateAsset = templateContainer?.GetVisualElementAsset() as TemplateAsset;

            return templateAsset?.attributeOverrides.Count(x => x.m_ElementName == visualElement.name && x.m_AttributeName == attributeName) > 0;
        }

        public static List<TemplateAsset.AttributeOverride> GetAccumulatedAttributeOverrides(VisualElement visualElement)
        {
            VisualElement parent = visualElement.parent;
            List<TemplateAsset.AttributeOverride> attributeOverrides = new List<TemplateAsset.AttributeOverride>();

            while (parent != null)
            {
                if (parent is TemplateContainer)
                {
                    TemplateAsset templateAsset;
                    if (parent.HasProperty(VisualTreeAsset.LinkedVEAInTemplatePropertyName))
                    {
                        templateAsset = parent.GetProperty(VisualTreeAsset.LinkedVEAInTemplatePropertyName) as TemplateAsset;
                    }
                    else
                    {
                        templateAsset = parent.GetVisualElementAsset() as TemplateAsset;
                    }

                    if (templateAsset != null)
                    {
                        attributeOverrides.AddRange(templateAsset.attributeOverrides);
                    }

                    // We reached the root template
                    if (parent.GetVisualElementAsset() != null)
                    {
                        break;
                    }
                }

                parent = parent.parent;
            }

            // Parent attribute overrides have higher priority
            attributeOverrides.Reverse();

            return attributeOverrides;
        }

        static public bool WriteTextFileToDisk(string path, string content)
        {
            bool success = FileUtil.WriteTextFileToDisk(path, content, out string message);

            if (!success)
            {
                Debug.LogError(message);
                BuilderDialogsUtility.DisplayDialog("Save - " + path, message, "Ok");
            }

            return success;
        }
    }
}
