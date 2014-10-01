#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace ElementalAbyss
{
    class Button
    {
        //variables for position and size of the button
        public Vector2 topLeft;
        public Vector2 size;

        //shows up at this gamestate
        public GameState visibleWhen;

        //Picture of the button
        public Texture2D picture;

        //extra argument
        int extraArgument;

        //hold the text that is displayed on the button
        public string text;

        Func<int, GameState> buttonFunction;

        //For buttons without any text
        public Button(Vector2 topLeft, Vector2 size, Texture2D picture, GameState visibleWhen, Func<int, GameState> buttonFunction, int extraArgument)
        {
            this.topLeft = topLeft;         //position of the button
            this.size = size;               //total size of the button (!= size of the picture!!)
            this.picture = picture;         //Picture to display
            this.visibleWhen = visibleWhen; //shows up when this gamestate is active
            this.buttonFunction = buttonFunction;
            this.extraArgument = extraArgument;
            this.text = "";
        }

        //For buttons with text
        public Button(Vector2 topLeft, Vector2 size, Texture2D picture, GameState visibleWhen, Func<int, GameState> buttonFunction, int extraArgument, string text)
        {
            this.topLeft = topLeft;         //position of the button
            this.size = size;               //total size of the button (!= size of the picture!!)
            this.picture = picture;         //Picture to display
            this.visibleWhen = visibleWhen; //shows up when this gamestate is active
            this.buttonFunction = buttonFunction;
            this.extraArgument = extraArgument;
            this.text = text;
        }

        //Check if the mouse collides with the button
        public bool MouseCollision()
        {
            Vector2 mousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

            if (mousePos.X > this.topLeft.X && mousePos.X < this.topLeft.X + size.X && mousePos.Y > this.topLeft.Y && mousePos.Y < this.topLeft.Y + size.Y && Mouse.GetState().LeftButton == ButtonState.Released && Main.oldMouseState.LeftButton == ButtonState.Pressed)
            {
                buttonFunction(extraArgument);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
