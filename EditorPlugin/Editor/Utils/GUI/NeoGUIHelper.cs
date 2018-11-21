using UnityEngine;
using UnityEditor;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace Neoglyphic.Editor
{
    /// <summary>
    /// Class for often-used GUI functions
    /// </summary>
    public class NeoGUIHelper
    {
        public static class Colors
        {
            public static readonly Color DisabledColor = Color.grey;
            public static readonly Color DarkGrey = Color.grey * .85f;
            public static readonly Color LightGrey = Color.grey * 1.65f;
        }

        private static Styles _styles;
        private class Styles
        {
            public GUIStyle line;
            public Color lineColor;

            public GUIStyle header;
            public GUIStyle offsetHeader;
            public GUIStyle headerCheckbox;
            public GUIStyle headerFoldout;

            //colors
            public Color prevColor;
            public Color prevTextColor;
            public Color prevBGColor;

            //floats
            public float indentSpacing = 10f;

            internal Styles()
            {
                //setup line
                lineColor = Color.grey;
                line = new GUIStyle();
                line.normal.background = EditorGUIUtility.whiteTexture;
                line.stretchWidth = true;
                
                //setup headers
                header = "ShurikenModuleTitle";
                header = new GUIStyle();
                header.font = (new GUIStyle("Label")).font;
                header.border = new RectOffset(15, 7, 4, 4);
                header.fixedHeight = 22;
                header.contentOffset = new Vector2(0, -2f);

                offsetHeader = "ShurikenModuleTitle";
                //offsetHeader = new GUIStyle();
                //offsetHeader.normal.background = EditorGUIUtility.whiteTexture;
                offsetHeader.font = (new GUIStyle("Label")).font;
                offsetHeader.border = new RectOffset(15, 7, 4, 4);
                offsetHeader.fixedHeight = 22;
                offsetHeader.contentOffset = new Vector2(20f, -2f);

                //setup header checkbox
                headerCheckbox = "ShurikenCheckMark";

                //setup foldout
                headerFoldout = "Foldout";

            }
        }

        static NeoGUIHelper()
        {
            _styles = new Styles();
        }

        public static void PushColor(Color c)
        {
            _styles.prevColor = GUI.color;
            GUI.color = c;
        }

        public static void PushTextColor(Color c)
        {
            _styles.prevTextColor = GUI.contentColor;
            GUI.contentColor = c;
        }

        public static void PushBGColor(Color c)
        {
            _styles.prevBGColor = GUI.backgroundColor;
            GUI.backgroundColor = c;
        }

        public static void PopColor()
        {
            GUI.color = _styles.prevColor;
        }

        public static void PopTextColor()
        {
            GUI.contentColor = _styles.prevTextColor;
        }

        public static void PopBGColor()
        {
            GUI.backgroundColor = _styles.prevBGColor;
        }

        public static void PushIndent()
        {
            EditorGUI.indentLevel += 1;
        }

        public static void PopIndent()
        {
            EditorGUI.indentLevel -= 1;

            if (EditorGUI.indentLevel < 0) EditorGUI.indentLevel = 0;
        }

        // header without toggle
        public static bool Header(string label, bool isDisplaying, bool useIndent = true)
        {
            var display = isDisplaying;

            Rect rect = GUILayoutUtility.GetRect(16f, 22f, _styles.offsetHeader);

            if (useIndent)
            {
                rect = GetIndentOffset(rect);
            }

            GUI.Box(rect, label, _styles.offsetHeader);

            Event e = Event.current;

            if (e.type == EventType.MouseDown)
            {
                if (rect.Contains(e.mousePosition))
                {
                    display = !display;
                    e.Use();
                }
            }

            return display;
        }

        public static bool HeaderWithFoldout(string label, bool isDisplaying, bool useIndent = true)
        {
            var display = isDisplaying;

            Rect rect = GUILayoutUtility.GetRect(16f, 22f, _styles.offsetHeader);

            if (useIndent)
            {
                rect = GetIndentOffset(rect);
            }

            label = label.ToUpper();

            GUI.Box(rect, label, _styles.offsetHeader);

            Event e = Event.current;

            Rect foldoutRect = new Rect(rect.x + 4f, rect.y + 4f, 13f, 13f);
            if (Event.current.type == EventType.Repaint)
                _styles.headerFoldout.Draw(foldoutRect, false, false, display, false);

            if (e.type == EventType.MouseDown)
            {
                if (rect.Contains(e.mousePosition) || foldoutRect.Contains(e.mousePosition))
                {
                    display = !display;
                    e.Use();
                }
            }

            return display;
        }

        public static Vector2 GetStringDimensions(string str)
        {
            return GUI.skin.label.CalcSize(new GUIContent(str));
        }

        private static Rect GetIndentOffset(Rect r)
        {
            float offset = _styles.indentSpacing * EditorGUI.indentLevel;
            r.x += offset;
            r.width -= offset;

            return r;
        }

        public static bool LabelWithToggle(Rect rect, string label, bool toggle, ref bool display, bool leftSide = true)
        {
            var enabled = false;

            rect = GetIndentOffset(rect);

            Rect toggleRect = new Rect(rect.x, rect.y + 4f, 13f, 13f);
            Rect labelRect = new Rect(toggleRect.x + toggleRect.width + 4f, rect.y, rect.width - toggleRect.width - toggleRect.x, rect.height);

            if (!leftSide) // show toggle on right side of label
            {
                labelRect.x = toggleRect.x;
                toggleRect.x = labelRect.x + labelRect.width + 4f;
            }

            if (Event.current.type == EventType.Repaint)
                _styles.headerCheckbox.Draw(toggleRect, false, false, toggle, false);

            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                if (toggleRect.Contains(e.mousePosition))
                {
                    enabled = !enabled;
                    e.Use();
                }
                else if (rect.Contains(e.mousePosition))
                {
                    display = !display;
                    e.Use();
                }
            }

            GUI.Label(labelRect, label);

            return enabled;
        }

        public static bool MaterialHeaderWithToggle(MaterialProperty prop, MaterialEditor editor, string label, bool display)
        {
			display = MaterialHeaderWithToggle(label, prop, editor, display);
			return display;
		}

        //Taken from UnityStandardAssets.CinematicEffects.EditorGUIHelper
        // header with toggle
        private static bool MaterialHeaderWithToggle(string label, MaterialProperty prop, MaterialEditor editor, bool display)
        {
            label = label.ToUpper();
			
			Rect rect = GUILayoutUtility.GetRect(16f, 22f, _styles.offsetHeader);
			rect = GetIndentOffset(rect);

			Rect toggleRect = rect;
			toggleRect.width = 12;
			toggleRect.x += 2;

			GUI.Box(rect, label, _styles.offsetHeader);

			editor.ShaderProperty(toggleRect, prop, "", -EditorGUI.indentLevel);
			
			if (Event.current.type == EventType.MouseDown)
			{
				if (rect.Contains(Event.current.mousePosition))
				{
					display = !display;
					Event.current.Use();
				}
			}

			return display;
		}

        public static void Line()
        {
            Line(1, _styles.lineColor);
        }

        public static void Line(Color c)
        {
            Line(1, c);
        }

        public static void Line(float height)
        {
            Line(height, _styles.lineColor);
        }

        public static void Line(float height, Color c)
        {
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, _styles.line, GUILayout.Height(height));

            if (Event.current.type != EventType.Repaint) return;

            Color prev = GUI.color;
            GUI.color = c;
            _styles.line.Draw(r, false, false, false, false);
            GUI.color = prev;
        }
    }
}
