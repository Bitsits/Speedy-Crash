using System;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Media;

namespace BitSits_Framework
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields

        float score;

        // Meta-level game state.
        private const int MaxLevelIndex = 1;
        private int levelIndex = 0;
        private Level level;
        private bool wasContinuePressed;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();

            LoadNextLevel();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                level.Update(gameTime);

                if (level.IsLevelUp)
                    ScreenManager.AddScreen(new IntroScreen("gameOver"), null);
            }
        }


        private void LoadNextLevel()
        {
            if (levelIndex == MaxLevelIndex)
            {
                score = level.Score;

                LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());
                return;
            }

            score = 0;
            // Unloads the content for the current level before loading the next one.
            if (level != null) { score = level.Score; level.Dispose(); }

            // Load the level.            
            level = new Level(ScreenManager.GameContent, levelIndex, (int)score); ++levelIndex;
        }

        private void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null) throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            {
                level.HandleInput(input, ControllingPlayer);
            }
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.CornflowerBlue, 0, 0);

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            level.Draw(gameTime, spriteBatch);

            DrwaScore(gameTime, spriteBatch);

            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0) ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
        }

        private void DrwaScore(GameTime gameTime, SpriteBatch spriteBatch)
        {
            float fps = (1000.0f / (float)gameTime.ElapsedRealTime.TotalMilliseconds);
            //spriteBatch.DrawString(ScreenManager.GameContent.fontMenu,
              //  "fps : " + fps.ToString("00"), new Vector2(20, 20), Color.White);

            if (level.IsLevelUp) score = level.Score;

            score = Math.Min(score + (float)gameTime.ElapsedGameTime.TotalSeconds * 500, (float)level.Score);
            spriteBatch.DrawString(ScreenManager.GameContent.font,
                "Score " + score.ToString("000000000"), new Vector2(167, 560), Color.Gold, 0,
                Vector2.Zero, new Vector2(0.45f), SpriteEffects.None, 1);
        }


        #endregion
    }
}
