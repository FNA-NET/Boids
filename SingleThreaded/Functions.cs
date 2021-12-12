using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FNA.Boids
{
    public static class Functions
    {
        public static Random Rand = new Random();


        #region Sprite Component Functions

        public static void Sprite_CenterOrigin(ComponentSprite Sprite)
        {
            Sprite.origin.X = Sprite.drawRec.Width * 0.5f;
            Sprite.origin.Y = Sprite.drawRec.Height * 0.5f;
        }

        #endregion


        #region Animation System Functions

        public static void Animation_Update(ComponentAnimation Anim, ComponentSprite Sprite)
        {
            Anim.counter++;
            if (Anim.counter >= Anim.speed)
            {
                Anim.counter = 0; Anim.index++;
                if (Anim.index >= Anim.Frames.Count) { Anim.index = 0; }
                Sprite.currentFrame = Anim.Frames[Anim.index];
            }
        }

        #endregion


        #region Drawing Functions

        public static void Draw(ComponentSprite Sprite)
        {
            Vector2 pos = new Vector2();
            if (Sprite.visible)
            {   //set draw rec
                Sprite.drawRec.X = (Sprite.drawRec.Width * Sprite.currentFrame.X);
                Sprite.drawRec.Y = (Sprite.drawRec.Height * Sprite.currentFrame.Y);
                //set sprite effect
                if (Sprite.currentFrame.flipHori)
                { Sprite.spriteEffect = SpriteEffects.FlipHorizontally; }
                else { Sprite.spriteEffect = SpriteEffects.None; }

                if (Sprite.flipHorizontally)
                { Sprite.spriteEffect = SpriteEffects.FlipHorizontally; }

                //setup pos
                pos.X = Sprite.X; pos.Y = Sprite.Y;
                //draw the sprite
                Data.SB.Draw(
                    Sprite.texture, pos, Sprite.drawRec,
                    Sprite.drawColor * Sprite.alpha, Sprite.rotationValue,
                    Sprite.origin, Sprite.scale, Sprite.spriteEffect,
                    Sprite.layer);
            }
        }

        public static void Draw(ComponentText Text)
        {
            Data.SB.DrawString(Text.font, Text.text, Text.position,
                Text.color * Text.alpha, Text.rotation, Vector2.Zero,
                Text.scale, SpriteEffects.None, Text.zDepth);
        }

        #endregion

    }
}