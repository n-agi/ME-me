namespace me_and_me
{
    using System;
    using Microsoft.Kinect;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public delegate Vector2 SkeletonPointMap(SkeletonPoint point);

    public class SkeletonStreamRenderer : object2D
    {
        private readonly SkeletonPointMap mapMethod;
        private static Skeleton[] skeletonData;
        private static bool skeletonDrawn = true;
        private Vector2 jointOrigin;
        private Texture2D jointTexture;
        private Vector2 boneOrigin;
        private Texture2D boneTexture;
        private bool initialized;
        public SkeletonStreamRenderer(Game game, SkeletonPointMap map)
            : base(game)
        {
            this.mapMethod = map;
        }

        public override void Initialize()
        {
            base.Initialize();
            this.Size = new Vector2(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height);
            this.initialized = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (null == this.Chooser.Sensor ||
                false == this.Chooser.Sensor.IsRunning ||
                KinectStatus.Connected != this.Chooser.Sensor.Status)
            {
                return;
            }

            if (skeletonDrawn)
            {
                using (var skeletonFrame = this.Chooser.Sensor.SkeletonStream.OpenNextFrame(0))
                {
                    if (null == skeletonFrame)
                    {
                        return;
                    }

                    if (null == skeletonData || skeletonData.Length != skeletonFrame.SkeletonArrayLength)
                    {
                        skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(skeletonData);
                    skeletonDrawn = false;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (null == this.jointTexture)
            {
                this.LoadContent();
            }

            if (null == skeletonData || null == this.mapMethod)
            {
                return;
            }

            if (false == this.initialized)
            {
                this.Initialize();
            }

            this.SharedSpriteBatch.Begin();

            foreach (var skeleton in skeletonData)
            {
                switch (skeleton.TrackingState)
                {
                    case SkeletonTrackingState.Tracked:
                        this.DrawBone(skeleton.Joints, JointType.Head, JointType.ShoulderCenter);
                        this.DrawBone(skeleton.Joints, JointType.ShoulderCenter, JointType.ShoulderLeft);
                        this.DrawBone(skeleton.Joints, JointType.ShoulderCenter, JointType.ShoulderRight);
                        this.DrawBone(skeleton.Joints, JointType.ShoulderCenter, JointType.Spine);
                        this.DrawBone(skeleton.Joints, JointType.Spine, JointType.HipCenter);
                        this.DrawBone(skeleton.Joints, JointType.HipCenter, JointType.HipLeft);
                        this.DrawBone(skeleton.Joints, JointType.HipCenter, JointType.HipRight);

                        this.DrawBone(skeleton.Joints, JointType.ShoulderLeft, JointType.ElbowLeft);
                        this.DrawBone(skeleton.Joints, JointType.ElbowLeft, JointType.WristLeft);
                        this.DrawBone(skeleton.Joints, JointType.WristLeft, JointType.HandLeft);

                        this.DrawBone(skeleton.Joints, JointType.ShoulderRight, JointType.ElbowRight);
                        this.DrawBone(skeleton.Joints, JointType.ElbowRight, JointType.WristRight);
                        this.DrawBone(skeleton.Joints, JointType.WristRight, JointType.HandRight);

                        this.DrawBone(skeleton.Joints, JointType.HipLeft, JointType.KneeLeft);
                        this.DrawBone(skeleton.Joints, JointType.KneeLeft, JointType.AnkleLeft);
                        this.DrawBone(skeleton.Joints, JointType.AnkleLeft, JointType.FootLeft);

                        this.DrawBone(skeleton.Joints, JointType.HipRight, JointType.KneeRight);
                        this.DrawBone(skeleton.Joints, JointType.KneeRight, JointType.AnkleRight);
                        this.DrawBone(skeleton.Joints, JointType.AnkleRight, JointType.FootRight);

                        foreach (Joint j in skeleton.Joints)
                        {
                            Color jointColor = Color.Green;
                            if (j.TrackingState != JointTrackingState.Tracked)
                            {
                                jointColor = Color.Yellow;
                            }

                            this.SharedSpriteBatch.Draw(
                                this.jointTexture,
                                this.mapMethod(j.Position),
                                null,
                                jointColor,
                                0.0f,
                                this.jointOrigin,
                                1.0f,
                                SpriteEffects.None,
                                0.0f);
                        }

                        break;
                    case SkeletonTrackingState.PositionOnly:
                        this.SharedSpriteBatch.Draw(
                                this.jointTexture,
                                this.mapMethod(skeleton.Position),
                                null,
                                Color.Blue,
                                0.0f,
                                this.jointOrigin,
                                1.0f,
                                SpriteEffects.None,
                                0.0f);
                        break;
                }
            }

            this.SharedSpriteBatch.End();
            skeletonDrawn = true;

            base.Draw(gameTime);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            this.jointTexture = Game.Content.Load<Texture2D>("Joint");
            this.jointOrigin = new Vector2(this.jointTexture.Width / 2, this.jointTexture.Height / 2);

            this.boneTexture = Game.Content.Load<Texture2D>("Bone");
            this.boneOrigin = new Vector2(0.5f, 0.0f);
        }

        private void DrawBone(JointCollection joints, JointType startJoint, JointType endJoint)
        {
            Vector2 start = this.mapMethod(joints[startJoint].Position);
            Vector2 end = this.mapMethod(joints[endJoint].Position);
            Vector2 diff = end - start;
            Vector2 scale = new Vector2(1.0f, diff.Length() / this.boneTexture.Height);

            float angle = (float)Math.Atan2(diff.Y, diff.X) - MathHelper.PiOver2;

            Color color = Color.LightGreen;
            if (joints[startJoint].TrackingState != JointTrackingState.Tracked ||
                joints[endJoint].TrackingState != JointTrackingState.Tracked)
            {
                color = Color.Gray;
            }

            this.SharedSpriteBatch.Draw(this.boneTexture, start, null, color, angle, this.boneOrigin, scale, SpriteEffects.None, 1.0f);
        }
    }
}
