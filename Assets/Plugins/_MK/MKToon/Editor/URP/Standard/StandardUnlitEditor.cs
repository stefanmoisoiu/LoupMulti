//////////////////////////////////////////////////////
// MK Toon URP Standard Unlit Editor        		//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#if UNITY_EDITOR
using Plugins._MK.MKToon.Editor.Base;

namespace Plugins._MK.MKToon.Editor.URP.Standard
{
    internal class StandardUnlitEditor : UnlitEditorBase 
    {
        public StandardUnlitEditor() : base(RenderPipeline.Universal) {}
    }
}
#endif