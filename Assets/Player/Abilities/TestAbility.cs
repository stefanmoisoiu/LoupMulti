using UnityEngine;

namespace Player.Abilities
{
    public class TestAbility : Ability
    {
        [SerializeField] private Color cubeColor;
        public override void TryUseAbility(out bool success)
        {
            base.TryUseAbility(out success);
            if (success)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = transform.position + transform.forward * 2;
                cube.transform.localScale = Vector3.one * 0.5f;
                cube.GetComponent<Renderer>().material.color = cubeColor;
            }
        }
    }
}
