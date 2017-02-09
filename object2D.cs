using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace me_and_me
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class object2D :DrawableGameComponent
    {
        public object2D(Game game)
            : base(game)
        {     }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public KinectChooser Chooser
        {
            get
            {
                return (KinectChooser)this.Game.Services.GetService(typeof(KinectChooser));
            }
        }
        public SpriteBatch SharedSpriteBatch
        {
            get
            {
                return (SpriteBatch)this.Game.Services.GetService(typeof(SpriteBatch));
            }
        }
    }
}
