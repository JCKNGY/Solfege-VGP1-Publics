using Microsoft.Xna.Framework.Graphics;

namespace Solfège
{
    public class ZombieAnimator
    {
        private Texture2D idleTexture;
        private Texture2D attackTexture;

        private float attackTimer;
        private const float AttackFrameDuration = 0.30f;

        // save the idle and attack texture
        public ZombieAnimator(Texture2D idle, Texture2D attack)
        {
            idleTexture = idle;
            attackTexture = attack;
        }

        // switch to attack texture for a bit
        public void TriggerAttack()
        {
            attackTimer = AttackFrameDuration;
        }

        // count down the attack frame timer
        public void Update(float elapsed)
        {
            if (attackTimer > 0f)
                attackTimer -= elapsed;
        }

        public Texture2D CurrentTexture => attackTimer > 0f ? attackTexture : idleTexture;
    }
}
