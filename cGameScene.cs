using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace me_and_me
{
    //door size : 67 x 61
    public class cGameScene :object2D
    {
        public struct levelInfo
        {
            public int start_x;
            public int start_y;
            public int door_x;
            public int door_y;
            public bool needsKey;
        }
        const double gravity = -9.81/13;
        double velocityY;
        double jumpTime;
        double dropTime;
        int d_loop;
        int stage;
        int deathCount;
        bool needsKey = false;
        string level_s = "";
        public cMap Map;
        public cPlayer Player;
        KeyboardState previousKeyboard;
        SpriteFont font;
        String debug = "";
        levelInfo[] levels;
        Rectangle dRec;

        Texture2D[] doors = new Texture2D[2];
        Texture2D nDoor;
        Texture2D key;
        Texture2D[] bgs = new Texture2D[10];
        int nonBlock = 0;
        public cGameScene(Game1 game)
            :base(game)
        {
            Initialize();
        }
        public override void Initialize()
        {
            d_loop = 0;
            levels = new levelInfo[5];

            levels[1].start_x = 0;
            levels[1].start_y = 300;


            this.stage = 0;
            Map = new cMap(this.Game);
            Player = new cPlayer(this.Game);
            this.previousKeyboard = Keyboard.GetState();
            this.font = Game.Content.Load<SpriteFont>("Segoe16");
            doors[0] = Game.Content.Load<Texture2D>("door1");
            doors[1] = Game.Content.Load<Texture2D>("door2");
            bgs[0] = Game.Content.Load<Texture2D>("bg");
            bgs[1] = Game.Content.Load<Texture2D>("bg2");
            bgs[2] = Game.Content.Load<Texture2D>("bg3");
            bgs[3] = Game.Content.Load<Texture2D>("bg4");
            key = Game.Content.Load<Texture2D>("key");
            nDoor = doors[0];

            #region Levels
            levels[2].start_x = 0;
            levels[2].start_y = 0;
            levels[2].door_x = 0;
            levels[2].door_y = 0;
            levels[2].needsKey = true;

            levels[1].start_x = 10;
            levels[1].start_y = 850;
            levels[1].door_x = 0;
            levels[1].door_y = 0;
            levels[1].needsKey = false;

            levels[0].start_x = 10;
            levels[0].start_y = 400;
            levels[0].door_x = 1140 - 67;
            levels[0].door_y = 200 - 61;
            levels[0].needsKey = false;
            #endregion

            init_Stage(this.stage);
        }
        public override void Update(GameTime gameTime)
        {
            nonBlock = 0;
            level_s = "Stage " + stage.ToString() + "\nDeath " + deathCount.ToString();
            base.Draw(gameTime);
            for (int i = 0; i < 38; i++)
            {
                try
                {
                    if (Map.finalData[((Player.y + 54) * 1280) + (Player.x + i)] == block_type.BLOCK_AIR)
                    {
                        nonBlock++;
                    }
                }
                catch
                {
                    restart(true);
                }
            }
            //non-ground
            if (nonBlock > 30)
            {
                Player.cannotJump = true;
                if (!Player.Jumping)
                {
                    if (dropTime == 0)
                    {
                        dropTime = gameTime.ElapsedGameTime.TotalMilliseconds * 1000000;
                        velocityY = 0.0f;
                    }
                    double currentTime = gameTime.ElapsedGameTime.TotalMilliseconds * 1000000 + 1f;
                    int jumpSpan = (int)(velocityY * (currentTime - dropTime));
                    Player.y -= jumpSpan;
                    velocityY += gravity * (float)((currentTime - dropTime));
                    if (velocityY >= 60.0f) velocityY = 60.0f;
                    else if (velocityY <= -20.0f) velocityY = -20.0f;
                }
            }
            else
            {
                Player.cannotJump = false;
                Player.jumpedHeight = 0;
                velocityY = 12.0;
                Player.Jumping = false;
                jumpTime = 0;
                dropTime = 0;
            }
            #region GamePad
            GamePadState state = GamePad.GetState(PlayerIndex.One);
            if (state.ThumbSticks.Left.X != 0.0f)
            {
                if (state.ThumbSticks.Left.X < 0.0f)
                {
                    Player.pS = cPlayer.pState.WALK_LEFT;
                    Player.lS = cPlayer.lState.LOOK_LEFT;
                }
                else if (state.ThumbSticks.Left.X > 0.0f)
                {
                    Player.pS = cPlayer.pState.WALK_RIGHT;
                    Player.lS = cPlayer.lState.LOOK_RIGHT;
                }
                if (Player.x > 0)
                    Player.x += (int)((state.ThumbSticks.Left.X) * 6);
            }
            if (state.Buttons.B == ButtonState.Pressed && !Player.cannotJump)
            {
                Player.cannotJump = true;
                Player.Jumping = true;
                jumpTime = gameTime.ElapsedGameTime.TotalMilliseconds * 1000000;
            }
            if (state.DPad.Left == ButtonState.Pressed)
            {
                Player.pS = cPlayer.pState.WALK_LEFT;
                Player.lS = cPlayer.lState.LOOK_LEFT;
                if (Player.x > 0)
                    Player.x -= 7;
            }
            else if (state.DPad.Right == ButtonState.Pressed)
            {
                Player.pS = cPlayer.pState.WALK_RIGHT;
                Player.lS = cPlayer.lState.LOOK_RIGHT;
                if (Player.x + 40 < 1280)
                    Player.x += 7;
            }
            else
            {
                Player.pS = cPlayer.pState.STAND;
            }
            #endregion

            #region Collision_down
            

            if (Player.Jumping)
            {
                double currentTime = gameTime.ElapsedGameTime.TotalMilliseconds * 1000000 + 1f;
                int jumpSpan = (int)(velocityY * (currentTime - jumpTime));
                debug = (jumpTime-currentTime).ToString();
                Player.y -= jumpSpan;
                velocityY += gravity * (float)((currentTime - jumpTime));
                if (velocityY >= 60.0f) velocityY = 60.0f;
                else if (velocityY <= -30.0f) velocityY = -30.0f;
                Player.jumpedHeight += Math.Abs(jumpSpan);
            }

            #endregion
            
            #region Stage
            d_loop++;
            if (d_loop == 15) d_loop = 0;
            if (d_loop < 8) nDoor = doors[0];
            else nDoor = doors[1];

            if (dRec.Intersects(Player.pRec))
            {
                if (levels[stage].needsKey)
                {
                    if (Player.hasKey)
                    {
                        stage++;
                        restart(false);
                        init_Stage(stage);
                        needsKey = false;
                    }
                    else
                    {
                        needsKey = true;
                    }
                }
                else
                {
                    stage++;
                    restart(false);
                    init_Stage(stage);
                }
            }
            #endregion
            Map.Update(gameTime);
            Player.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Map.Draw(gameTime);
            Player.Draw(gameTime);

            this.SharedSpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null);
            this.SharedSpriteBatch.Draw(nDoor, new Rectangle(levels[stage].door_x, levels[stage].door_y, 61, 67), Color.White);
            this.SharedSpriteBatch.DrawString(font, level_s, new Vector2(1100, 0), Color.White);
            this.SharedSpriteBatch.DrawString(font, debug, new Vector2(0, 0), Color.White);
            if (levels[stage].needsKey && !Player.hasKey)
            {
                this.SharedSpriteBatch.Draw(key, new Rectangle(Player.x + 20, Player.y - 20, 20, 20), Color.White);
                this.SharedSpriteBatch.Draw(key, new Rectangle(0, 0, 0, 0), Color.White);
            }
            if (this.stage == 0)
            {
                this.SharedSpriteBatch.DrawString(font, "<- -> to MOVE\nB to JUMP", new Vector2(900, 0), Color.White);

            }
            this.SharedSpriteBatch.End();
        }
        public void restart(bool isDead)
        {
            Player.x = levels[stage].start_x;
            Player.y = levels[stage].start_y;
            if(isDead) deathCount++;
            Player.hasKey = false;
        }
        public void init_Stage(int idx)
        {
            cMap.Block t = new cMap.Block();
            if (idx == 0)
            {
                Map.print_bg[1] = bgs[0];
                Map.print_bg[0] = bgs[1];
                Map.clear_buffer();
                Map.mapData = t.setBlock(Map.mapData, 0, 480, 100, 40);
                Map.Blocks.Add(t);
                Map.catchedCoords = t.setBlock(Map.catchedCoords, 130, 440, 100, 40);
                Map.Blocks.Add(t);
                Map.mapData = t.setBlock(Map.mapData, 260, 400, 100, 40);
                Map.Blocks.Add(t);
                Map.catchedCoords = t.setBlock(Map.catchedCoords, 390, 360, 100, 40);
                Map.Blocks.Add(t);
                Map.mapData = t.setBlock(Map.mapData, 520, 320, 100, 40);
                Map.Blocks.Add(t);
                Map.catchedCoords = t.setBlock(Map.catchedCoords, 650, 280, 100, 40);
                Map.Blocks.Add(t);
                Map.catchedCoords = t.setBlock(Map.catchedCoords, 650+130, 320-80, 100, 40);
                Map.Blocks.Add(t);
                Map.catchedCoords = t.setBlock(Map.catchedCoords, 910, 200, 100, 40);
                Map.Blocks.Add(t);
                Map.mapData = t.setBlock(Map.mapData, 1040, 200, 100, 40);
                Map.Blocks.Add(t);


                dRec = new Rectangle(levels[stage].door_x, levels[stage].door_y, 61, 67);
            }
            else if (idx == 1)
            {
                Map.print_bg[1] = bgs[2];
                Map.print_bg[0] = bgs[3];
                Map.clear_buffer();
                Map.mapData = t.setBlock(Map.mapData, 0, 900, 1280, 50,block_type.BLOCK_BORDER);
                Map.Blocks.Add(t);
                Map.catchedCoords = t.setBlock(Map.catchedCoords, 200, 750, 900, 40);
                Map.Blocks.Add(t);
                Map.catchedCoords = t.setBlock(Map.catchedCoords, 200, 600, 900, 40);
                Map.Blocks.Add(t);
                Map.catchedCoords = t.setBlock(Map.catchedCoords, 200, 450, 900, 40);
                Map.Blocks.Add(t);
                Map.catchedCoords = t.setBlock(Map.catchedCoords, 200, 300, 900, 40);
                Map.Blocks.Add(t);
                Map.mapData = t.setBlock(Map.mapData, 1100, 825, 100, 40);
                Map.Blocks.Add(t);
                Map.catchedCoords = t.setBlock(Map.catchedCoords, 100, 675, 100, 40);
                Map.Blocks.Add(t);
                Map.catchedCoords = t.setBlock(Map.catchedCoords, 1100, 525, 100, 40);
                Map.Blocks.Add(t);
                Map.catchedCoords = t.setBlock(Map.catchedCoords, 100, 375, 100, 40);
                Map.Blocks.Add(t);
            }
        }

    }
}
