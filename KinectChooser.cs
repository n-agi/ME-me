namespace me_and_me
{
    //인터페이스 Load
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Kinect;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    //Kinect를 선택하고 그것을 연동하는 과정을 Class로 설계
    //DrawableGameComponent를 상속하는 이유는 Kinect가 연동되었다는
    //문구 등을 출력하기 위함이다.
    public class KinectChooser:DrawableGameComponent
    {
        //각 상태마다 문구를 저장할 때 사용할 변수(KinectStatus, string 으로 map됨)
        private readonly Dictionary<KinectStatus, string> statusMap = new Dictionary<KinectStatus, string>();
        //Color Image Format, Depth Image Format
        public readonly ColorImageFormat colorImageFormat;
        public readonly DepthImageFormat depthImageFormat;
        //Kinect 사진 저장용
        private Texture2D chooserBackground;
        private SpriteBatch spriteBatch;
        //출력할 Batch
        private SpriteFont font;
        //출력할 Font

        //생성자(생성자가 DrawableGameComponent를 상속했기때문에 조금 구성이 다르다.)
        public KinectChooser(Game game, ColorImageFormat colorFormat, DepthImageFormat depthFormat):base(game)
            //base(game)을 선언하여 상속된 상위의 생성자를 호출
        {
            //매개변수에서 받아준 값을 대입
            this.colorImageFormat = colorFormat;
            this.depthImageFormat = depthFormat;
            //Event Handler에 Status가 바뀐 경우 값을 넣어주는 핸들러를 넣어줌
            KinectSensor.KinectSensors.StatusChanged += this.KinectSensors_StatusChanged;
            
            //Sensor가 있는지 없는지 확인하는 함수를 실행
            this.DiscoverSensor();


            //위에서 Map에 넣어줄 값들
            //MSDN에서 복사 붙여넣기함..
            this.statusMap.Add(KinectStatus.Connected, string.Empty);
            this.statusMap.Add(KinectStatus.DeviceNotGenuine, "Device Not Genuine");
            this.statusMap.Add(KinectStatus.DeviceNotSupported, "Device Not Supported");
            this.statusMap.Add(KinectStatus.Disconnected, "Required");
            this.statusMap.Add(KinectStatus.Error, "Error");
            this.statusMap.Add(KinectStatus.Initializing, "Initializing");
            this.statusMap.Add(KinectStatus.InsufficientBandwidth, "Insufficient Bandwidth");
            this.statusMap.Add(KinectStatus.NotPowered, "Not Powered");
            this.statusMap.Add(KinectStatus.NotReady, "Not Ready");
        }
        //Sensor와 최근 상태는 다른 클래스에서 입/출력 할 수 있도록 설정
        public KinectSensor Sensor { get; private set; }
        public KinectStatus LastStatus { get; private set; }

        //XNA 초기화
        //별로 해줄 것이 없음..
        public override void Initialize()
        {
            base.Initialize();
            this.spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        //문구를 출력하거나 할 때 사용하는 것
        //이는 Debug 용으로 만들었기 때문에 호출되지 않음
        public override void Draw(GameTime gameTime)
        {
            //spriteBatch, Background가 load되지 않은 경우 재 로드
            if (this.spriteBatch == null)
            {
                this.Initialize();
            }
            if (this.chooserBackground == null)
            {
                this.LoadContent();
            }
            //Sensor가 없거나 최근 상태가 연결된 상태가 아닌 경우
            if (this.Sensor == null || this.LastStatus != KinectStatus.Connected)
            {
                //경고 사진을 출력
                this.spriteBatch.Begin();
                this.spriteBatch.Draw(
                    this.chooserBackground,
                    new Vector2(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2),
                    null,
                    Color.White,
                    0,
                    new Vector2(this.chooserBackground.Width / 2, this.chooserBackground.Height / 2),
                    1,
                    SpriteEffects.None,
                    0);
                //경고 문구를 출력
                string txt = "Required";
                if(this.Sensor != null){
                    txt = this.statusMap[this.LastStatus];
                }
                Vector2 size = this.font.MeasureString(txt);
                this.spriteBatch.DrawString(
                    this.font,
                    txt,
                    new Vector2((Game.GraphicsDevice.Viewport.Width - size.X) / 2, (Game.GraphicsDevice.Viewport.Height / 2) + size.Y),
                    Color.White);
                this.spriteBatch.End();

            }
            base.Draw(gameTime);
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            //사진, 폰트 가져옴
            this.chooserBackground = Game.Content.Load<Texture2D>("ChooserBackground");
            this.font = Game.Content.Load<SpriteFont>("Segoe16");
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
            //UnloadContent가 실행되는 경우 Sensor가 더 이상 메모리에 있을
            //이유가 없기에 Stop 해줍시다
            if (this.Sensor != null)
            {
                this.Sensor.Stop();
            }
        }
        
        private void DiscoverSensor()
        {
            //Sensor 안에 있는 값을 Kinect센서들의 가장 첫번째나 기본으로 가져옵니다.
            //센서가 하나 밖에 없기 때문에..
            this.Sensor = KinectSensor.KinectSensors.FirstOrDefault();

            //가져오는데 성공했을 경우
            if (this.Sensor != null)
            {
                //최근 상태는 센서의 상태로 저장해주고
                this.LastStatus = this.Sensor.Status;

                if (this.LastStatus == KinectStatus.Connected)
                {
                    //만약 이게 연결됨이 확정이면
                    try
                    {
                        //SkeletonStream, ColorStream, DepthStream을 사용하도록
                        //허가
                        this.Sensor.SkeletonStream.Enable();
                        this.Sensor.ColorStream.Enable(this.colorImageFormat);
                        this.Sensor.DepthStream.Enable(this.depthImageFormat);

                        try
                        {
                            //센서 실행
                            this.Sensor.Start();
                        }
                        catch (IOException)
                        {
                            this.Sensor = null;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        //실패하면(반드시 Sensor 연결이 실패하면 InvalidOperationException
                        //예외로 넘어옴
                        this.Sensor = null;
                    }
                }
            }
            else
            {
                //연결 못하면 못했다고..
                this.LastStatus = KinectStatus.Disconnected;
            }
        }
        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            //Connected가 아니면 Sensor 연결을 일단 해제해보고
            if (e.Status != KinectStatus.Connected)
            {
                e.Sensor.Stop();
            }
            //재연결 시도
            this.LastStatus = e.Status;
            this.DiscoverSensor();
        }
    }
}
