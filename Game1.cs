//C#에서 반드시 필요한 Refernce
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
//새롭게 필요한 Reference, Game 클래스에서는 가능한 많은 Reference를 참조하는 것이 좋다.
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Kinect;

namespace me_and_me
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region privates
        /* class를 구성하는 내부 private 변수들이다. */
        int tloop = 0;
        int lloop = 0;
        int bloop = 0;
        float xx = 0;
        float endblend = 1.0f;
        Texture2D titleScreen;
        Texture2D titleLogo;
        Texture2D startButton;
        bool start_fade = false;
        private enum gameState { title, running, over }
        gameState mCurrentState = gameState.title;
        //gameState에 Scene의 종류를 선언한다.
        //currentState에는 현재 진행되고 있는 Scene의 상태를 저장한다.
        //가장 먼저 실행되는 Scene은 Title이다.
        private const int Width = 1280;
        private const int Height = 960;
        //Width와 Height에는 프로그램의 윈도우 사이즈를 결정한다.
        private readonly GraphicsDeviceManager graphics;
        //XNA에서 내부적으로 사용하는 Graphic 관리자를 선언한다. 이는
        //내부에서 호출하고 값을 바꾸고 Game 인스턴스로 넘어온다.

        public readonly KinectChooser chooser;
        //Kinect를 선택하고 현재 상태를 추적하는 것을 Class화 한 것이다.
        //이는 다른 Class에서도 사용할 수 있어야 하기 때문에 public이다.
        private readonly Rectangle viewPortRectangle;
        
        //Map과 Player의 Prototype
        private cGameScene gScene;
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        //font와 SpriteBatch를 저장

        KeyboardState previousKeyboard;
        bool disconnected = false;
        //KeyboardState class는 눌리고 있는가에 대해서만 판별하기에 이전 상태를
        //저장해서 비교해야 한다.
        #endregion
        //Game1 생성자
        public Game1()
        {
            this.Window.Title = "me & ME";
            //ㅇㅇ..
            graphics = new GraphicsDeviceManager(this);
            //graphics 변수에 지금 사용된 GraphicsDeviceManager를 넣어준다.
            //GraphicsDeviceManager 내부에는 시스템에서 사용하는 하드웨어의 값을
            //할당해준다.
            this.graphics.PreferredBackBufferWidth = Width;
            this.graphics.PreferredBackBufferHeight = Height;
            this.graphics.PreparingDeviceSettings += this.GraphicsDevicePreparingDeviceSettings;
            this.viewPortRectangle = new Rectangle(10, 80, Width - 20, ((Width - 2) / 4) * 3);
            this.graphics.SynchronizeWithVerticalRetrace = true;
            //Window의 사이즈를 결정한다. Width와 Height의 Size로 설정해주는 부분이다.
            //하지만 이와 같은 Size로 선택이 실패할 경우가 있기에 변수 명에도
            //보이듯이 Preferred이다.

            Content.RootDirectory = "Content";
            //Content가 저장되는 최상위 디렉토리 이름
            this.chooser = new KinectChooser(this, ColorImageFormat.RgbResolution640x480Fps30, DepthImageFormat.Resolution640x480Fps30);
            this.Services.AddService(typeof(KinectChooser), this.chooser);
            //KinectChooser를 메모리에 할당하고 Service로 등록해준다.
            //Service로 등록되는 순간 chooser 안에 있는 함수가 자동으로 필요할 때나
            //최적화된 순서로 실행해준다.

            this.previousKeyboard = Keyboard.GetState();
            //생성자 내부에 이전 키보드 입력 상태로 설정해준다.
        }

        protected override void Initialize()
        {

            if (chooser.LastStatus == KinectStatus.Disconnected && !disconnected)
            {
                disconnected = true;
                MessageBox.Show("Kinect is not connected.");
                Exit();
            }
            else
            {
                gScene = new cGameScene(this);
            }
            bloop = 0;
            base.Initialize();
            //Initialize 내부에 Map과 Player를 할당해준다.
        }

        protected override void LoadContent()
        {
            // SpriteBatch를 선언하고 이것을 Service에 등록해준다.
            // Service에 등록하여 최적화된 실행 순서를 보장하는 상태로 바꾼다.
            //그리고 font나 타이틀 화면을 Load해준다.
            this.spriteBatch = new SpriteBatch(GraphicsDevice);
            this.Services.AddService(typeof(SpriteBatch), this.spriteBatch);
            this.font = Content.Load<SpriteFont>("Segoe16");
            titleScreen = Content.Load<Texture2D>("title");
            titleLogo = Content.Load<Texture2D>("meme");
            startButton = Content.Load<Texture2D>("start_button");
            base.LoadContent();

        }

        protected override void UnloadContent()
        {
            //아직은 외부 리소스를 불러들이지 않고 있기에 필요하지 않다.
        }

        protected override void Update(GameTime gameTime)
        {
            if (tloop < 280)
            {
                tloop += (int)xx;
                xx += 0.15f;
            }
            else
            {
                if (lloop <= 100) lloop += 1;
                this.bloop++;
            }
            if (this.bloop > 60) bloop = 0;
            KeyboardState newState = Keyboard.GetState();
            GamePadState state = GamePad.GetState(PlayerIndex.One);
            //Update 함수가 실행될 때에 State를 불러들인 후 저장한ㄷ.
            switch (mCurrentState)
            {
                case gameState.title:
                    {
                        //타이틀에서 Enter가 입력된 경우
                        if (state.Buttons.Start == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                        {
                            start_fade = true;
                        }
                        break;
                    }
                case gameState.running:
                    {
                        //게임이 실행중인 경우 각 요소의 Update를 실행한다.
                        gScene.Update(gameTime);
                        break;
                    }
                default:
                    {
                        //없어요
                        break;
                    }
            }
            if (start_fade)
            {
                endblend -= 0.0125f;
                if (endblend <= 0.0f)
                    mCurrentState = gameState.running;
            }
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            switch (mCurrentState)
            {
                case gameState.title:
                    {
                        //title 화면을 출력한다.
                        this.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                        this.spriteBatch.Draw(titleScreen, new Rectangle(0, this.tloop - 300, 1280, 1264), Color.White * endblend);
                        this.spriteBatch.Draw(titleLogo, new Rectangle(348, 75, 555, 289), Color.White * ((float)(lloop) / 100) * endblend);
                        if(this.bloop > 30)
                            this.spriteBatch.Draw(startButton, new Rectangle(260, 511, 747, 81), Color.White * endblend);
                        this.spriteBatch.End();
                        break;
                    }
                case gameState.running:
                    {
                        //실행중인 경우 각 요소를 Draw한다.
                        gScene.Draw(gameTime);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            base.Draw(gameTime);
        }
        private void GraphicsDevicePreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            // backbuffer(render에 사용)할 때 데이터를 미리 보존해야 하는 경우 설정을 미리 해줘야하는데,
            // 이는 callback 형식이기 때문에 callback을 만들어주자
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }
    }
}
