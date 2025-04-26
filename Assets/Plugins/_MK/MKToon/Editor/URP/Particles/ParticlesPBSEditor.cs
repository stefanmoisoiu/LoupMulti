//////////////////////////////////////////////////////
// MK Toon Particles PBS Editor        			    //
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

namespace Plugins._MK.MKToon.Editor.URP.Particles
{
    internal sealed class ParticlesPBSEditor : PhysicallyBasedEditorBase
    {
        public ParticlesPBSEditor() : base(RenderPipeline.Universal) {}
        
        /////////////////////////////////////////////////////////////////////////////////////////////
		// Properties                                                                              //
		/////////////////////////////////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////////////////////////////////////////////////
		// Draw                                                                                    //
		/////////////////////////////////////////////////////////////////////////////////////////////

        /////////////////
        // Options     //
        /////////////////
        protected override void DrawEmissionFlags(MaterialEditor materialEditor)
        {

        }
        
        protected override void EmissionRealtimeSetup(Material material)
        {
            if(Properties.emissionColor.GetValue(material).maxColorComponent <= 0)
                material.globalIlluminationFlags |= MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }

        /////////////////
        // Advanced    //
        /////////////////

        /////////////////////////////////////////////////////////////////////////////////////////////
		// Variants Setup                                                                          //
		/////////////////////////////////////////////////////////////////////////////////////////////
    }
}
#endif