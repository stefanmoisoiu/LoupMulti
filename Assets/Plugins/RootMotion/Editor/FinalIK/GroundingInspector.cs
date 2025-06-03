using Plugins.RootMotion.Editor.Shared_Scripts;
using Plugins.RootMotion.FinalIK.Grounder;
using UnityEngine;

namespace Plugins.RootMotion.Editor.FinalIK {

	public class GroundingInspector : UnityEditor.Editor {

		public static void Visualize(Grounding grounding, Transform root, Transform foot) {
			Inspector.SphereCap (0, foot.position + root.forward * grounding.footCenterOffset, root.rotation, grounding.footRadius * 2f);
		}
	}
}
