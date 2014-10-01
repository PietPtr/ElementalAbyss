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
    public class Character
    {
        //Time measurment
        public static Stopwatch stopwatch = new Stopwatch();

        public long timeSinceShot = stopwatch.ElapsedMilliseconds;
        public long timeSinceEnergyRecovery = stopwatch.ElapsedMilliseconds;

        //vector variables for player movement
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 destination;
        public Vector2 destinationPositionNormalized;

        public double angle;
        public double speed;

        //Angle calculation
        public double horizontalSide;
        public double verticalSide;
        public double diagonalSide;

        public Element element;

        //To render in the right GameState
        public int gamestate;

        public int energy;
        public int health;
        public int maxEnergy;
        public int maxHealth;
        
        //for colisions
        public Rectangle objectRectangle;

        //Stats
        public double maxSpeed = 6; //change on levelup/element

        //Pause weapons/regen when talking
        //The int represents the enemy the player is talking to
        // < 0 means the player isn't talking
        public int talking = -1;

        static Random random = new Random();

        //Generates a random integer
        public int randomInt(int low, int high)
        {
            int randomInteger = random.Next(low, high + 1);
            return randomInteger;
        }

        //Other methods are called when this enemy is dead.
        public bool died = false;

        public Character(Element element)
        {
            this.element = element;

            this.position = new Vector2(this.randomInt(2, 18) * 64, this.randomInt(2, 9) * 64);
            this.velocity = new Vector2(0, 0);

            this.destination = new Vector2(5 * 64, 5 * 64);

            this.angle = 0;
            
            this.destinationPositionNormalized = new Vector2(destination.X - position.X, destination.Y - position.Y);
            this.destinationPositionNormalized.Normalize();

            //Different speed, health and energy for all elements
            switch (this.element)
            {
                case (Element.air):
                    this.speed = 7;
                    this.maxHealth = 75;
                    this.maxEnergy = 100;
                    break;
                case (Element.fire):
                    this.speed = 6;
                    this.maxHealth = 100;
                    this.maxEnergy = 140;
                    break;
                case (Element.earth):
                    this.speed = 3;
                    this.maxHealth = 140;
                    this.maxEnergy = 90;
                    break;
                case (Element.water):
                    this.speed = 6;
                    this.maxHealth = 120;
                    this.maxEnergy = 100;
                    break;
            }

            //everything starts fully healed
            this.energy = this.maxEnergy;
            this.health = this.maxHealth;

            //Set the rectangle used for collisions
            this.objectRectangle = new Rectangle((int)this.position.X - 32, (int)this.position.Y - 32, 64, 64);

            //start the stopwatch for time measuring
            stopwatch.Start();
        }

        public int CheckPosition()
        {
            //Check if the position is still valid
            //if there are still enemies left, the entrances are blocked by fires
            if (Main.blockTransparency <= 0 || Main.doneFighting > 0)
            {
                //Check if the player is in one of the entrance spots and start going to the next room (fading in/out)
                //The int returned indicates the tile where the player is standing
                if (this.position.X >= 576 && this.position.X < 704 && this.position.Y < 128)
                {
                    return 1; //Top
                }
                else if (this.position.X >= 576 && this.position.X < 704 && this.position.Y > 640)
                {
                    return 2; //Bottom
                }
                else if (this.position.Y >= 384 && this.position.Y < 448 && this.position.X < 64)
                {
                    return 3; //Left
                }
                else if (this.position.Y >= 384 && this.position.Y < 448 && this.position.X > 1216)
                {
                    return 4; //Right
                }
                else
                {
                    this.position.X = (this.position.X < 1 * 64) ? 1 * 64 : this.position.X;
                    this.position.X = (this.position.X > 19 * 64) ? 19 * 64 : this.position.X;

                    this.position.Y = (this.position.Y < 2 * 64) ? 2 * 64 : this.position.Y;
                    this.position.Y = (this.position.Y > 10 * 64) ? 10 * 64 : this.position.Y;
                }
            }
            else if (Main.blockTransparency > 0)
            {
                this.position.X = (this.position.X < 1 * 64) ? 1 * 64 : this.position.X;
                this.position.X = (this.position.X > 19 * 64) ? 19 * 64 : this.position.X;

                this.position.Y = (this.position.Y < 2 * 64) ? 2 * 64 : this.position.Y;
                this.position.Y = (this.position.Y > 10 * 64) ? 10 * 64 : this.position.Y;
            }

            return 0;
        }

        public void ChangePos()
        {
            //Update the normalized Vector between the destination and the position
            this.destinationPositionNormalized = new Vector2(destination.X - position.X, destination.Y - position.Y);
            this.destinationPositionNormalized.Normalize();

            this.velocity = this.destinationPositionNormalized;

            //update the position of the Character
            if ((int)(this.position.X / 5) != (int)(this.destination.X / 5))
            {
                this.position.X = (float)(this.position.X + this.velocity.X * this.speed);
            }
            if ((int)(this.position.Y / 5) != (int)(this.destination.Y / 5))
            {
                this.position.Y = (float)(this.position.Y + this.velocity.Y * this.speed);
            }

            //Update the rectangle used for collisions
            this.objectRectangle = new Rectangle((int)this.position.X - 32, (int)this.position.Y - 32, 64, 64);
        }


        public void UpdateRotation()
        {
            //Calculate the sides of the triangle
            this.verticalSide = Mouse.GetState().Y - this.position.Y;
            this.horizontalSide = Mouse.GetState().X - this.position.X;
            this.diagonalSide = Math.Sqrt(Math.Pow(this.verticalSide, 2) + Math.Pow(this.horizontalSide, 2));

            
            //Calculate the new angle
            this.angle = Math.Asin(this.verticalSide / this.diagonalSide);

            //Reverse the angle for position bigger than player X
            if (Mouse.GetState().X < this.position.X)
            {
                this.angle = Math.PI - this.angle;
            }
        }

        //Adds a projectile to the projectile list, so it can be updated/rendered
        public void addToProjectileList(Projectile projectile, int energyCost, int shootingDelay)
        {
            //Check if the character has enough energy left to use the weapon
            if (this.energy - energyCost >= 0 && stopwatch.ElapsedMilliseconds - this.timeSinceShot > shootingDelay)
            {
                //lower the players energy
                this.energy = this.energy - energyCost;

                Main.projectileList.Add(projectile);
                timeSinceShot = stopwatch.ElapsedMilliseconds;

                //Also play/start the sound matching the element
                switch(projectile.ToString())
                {
                    case ("ElementalAbyss.Fireball"):
                        
                        Main.fireblastSound.Play();
                        break;
                    case ("ElementalAbyss.WaterStream"):
                        //Sound for water is in Main, because it is a continuing sound as long as there is water in projectileList
                        break;
                    case ("ElementalAbyss.Wind"):
                        Main.windSound.Play();
                        break;
                    case ("ElementalAbyss.Rock"):
                        Main.rockSound.Play();
                        break;
                }
            }
        }

        public void SummonProjectile(Vector2 normalizedCharacterVelocity, Owner owner)
        {
            //Can only shoot if the player has energy left
            if (this.energy != 0)
            {
                //Check the element of the player and call addToProjectileList with the right element
                switch (this.element)
                {
                    case (Element.air):
                        addToProjectileList(new Wind(this.position, normalizedCharacterVelocity, 12, this.angle, (10) * 0.1f, owner), 20, 250);
                        break;
                    case (Element.fire):
                        addToProjectileList(new Fireball(this.position, normalizedCharacterVelocity, 7, this.angle, (8) * 0.1f, owner), 10, 250);
                        break;
                    case (Element.earth):
                        addToProjectileList(new Rock(this.position, normalizedCharacterVelocity, (8) * 0.2f, this.angle, ((float)(this.energy * 1.5) / 100) + 0.4f, owner), (int)(this.energy / 1.5f), 350);
                        break;
                    case (Element.water):
                        addToProjectileList(new WaterStream(this.position, normalizedCharacterVelocity, 10, this.angle, owner), 1, 15);
                        break;
                }
            }
        }

        public void FillEnergyBar()
        {
            //Fill up the energy bar
            if (stopwatch.ElapsedMilliseconds - this.timeSinceEnergyRecovery > 100 && this.energy < this.maxEnergy)
            {
                this.energy++;
                this.timeSinceEnergyRecovery = stopwatch.ElapsedMilliseconds;
            }
        }
        
        public int doDamage(Projectile projectile)
        {
            switch (projectile.ToString())
            {
                //The player level is used for damage calculation, enemies will hit more if the player has a higher level, but so does the player.
                case ("ElementalAbyss.Wind"):
                    int damage = 0;
                    //Hit more with a higher level
                    damage = 2;
                    //Lower health
                    this.health = this.health - damage;
                    Main.labelList.Add(new Label(this.position, 0.6f, Color.Red, "-" + damage.ToString(), GameState.Explorering));
                    break;
                case ("ElementalAbyss.Rock"):
                    //Knockback
                    this.destination = new Vector2(this.destination.X + projectile.velocity.X * 35 * (float)projectile.speed, this.destination.Y + projectile.velocity.Y * 35 * (float)projectile.speed);
                    //Lower health
                    damage = (int)((projectile.scale / 1.9f) * 75);
                    this.health = this.health - damage;
                    //Add label with a different size, depending on the damage dealt
                    Main.labelList.Add(new Label(this.position, (float)damage / 60.0f, Color.Red, "-" + damage.ToString(), GameState.Explorering));
                    return 1;
                case ("ElementalAbyss.WaterStream"):
                    //Knockback
                    this.destination = new Vector2(this.destination.X + projectile.velocity.X * 10, this.destination.Y + projectile.velocity.Y * 10);
                    //Lower health
                    damage = 1;
                    this.health = this.health - damage;
                    //Add label
                    Main.labelList.Add(new Label(this.position, 0.6f, Color.Red, "-" + damage, GameState.Explorering));
                    return 1;
                case ("ElementalAbyss.Fireball"):
                    //Knockback
                    this.destination = new Vector2(this.destination.X + projectile.velocity.X * 40, this.destination.Y + projectile.velocity.Y * 40);
                    //Lower health, if the player shoots, the amount is more at a higher level
                    damage = 0;
                    //Calculate damage
                    damage = (int)(projectile.opacity * 35);
                    //Lower health
                    this.health = this.health - damage;
                    //Add a damage label
                    Main.labelList.Add(new Label(this.position, 0.6f, Color.Red, "-" + damage.ToString(), GameState.Explorering));

                    return 1;
            }
            return 0;
        }
    }

    public class Enemy : Character
    {
        //Dont shoot if the enemy is waiting for his energy to regenerate
        bool waitForRegen = false;

        //true if the player talked to the enemy
        public bool talked = false;

        //move only every x ms
        long lastMove = stopwatch.ElapsedMilliseconds;

        //The time when the enemy died (used in Fader())
        public long deadSince = 0;
        public float transparency = 1;

        public Enemy(Element element)
            : base(element)
        {
            this.element = element;
        }

        public void UpdateEnemyRotation()
        {
            //Calculate the sides of the triangle
            this.verticalSide = Main.player.position.Y - this.position.Y;
            this.horizontalSide = Main.player.position.X - this.position.X;
            this.diagonalSide = Math.Sqrt(Math.Pow(this.verticalSide, 2) + Math.Pow(this.horizontalSide, 2));

            //Only rotate if the enemy is still alive
            //This extra condition exists because diagonalside should still be updated to check
            //how nearby the player is
            if (this.died == false)
            {
                //Only look at the player if they are under 500px away
                if (this.diagonalSide <= 500)
                {
                    //Calculate the new angle
                    this.angle = Math.Asin(this.verticalSide / this.diagonalSide);

                    //Reverse the angle for position bigger than player X
                    if (Main.player.position.X < this.position.X)
                    {
                        this.angle = Math.PI - this.angle;
                    }
                }
            }
        }

        //conatins logic for the enemies so they can track the player and attack them
        public void EnemyAI()
        {
            //Shoot at the player if they come closer than 400px
            //waitForRegen exists so the enemy wont keep shooting when their energy is just enough to shoot once
            //this way they unleash a rain of their weapon untill their energy is 0
            if (this.diagonalSide <= 400 && waitForRegen == false)
            {
                //calculates the direction of the projectile
                //Main.randomInt randomizes the spread of their projectiles, so the enemies can miss
                Vector2 normalizedCharacterVelocity = new Vector2(Main.player.position.X - this.position.X, Main.player.position.Y - this.position.Y);
                normalizedCharacterVelocity.Normalize();

                this.SummonProjectile(normalizedCharacterVelocity, Owner.enemy);
            }

            //move to the player if they are too far away
            if (this.diagonalSide > 500)
            {
                this.destination = new Vector2(Main.player.position.X + Main.randomInt(-256 * 2, 256 * 2), Main.player.position.Y + Main.randomInt(-256 * 2, 256 * 2));
            }

            //wait untill their enemy is regenerated
            if (this.energy <= 10)
            {
                waitForRegen = true;
            }
            else if (this.energy > Main.randomInt(35, 100))
            {
                waitForRegen = false;
            }

            //while the enemy is waiting for their energy to regenerate, 
            //they walk around trying to 'dodge' the players attacks
            if (waitForRegen == true && stopwatch.ElapsedMilliseconds - lastMove > Main.randomInt(500, 1500))
            {
                this.destination = new Vector2(Main.randomInt(1 * 64, 19 * 64), Main.randomInt(1 * 64, 9 * 64));
                lastMove = stopwatch.ElapsedMilliseconds;
            }
        }

        //Player can talk to the enemy
        //After talking, the enemy disappears
        public float Fader()
        {
            if (this.died == true)
            {
                //Calculate the transparency with the time, so the dead enemy slowly fades out
                this.transparency = (stopwatch.ElapsedMilliseconds - this.deadSince);

                //slowly make the transparency higher
                this.transparency = 1 - (this.transparency / 2500);

                //Transparency can't go lower than 0
                if (this.transparency < 0)
                {
                    this.transparency = 0;
                }

                return this.transparency;
            }

            //Drawn at full visibility if the enemy is not dead
            return this.transparency;
        }

        //Calls necessary methods for the enemy to function
        public int Tasks()
        {
            if (died == false && stopwatch.ElapsedMilliseconds - Main.enteredNewDungeonAtTime > 1000)
            {
                //Calculate the new rotation
                this.UpdateEnemyRotation();

                //Change the position
                this.ChangePos();

                //Check if the position is valid
                this.CheckPosition();

                //Run the AI
                this.EnemyAI();

                //Fill up the energybar
                this.FillEnergyBar();
            }
            else if (died == true)
            {
                //Calculate the distance player <-> enemy
                this.UpdateEnemyRotation();

                //Show empty bars, health cant go lower than 0 in case of extra attacks
                this.health = 0;
                this.energy = 0;
            }
            
            //Kill the enemy if its health drops below 0
            if (this.health <= 0 && this.died == false)
            {
                this.died = true;
            }

            //The enemy is dead, the player talked to them, and they fully faded away
            if (this.died == true && this.transparency == 0)
            {
                //return 1;
            }

            //Nothing happened, enemy stays alive
            return 0;
        }
    }
}
