using Microsoft.Xna.Framework.Graphics;

namespace Solfège
{
    public class ZombieAnimator
    {
        public Texture2D idleTexture;
        public Texture2D attackTexture;

        public float attackTimer;
        public const float AttackFrameDuration = 0.30f;


        public ZombieAnimator(Texture2D idle, Texture2D attack)
        {
            idleTexture = idle;
            attackTexture = attack;
        }


        public void TriggerAttack()
        {
            attackTimer = AttackFrameDuration;
        }


        public void Update(float elapsed)
        {
            if (attackTimer > 0f)
            {
                attackTimer -= elapsed;
            }
        }


        public Texture2D CurrentTexture
        {
            get
            {
                if (attackTimer > 0f)
                {
                    return attackTexture;
                }

                return idleTexture;
            }
        }
    }
}
