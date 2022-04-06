#if UNITY_EDITOR
using System.Collections.Generic;
using CoinMode.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoinMode.Editor
{
    public abstract class CoinModeEditorWindow : EditorWindow
    {
        protected static GUIStyle headingLabel
        {
            get
            {
                if (EditorStyles.largeLabel != null && (_headingLabel == null || _headingLabel.fontSize != 18))
                {
                    _headingLabel = new GUIStyle(EditorStyles.largeLabel);
                    _headingLabel.fontSize = 18;
                }
                return _headingLabel;
            }
        }
        private static GUIStyle _headingLabel = null;

        protected static GUIStyle subHeadingLabel
        {
            get
            {
                if (EditorStyles.largeLabel != null && (_subHeadingLabel == null || _subHeadingLabel.fontSize != 14))
                {
                    _subHeadingLabel = new GUIStyle(EditorStyles.largeLabel);
                    _subHeadingLabel.fontSize = 14;
                }
                return _subHeadingLabel;
            }
        }
        private static GUIStyle _subHeadingLabel = null;

        protected Object objectInContext = null;
        protected bool unsavedChanges = false;
        
        protected float tempFloat;        
        protected Sprite tempSprite;
        protected Font tempFont;

        protected virtual void Awake()
        {
            Init();
        }

        protected virtual void OnEnable()
        {
            Init();
        }

        protected virtual void OnDestroy()
        {
            Undo.postprocessModifications -= MyPostprocessModificationsCallback;
        }

        protected virtual void OnGUI()
        {            
            EditorGUI.BeginDisabledGroup(!unsavedChanges);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            if (GUILayout.Button("Save changes"))
            {
                SaveObject(true);
            }
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }

        protected virtual void Init()
        {
            Undo.postprocessModifications -= MyPostprocessModificationsCallback;
            Undo.postprocessModifications += MyPostprocessModificationsCallback;
        }

        protected Font DrawFont(string name, float offset, Font font)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(offset);
            tempFont = EditorGUILayout.ObjectField(name, font, typeof(Font), false, GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Font;
            GUILayout.EndHorizontal();
            return tempFont;
        }

        protected Sprite DrawSprite(string name, float offset, Sprite sprite)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(offset);
            tempSprite = EditorGUILayout.ObjectField(name, sprite, typeof(Sprite), false, GUILayout.Height(EditorGUIUtility.singleLineHeight)) as Sprite;
            GUILayout.EndHorizontal();
            return tempSprite;
        }

        protected void DrawFontStyle(string name, float offset, ref FontStyle fontStyle)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(offset);
            fontStyle = (FontStyle)EditorGUILayout.EnumPopup(name, fontStyle);
            GUILayout.EndHorizontal();
        }        

        protected void DrawColorMode(string name, float offset, ref ColorMode colorMode)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(offset);
            colorMode = (ColorMode)EditorGUILayout.EnumPopup(name, colorMode);
            GUILayout.EndHorizontal();
        }

        protected void DrawTextColorBlock(string name, float offset, float innerOffset, ref CoinModeMenuStyle.TextColorBlock colorBlock)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(offset);
            GUILayout.Label(name);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(innerOffset);
            colorBlock.normalColor = EditorGUILayout.ColorField("Normal Color", colorBlock.normalColor);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(innerOffset);
            colorBlock.disabledColor = EditorGUILayout.ColorField("Disabled Color", colorBlock.disabledColor);
            GUILayout.EndHorizontal();
        }

        protected void DrawColorBlock(string name, float offset, float innerOffset, ref ColorBlock colorBlock)
        {            
            GUILayout.BeginHorizontal();
            GUILayout.Space(offset);
            GUILayout.Label(name);
            GUILayout.EndHorizontal();

            BeginChangeCheck();

            GUILayout.BeginHorizontal();
            GUILayout.Space(innerOffset);
            colorBlock.normalColor = EditorGUILayout.ColorField("Normal Color", colorBlock.normalColor);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(innerOffset);
            colorBlock.highlightedColor = EditorGUILayout.ColorField("Highlighted Color", colorBlock.highlightedColor);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(innerOffset);
            colorBlock.pressedColor = EditorGUILayout.ColorField("Pressed Color", colorBlock.pressedColor);
            GUILayout.EndHorizontal();

#if UNITY_2019_1_OR_NEWER
            GUILayout.BeginHorizontal();
            GUILayout.Space(innerOffset);
            colorBlock.selectedColor = EditorGUILayout.ColorField("Selected Color", colorBlock.selectedColor);
            GUILayout.EndHorizontal();
#endif

            GUILayout.BeginHorizontal();
            GUILayout.Space(innerOffset);
            colorBlock.disabledColor = EditorGUILayout.ColorField("Disabled Color", colorBlock.disabledColor);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(innerOffset);
            colorBlock.colorMultiplier = EditorGUILayout.Slider("Color Multiplier", colorBlock.colorMultiplier, 1.0F, 5.0F);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(innerOffset);
            colorBlock.fadeDuration = EditorGUILayout.FloatField("Fade Duration", colorBlock.fadeDuration);
            GUILayout.EndHorizontal();
        }

        protected void DrawColor(string name, float offset, ref Color color)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(offset);
            color = EditorGUILayout.ColorField(name, color);
            GUILayout.EndHorizontal();
        }

        protected float DrawRange(string name, float offset, float min, float max, float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(offset);
            tempFloat = EditorGUILayout.Slider(name, value, min, max);
            GUILayout.EndHorizontal();
            return tempFloat;
        }

        protected float DrawFloat(string name, float offset, float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(offset);
            tempFloat = EditorGUILayout.FloatField(name, value);
            GUILayout.EndHorizontal();
            return tempFloat;
        }

        protected void BeginChangeCheck()
        {
            EditorGUI.BeginChangeCheck();
        }

        protected bool EndChangeCheck(string undoMessage = "")
        {
            if (EditorGUI.EndChangeCheck())
            {
                RegisterChange(undoMessage);
                return true;
            }
            return false;
        }

        protected void RegisterChange(string undoMessage = "")
        {
            Undo.RegisterCompleteObjectUndo(objectInContext, undoMessage);
            unsavedChanges = true;
        }

        protected void SaveObject(bool force = false)
        {
            Undo.ClearUndo(objectInContext);
#if UNITY_2020_3_OR_NEWER
            if (force) EditorUtility.SetDirty(objectInContext);
            AssetDatabase.SaveAssetIfDirty(objectInContext);
#else
            EditorUtility.SetDirty(objectInContext);
            AssetDatabase.SaveAssets();
#endif
            unsavedChanges = false;
        }

        private UndoPropertyModification[] MyPostprocessModificationsCallback(UndoPropertyModification[] modifications)
        {
            for (int i = 0; i < modifications.Length; i++)
            {
                if (modifications[i].currentValue.objectReference == objectInContext)
                {
                    unsavedChanges = true;
                    return modifications;
                }
            }
            return modifications;
        }
    }
}
#endif