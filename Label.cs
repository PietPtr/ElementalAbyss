#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;
#endregion;

namespace ElementalAbyss
{
    //A label is a small string of text that slowly goes up and fades away
    public class Label
    {
        //All the variables necessary for drawing the label
        public Vector2 position;
        public float scale;
        public Color color;
        public float opacity = 2;

        //The text the label will display
        public string text;

        //Limit visibility to this GameState
        public GameState visibleWhen;

        //these variables update the position/opacity of the label
        public Vector2 velocity = new Vector2(0, -0.5f);

        public Label(Vector2 position, float scale, Color color, string text, GameState visibleWhen)
        {
            this.position = new Vector2(position.X + Main.randomInt(-32, 32), position.Y + Main.randomInt(-32, 32));
            this.scale = scale;
            this.color = color;
            this.text = text;
            this.visibleWhen = visibleWhen;
        }

        //Updates the position, opacity etc of the label
        public void Update()
        {
            //The label slowly goes up
            this.position = this.position + this.velocity;

            //Slowly fade the label away
            this.opacity = this.opacity - 0.0125f;

            //TODO: DELETION 
        }
    }
}
