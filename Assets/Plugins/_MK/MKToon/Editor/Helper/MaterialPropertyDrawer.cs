//////////////////////////////////////////////////////
// MK Toon Editor Material Property Drawer Base     //
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#if UNITY_EDITOR
using UnityEngine;

namespace Plugins._MK.MKToon.Editor.Helper
{
    internal abstract class MaterialPropertyDrawer : UnityEditor.MaterialPropertyDrawer
    {
        protected GUIContent _guiContent;
        public MaterialPropertyDrawer(GUIContent ui)
        {
            this._guiContent = ui;
        }
        public MaterialPropertyDrawer()
        {
            this._guiContent = GUIContent.none;
        }
    }
}
#endif