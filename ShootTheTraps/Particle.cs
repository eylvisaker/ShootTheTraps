using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ShootTheTraps
{
    public class Particle : GameObject
    {
        private Color color;
        private int alpha = 255;
        private int mImageIndex;
        private const double particleLifeTimeMilliseconds = 2000;

        public static List<Texture2D> Images { get; private set; } = new List<Texture2D>();
        

        public override Rectangle BoundingRect
        {
            get
            {
                return new Rectangle(
                    (int)Position.X,
                    (int)Position.Y,
                    1,
                    1);
            }
        }
        /// Creates a new instance of Particle */
        public Particle(Color clr, Random rnd)
        {
            Acceleration.Y = Gravity;

            color = clr;

            mImageIndex = rnd.Next(Images.Count);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            alpha = (int)(255 * (1 - LifeTime_ms / particleLifeTimeMilliseconds));
            if (alpha < 0)
            {
                alpha = 0;
                return;
            }

            var image = Images[mImageIndex];

            //image.DisplayAlignment = OriginAlignment.Center;
            //image.RotationCenter = OriginAlignment.Center;
            //image.Color = Color.FromArgb(alpha, color);
            //image.RotationAngle = RotationAngle;

            //image.Draw((float)Position.X, (float)Position.Y);

            spriteBatch.Draw(image,
                position: Position,
                origin: new Vector2(image.Width / 2, image.Height / 2),
                rotation: RotationAngle,
                color: new Color(color, alpha));
        }

        public override bool DeleteMe
        {
            get
            {
                if (OutsideField)
                    return true;

                if (alpha <= 0)
                    return true;
                else
                    return false;
            }
        }
    }

}
