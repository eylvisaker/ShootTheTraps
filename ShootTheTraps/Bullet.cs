using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShootTheTraps
{
    public class Bullet : GameObject
    {
        public static Texture2D Image { get; set; }

        public Bullet()
        {
        }

        /// <summary>
        /// Draws the image for the bullet.
        /// </summary>
        public override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 direction = Velocity;
            direction.Normalize();

            //Image.DisplayAlignment = OriginAlignment.Center;
            //Image.RotationCenter = OriginAlignment.Center;
            //Image.RotationAngle = RotationAngle;

            //Image.Draw((float)Position.X, (float)Position.Y);

            spriteBatch.Draw(Image, 
                             Position, 
                             null, 
                             Color.White, 
                             RotationAngle, 
                             Vector2.Zero,
                             1, 
                             SpriteEffects.None, 
                             0);
        }

        public override Rectangle BoundingRect
        {
            get
            {
                int width = Image.Width;
                int height = Image.Height;

                return new Rectangle(
                    (int)Position.X - width / 2,
                    (int)Position.Y - height / 2,
                    width,
                    height);
            }
        }

        /// <summary>
        /// Delete the bullet if it has gone outside the bounds of the screen.
        /// </summary>
        public override bool DeleteMe
        {
            get
            {
                if (OutsideField)
                    return true;

                return false;
            }
        }
    }
}
