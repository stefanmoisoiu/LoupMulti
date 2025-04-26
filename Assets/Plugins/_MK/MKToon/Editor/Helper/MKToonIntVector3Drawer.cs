//////////////////////////////////////////////////////
// MK Toon Editor Int Slider Drawer        			//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Plugins._MK.MKToon.Editor.Helper
{
    internal class MKToonVector3Drawer : MaterialPropertyDrawer
    {
        public MKToonVector3Drawer(GUIContent ui) : base(ui) {}
        public MKToonVector3Drawer() : base(GUIContent.none) {}

        public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            Vector3 vectorValue = prop.vectorValue;
            EditorGUI.BeginChangeCheck();

            vectorValue = EditorGUI.Vector3Field(position, new GUIContent(label), vectorValue);

            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = vectorValue;
            }
            EditorGUI.showMixedValue = false;
        }
    }
    internal class MKToonVertexAnimationFrequencyDrawer : MKToonVector3Drawer
    {
        public MKToonVertexAnimationFrequencyDrawer() : base(UI.vertexAnimationFrequency) {}
    }
}
#endif