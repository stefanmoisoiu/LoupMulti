//////////////////////////////////////////////////////
// MK Toon URP Standard Simple Editor        		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#if UNITY_EDITOR
using Plugins._MK.MKToon.Editor.Base;
using Plugins._MK.MKToon.Scripts;
using UnityEditor;
using UnityEngine;

namespace Plugins._MK.MKToon.Editor.URP.Standard
{
    internal class StandardSimpleEditor : SimpleEditorBase 
    {
        public StandardSimpleEditor() : base(RenderPipeline.Universal) {}

        protected override void DrawEmissionFlags(MaterialEditor materialEditor)
        {

        }

        protected override void EmissionRealtimeSetup(Material material)
        {
            if(Properties.emissionColor.GetValue(material).maxColorComponent <= 0)
                material.globalIlluminationFlags |= MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }
        
        protected override void DrawPipeline(MaterialEditor materialEditor)
        {
            DrawPipelineHeader();

            materialEditor.EnableInstancingField();
            DrawRenderPriority(materialEditor);
            DrawAddPrecomputedVelocity(materialEditor);
        }
    }
}
#endif