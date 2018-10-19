using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ShootTheTraps
{
    public class Trap : GameObject
    {
        private const int width = 24;
        private const int height = 16;
        private Color color = Color.Red;
        private bool mDeleteMe = false;
        private static Random sRandom;
        private static Color[] sColors =
        {
            Color.White, Color.Blue, Color.Red,
            Color.Purple, Color.Yellow, Color.Green,
            Color.Cyan,
        };

        public static Texture2D Image { get; set; }

        /// <summary>
        /// Creates a new instance of Trap 
        /// </summary>
        public Trap()
        {
            if (sRandom == null)
                sRandom = new Random();

            // only gravity affects this object.
            Acceleration = new Vector2(0, Gravity);

            color = sColors[sRandom.Next(sColors.Length)];
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

        public void SetDeleteMeFlag()
        {
            mDeleteMe = true;
        }

        public bool ContainsPoint(Vector2 pt)
        {
            Vector2 dist = Position - pt;

            if (Math.Abs(dist.X) > width / 2) return false;
            if (Math.Abs(dist.Y) > height / 2) return false;

            return true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //Image.DisplayAlignment = OriginAlignment.Center;
            //Image.RotationCenter = OriginAlignment.Center;

            //Image.Color = color;
            //Image.RotationAngle = RotationAngle;

            //Image.Draw(Position.X, Position.Y);

            spriteBatch.Draw(Image,
                             Position,
                             (Rectangle?)null,
                             color,
                             RotationAngle,
                             new Vector2(Image.Width / 2, Image.Height / 2),
                             1,
                             SpriteEffects.None,
                             0);

            if (OutsideField && Velocity.Y > 0)
                mDeleteMe = true;
            else
                mDeleteMe = false;
        }

        public override bool DeleteMe
        {
            get { return mDeleteMe; }
        }

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        private const int NumberOfParticles = 20;
        private const double ParticleSpeed = 100;

        protected override List<GameObject> ProtectedCreateDebris()
        {
            List<GameObject> retval = new List<GameObject>();
            Vector2 totalVelocity = Vector2.Zero;
            Random rnd = new Random();

            for (int i = 0; i < NumberOfParticles; i++)
            {
                Particle p = new Particle(Color, rnd);

                p.Position = Position;

                p.Velocity.X = (float)sRandom.NextDouble() * 2 - 1;
                p.Velocity.Y = (float)sRandom.NextDouble() * 2 - 1;
                p.Velocity.Normalize();

                p.Velocity *= (float)(sRandom.NextDouble() * ParticleSpeed);
                p.RotationalVelocity = (float)(sRandom.NextDouble() - 0.5) * 40;

                totalVelocity = totalVelocity + p.Velocity;
                retval.Add(p);
            }

            // now apply conservation of momentum, by giving a small portion
            // of the excess momentum to each particle
            Vector2 give = totalVelocity * (-1.0f / NumberOfParticles);

            for (int i = 0; i < NumberOfParticles; i++)
            {
                Particle p = (Particle)retval[i];

                p.Velocity = p.Velocity + Velocity + give;
            }


            return retval;
        }

    }
}