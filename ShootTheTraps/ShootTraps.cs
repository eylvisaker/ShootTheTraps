using AgateLib;
using AgateLib.Mathematics;
using AgateLib.Mathematics.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShootTheTraps
{
    public class ShootTraps : IDisposable
    {
        private List<GameObject> mGameObjects;
        private Random mRandom;
        private readonly int mGroundY;
        private readonly int mTrapVYScale;
        private readonly int mTrapMaxVX;
        private readonly int mTrapMinVX;
        private readonly Vector2 gunPosition;
        private const int BulletSpeed = 800;
        private const int MaxBullets = 4;
        private int[] mLaunchX;
        private bool mFiredTraps;
        private Texture2D gunImage;
        private Texture2D gunBolt;
        private Point mMousePos;
        private float gunAngle;
        private float levelElapsedTime_ms;

        /// <summary>
        /// Creates a new instance of ShootTraps 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public ShootTraps(int width, int height, IContentProvider content)
        {
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException();

            gunImage = content.Load<Texture2D>("barrel");
            gunBolt = content.Load<Texture2D>("barrel-hex");

            mGameObjects = new List<GameObject>();
            mRandom = new Random();

            mGroundY = height - 10;

            // choose a y velocity (pixels / second) that will get the 
            // traps nearly to the top of the screen
            mTrapVYScale = (int)Math.Sqrt(2 * GameObject.Gravity * (height - 10));

            double timeInAir = mTrapVYScale * 2 / GameObject.Gravity;

            // choose a minimum x velocity so that the trap will make it at least 1/3
            // the way across the field.
            mTrapMaxVX = mTrapVYScale / 2;
            mTrapMinVX = (int)(width / (3.0 * timeInAir));

            gunPosition = new Vector2(width / 2, mGroundY);

            mLaunchX = new int[4];

            // area of screen where traps are launched from.
            int launchScale = width / 2 - 50;

            mLaunchX[0] = width / 2 - launchScale;
            mLaunchX[1] = width / 2 - launchScale + 40;

            mLaunchX[2] = width - mLaunchX[1];
            mLaunchX[3] = width - mLaunchX[0];

            GameObject.FieldArea = RectangleX.FromLTRB(-10, -20, width + 10, (int) gunPosition.Y);

            Bullet.Image      = content.Load<Texture2D>("bullet");
            Trap.Image        = content.Load<Texture2D>("enemy");
            Particle.Images.Add(content.Load<Texture2D>("splatter-1"));
            Particle.Images.Add(content.Load<Texture2D>("splatter-2"));
            Particle.Images.Add(content.Load<Texture2D>("splatter-3"));
            Particle.Images.Add(content.Load<Texture2D>("splatter-4"));

        }

        /// <summary>
        /// Total time elapsed in milliseconds.
        /// </summary>
        public float LevelTimeElapsed => levelElapsedTime_ms;

        public void Dispose()
        {
            gunImage.Dispose();
            Bullet.Image.Dispose();
            Trap.Image.Dispose();
            Particle.Images.ForEach(x => x.Dispose());
        }

        private void AddScore(int points)
        {
            if (points == BonusPoints)
                BonusPoints *= ScoreMultiplier;

            points *= ScoreMultiplier;

            Score += points;
            PointsThisLevel += points;
        }
        public void NextLevel()
        {
            if (CanAdvanceLevel == false)
                return;

            ScoreMultiplier = 1;

            CanAdvanceLevel = false;
            levelElapsedTime_ms = 0;

            Level++;
            PullsLeft = Math.Min(9 + 2 * Level, 21);

            PointsThisLevel = 0;
        }

        public void Update(GameTime gameTime)
        {
            levelElapsedTime_ms += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            UpdateAllObjects(gameTime);
            CheckForCollisions();

            CalcBonus();
            CheckEndLevel();

            DeleteObjects();
        }

        private void UpdateAllObjects(GameTime gameTime)
        {
            mGameObjects.ForEach(x => x.Update((float)gameTime.ElapsedGameTime.TotalMilliseconds));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (GameObject obj in mGameObjects)
            {
                obj.Draw(spriteBatch);
            }
            
            spriteBatch.Draw(gunImage, gunPosition, (Rectangle?)null,
                             Color.White, gunAngle, 
                             new Vector2(gunImage.Width / 2, gunImage.Height), 
                             1,
                             SpriteEffects.None, 0f);

            //gunBolt.DisplayAlignment = OriginAlignment.Center;
            //gunBolt.RotationCenter = OriginAlignment.Center;
            //gunBolt.RotationAngle = gunAngle;

            //gunBolt.Draw(mGunX, mGunY);
            spriteBatch.Draw(gunBolt, gunPosition, null, 
                             Color.White, gunAngle, 
                             new Vector2(gunBolt.Width / 2, gunBolt.Height / 2), 
                             1,
                             SpriteEffects.None, 0);
        }

        private void DeleteObjects()
        {
            for (int i = 0; i < mGameObjects.Count; i++)
            {
                if (mGameObjects[i].DeleteMe)
                {
                    List<GameObject> extras = mGameObjects[i].CreateDebris();

                    mGameObjects.RemoveAt(i--);

                    if (extras != null)
                        mGameObjects.AddRange(extras);
                }
            }
        }

        private void CalcBonus()
        {
            if (TrapCount != 0 || mFiredTraps == false)
                return;
            if (BonusAdded)
                return;
            if (EndOfLevelBonus)
                return;

            if (TrapsHit >= 5 && TrapsHit >= MaxTraps - 2 && Level >= 6)
            {
                if (ScoreMultiplier < 3)
                    ScoreMultiplier++;
            }
            else if (TrapsHit == 0)
                ScoreMultiplier = 1;
            else if (TrapsHit < TrapsFired && ScoreMultiplier > 1)
                ScoreMultiplier--;

            BonusAdded = true;
            BonusPoints = 0;

            int bonusRate = 250;

            if (Level >= 15) bonusRate = 100;
            else if (Level >= 10) bonusRate = 150;
            else if (Level >= 5) bonusRate = 200;

            if (TrapsHit > 1)
            {
                BonusPoints = bonusRate * (TrapsHit - 1);
            }

            if (BonusPoints == 0)
            {
                BonusAdded = false;
            }

            mFiredTraps = false;
            AddScore(BonusPoints);
        }

        private void CheckEndLevel()
        {
            if (TrapCount != 0)
                return;
            if (BonusAdded)
                return;
            if (EndOfLevelBonus)
                return;

            // check for game over conditions
            if (PointsThisLevel >= LevelRequirement)
            {
                if (BonusPoints != 0)
                    return;

                CanAdvanceLevel = true;

                if (PullsLeft > 0 && EndOfLevelBonus == false)
                {
                    ScoreMultiplier = 1;
                    EndOfLevelBonus = true;
                    BonusPoints = ExtraPullBonus * PullsLeft;
                    AddScore(BonusPoints);
                }
            }
            else if (PullsLeft == 0)
            {
                GameOver = true;
            }
        }

        #region --- Collision Checking ---

        private void CheckForCollisions()
        {
            foreach (GameObject obj in mGameObjects)
            {
                if (obj is Bullet == false)
                    continue;

                Bullet bullet = (Bullet)obj;

                Rectangle bulletRect = bullet.BoundingRect;

                foreach (GameObject t in mGameObjects)
                {
                    if (t is Trap == false) continue;
                    if (t.DeleteMe) continue;

                    Trap trap = (Trap)t;

                    if (RectsIntersect(bulletRect, bullet.RotationAngle, trap.BoundingRect, trap.RotationAngle))
                    {
                        trap.SetDeleteMeFlag();
                        trap.ShouldCreateDebris = true;

                        TrapsHit++;

                        int score = 50;

                        if (trap.Color == Color.White)
                            score = 100;

                        AddScore(score);
                    }
                }
            }
        }

        // store these in the object instance so that we
        // don't generate a whole lot of garbage when checking for
        // intersections.
        private Vector2[] vertsA = new Vector2[4];
        private Vector2[] vertsB = new Vector2[4];

        private bool RectsIntersect(Rectangle rectA, float angleA, Rectangle rectB, float angleB)
        {
            // produce vertices for each rectangle
            ComputeVertices(rectA, angleA, vertsA);
            ComputeVertices(rectB, angleB, vertsB);

            // now we need to do the separating axis test for each edge in each square.
            if (FindSeparatingAxis(vertsA, vertsB))
                return false;

            if (FindSeparatingAxis(vertsB, vertsA))
                return false;

            return true;
        }

        /// <summary>
        /// Checks to see if any of the lines in the first set of vectors groups
        /// all the points in the second set of vectors entirely into one side.
        /// This algorithm can be used to determine if two convex polygons intersect.
        /// </summary>
        /// <param name="va"></param>
        /// <param name="vb"></param>
        /// <returns></returns>
        private bool FindSeparatingAxis(Vector2[] va, Vector2[] vb)
        {
            for (int i = 0; i < va.Length; i++)
            {
                int next = i + 1;
                if (next == va.Length) next = 0;

                int nextnext = next + 1;
                if (nextnext == va.Length) nextnext = 0;

                Vector2 edge = va[next] - va[i];

                bool separating = true;

                // first check to see which side of the axis the points in 
                // va are on, stored in the inSide variable.
                Vector2 indiff = va[nextnext] - va[i];
                var indot = Vector2.Dot(indiff, edge);
                int inSide = Math.Sign(indot);
                int lastSide = 0;

                for (int j = 0; j < vb.Length; j++)
                {
                    Vector2 diff = vb[j] - va[i];

                    var dot = Vector2.Dot(diff, edge);
                    var side = Math.Sign(dot);

                    // this means points in vb are on the same side 
                    // of the edge as points in va. Thus, it is not 
                    // a separating axis.
                    if (side == inSide)
                    {
                        separating = false;
                        break;
                    }

                    if (lastSide == 0)
                        lastSide = side;
                    else if (lastSide != side)
                    {
                        // if we fail here, it means the axis goes right through
                        // the polygon defined in vb, so this is not a separating
                        // axis.
                        separating = false;
                        break;
                    }
                }

                if (separating)
                    return true;
            }

            return false;
        }
        private void ComputeVertices(Rectangle rect, float angle, Vector2[] verts)
        {
            Vector2 center = RectCenter(rect);

            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);

            // translate so the center of the rect is at the origin, apply the rotation
            // and translate back.
            verts[0] = center + RotatePoint(cos, sin, new Vector2(rect.Left, rect.Top) - center);
            verts[1] = center + RotatePoint(cos, sin, new Vector2(rect.Right, rect.Top) - center);
            verts[2] = center + RotatePoint(cos, sin, new Vector2(rect.Right, rect.Bottom) - center);
            verts[3] = center + RotatePoint(cos, sin, new Vector2(rect.Left, rect.Bottom) - center);
        }

        private Vector2 RectCenter(Rectangle rect)
        {
            return new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }

        private Vector2 RotatePoint(float cos, float sin, Vector2 point)
        {
            Vector2 retval = new Vector2(
                (cos * point.X - sin * point.Y),
                (sin * point.X + cos * point.Y));

            return retval;
        }

        #endregion

        public void MouseMove(int mouseX, int mouseY)
        {
            mMousePos = new Point(mouseX, mouseY);

            gunAngle = MathF.Atan2(
                gunPosition.Y - mouseY,
                gunPosition.X - mouseX) - MathF.PI / 2;
        }
        public void FireBullet(int towardsX, int towardsY)
        {
            if (BulletCount >= MaxBullets)
                return;
            if (GameOver)
                return;

            Vector2 direction = new Vector2(towardsX - gunPosition.X, towardsY - gunPosition.Y);
            direction.Normalize();

            if (direction.Y > 0)
            {
                if (direction.X == 0)
                    direction.X = 1;

                direction.Normalize();
            }

            Bullet ar = new Bullet();

            ar.RotationAngle = gunAngle;
            ar.Position = gunPosition;

            ar.Velocity.X = direction.X * BulletSpeed;
            ar.Velocity.Y = direction.Y * BulletSpeed;

            mGameObjects.Add(ar);
        }
        public void FireTraps()
        {
            if (GameOver)
                return;
            if (PullsLeft <= 0)
                return;

            // Don't fire traps if there are any traps left on the screen, or 
            // if the player has fired some bullets on the screen. No cheating that way!
            if (mGameObjects.Count > 0)
                return;

            PullsLeft--;
            TrapsHit = 0;
            BonusPoints = 0;
            BonusAdded = false;
            mFiredTraps = true;

            TrapsFired = mRandom.Next(MinTraps, MaxTraps + 1);

            if (PullsLeft == 0)
                TrapsFired = MaxTraps;

            for (int i = 0; i < TrapsFired; i++)
            {
                Trap t = new Trap();

                int xpos = mRandom.Next(mLaunchX.Length);

                t.Position.X = mLaunchX[xpos];
                t.Position.Y = mGroundY;

                t.Velocity.X = mTrapMinVX + mRandom.Next(mTrapMaxVX - mTrapMinVX);
                t.Velocity.Y = -mTrapVYScale * (float)(1 - mRandom.Next(50) / 200.0);

                t.RotationalVelocity = (float)(mRandom.NextDouble() - 0.5) * 40;

                if (t.Position.X > gunPosition.X)
                    t.Velocity.X *= -1;

                mGameObjects.Add(t);
            }
        }

        private int MinTraps
        {
            get
            {
                int minTraps = 1;

                if (Level >= 8) minTraps++;
                if (Level >= 16) minTraps++;

                return minTraps;
            }
        }

        public int BulletCount
        {
            get { return mGameObjects.Count(x => x is Bullet); }
        }
        public int TrapCount
        {
            get { return mGameObjects.Count(x => x is Trap); }
        }
        public int TrapsFired { get; private set; }

        public int MaxTraps
        {
            get { return Level / 2 + 2; }
        }
        public bool CanAdvanceLevel { get; private set; }
        public bool GameOver { get; private set; }
        public int Level { get; private set; }
        public int LevelRequirement
        {
            get { return 1000 * Level; }
        }
        public int PullsLeft { get; private set; }
        public int TrapsHit { get; private set; }
        public bool BonusAdded { get; private set; }
        public bool EndOfLevelBonus { get; private set; }
        public int BonusPoints { get; private set; }
        public int PointsThisLevel { get; set; }
        public int Score { get; set; }
        public int ScoreMultiplier { get; set; }
        public bool FiredTraps { get { return mFiredTraps; } }

        public int ExtraPullBonus
        {
            get { return 250; }
        }
        public string LevelMessage
        {
            get
            {
                switch (Level)
                {
                    case 2: return "Extra pulls! Score " + LevelRequirement.ToString() + " to advance";
                    case 4: return "Up to 4 traps at a time!";
                    case 6: return "Hit 5 traps to multiply score!";
                    case 8: return "Bonus chance on every pull!";
                    case 12: return "Hit 6 traps to multiply score!";
                    case 16: return "At least 3 traps at a time!";
                    case 5:
                    case 10:
                    case 15:
                        return "Bonus REDUCED!";
                    default:
                        return "Score " + LevelRequirement.ToString() + " to advance";
                }
            }
        }

        public void SkipToNextLevel()
        {
            CanAdvanceLevel = true;
        }

        public void ClearBonus()
        {
            BonusAdded = false;
            EndOfLevelBonus = false;
            BonusPoints = 0;
            TrapsHit = 0;
        }
    }

}