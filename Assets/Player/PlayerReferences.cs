using Player.Camera;
using Player.Movement;
using Player.Movement.Stamina;
using Player.Tool.Drill;
using UnityEngine;

namespace Player
{
    public class PlayerReferences : MonoBehaviour
    {
        [SerializeField] private Hitbox.HitboxTarget hitboxTarget;
        public Hitbox.HitboxTarget HitboxTarget => hitboxTarget;
        [SerializeField] private Health.Health health;
        public Health.Health Health => health;

        [SerializeField] private Movement.Movement movement;
        public Movement.Movement Movement => movement;
        [SerializeField] private Grounded grounded;
        public Grounded Grounded => grounded;
        [SerializeField] private Jump jump;
        public Jump Jump => jump;
        [SerializeField] private Run run;
        public Run Run => run;
        [SerializeField] private Stamina stamina;
        public Stamina Stamina => stamina;
        [SerializeField] private PCamera pCamera;
        public PCamera PCamera => pCamera;
        [SerializeField] private Drill drill;
        public Drill Drill => drill;
        
    }
}