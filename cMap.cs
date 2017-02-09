using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace me_and_me
{
    public enum block_type { BLOCK_AIR=0, BLOCK_BLOCK=1, BLOCK_BORDER=2, BLOCK_USER=3};

    public class cMap : object2D
    {
        public struct Block
        {
            public int x, y, xsize, ysize;
            public block_type[] setBlock(block_type[] mapData, int x, int y, int xsize, int ysize, block_type TYPE=block_type.BLOCK_BLOCK)
            {
                this.x = x;
                this.y = y;
                this.xsize = xsize;
                this.ysize = ysize;
                for (int i = y; i < ysize + y; i++)
                {
                    for (int j = x; j < xsize + x; j++)
                    {
                        mapData[(i * 1280) + j] = TYPE;
                    }
                }
                return mapData;
            }

        }
        SpriteFont font;
        String debug = "";
        const int mapWidth = 1280;
        const int mapHeight = 960;
        //Map에 존재하는 Block type
        //AIR -> 지나다님
        //BLOCK -> 못지나감
        //BORDER -> 테두리
        //USER -> 그외 블록(예외 처리용)
        //맵의 총 크기
        //Map에 표시되어야 할 block의 위치를 저장할 변수
        public block_type[] mapData = new block_type[mapHeight*mapWidth];
        public block_type[] catchedCoords = new block_type[mapHeight*mapWidth];
        public block_type[] finalData = new block_type[mapHeight*mapWidth];
        public List<Block> Blocks;

        //출력하는 Texture들
        public Texture2D[] print_bg = new Texture2D[2];
        Texture2D t;
        Texture2D newone;
        Texture2D block;
        Texture2D _100_40;
        Color[] b_100_40;
        Texture2D _900_40;
        Color[] s_900_40;
        //두 번 Render 할 필요 없을 때 사용할 변수
        //Prototype이니 안 넣어줘도 될 듯..
        private bool needToRedrawBackBuffer = true;

        //depth Pixel, color 좌표 값을 저장 할 변수
        private DepthImagePixel[] depthPixels;
        private ColorImagePoint[] colorCoordinates;
        //player만이 가지고 있는 depthStream의 Pixel
        private int[] playerPixelData;
        //x,y 연산 용
        private int depthHeight;
        private int depthWidth;

        public cMap(Game game)
            : base(game)
        {
            Initialize();
        }
        public override void Initialize()
        {
            block = new Texture2D(this.Game.GraphicsDevice, 1280, 960);
            Blocks = new List<Block>();
            b_100_40 = new Color[100 * 40];
            _100_40 = Game.Content.Load<Texture2D>("block100x30");
            _100_40.GetData(b_100_40);
            s_900_40 = new Color[900 * 40];
            _900_40 = Game.Content.Load<Texture2D>("stone900x40");
            _900_40.GetData(s_900_40);
            for (int i = 0; i < mapData.Length; i++)
            {
                mapData[i] = block_type.BLOCK_AIR;
                catchedCoords[i] = block_type.BLOCK_AIR;
            }
 	        base.Initialize();
            
            newone = new Texture2D(this.Game.GraphicsDevice, 640, 480);
            //위 소스는 x와 y가 극값을 갖고있거나
            //0인 경우 색을 칠해주는 역할입니다. 그 외는 투명으로 넣어줌

            //Kinect를 연동하는데 필요한 변수들의 메모리 공간 생성
            this.depthPixels = new DepthImagePixel[this.Chooser.Sensor.DepthStream.FramePixelDataLength];
            this.colorCoordinates = new ColorImagePoint[this.Chooser.Sensor.DepthStream.FramePixelDataLength];
            this.playerPixelData = new int[this.Chooser.Sensor.DepthStream.FramePixelDataLength];
            this.depthWidth = this.Chooser.Sensor.DepthStream.FrameWidth;
            this.depthHeight = this.Chooser.Sensor.DepthStream.FrameHeight;

            //Blocks

        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            //depthReceived가 true일 때만 작업해야 하니 만들어줍시다
            bool depthReceived = false;
            //부모의 Update를 한 번 실행 해주고
            Color[] blockColor = new Color[mapWidth * mapHeight];
            //DepthImageFrame 하나를 가져옵니다.
            //OpenNextFrame은 직접 만든 함수로써 인자 안에 넣어준 프레임 후의 프레임을 가져옵니다.
            //즉, 1을 대입하면 1 프레임 전 프레임을 가져오는 것입니다.
            try
            {
                using (DepthImageFrame frame = this.Chooser.Sensor.DepthStream.OpenNextFrame(0))
                {
                    if (frame != null)
                    {
                        //프레임을 가져 왔으면 depthPixels 안에다가 값을 복사해주고
                        //값을 가져왔다는 변수를 true로 해줍니다
                        frame.CopyDepthImagePixelDataTo(this.depthPixels);
                        depthReceived = true;
                    }
                }
                //만약 가져왔다면
                if (depthReceived)
                {
                    Array.Copy(mapData, finalData, mapData.Length);
                    //color, color2를 만들어 주고
                    Color[] color = new Color[newone.Width * newone.Height];
                    Color[] color2 = new Color[newone.Width * newone.Height];
                    //이 안에 bg, bg2의 색을 넣어줍시다
                    print_bg[0].GetData(color2);
                    print_bg[1].GetData(color);

                    Array.Clear(this.playerPixelData, 0, this.playerPixelData.Length);


                    // 그 후 640x960의 픽셀 중에서
                    for (int y = 0; y < depthHeight; ++y)
                    {
                        for (int x = 0; x < depthWidth; ++x)
                        {
                            //2차원 배열이므로 값을 만들어줘야함..
                            int depthIndex = x + (y * this.depthWidth);
                            //Pixel 하나를 넣어준 후
                            DepthImagePixel depthPixel = this.depthPixels[depthIndex];
                            int player = depthPixel.PlayerIndex;
                            //Pixel이 가지고 있는 Player의 Index를 구해줍니다.

                            //그게 만약 0이 아닌 값이라면 인식된 것입니다.
                            if (player > 0)
                            {
                                //color이라는 색을 저장하는 배열에 bg2의 색을 대입하고
                                color[(y * depthWidth) + x] = color2[(y * depthWidth) + x];
                                //catchedCoords을 그 부분만 true로 바꿔줍니다
                                int remappedX = (int)2 * x;
                                int remappedY = (int)2 * y;
                                if (catchedCoords[(remappedY * 1280) + remappedX] == block_type.BLOCK_BLOCK)
                                {
                                    finalData[(remappedY * 1280) + remappedX] = catchedCoords[(remappedY * 1280) + remappedX];
                                    finalData[((remappedY + 1) * 1280) + (remappedX + 1)] = catchedCoords[((remappedY + 1) * 1280) + (remappedX + 1)];
                                    finalData[((remappedY) * 1280) + (remappedX + 1)] = catchedCoords[((remappedY) * 1280) + (remappedX + 1)];
                                    finalData[((remappedY + 1) * 1280) + (remappedX)] = catchedCoords[((remappedY + 1) * 1280) + (remappedX)];
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                            }
                        }
                    }
                    //newone이 이미 할당되어 있는 상태여서 더 이상 SetData가 불가능 한 경우
                    //초기화를 한번 해주고 값을 넣어줍니다.
                    if (newone.GraphicsDevice.Textures[0] == newone)
                        newone.GraphicsDevice.Textures[0] = null;
                    newone.SetData(color);
                }

                foreach (Block b in Blocks)
                {
                    int x = 0;
                    int y = 0;
                    for (int i = b.y; i < b.ysize + b.y; i++)
                    {
                        x = 0;
                        for (int j = b.x; j < b.xsize + b.x; j++)
                        {
                            if (finalData[(i * mapWidth) + j] == block_type.BLOCK_BLOCK)
                            {
                                if (b.xsize == 100 && b.ysize == 40)
                                    blockColor[(i * mapWidth) + j] = b_100_40[(y * 100) + x];
                                else if (b.xsize == 900 && b.ysize == 40)
                                    blockColor[(i * mapWidth) + j] = s_900_40[(y * 900) + x];
                            }
                            x++;
                        }
                        y++;
                    }
                }


                if (block.GraphicsDevice.Textures[0] == block)
                    block.GraphicsDevice.Textures[0] = null;
                block.SetData(blockColor);
            }
            catch(InvalidOperationException)
            {
                
            }
            
        }
        public override void Draw(GameTime gameTime)
        {
            //그리고 새롭게 그려진 배경을 대입합니다.
            //연산의 속도를 올리기 위해 다소 블렌딩 등을 포기하고 그리는
            //Immediate를 사용합니다.
            this.SharedSpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null);
            this.SharedSpriteBatch.Draw(newone, new Rectangle(0, 0, 1280, 960), Color.White);
            this.SharedSpriteBatch.Draw(block, new Rectangle(0, 0, 1280, 960), Color.White);
            //this.SharedSpriteBatch.DrawString(font, debug, new Vector2(0,30), Color.Black);
            this.SharedSpriteBatch.End();

            base.Draw(gameTime);
        }
        public void clear_buffer()
        {
            this.Blocks.Clear();
            Array.Clear(mapData, 0, mapData.Length);
            Array.Clear(catchedCoords, 0, mapData.Length);
            Array.Clear(finalData, 0, mapData.Length);

        }


    }

}

