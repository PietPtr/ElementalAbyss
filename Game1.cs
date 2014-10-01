#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
#endregion

/* TODO:
 * 1. Add player movement                                                                       [DONE]
 *   - move around with WASD                                                                    [DONE]
 *   - face follows mouse                                                                       [DONE]
 * 2. Add player fighthing                                                                      [DONE]
 *   - Fireball with configurable size                                                          [DONE]
 *   - Rock with configurable size and speed                                                    [DONE]
 *   - Water stream with configurable durablity (is now energy)                                 [DONE]
 *   - Air blast with configurable width                                                        [DONE]
 *   - Attributes level up when player defeats more enemies, configurable values change         [DONE]
 * 3. Add enemies                                                                               [DONE]
 *   - Can fight the same way the player can                                                    [DONE]
 *     1. Aims at the player and tries to get off as much damage as possible (higher priority)  [DONE]
 *   - Never further away from the player than x pixels                                         [DONE]
 *   - Health                                                                                   [DONE]
 *   - health bars displayed                                                                    [DONE]
 *   - hitboxes...                                                                              [DONE]
 *   - Knockback                                                                                [DONE]
 *   - Death state for enemies                                                                  [DONE]
 *   - Reddit death messages                                                                    [DONE]
 *   - Player death state                                                                       [DONE]
 *   - Display Score                                                                            [DONE]
 * 4. Add Particle class and other misc visual additions
 *   - Animations with several frames and variable time/frame
 *   - the particles can move with a velocity/speed
 *   - Particles can dissapear over time
 *   - Fade fire in and out at the dungeon entrances                                            [DONE]
 *   - Add damage labels (-{damage} above the heads of hit entities)                            [DONE]
 *   - Add +{exp} label                                                                         [DONE]
 * 5. Add doors to the dungeons                                                                 [DONE]
 *   - Doors lead to new dungeons                                                               [DONE]
 *   - 2 types:
 *     1. - Fight dungeons, enemies who attack on sight                                         [DONE]
 *        - XP is awarded when defeated                                                         [DONE]
 *     2. - NPC dungeon, friendly NPC's the player can talk to [Reddit]
 *        - Random amount of NPC's (2-8)
 *        - Only use is that the player can talk to them over and over.
 *        - Can't die/be attacked. the player can shoot, but it won't do anything
 *  6. Add menu                                                                                 [DONE]
 *    - Play game                                                                               [DONE]
 *    - High scores
 *    - Reload texts                                                                            [DONE]
 *    - Quit                                                                                    [DONE]
 *  7. Misc                                                                                     [DONE]
 *    - CHEATS code -> LVL++                                                                    [DONE]
 *    
 *  Fix before release:
 *    - Knockback
 *    - Fire                                                                                    [DONE]
 *    - Credits                                                                                 [DONE]
 *    - No death                                                                                [DONE]
 *    - Fading does not work
 *    - Add difficulty ( = Amount of enemies)
 *    - Should not take input when not focused
 */

namespace ElementalAbyss
{
    //This is the main type for your game
    public class Main : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //texture variables
        Texture2D dungeonPic;
        Texture2D fireBlockadePic, waterBlockadePic, earthBlockadePic, airBlockadePic;
        Texture2D[] bannerList;
        Texture2D choosePic;
        public static Texture2D airPlayerPic, firePlayerPic, earthPlayerPic, waterPlayerPic;
        public static Texture2D airEnemyPic, fireEnemyPic, earthEnemyPic, waterEnemyPic;
        public static Texture2D fireballPic, rockPic, waterstreamPic, windPic;
        public static Texture2D deadCharacterPic, deathScreenPic;
        Texture2D healthBarPic, energyBarPic, expBarPic, energyOverlayPic;
        Texture2D buttonPic, textBoxPic;
        Texture2D titlePic;

        //Sound variables
        public static SoundEffect fireblastSound;
        public static SoundEffect rockSound;
        public static SoundEffect windSound;
        public static SoundEffect waterstreamSound;
        public static SoundEffect music;
        //Create an instance of waterstream so the sound can be started and stopped
        public static SoundEffectInstance waterstreamSoundInstance;
        public static SoundEffectInstance musicInstance;
        //Only play water if there is water available
        bool playWaterSound = false;

        //Variable for the spritefont
        public static SpriteFont font;

        //The current frame. +1 every frame to count the amount of frames that have been passed
        public int frame = 0;

        //List for all the projectiles
        public static List<Projectile> projectileList = new List<Projectile>();

        //Player variables
        public static Character player = new Character(Element.water);

        //Path to the directory from which the program is running
        string path = Directory.GetCurrentDirectory();

        //Constants
        public const int SCREENWIDTH = 1280;
        public const int SCREENHEIGHT = 704;

        //Gamestate variables
        public static GameState CurrentGameState = GameState.Menu;

        //List to hold all the buttons
        List<Button> buttonList = new List<Button>();

        //List for all the labels
        public static List<Label> labelList = new List<Label>();

        //List to hold all enemies
        public static List<Enemy> enemyList = new List<Enemy>();

        //When all the enemies are dead, this is the time when the last enemy died
        //0 when there are still enemies alive.
        public static long doneFighting = 0;

        //Old states
        public static MouseState oldMouseState = Mouse.GetState();
        public static KeyboardState oldKeyBoardState = Keyboard.GetState();
        public static GameState oldGameState = GameState.Menu;

        //Time measurment
        static Stopwatch stopwatch = new Stopwatch();
        //Variables used for timed events (eg shooting delay)
        long timeSinceShot = stopwatch.ElapsedMilliseconds;
        long timeSinceEnergyRecovery = stopwatch.ElapsedMilliseconds;
        //represents the time the gamestate is active
        long gamestateInitiation = stopwatch.ElapsedMilliseconds;
        //represents the time when the player entered a new dungeon
        public static long enteredNewDungeonAtTime = stopwatch.ElapsedMilliseconds;
        
        //Used to fade away the blockades after the player is done fighting
        public static float blockTransparency = 1;
        public float deathScreenTransparency = 0;

        //Represents the score
        public static int score;

        //Window has to be active
        bool isWindowActive;


        static Random random = new Random();

        public Main()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = SCREENHEIGHT;
            graphics.PreferredBackBufferWidth = SCREENWIDTH;

            //Make the mouse visible
            this.IsMouseVisible = true;
        }

        //Generate a random Integer
        public static int randomInt(int low, int high)
        {
            int randomInteger = random.Next(low, high + 1);
            return randomInteger;
        }

        //Calculate rotation of points
        public static Vector2 Rotation(Vector2 point, float angle)
        {
            Vector2 rotatedPoint;
            rotatedPoint.X = (float)(point.X * Math.Cos(angle) - point.Y * Math.Sin(angle));
            rotatedPoint.Y = (float)(point.X * Math.Sin(angle) + point.Y * Math.Cos(angle));
            return rotatedPoint;
        }
        
        //Starts the game
        public static GameState ChangeGameStateAndElement(int element)
        {
            player.element = (Element)element;

            //Pause the enemy AI for some breathing time for the player to get started
            enteredNewDungeonAtTime = stopwatch.ElapsedMilliseconds;

            CurrentGameState = GameState.Explorering;
            return CurrentGameState;
        }

        //For buttons, so they can change the gamestate
        public static GameState changeGameState(int newGameState)
        {
            CurrentGameState = (GameState)newGameState;
            return CurrentGameState;
        }
        
        //Creation of delegates
        Func<int, GameState> gameStateAndElementDelegate = ChangeGameStateAndElement;
        Func<int, GameState> gameStateDelegate = changeGameState;

        //All buttons are added to the buttonlist in this method
        public void GenerateButtons()
        {
            for (int i = 0; i < 4; i++)
            {
                //Turn the banners into clickable buttons, so the player can choose their element
                buttonList.Add(new Button(new Vector2((SCREENWIDTH / 2 + 92 * 2) - i * 92 * 2, 92 * 2 + 8), new Vector2(92 * 2, 256 * 2), bannerList[i], GameState.ElementChoice, gameStateAndElementDelegate, i));
            }

            //Start the game if this button is clicked
            buttonList.Add(new Button(new Vector2(100, 64 + 0 * buttonPic.Height), new Vector2(buttonPic.Width, buttonPic.Height), buttonPic, GameState.Menu, gameStateDelegate, (int)GameState.ElementChoice, "Start Game"));

            //Quit the game when pressed
            buttonList.Add(new Button(new Vector2(100, 64 + 4 * buttonPic.Height), new Vector2(buttonPic.Width, buttonPic.Height), buttonPic, GameState.Menu, gameStateDelegate, (int)GameState.Exit, "Quit"));

            //Display the credits
            buttonList.Add(new Button(new Vector2(100, 64 + 1 * buttonPic.Height), new Vector2(buttonPic.Width, buttonPic.Height), buttonPic, GameState.Menu, gameStateDelegate, (int)GameState.Credits, "Credits"));

            //Back to menu when pressed after death
            buttonList.Add(new Button(new Vector2((SCREENWIDTH / 2) - (buttonPic.Width / 2), 64 + 4 * buttonPic.Height), new Vector2(buttonPic.Width, buttonPic.Height), buttonPic, GameState.Dead, gameStateDelegate, (int)GameState.Menu, "Menu"));

            //Or quit the game after the player dies
            buttonList.Add(new Button(new Vector2((SCREENWIDTH / 2) - (buttonPic.Width / 2), 64 + 5 * buttonPic.Height), new Vector2(buttonPic.Width, buttonPic.Height), buttonPic, GameState.Dead, gameStateDelegate, (int)GameState.Exit, "Quit"));

            //Back to menu after reading the credits
            buttonList.Add(new Button(new Vector2((SCREENWIDTH / 2) - (buttonPic.Width / 2), 64 + 5 * buttonPic.Height), new Vector2(buttonPic.Width, buttonPic.Height), buttonPic, GameState.Credits, gameStateDelegate, (int)GameState.Menu, "Menu"));
        }

        //Creates a new dungeon with new enemies
        public static void GenerateNewDungeon(int playerTile)
        {
            //Delete all enemies
            enemyList = new List<Enemy>();

            //Add an amount of enemies with a random element to the dungeon
            enemyList.Add(new Enemy((Element)randomInt(0, 3)));

            //new dungeon, new enemies, the player isn't done fighting yet
            doneFighting = 0;

            //Regenerate the players health
            player.health = player.health + 50;
            //but not higher than 100
            if (player.health > player.maxHealth)
            {
                player.health = player.maxHealth;
            }

            //Set the players position to the appriopriate entrance
            //The player will spawn right in front of the entrance hall, not inside of it, 
            //to prevent accidental teleporting to a new dungeon. (That is what the -/+ 1 is for)
            switch (playerTile)
            {
                case (1): //Bottom
                    player.position = new Vector2(640, 576 - 1);
                    break;
                case (2): //Top
                    player.position = new Vector2(640, 128 + 1);
                    break;
                case (3): //Right
                    player.position = new Vector2(1216 - 1, 408);
                    break;
                case (4): //Left
                    player.position = new Vector2(64 + 1, 408);
                    break;
            }

            //Update the variable that holds the time when the player entered the dungeon
            //Enemy AI will pause for 1 second to give the player some time to enter and prepare for the fight
            enteredNewDungeonAtTime = stopwatch.ElapsedMilliseconds;
        }

        //Draws the appropriate blockade that corresponds with the enemy in the dungeon
        public void DrawBlockades(Texture2D picture)
        {
            Vector2[] blockadePositions = new Vector2[] { new Vector2(9, 1), new Vector2(10, 1), new Vector2(9, 10), new Vector2(10, 10), new Vector2(0, 6), new Vector2(19, 6) };

            foreach (Vector2 blockPos in blockadePositions)
            {
                if (doneFighting > 0)
                {
                    //if the player is done fighting, slowly fade the transparency of the blockade
                    blockTransparency = 1 - ((float)(stopwatch.ElapsedMilliseconds - doneFighting) / 5000.0f);
                }
                else
                {
                    blockTransparency = 1;
                }

                spriteBatch.Draw(picture, new Vector2(blockPos.X * 64, blockPos.Y * 64), Color.White * blockTransparency);
            }
        }

        //Allows the game to perform any initialization it needs to before starting to run.
        //This is where it can query for any required services and load any non-graphic
        //related content.  Calling base.Initialize will enumerate through any components
        //and initialize them as well.
        protected override void Initialize()
        {
            //Start the stopwatch for time managment
            stopwatch.Start();
            
            //Start with a random enemy
            enemyList.Add(new Enemy((Element)randomInt(0, 3)));

            //Create a new instance of the player class. Element.water is the default element, 
            //it is changed when the player chooses their element during GameState.ElementChoice GameState
            player = new Character(Element.water);

            base.Initialize();
        }

        //LoadContent will be called once per game and is the place to load
        //all of your content.
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Pictures for dungeons
            dungeonPic = Content.Load<Texture2D>("dungeon");

            //Load all the blockades
            fireBlockadePic = Content.Load<Texture2D>("blockades\\fireblockade");
            waterBlockadePic = Content.Load<Texture2D>("blockades\\waterblockade");
            earthBlockadePic = Content.Load<Texture2D>("blockades\\earthblockade");
            airBlockadePic = Content.Load<Texture2D>("blockades\\airblockade");

            //player pictures and NPCs
            //Load the pictures of different player sprites
            airPlayerPic = Content.Load<Texture2D>("characters\\airCharacter");
            firePlayerPic = Content.Load<Texture2D>("characters\\fireCharacter");
            earthPlayerPic = Content.Load<Texture2D>("characters\\earthCharacter");
            waterPlayerPic = Content.Load<Texture2D>("characters\\waterCharacter");

            //Load the font
            font = Content.Load<SpriteFont>("font");

            //Load the banners of the 4 elements
            bannerList = new Texture2D[4] { Content.Load<Texture2D>("choice\\earthBanner"),
                                            Content.Load<Texture2D>("choice\\waterBanner"),
                                            Content.Load<Texture2D>("choice\\fireBanner"),
                                            Content.Load<Texture2D>("choice\\airBanner") };

            choosePic = Content.Load<Texture2D>("choice\\choose");

            //Load the default button Picture
            buttonPic = Content.Load<Texture2D>("button");

            //Load the textbox picture
            textBoxPic = Content.Load<Texture2D>("textbox");

            //Add buttons to the buttonlist once the content is loaded.
            GenerateButtons();

            //Load the picture for the title screen
            titlePic = Content.Load<Texture2D>("title");

            //'weapons' (fireballs, rocks, water, wind etc)
            fireballPic = Content.Load<Texture2D>("weapons\\fireball");
            rockPic = Content.Load<Texture2D>("weapons\\rock");
            waterstreamPic = Content.Load<Texture2D>("weapons\\waterstream");
            windPic = Content.Load<Texture2D>("weapons\\wind");

            //Load the pictures for the energy bar and health bar
            energyBarPic = Content.Load<Texture2D>("energyBar");
            healthBarPic = Content.Load<Texture2D>("healthBar");
            expBarPic = Content.Load<Texture2D>("expBar");
            energyOverlayPic = Content.Load<Texture2D>("energyOverlay");

            //Load the pictures of different enemies
            airEnemyPic = Content.Load<Texture2D>("characters\\airEnemy");
            fireEnemyPic = Content.Load<Texture2D>("characters\\fireEnemy");
            earthEnemyPic = Content.Load<Texture2D>("characters\\earthEnemy");
            waterEnemyPic = Content.Load<Texture2D>("characters\\waterEnemy");

            //Picture for all the dead things (mostly enemies)
            deadCharacterPic = Content.Load<Texture2D>("characters\\deadCharacter");
            deathScreenPic = Content.Load<Texture2D>("deathScreen");

            //Sound loading
            fireblastSound = Content.Load<SoundEffect>("sounds\\fireblast");
            rockSound = Content.Load<SoundEffect>("sounds\\rock");
            windSound = Content.Load<SoundEffect>("sounds\\wind");
            waterstreamSound = Content.Load<SoundEffect>("sounds\\waterstream");
            music = Content.Load<SoundEffect>("sounds\\Vindsvept - The Mad Harvester");

            //Set some variables for sounds
            waterstreamSoundInstance = waterstreamSound.CreateInstance();
            waterstreamSoundInstance.IsLooped = true;

            musicInstance = music.CreateInstance();
            musicInstance.IsLooped = true;
        }

        //UnloadContent will be called once per game and is the place to unload
        //all content.
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        //Allows the game to run logic such as updating the world,
        //checking for collisions, gathering input, and playing audio.
        protected override void Update(GameTime gameTime)
        {
            //Exit program when Escape is pressed
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //Debuggin key
            if (Keyboard.GetState().IsKeyDown(Keys.G))
            {
                try
                {
                    Console.WriteLine(enemyList.Count);
                }
                catch
                {

                }
            }

            //Update the active window check
            isWindowActive = this.IsActive;

            if (isWindowActive)
            {
                //Play the background music
                if (musicInstance.State == SoundState.Stopped)
                {
                    musicInstance.Play();
                }

                //Checks if buttons are being clicked
                foreach (Button button in buttonList)
                {
                    if (button.visibleWhen == CurrentGameState)
                    {
                        //Check for mouse input on buttons in their appropriate gamestate
                        button.MouseCollision();
                    }
                }

                //reset the gamestateTime var if there is a new gamestate
                if (oldGameState != CurrentGameState)
                {
                    gamestateInitiation = stopwatch.ElapsedMilliseconds;
                }

                oldGameState = CurrentGameState;

                if (CurrentGameState == GameState.Exit)
                {
                    Exit();
                }

                //Menu does not accept any other input than buttons
                if (CurrentGameState == GameState.Menu)
                {
                    score = 0;
                }

                if (CurrentGameState == GameState.Explorering && player.talking < 0)
                {
                    //Update the normalized player vector (used in player/projectile movement)
                    Vector2 normalizedPlayerVelocity = new Vector2(Mouse.GetState().X - player.position.X, Mouse.GetState().Y - player.position.Y);
                    normalizedPlayerVelocity.Normalize();

                    //Move the player with point and click system when the player has clicked the mouse
                    if (Mouse.GetState().LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
                    {
                        player.destination = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                    }

                    //Update player rotation and enemy rotation
                    player.UpdateRotation();

                    //summon projectile
                    if (Keyboard.GetState().IsKeyDown(Keys.Space) && player.energy != 0)
                    {
                        normalizedPlayerVelocity = new Vector2(Mouse.GetState().X - player.position.X, Mouse.GetState().Y - player.position.Y);
                        normalizedPlayerVelocity.Normalize();

                        player.SummonProjectile(normalizedPlayerVelocity, Owner.player);
                    }

                    //Update projectile position and other information
                    for (int i = projectileList.Count - 1; i >= 0; i--)
                    {
                        //Update the position of the projectile
                        projectileList[i].ChangePos();

                        //Check if the projectile hit someone
                        if (projectileList[i].CharacterCollision(projectileList[i].hitpoints) == 1)
                        {
                            //Delete the projectile that hit something
                            projectileList.RemoveAt(i);
                            continue;
                        }

                        //Delete a projectile when the update method returns 1
                        //this is used when a projectile dies out after a certain amount of time or hits a wall.
                        if (projectileList[i].Update() == 1 || projectileList[i].CheckIfPositionIsValid() == 1)
                        {
                            //Delete the projectile that died out/hit something
                            projectileList.RemoveAt(i);
                            continue;
                        }
                    }

                    foreach (Projectile proj in projectileList)
                    {
                        //Play water sounds if there is water available
                        if (proj.ToString() == "ElementalAbyss.WaterStream")
                        {
                            playWaterSound = true;
                            break;
                        }
                        else
                        {
                            playWaterSound = false;
                        }
                    }

                    //If the list is empty, there is no water, so no water sound should be played
                    if (projectileList.Count == 0)
                    {
                        playWaterSound = false;
                    }

                    //play the water sound if there is water available.
                    if (waterstreamSoundInstance.State == SoundState.Stopped && playWaterSound == true)
                    {
                        //Play() method is called when the sound was stopped (or when it has not yet been played)
                        waterstreamSoundInstance.Play();
                    }
                    if (waterstreamSoundInstance.State == SoundState.Paused && playWaterSound == true)
                    {
                        //Resume() method is called when the sound was paused (the sound has been played before)
                        waterstreamSoundInstance.Resume();
                    }
                    else if (playWaterSound == false)
                    {
                        waterstreamSoundInstance.Pause();
                    }

                    //Update all the enemies (their position, angle etc etc)
                    //If their are no enemies left in the room, dont try updating anything
                    if (enemyList.Count != 0)
                    {
                        for (int i = enemyList.Count - 1; i >= 0; i--)
                        {
                            //Tasks calls all methods necessary for the specific enemy
                            //It also checks if the enemy is dead, and calls other methods if it is dead
                            //The method returns 1 if the enemy should be deleted
                            if (enemyList[i].Tasks() == 1)
                            {
                                enemyList.RemoveAt(i);
                            }
                        }
                    }

                    //Check if all the enemies are dead
                    if (doneFighting == 0)
                    {
                        foreach (Enemy enemy in enemyList)
                        {
                            if (enemy.died == false)
                            {
                                //the player is not done fighting, immidiatly break out of the loop and doneFighting stays false.
                                doneFighting = 0;
                                break;
                            }
                            else if (enemy.died == true)
                            {
                                //if the loop finishes without ever being false, the player is done fighting
                                doneFighting = stopwatch.ElapsedMilliseconds;
                                
                                score++;
                            }
                        }
                    }

                    //Update the player position
                    player.ChangePos();

                    //Check if the players position is still valid
                    int playerTile = player.CheckPosition();

                    if (playerTile != 0)
                    {
                        GenerateNewDungeon(playerTile);
                    }

                    if (player.health <= 0)
                    {
                        CurrentGameState = GameState.Dead;
                    }

                    //Update the players energy
                    player.FillEnergyBar();

                    //Reset the deathscreen transparency
                    deathScreenTransparency = 0;
                }

                if (CurrentGameState == GameState.Dead)
                {
                    deathScreenTransparency += 0.01f;

                    //Reset all variables so the player can start over
                    enemyList = new List<Enemy>();
                    labelList = new List<Label>();
                    projectileList = new List<Projectile>();

                    player = new Character(Element.water);
                    enemyList.Add(new Enemy((Element)randomInt(0, 3)));
                }

                //Update all the labels
                foreach (Label label in labelList)
                {
                    label.Update();
                }

                //Update the variables for the previous keyboard/mouse states, so the next frame can use them
                oldMouseState = Mouse.GetState();
                oldKeyBoardState = Keyboard.GetState();
            }
            
            //Another frame passed
            frame++;

            base.Update(gameTime);
        }

        //This is called when the game should draw itself.
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            //determine the origin of 64x64 pictures
            Vector2 origin = new Vector2(64 / 2, 64 / 2);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

            if (CurrentGameState == GameState.Menu || CurrentGameState == GameState.Credits)
            {
                //Draw the title screen
                spriteBatch.Draw(titlePic, new Vector2(0, 0), Color.White);
            }

            if (CurrentGameState == GameState.Explorering)
            {
                //Draw the dungeon
                spriteBatch.Draw(dungeonPic, new Vector2(0, 0), Color.White);

                //Draw the blockade that blocks the player from entering a new dungeon if they haven't defeated the enemy inside
                if (enemyList.Count > 0)
                {
                    switch (enemyList[0].element)
                    {
                        case (Element.earth):
                            DrawBlockades(earthBlockadePic);
                            break;
                        case (Element.water):
                            DrawBlockades(waterBlockadePic);
                            break;
                        case (Element.fire):
                            DrawBlockades(fireBlockadePic);
                            break;
                        case (Element.air):
                            DrawBlockades(airBlockadePic);
                            break;
                    }
                }

                //Render enemies
                foreach (Enemy enemy in enemyList)
                {
                    //Draw the enemy sprite (different for every element)
                    //if the enemy is dead (enemy.died is true), draw a different picture
                    switch (enemy.element)
                    {
                        case (Element.air):
                            spriteBatch.Draw(enemy.died ? deadCharacterPic : airEnemyPic, new Vector2(enemy.position.X, enemy.position.Y), new Rectangle(0, 0, 64, 64), Color.White, (float)(enemy.angle), origin, 1.0f, SpriteEffects.None, 0);
                            break;
                        case (Element.fire):
                            spriteBatch.Draw(enemy.died ? deadCharacterPic : fireEnemyPic, new Vector2(enemy.position.X, enemy.position.Y), new Rectangle(0, 0, 64, 64), Color.White, (float)(enemy.angle), origin, 1.0f, SpriteEffects.None, 0);
                            break;
                        case (Element.earth):
                            spriteBatch.Draw(enemy.died ? deadCharacterPic : earthEnemyPic, new Vector2(enemy.position.X, enemy.position.Y), new Rectangle(0, 0, 64, 64), Color.White, (float)(enemy.angle), origin, 1.0f, SpriteEffects.None, 0);
                            break;
                        case (Element.water):
                            spriteBatch.Draw(enemy.died ? deadCharacterPic : waterEnemyPic, new Vector2(enemy.position.X, enemy.position.Y), new Rectangle(0, 0, 64, 64), Color.White, (float)(enemy.angle), origin, 1.0f, SpriteEffects.None, 0);
                            break;
                    }

                    //only draw the energy/health bars when the enemy is alive
                    if (enemy.died == false)
                    {
                        //Draw the enemy energy bars
                        spriteBatch.Draw(energyBarPic, new Vector2(enemy.position.X, enemy.position.Y - 16), new Rectangle(0, 0, 64, 12), Color.White, 0.0f, origin, new Vector2(((float)enemy.energy / enemy.maxEnergy) * 1, 1), SpriteEffects.None, 0);
                        spriteBatch.Draw(energyOverlayPic, new Vector2(enemy.position.X, enemy.position.Y - 16), new Rectangle(0, 0, 64, 12), Color.White, 0.0f, origin, new Vector2(1, 1), SpriteEffects.None, 0);

                        //Draw the enemy health bar
                        spriteBatch.Draw(healthBarPic, new Vector2(enemy.position.X, enemy.position.Y - 32), new Rectangle(0, 0, 64, 12), Color.White, 0.0f, origin, new Vector2(((float)enemy.health / enemy.maxHealth) * 1, 1), SpriteEffects.None, 0);
                        spriteBatch.Draw(energyOverlayPic, new Vector2(enemy.position.X, enemy.position.Y - 32), new Rectangle(0, 0, 64, 12), Color.White, 0.0f, origin, new Vector2(1, 1), SpriteEffects.None, 0);
                    }
                }

                //Render the player with the picture corresponding to the element they chose
                switch (player.element)
                {
                    case (Element.air):
                        spriteBatch.Draw(airPlayerPic, new Vector2(player.position.X, player.position.Y), new Rectangle(0, 0, 64, 64), Color.White, (float)(player.angle), origin, 1.0f, SpriteEffects.None, 0);
                        break;
                    case (Element.fire):
                        spriteBatch.Draw(firePlayerPic, new Vector2(player.position.X, player.position.Y), new Rectangle(0, 0, 64, 64), Color.White, (float)(player.angle), origin, 1.0f, SpriteEffects.None, 0);
                        break;
                    case (Element.earth):
                        spriteBatch.Draw(earthPlayerPic, new Vector2(player.position.X, player.position.Y), new Rectangle(0, 0, 64, 64), Color.White, (float)(player.angle), origin, 1.0f, SpriteEffects.None, 0);
                        break;
                    case (Element.water):
                        spriteBatch.Draw(waterPlayerPic, new Vector2(player.position.X, player.position.Y), new Rectangle(0, 0, 64, 64), Color.White, (float)(player.angle), origin, 1.0f, SpriteEffects.None, 0);
                        break;
                }

                //Render Projectiles
                foreach (Projectile proj in projectileList)
                {
                    spriteBatch.Draw(proj.picture, new Vector2(proj.position.X, proj.position.Y), new Rectangle(0, 0, proj.picture.Width, proj.picture.Height), Color.White * proj.opacity, (float)(proj.angle + 1.5 * Math.PI), new Vector2(proj.picture.Width / 2, proj.picture.Height / 2), proj.scale, SpriteEffects.None, 0);
                }

                //Display energy
                spriteBatch.Draw(energyBarPic, new Vector2(100, 100), new Rectangle(0, 0, 64, 12), Color.White, 0.0f, origin, new Vector2(((float)player.energy / player.maxEnergy) * 3, 3), SpriteEffects.None, 0);
                spriteBatch.Draw(energyOverlayPic, new Vector2(100, 100), new Rectangle(0, 0, 64, 12), Color.White, 0.0f, origin, 3.0f, SpriteEffects.None, 0);

                //Display Health
                spriteBatch.Draw(healthBarPic, new Vector2(300, 100), new Rectangle(0, 0, 64, 12), Color.White, 0.0f, origin, new Vector2(((float)player.health / player.maxHealth) * 3, 3), SpriteEffects.None, 0);
                spriteBatch.Draw(energyOverlayPic, new Vector2(300, 100), new Rectangle(0, 0, 64, 12), Color.White, 0.0f, origin, 3.0f, SpriteEffects.None, 0);

            }

            //Display death message and highscore
            if (CurrentGameState == GameState.Dead)
            {
                spriteBatch.Draw(deathScreenPic, new Vector2(0, 0), Color.White * deathScreenTransparency);

                spriteBatch.DrawString(font, "You died.", new Vector2(SCREENWIDTH / 2 - font.MeasureString("You died.").X, 32), Color.Black, 0.0f, new Vector2(0, 0), 2.0f, SpriteEffects.None, 0);

                //Text to display the score
                string scoreText = "Kills: " + score;
                spriteBatch.DrawString(font, scoreText, new Vector2(SCREENWIDTH / 2 - font.MeasureString(scoreText).X, 96), Color.Black, 0.0f, new Vector2(0, 0), 2.0f, SpriteEffects.None, 0);
            }

            //Player chooses the element they want to start with
            if (CurrentGameState == GameState.ElementChoice)
            {
                spriteBatch.Draw(choosePic, new Vector2((SCREENWIDTH / 2 + 92 * 2) - 3 * 92 * 2, 0), new Rectangle(0, 0, 364, 92), Color.White, (float)0, new Vector2(0, 0), 2.0f, SpriteEffects.None, 0);
            }

            //Show credits
            if (CurrentGameState == GameState.Credits)
            {
                spriteBatch.DrawString(font, "- Code by Pietdagamer (www.gamejolt.com/profile/pietdagamer/335767) \n  Contact: PietdagamerGames@gmail.com", new Vector2(32, 1 * font.MeasureString("A").Y), Color.Gold, 0.0f, new Vector2(0, 0), 0.8f, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, "", new Vector2(32, 2 * font.MeasureString("A").Y), Color.Gold, 0.0f, new Vector2(0, 0), 0.8f, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, "- Background music is \"The mad Harvester\" by Vindsvept \n  (Vindsvept.com)", new Vector2(32, 3 * font.MeasureString("A").Y), Color.Gold, 0.0f, new Vector2(0, 0), 0.8f, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, "- Rock sound by hiriak (freesound.org/people/hiriak)", new Vector2(32, 5 * font.MeasureString("A").Y), Color.Gold, 0.0f, new Vector2(0, 0), 0.8f, SpriteEffects.None, 0);
            }

            //Draw the labels visible in the current gamestate
            foreach (Label label in labelList)
            {
                if (CurrentGameState == label.visibleWhen)
                {
                    spriteBatch.DrawString(font, label.text, label.position, label.color * label.opacity, 0.0f, new Vector2(0, 0), label.scale, SpriteEffects.None, 0);
                }
            }

            //Draw buttons if their gamestate is the same as the current gamestate
            foreach (Button button in buttonList)
            {
                if (button.visibleWhen == CurrentGameState)
                {
                    //Button pictures that have to be scaled up can be resized in an image editor, not here
                    spriteBatch.Draw(button.picture, button.topLeft, new Rectangle(0, 0, (int)button.size.X, (int)button.size.Y), Color.White, (float)0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);

                    //Calculate where to display the button text (skip if there is no text)
                    if (button.text != "")
                    {
                        //Calculate the X and Y coordinates of the text
                        float textX = (button.topLeft.X + button.size.X / 2) - (font.MeasureString(button.text).X / 2);
                        float textY = (button.topLeft.Y + button.size.Y / 2) - (font.MeasureString(button.text).Y / 2);

                        spriteBatch.DrawString(font, button.text, new Vector2(textX, textY), Color.Gold, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);
                    }
                }
            }

            //Display the FPS in the top right corner
            string FPS = (Math.Round((1000.0f / gameTime.ElapsedGameTime.Milliseconds), 2)).ToString();
            //spriteBatch.DrawString(font, FPS, new Vector2(SCREENWIDTH - font.MeasureString(FPS).X, 0), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
