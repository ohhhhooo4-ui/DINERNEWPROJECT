using System.Collections.Generic;
using SnogDialogue.Runtime;
using UnityEditor;
using UnityEngine;

namespace SnogDialogue.Editor
{
    public static class GlobalVariableKeyUtility
    {
        public static List<string> GetAllKnownGlobalKeys()
        {
            List<string> keys = new List<string>();

            string[] guids = AssetDatabase.FindAssets("t:GlobalVariablesAsset");

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GlobalVariablesAsset asset = AssetDatabase.LoadAssetAtPath<GlobalVariablesAsset>(path);

                if (asset == null)
                {
                    continue;
                }

                SerializedObject so = new SerializedObject(asset);
                SerializedProperty entries = so.FindProperty("entries");

                if (entries == null || !entries.isArray)
                {
                    continue;
                }

                for (int e = 0; e < entries.arraySize; e++)
                {
                    SerializedProperty entry = entries.GetArrayElementAtIndex(e);
                    SerializedProperty keyProp = entry.FindPropertyRelative("Key");

                    if (keyProp == null)
                    {
                        continue;
                    }

                    string key = keyProp.stringValue;

                    if (string.IsNullOrWhiteSpace(key))
                    {
                        continue;
                    }

                    if (!keys.Contains(key))
                    {
                        keys.Add(key);
                    }
                }
            }

            keys.Sort();
            return keys;
        }
    }
}