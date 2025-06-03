using Plugins.RootMotion.FinalIK.IK_Components;
using UnityEditor;
using UnityEngine;

namespace Plugins.RootMotion.Editor.FinalIK {

	// Custom inspector for IKExecutionOrder
	[CustomEditor(typeof(IKExecutionOrder))]
	public class IKExecutionOrderInspector : UnityEditor.Editor {

		private IKExecutionOrder script { get { return target as IKExecutionOrder; }}

		private MonoScript monoScript;

		void OnEnable() {
			if (serializedObject == null) return;
			
			// Changing the script execution order
			if (!Application.isPlaying) {
				int executionOrder = 9996;
				monoScript = MonoScript.FromMonoBehaviour(script);
				int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);
				if (currentExecutionOrder != executionOrder) MonoImporter.SetExecutionOrder(monoScript, executionOrder);
			}
		}
	}
}
