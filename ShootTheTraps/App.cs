using AgateLib;
using AgateLib.Display;
using AgateLib.Input;
using AgateLib.Mathematics.Geometry;
using AgateLib.Scenes;
using AgateLib.UserInterface;
using AgateLib.UserInterface.Content;
using AgateLib.UserInterface.Content.Commands;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ShootTheTraps
{
    public class App : Scene
    {
        // Game declarations
        //int mDelay = 10; // time in milliseconds to wait
        private int displayedScore = 0;
        private float gameOverTimeLeft_ms = 0;
        private float bonusTimeLeft_ms = 0;
        private float betweenLevelTextTimeLeft_ms = 0;

        [Obsolete("What is this for?")]
        private float levelTimeElapsed_ms = 0;
        private int displayedMultiplier = 1;
        private ShootTraps gameState;

        // graphics declaration
        private Font font;
        private Texture2D background;
        private Texture2D white;
        private Texture2D cursor;
        private FontProvider fonts;
        private readonly SpriteBatch spriteBatch;
        private readonly MouseEvents mouse;
        private readonly KeyboardEvents keyboard;
        private const float totalTimeForBetweenLevelText = 10000;

        public App(GraphicsDevice graphics, GameWindow window, IContentProvider content)
        {
            this.graphics = graphics;
            this.content = content;

            background = content.Load<Texture2D>("background");
            white = content.Load<Texture2D>("white");
            cursor = content.Load<Texture2D>("cross");

            fonts = new FontProvider();
            font = Font.Load(content, "AgateLib/AgateSans");
            font.Size = 14;

            fonts.Add("AgateSans", font);

            spriteBatch = new SpriteBatch(graphics);

            mouse = new MouseEvents(graphics, window);
            keyboard = new KeyboardEvents();

            mouse.MouseDown += Mouse_MouseDown;
            mouse.MouseMove += Mouse_MouseMove;
            keyboard.KeyDown += Keyboard_KeyDown;
        }

        protected override void OnSceneStart()
        {
            NewGame();
        }

        #region --- Introduction ---

        private bool showingIntro = true;
        private bool mShowHelpText = true;
        private readonly string mIntroduction = @"{scale 2}Shoot the Traps{reset}

Your mission: Destroy as many filthy traps as you can.
These are traps: {draw trap}
They are ugly.

You have a cannon, which shoots where you point it.
{color yellow}Point and click to shoot.{color white} The traps have no chance to survive 
against your mighty cannon.

{color yellow}Right-click to release the traps.{reset}

Each trap you destroy will net you 50 points.
The white ones are especially hideous, worthy of 100 points.
If you destroy multiple traps at once you receive bonus points!

{color yellow}You must score a certain number of points to pass each level.{reset}

Good luck. You'll need it.

Click to start.";
        private string[] mIntroLines;
        private readonly GraphicsDevice graphics;
        private readonly GameWindow window;
        private IContentProvider content;
        private Vector2 mousePos;

        private void DrawIntro(SpriteBatch spriteBatch)
        {
            if (mIntroLines == null)
            {
                mIntroLines = mIntroduction.Split('\n');
            }

            ContentLayoutEngine layoutEngine = new ContentLayoutEngine(fonts);
            layoutEngine.AddCommand("draw", new DrawTextureCommand().Add("trap", Trap.Image));

            var content = layoutEngine.LayoutContent(mIntroduction);

            // center introduction text
            var textPt = new Vector2((Coordinates.Width - content.Size.Width) / 2, 20);

            var boxArea = new Rectangle(
                (int)textPt.X - 10,
                (int)textPt.Y - 10,
                content.Size.Width + 20,
                (mIntroLines.Length + 1) * font.FontHeight + 20);

            FillRect(spriteBatch, boxArea, new Color(Color.Black, 196));

            content.Draw(textPt, spriteBatch);

            //for (int i = 0; i < mIntroLines.Length; i++)
            //{
            //    font.DrawText(textPt.X, textPt.Y, mIntroLines[i],
            //        Trap.Image,
            //        LayoutCacheAlterFont.Scale(2, 2),
            //        LayoutCacheAlterFont.Scale(1, 1),
            //        LayoutCacheAlterFont.Color(Color.White),
            //        LayoutCacheAlterFont.Color(Color.Yellow));

            //    textPt.Y += font.FontHeight;
            //}
        }

        private void FillRect(SpriteBatch spriteBatch, Rectangle boxArea, Color color)
        {
            spriteBatch.Draw(white, boxArea, color);
        }

        #endregion

        private void Keyboard_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Keys.Escape)
            {
                IsFinished = true;
            }
            if (e.Key == Keys.OemPlus)
            {
                gameState.SkipToNextLevel();
            }
        }

        protected override void OnUpdateInput(IInputState input)
        {
            keyboard.Update(input.GameTime);
            mouse.Update(input.GameTime);
        }
        protected override void OnUpdate(GameTime time)
        {
            base.OnUpdate(time);

            if (showingIntro)
                return;

            float ms = (float)time.ElapsedGameTime.TotalMilliseconds;

            if (gameState.GameOver)
                gameOverTimeLeft_ms -= ms;

            bonusTimeLeft_ms -= ms;

            gameState.Update(time);
            UpdateDisplay(time);
        }

        protected override void DrawScene(GameTime time)
        {
            spriteBatch.Begin();

            spriteBatch.Draw(background, new Rectangle(0, 0, graphics.PresentationParameters.BackBufferWidth,
                                                        graphics.PresentationParameters.BackBufferHeight), Color.White);

            if (showingIntro)
            {
                DrawIntro(spriteBatch);
            }
            else
            {
                gameState.Draw(spriteBatch);
                DrawInformation(spriteBatch);
            }

            DrawMousePointer(spriteBatch);

            spriteBatch.End();
        }

        private void Mouse_MouseMove(object sender, MouseEventArgs e)
        {
            mousePos = e.MousePosition.ToVector2();

            gameState.MouseMove(e.MousePosition.X, e.MousePosition.Y);
        }

        private void Mouse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (showingIntro)
            {
                showingIntro = false;
                NewGame();
                return;
            }

            if (ContinueYet)
            {
                NewGame();
                return;
            }

            if (betweenLevelTextTimeLeft_ms > 0)
                return;

            if (bonusTimeLeft_ms > 0)
                return;

            // left click
            if (e.Button == MouseButton.Left)
            {
                gameState.FireBullet(e.MousePosition.X, e.MousePosition.Y);
            }
            // right click
            if (e.Button == MouseButton.Right)
            {
                // make sure that the score is done updating.
                if (gameState.Score == displayedScore)
                    gameState.FireTraps();

                if (gameState.FiredTraps)
                    mShowHelpText = false;

            }
        }

        private void NewGame()
        {
            gameState = new ShootTraps(graphics.PresentationParameters.BackBufferWidth,
                                   graphics.PresentationParameters.BackBufferHeight - 50,
                                   content);

            displayedScore = 0;
            gameOverTimeLeft_ms = 0;
            displayedMultiplier = 1;
            levelTimeElapsed_ms = 0;
            betweenLevelTextTimeLeft_ms = totalTimeForBetweenLevelText;
        }

        private bool ContinueYet
        {
            get
            {
                if (gameState.GameOver == false)
                    return false;

                return gameOverTimeLeft_ms <= 0;
            }
        }

        public Rectangle Coordinates => new Rectangle(
            0, 0, graphics.PresentationParameters.BackBufferWidth, graphics.PresentationParameters.BackBufferHeight);

        public void UpdateDisplay(GameTime gameTime)
        {
            int displayIncrement = 2;
            int scoreDiff = gameState.Score - displayedScore;

            if (scoreDiff > 1000)
                displayIncrement = 100;
            else if (scoreDiff > 200)
                displayIncrement = 10;

            levelTimeElapsed_ms += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            betweenLevelTextTimeLeft_ms -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (gameState.CanAdvanceLevel && (gameState.Score == displayedScore || gameState.EndOfLevelBonus))
            {
                gameState.NextLevel();
                betweenLevelTextTimeLeft_ms = totalTimeForBetweenLevelText;
            }

            if (Math.Abs(scoreDiff) < displayIncrement)
                displayedScore = gameState.Score;

            if (gameState.Score > displayedScore) displayedScore += displayIncrement;
            if (gameState.Score < displayedScore) displayedScore -= displayIncrement;

            if (gameState.Score != displayedScore && gameState.BonusAdded && gameState.TrapsHit > 1)
            {
                bonusTimeLeft_ms = 2000;
            }

            if (bonusTimeLeft_ms <= 0 && gameState.BonusAdded && betweenLevelTextTimeLeft_ms <= 0)
            {
                gameState.ClearBonus();
            }
        }

        private void DrawInformation(SpriteBatch spriteBatch)
        {
            int fontHeight = font.FontHeight;

            DrawBottomStatus();

            if (gameState.Score != displayedScore && gameState.BonusAdded && gameState.TrapsHit > 1)
            {
                bonusTimeLeft_ms = 2000;
            }

            if (bonusTimeLeft_ms > 0)
            {
                DrawBonusText();
                DrawMultiplierText();
            }
            else if (gameState.GameOver && displayedScore == gameState.Score)
            {
                DrawGameOverText();
            }
            else if (betweenLevelTextTimeLeft_ms > 0)
            {
                DrawBetweenLevelText();
            }
        }

        private void DrawMousePointer(SpriteBatch spriteBatch)
        {
            Vector2 center = new Vector2(cursor.Width, cursor.Height) / 2;

            spriteBatch.Draw(cursor, mousePos - center, Color.White);
        }

        private void DrawMultiplierText()
        {
            if (displayedMultiplier == gameState.ScoreMultiplier)
                return;

            int textY = 160 + font.FontHeight * 4;

            CenterText(font, textY, "Multiplier: " + gameState.ScoreMultiplier.ToString(), Color.White, Color.Black);

            if (bonusTimeLeft_ms <= 0)
                displayedMultiplier = gameState.ScoreMultiplier;
        }

        private void DrawBetweenLevelText()
        {
            float waitTime = totalTimeForBetweenLevelText / 2;

            if (gameState.Level > 1 && betweenLevelTextTimeLeft_ms > waitTime)
            {
                DrawLevelEndText();
            }
            else
            {
                DrawLevelBeginText();
            }
        }
        private void DrawLevelBeginText()
        {
            font.Size = 48;

            int textHeight = font.FontHeight * 3;
            int textY = 160;

            FillRect(spriteBatch, 
                new Rectangle(0, textY, Coordinates.Width, textHeight),
                new Color(Color.Black, 128));

            // back the border color for the text oscillate between red and black
            int r = (int)(255 * Math.Abs(Math.Sin(12 * levelTimeElapsed_ms / totalTimeForBetweenLevelText)));
            var borderColor = new Color(r, 0, 0);

            CenterText(font, textY, "Level " + gameState.Level, Color.White, borderColor);

            font.Size = 24;
            CenterText(font, textY + font.FontHeight * 2, gameState.LevelMessage, Color.White, borderColor);

            //if (levelTimeElapsed_ms > totalTimeForBetweenLevelText)
            //    mLevelTime = 0;
        }

        private void DrawLevelEndText()
        {
            font.Size = 48;

            int textHeight = font.FontHeight * 3;
            int textY = 160;

            FillRect(spriteBatch,
                new Rectangle(0, textY, Coordinates.Width, textHeight),
                new Color(Color.Black, 128));

            // back the border color for the text oscillate between red and black
            int b = (int)(255 * Math.Abs(Math.Sin(12 * betweenLevelTextTimeLeft_ms / totalTimeForBetweenLevelText)));
            var borderColor = new Color(0, 0, b);

            CenterText(font, textY, $"End of Level {gameState.Level - 1}", Color.White, borderColor);
            
            font.Size = 24;
            CenterText(font, textY + font.FontHeight * 2,
                "BONUS for remaining pulls: " + gameState.BonusPoints.ToString(),
                Color.White, borderColor);

            //if (Timing.TotalMilliseconds - mLevelTime > totalTimeForBetweenLevelText)
            //    mLevelTime = 0;
        }

        private void DrawGameOverText()
        {
            int fontHeight = font.FontHeight;

            if (gameOverTimeLeft_ms == 0)
                gameOverTimeLeft_ms = 5000;

            double deltaTime = Math.Min(gameOverTimeLeft_ms / 3000, 1);
            double extraScaleFactor = -3 * (1.1 - Math.Pow(deltaTime, 2));
            double scale = 3 + extraScaleFactor;

            font.Size = (int)(14 * scale);

            CenterText(font, (int)(200 + fontHeight - scale * fontHeight / 2.0),
                "GAME OVER", Color.White, Color.Black);

            font.Size = 21;

            if (ContinueYet)
                CenterText(font, 240 + font.FontHeight, "Click to restart", Color.White, Color.Black);
        }

        private void DrawBonusText()
        {
            int textY = 160;
            font.Size = 28;

            Color bonusColor = Color.White;

            if (gameState.LevelTimeElapsed % 500 < 250)
                bonusColor = Color.Yellow;

            FillRect(spriteBatch,
                new Rectangle(0, textY, Coordinates.Width, font.FontHeight * 4),
                new Color(Color.Black, 128));

            CenterText(font, textY, "HIT " + gameState.TrapsHit + " TRAPS", Color.White, Color.Black);
            textY += font.FontHeight;

            CenterText(font, textY, "BONUS: " + gameState.BonusPoints, bonusColor, Color.Black);
        }

        private int DrawBottomStatus()
        {
            font.Color = Color.White;
            font.Size = 14;

            int fontHeight = font.FontHeight;
            int textBoxHeight = font.FontHeight * 2;

            Point textStart = new Point(10, Coordinates.Height - textBoxHeight);

            if (mShowHelpText /*&& mLevelTime == 0*/)
            {
                font.DrawText(spriteBatch, textStart.X, textStart.Y - fontHeight, "Left-click to shoot... Right-click to release traps.");
            }

            FillRect(spriteBatch, new Rectangle(0, textStart.Y, Coordinates.Width, textBoxHeight), Color.Black);

            textStart.X = Coordinates.Width / 4;

            font.DrawText(spriteBatch, textStart.X, textStart.Y, "Score: " + displayedScore);
            font.DrawText(spriteBatch, textStart.X, textStart.Y + fontHeight, "Need: " +
                Math.Max(0, gameState.LevelRequirement - gameState.PointsThisLevel + (gameState.Score - displayedScore)));

            if (gameState.ScoreMultiplier != 1)
            {
                font.Size = 24;
                CenterText(font, textStart.Y, "x " + gameState.ScoreMultiplier.ToString(), Color.White);
            }

            font.Size = 14;
            textStart.X = Coordinates.Width * 3 / 4;

            font.DrawText(spriteBatch, textStart.X, textStart.Y, "Level: " + gameState.Level);
            font.DrawText(spriteBatch, textStart.X, textStart.Y + fontHeight, "Pulls Left: " + gameState.PullsLeft);
            return fontHeight;
        }

        private void CenterText(IFont font, int y, string text, Color color)
        {
            Size size = font.MeasureString(text);

            int x = (Coordinates.Width - size.Width) / 2;

            font.Color = color;
            font.DrawText(spriteBatch, x, y, text);
        }
        private void CenterText(IFont font, int y, string text, Color color, Color borderColor)
        {
            Size size = font.MeasureString(text);

            int x = (Coordinates.Width - size.Width) / 2;

            DrawBorderedText(font, x, y, text, color, borderColor);
        }

        private void DrawBorderedText(IFont font, int x, int y, string text, Color color, Color borderColor)
        {
            font.Color = borderColor;
            font.DrawText(spriteBatch, x + 1, y, text);
            font.DrawText(spriteBatch, x, y + 1, text);
            font.DrawText(spriteBatch, x - 1, y, text);
            font.DrawText(spriteBatch, x, y - 1, text);

            font.Color = color;
            font.DrawText(spriteBatch, x, y, text);
        }
    }

}
