using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Base_Scripts.Outline
{
    public class OutlineRenderFeature : ScriptableRendererFeature
    {
        List<OutlineGroup> groups = new List<OutlineGroup>();
    
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        public Material compMat;
    
        private OutlinePass[] passes;
        // Enregistre un groupe
        internal static void RegisterGroup(OutlineGroup g)
        {
            if (Instance.groups.Contains(g)) return;
            Instance.groups.Add(g);
        }

        internal static void UnregisterGroup(OutlineGroup g)
        {
            if (!Instance.groups.Contains(g)) return;
            Instance.groups.Remove(g);
        }

        static OutlineRenderFeature Instance { get; set; }

        public override void Create()
        {
            Instance = this;
        
            passes = new OutlinePass[groups.Count];
            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                passes[i] = new OutlinePass(group, compMat);
                passes[i].renderPassEvent = renderPassEvent;
            }
        }
    
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (groups.Count == 0) return;

            foreach (var pass in passes) 
            {
                if (pass == null) continue;
                renderer.EnqueuePass(pass);
            }
        }
        class OutlinePass : ScriptableRenderPass
        {
            readonly OutlineGroup group;
            readonly Material compMat;

            public OutlinePass(OutlineGroup g, Material compMat)
            {
                requiresIntermediateTexture = true;
                group = g;
                this.compMat = compMat;
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
            }
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                // 2) Création de la texture masque
                var resData = frameData.Get<UniversalResourceData>();
                var sceneDesc = renderGraph.GetTextureDesc(resData.activeColorTexture);
                var compDesc = new TextureDesc(sceneDesc)
                {
                    clearBuffer = false 
                };
                TextureHandle destHandle = renderGraph.CreateTexture(compDesc);
            
                RenderGraphUtils.BlitMaterialParameters blitParams = new RenderGraphUtils.BlitMaterialParameters
                {
                    material = compMat,
                    source   = resData.activeColorTexture,
                    destination = destHandle,
                };
            
                renderGraph.AddBlitPass(blitParams, passName:"Outline Pass");
                resData.cameraColor = destHandle;

            }
        }
    }
}
