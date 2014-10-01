#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;
using System.Diagnostics;
#endregion

namespace ElementalAbyss
{
    public class Projectile
    {
        //Time measurment
        public static Stopwatch stopwatch = new Stopwatch();

        public Vector2 position;
        public Vector2 velocity;

        public double speed;
        public double angle;

        public float scale;
        public float opacity;

        //A list of hitpoints where the program will check if the projectile has hit an enemy or player
        public Point[] hitpoints;
        public Owner owner;

        //Picture displayed when the player fires their weapon
        public Texture2D picture;
        
        public Projectile(Vector2 position, Vector2 velocity, double speed, double angle, Owner owner)
        {
            this.position = position;
            this.velocity = velocity;
            this.speed = speed;
            this.angle = angle;
            this.owner = owner;

            this.scale = 1.0f;
            this.opacity = 1.0f;

            this.hitpoints = new Point[] { new Point((int)this.position.X, (int)this.position.Y) };
            
            //Start the stopwatch to calculate time difference
            stopwatch.Start();
        }

        //The position of the projectile is calculated by adding the velocity * speed
        //to the original
        public void ChangePos()
        {
            //update the position of the projectile
            this.position.X = this.position.X + ((float)this.speed * this.velocity.X);
            this.position.Y = this.position.Y + ((float)this.speed * this.velocity.Y);
        }

        //Empty method in the base class, so the different weapons
        //can have their own Update function
        public virtual int Update() 
        {
            return 0;
        }

        public int CheckIfPositionIsValid()
        {
            //Check if the projectile doesnt go outside the screen
            if (this.position.X > Main.SCREENWIDTH || this.position.X < 0 || this.position.Y > Main.SCREENHEIGHT || this.position.Y < 0)
            {
                //Main will delete the projectile now
                return 1;
            }
            //Main saves the projectile
            return 0;
        }

        //Lowers the health of the target (either an enemy or the player) and calculates knockback


        //Checks if the projectile hits anyone
        public int CharacterCollision(Point[] hitpoints)
        {
            //Loops through all the enemies
            foreach (Enemy enemy in Main.enemyList)
            {
                //Loops through all the hitpoints where the projectile should hit something
                foreach (Point hitpoint in hitpoints)
                {
                    if (enemy.objectRectangle.Contains(hitpoint) && this.owner != Owner.enemy)
                    {
                        return enemy.doDamage(this);
                    }
                    if (this.owner == Owner.enemy && Main.player.objectRectangle.Contains(hitpoint))
                    {
                        return Main.player.doDamage(this);
                    }
                }
            }

            return 0;
        }
    }

    class Fireball : Projectile
    {
        //The fire slowly fades every x ms
        long timeSinceFade = stopwatch.ElapsedMilliseconds;

        public Fireball(Vector2 position, Vector2 velocity, double speed, double angle, float scale, Owner owner)
            : base(position, velocity, speed, angle, owner)
        {
            this.position = position;
            this.velocity = velocity;
            this.speed = speed;
            this.angle = angle;
            this.picture = Main.fireballPic;
            this.scale = scale;

            //The game will check for enemies on these points
            this.hitpoints = new Point[] { new Point((int)this.position.X, (int)this.position.Y) };

            //The fireball will exist longer if the player has a higher level
            this.opacity = 1 * (1.1f);
        }
        public override int Update()
        {
            //Updates the hitpoint list
            this.hitpoints[0] = new Point((int)this.position.X, (int)this.position.Y);

            if (stopwatch.ElapsedMilliseconds - this.timeSinceFade > 100 && this.opacity > 0)
            {
                this.opacity -= 0.01f;
            }
            if (this.opacity <= 0)
            {
                return 1;
            }



            return 0;
        }
    }

    class Rock : Projectile
    {
        //for making sure the rock only accelerates every x ms
        long lastAcceleration;

        public Rock(Vector2 position, Vector2 velocity, double speed, double angle, float scale, Owner owner)
            : base(position, velocity, speed, angle, owner)
        {
            this.position = position;
            this.velocity = velocity;
            this.speed = speed;
            this.angle = angle;
            this.picture = Main.rockPic;
            this.scale = scale;
            this.owner = owner;

            this.hitpoints = new Point[] { new Point((int)this.position.X, (int)this.position.Y) };

            this.lastAcceleration = stopwatch.ElapsedMilliseconds;
        }

        public override int Update()
        {
            this.hitpoints[0] = new Point((int)this.position.X, (int)this.position.Y);

            //Accelerate 0.1 px/frame every 10 ms
            if (stopwatch.ElapsedMilliseconds - this.lastAcceleration >= 10)
            {
                speed += 0.1;

                //Update the lastAcceleration variable to the current time
                this.lastAcceleration = stopwatch.ElapsedMilliseconds;
            }

            return 0;
        }
    }

    class WaterStream : Projectile
    {
        public WaterStream(Vector2 position, Vector2 velocity, double speed, double angle, Owner owner)
            : base(position, velocity, speed, angle,owner)
        {
            this.position = position;
            this.velocity = velocity;
            this.speed = speed;
            this.angle = angle;
            this.picture = Main.waterstreamPic;
            this.owner = owner;

            this.hitpoints = new Point[] { new Point((int)this.position.X, (int)this.position.Y) };
        }

        public override int Update()
        {
            //Updates the hitpoint list
            this.hitpoints[0] = new Point((int)this.position.X, (int)this.position.Y);

            return 0;
        }
    }

    class Wind : Projectile
    {
        //these variables exist to check time inbetween 2 actions, so it happens at roughly the same time on every machine
        long timeInExistance = stopwatch.ElapsedMilliseconds;
        long lastOpacityFade = stopwatch.ElapsedMilliseconds;

        public Wind(Vector2 position, Vector2 velocity, double speed, double angle, float scale,Owner owner)
            : base(position, velocity, speed, angle, owner)
        {
            this.position = position;
            this.velocity = velocity;
            this.speed = speed;
            this.angle = angle;
            this.picture = Main.windPic;
            this.owner = owner;

            this.hitpoints = new Point[] { new Point((int)this.position.X, (int)this.position.Y), new Point((int)this.position.X, (int)this.position.Y - 32), new Point((int)this.position.X, (int)this.position.Y - 64), new Point((int)this.position.X, (int)this.position.Y + 32), new Point((int)this.position.X, (int)this.position.Y + 64) };

            //Only during the last 100ms will the wind fade away
            this.opacity = 8;

            this.scale = 1; //wind is *not* larger when the player has a higer level
        }

        public override int Update()
        {
            this.hitpoints[0] = new Point((int)this.position.X, (int)this.position.Y);

            //Calculate the rotated  axis to determine the position of the hitpoints
            Vector2 rotatedXAxis = Main.Rotation(new Vector2(1, 0), (float)this.angle);
            Vector2 rotatedYAxis = Main.Rotation(new Vector2(0, 1), (float)this.angle);

            //Update the positions of all hitpoints
            for (int i = -2; i <= 2; i++ )
            {
                if (i != 0)
                {
                    Vector2 tempHitpoint = (0 * rotatedXAxis) + ((i * -32) * rotatedYAxis);
                    if (i < 0)
                        hitpoints[i + 3] = new Point((int)(hitpoints[0].X + tempHitpoint.X), (int)(hitpoints[0].Y + tempHitpoint.Y));
                    else if (i > 0)
                        hitpoints[i + 2] = new Point((int)(hitpoints[0].X + tempHitpoint.X), (int)(hitpoints[0].Y + tempHitpoint.Y));
                }
            }
            //The wind can only exist 800ms
            if (stopwatch.ElapsedMilliseconds - timeInExistance >= 800)
            {
                //returning 1 means the object will be deleted (works for every child of projectile)
                return 1;
            }

            return 0;
        }
    }
}