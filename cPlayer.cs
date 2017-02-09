using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace me_and_me
{
    public class cPlayer :object2D
    {
        private String debug = "";
        private const float gravity = 0.5f;
        private int x=0;
        private int y=312;
        private int jumpedHeight = 0;
        private bool cannotJump = false;
        private bool Jumping = false;
        Texture2D playerModel;
        KeyboardState previousKeyboard;
        private SpriteFont font;
        public cPlayer(Game game)
            :base(game)
        {
            Initialize();
        }
        public override void Initialize()
        {
            this.font = Game.Content.Load<SpriteFont>("Segoe16");
            base.Initialize();
            this.x = 0;
            this.y = 500;
            playerModel = Game.Content.Load<Texture2D>("player");
            this.previousKeyboard = Keyboard.GetState();
        }
        public override void Update(GameTime gameTime)
        {
            if (KeyCheck(previousKeyboard, Keyboard.GetState(), Keys.Right))
            {
                x += 5;
            }
            if (KeyCheck(previousKeyboard, Keyboard.GetState(), Keys.Left))
            {
                x -= 5;
            }
            if (KeyCheck(previousKeyboard, Keyboard.GetState(), Keys.Up) && !cannotJump)
            {
                Jumping = true;
            }
            if (Jumping)
            {
                const int maxJumpHeight = 80;
                if (jumpedHeight > maxJumpHeight)
                {
                    this.y += 6;
                }
                else
                {
                    this.y -= 6;
                }
                jumpedHeight += 6;
                if (jumpedHeight >= 166)
                {
                    cannotJump = false;
                    jumpedHeight = 0;
                    Jumping = false;
                }
                else
                {
                    cannotJump = true;
                }

            }
            debug = x.ToString() + " " + y.ToString();
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            this.SharedSpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null);
            this.SharedSpriteBatch.Draw(playerModel, new Rectangle(x, y, 40, 40), Color.White);
            this.SharedSpriteBatch.DrawString(
                     font,
                     debug,
                     new Vector2(0,0),
                     Color.Black);
            this.SharedSpriteBatch.End();
            base.Draw(gameTime);
        }
        private bool KeyCheck(KeyboardState previous, KeyboardState now, Keys e)
        {
            if (previous.IsKeyUp(e) && now.IsKeyDown(e)) return true;
            else return false;
        }
    }
}
