using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Popcron.Referencer
{
    [CustomEditor(typeof(References))]
    public class ReferencesInspector : Editor
    {
        private List<Reference> list = new List<Reference>();
        private static List<Type> allTypes = null;
        private string selectedAsset;
        private int lastSize;

        private string SearchQuery
        {
            get => EditorPrefs.GetString($"{Settings.UniqueIdentifier}.SearchQuery", "");
            set => EditorPrefs.SetString($"{Settings.UniqueIdentifier}.SearchQuery", value);
        }

        private void OnEnable()
        {
            Search(SearchQuery);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty assets = serializedObject.FindProperty("assets");

            //size changed, repaint and research again
            if (assets.arraySize != lastSize)
            {
                lastSize = assets.arraySize;
                Search(SearchQuery);
                Repaint();
            }

            //show search field
            SearchField();

            //list size dont match array size
            string title = $"{assets.arraySize} built in assets";
            if (assets.arraySize != list.Count)
            {
                title = $"Found {list.Count} assets";
            }

            //draw the list
            Event e = Event.current;
            assets.isExpanded = EditorGUILayout.Foldout(assets.isExpanded, new GUIContent(title), true);
            if (assets.isExpanded)
            {
                float lineHeight = 22f;
                for (int i = 0; i < list.Count; i++)
                {
                    Reference element = list[i];
                    Color color = i % 2 == 0 ? Color.white : Color.gray;
                    color.a = 0.1f;

                    //if selected
                    if (element.Path.Equals(selectedAsset, StringComparison.OrdinalIgnoreCase))
                    {
                        color = GUI.skin.settings.selectionColor;
                    }

                    //draw each element
                    Rect rect = EditorGUILayout.GetControlRect(true, lineHeight);
                    float y = GUIUtility.GUIToScreenRect(rect).y;
                    if (rect.y > 0 && y > 0 && y < Screen.height)
                    {
                        if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
                        {
                            if (e.clickCount == 2)
                            {
                                //double click, point to file
                                EditorGUIUtility.PingObject(element.Object);
                            }

                            //clicked here
                            selectedAsset = element.Path;
                            Repaint();
                            break;
                        }

                        //draw rect behind
                        EditorGUI.DrawRect(rect, color);

                        //fits on screen, so draw the icon first
                        Rect iconPosition = new Rect(rect.x, rect.y, rect.height, rect.height);
                        GUI.DrawTexture(iconPosition, EditorGUIUtility.ObjectContent(element.Object, element.Type).image);

                        //then draw the label
                        Rect labelPosition = new Rect(rect.x + rect.height, rect.y, rect.width - rect.height, rect.height);

                        string tooltip = $"Path: {element.Path}\nType: {element.Type}";
                        if (element.Object)
                        {
                            GUIContent label = new GUIContent(element.Object.name, tooltip);
                            EditorGUI.LabelField(labelPosition, label);
                        }
                        else
                        {
                            GUIContent label = new GUIContent(element.Path, tooltip);
                            EditorGUI.LabelField(labelPosition, label);
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void SearchField()
        {
            string newSearch = GUILayout.TextField(SearchQuery, EditorStyles.toolbarTextField, GUILayout.Width(EditorGUIUtility.currentViewWidth - 40));
            if (!newSearch.Equals(SearchQuery, StringComparison.OrdinalIgnoreCase))
            {
                SearchQuery = newSearch;
                Search(newSearch);
            }
        }

        /// <summary>
        /// Custom get type method because of stupid system get type.
        /// </summary>
        private Type GetType(string type)
        {
            if (allTypes == null)
            {
                allTypes = new List<Type>();
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                    Type[] types = assembly.GetTypes();
                    allTypes.AddRange(types);
                }
            }

            for (int i = 0; i < allTypes.Count; i++)
            {
                if (allTypes[i].FullName.Equals(type, StringComparison.OrdinalIgnoreCase))
                {
                    return allTypes[i];
                }
                else if (allTypes[i].Name.Equals(type, StringComparison.OrdinalIgnoreCase))
                {
                    return allTypes[i];
                }
            }

            return null;
        }

        private void Search(string query)
        {
            References references = target as References;
            list.Clear();

            if (string.IsNullOrEmpty(query))
            {
                //empty, so populate from origin
                list = references.Assets.ToList();
            }
            else
            {
                //if starting with t:, then only search based on type
                if (query.StartsWith("t:", StringComparison.OrdinalIgnoreCase))
                {
                    string type = query.Substring(2);
                    Type systemType = GetType(type);
                    for (int i = 0; i < references.Assets.Count; i++)
                    {
                        Reference item = references.Assets[i];
                        if (item.Type.Name.Equals(type, StringComparison.OrdinalIgnoreCase))
                        {
                            list.Add(item);
                        }
                        else if (item.Type.FullName.Equals(type, StringComparison.OrdinalIgnoreCase))
                        {
                            list.Add(item);
                        }
                        else if (systemType != null)
                        {
                            if (item.Type == systemType)
                            {
                                list.Add(item);
                            }
                            else if (systemType.IsAssignableFrom(item.Type))
                            {
                                list.Add(item);
                            }
                        }
                    }
                }
                else
                {
                    //search using contains without case sensitivity
                    for (int i = 0; i < references.Assets.Count; i++)
                    {
                        Reference asset = references.Assets[i];
                        if (asset.Object)
                        {
                            if (asset.Path.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                list.Add(asset);
                            }
                            else if (asset.Object.name.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                list.Add(asset);
                            }
                            else if (asset.Type.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                list.Add(asset);
                            }
                        }
                    }
                }
            }

            Repaint();
        }
    }
}