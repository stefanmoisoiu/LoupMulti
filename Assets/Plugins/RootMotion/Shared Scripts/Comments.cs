using UnityEngine;

namespace Plugins.RootMotion.Shared_Scripts {

	/// <summary>
	/// Adding comments to GameObjects in the Inspector.
	/// </summary>
	public class Comments : MonoBehaviour {
	
		/// <summary>
		/// The comment.
		/// </summary>
		[Multiline]
		public string text;
	}
}
