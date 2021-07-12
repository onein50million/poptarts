using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
//using System.Drawing;

namespace pop
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    /// 
    public enum Job {None = -1, Noble, Tradesman, Merchant, Labourer, Farmer, Miner};

    public enum CommunityType {None = -1, Farm, Mine, City, Castle};
    public enum Good { None = -1, Food, Wood, Trade};
    public class Game1 : Game
    {
        public String[] town_names = { "Shipton", "Ubbin Falls", "Nerton", "Penkurth", "Irragin ", "King's Watch", "Ballymena", "Bailymena", "Ballinamallard", "Knife's Edge" };
        private GraphicsDeviceManager graphics;
        SpriteBatch worldSpriteBatch;
        SpriteBatch interfaceSpriteBatch;
        List<Community> communities = new List<Community>();
        private SpriteFont date;
        Effect test_effect;
        public static Random random = new Random();
        int frames = 0;
        int day = 1;
        int year = 0;
        int speed = 4;
        int[] speeds = new int[] { 1, 10, 20, 30, 60 };
        List<Dot> foodGraph = new List<Dot>();
        List<Dot> woodGraph = new List<Dot>();
        List<Dot> tradeGraph = new List<Dot>();
        Vector2 mouseStart = new Vector2(0, 0);
        Vector2 pan = new Vector2(0, 0);
        Vector2 temporaryPan = new Vector2(0,0);
        
        float scale = 1f;
        float oldScale = 1f;
        MouseState mouseState;
        MouseState lastMouseState;
        KeyboardState keyState;
        KeyboardState lastKeyState;
        int deltaScrollWheel = 0;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            Window.IsBorderless = false;
            this.IsMouseVisible = true;
            graphics.ApplyChanges();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            lastKeyState = Keyboard.GetState();
            keyState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            lastMouseState = Mouse.GetState();
            mouseStart = mouseState.Position.ToVector2();



            for (int i = 0; i < 1; i++)
            {
                Community community = new Community(CommunityType.City, town_names[random.Next(0, town_names.Length - 1)]);
                community.Initialize(this);
                communities.Add(community);

            }

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            worldSpriteBatch = new SpriteBatch(GraphicsDevice);
            interfaceSpriteBatch = new SpriteBatch(GraphicsDevice);
            Community.Load(this);
            Dot.Load(this);
            date = Content.Load<SpriteFont>("population");
            test_effect = Content.Load<Effect>("test");

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            lastKeyState = keyState;
            lastMouseState = mouseState;
            keyState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            deltaScrollWheel = mouseState.ScrollWheelValue - lastMouseState.ScrollWheelValue;

            if (mouseState.RightButton == ButtonState.Pressed && lastMouseState.RightButton == ButtonState.Released)
            {
                mouseStart = mouseState.Position.ToVector2();
                temporaryPan = pan;
            }
            else if (mouseState.RightButton == ButtonState.Pressed)
            {
                pan = temporaryPan + mouseStart - mouseState.Position.ToVector2();
            }
            if (mouseState.RightButton == ButtonState.Released && lastMouseState.RightButton == ButtonState.Pressed)
            {
                pan = temporaryPan;
                pan += mouseStart - mouseState.Position.ToVector2();
            }

            oldScale = scale;
            if(deltaScrollWheel > 0)
            {
                scale *= 1.1f;
            }
            else if(deltaScrollWheel < 0)
            {
                scale *= 0.9f;
            }
            scale = Math.Min(3f,Math.Max(0.2f, scale));
            
            if (keyState.IsKeyDown(Keys.Right) && lastKeyState.IsKeyUp(Keys.Right))
            {
                speed = Math.Min(speeds.Length-1, speed + 1);
            }
            if (keyState.IsKeyDown(Keys.Left) && lastKeyState.IsKeyUp(Keys.Left))
            {
                speed = Math.Max(0, speed - 1);
            }
            if (keyState.IsKeyDown(Keys.Up) && lastKeyState.IsKeyUp(Keys.Up))
            {
                for(int i = 0; i <communities[0].slices.Count; i++)
                {
                    if(communities[0].slices[i].job == Job.Farmer)
                    {
                        communities[0].slices[i].goodProduction[Good.Food] += 0.001;
                    }
                }
            }
            if (keyState.IsKeyDown(Keys.Down) && lastKeyState.IsKeyUp(Keys.Down))
            {
                for (int i = 0; i < communities[0].slices.Count; i++)
                {
                    if (communities[0].slices[i].job == Job.Farmer)
                    {
                        communities[0].slices[i].goodProduction[Good.Food] -= 0.001;
                    }
                }
            }

            if (frames == 0)
            {
//                Console.WriteLine("Day {0}, Year {1}", day, year);
                foreach (Community community in communities)
                {
                    community.Update(gameTime);
                }
                day++;
                if (day > 365)
                {
                    year++;
                    day = 0;

                }
                foodGraph.Add(new Dot(foodGraph, 300, 450, communities[0].market[Good.Food].cost, 100, 400, 100, Color.Green));
                woodGraph.Add(new Dot(woodGraph, 200, 450, communities[0].market[Good.Wood].cost, 100, 400, 100, Color.Brown));
                tradeGraph.Add(new Dot(tradeGraph, 100, 450, communities[0].market[Good.Trade].cost, 100, 400, 100, Color.Gold));

                for (int i = 0; i < foodGraph.Count; i++)
                {
                    foodGraph[i].Update();

                }

                for (int i = 0; i < woodGraph.Count; i++)
                {
                    woodGraph[i].Update();
                }

                for (int i = 0; i < tradeGraph.Count; i++)
                {
                    tradeGraph[i].Update();
                }
            }
            base.Update(gameTime);
            frames = (frames + 1)% speeds[speed];

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            float deltaScale = scale - oldScale;
            Vector2 transformedMouse = Vector2.Transform(mouseState.Position.ToVector2(), Matrix.CreateTranslation(pan.X, pan.Y, 0f) * Matrix.CreateScale(1/scale));
            Console.WriteLine(transformedMouse);
            pan += transformedMouse * deltaScale;
            worldSpriteBatch.Begin(transformMatrix: Matrix.CreateScale(scale) * Matrix.CreateTranslation(-pan.X, -pan.Y, 0f),effect:test_effect);

            foreach (Community community in communities)
            {
                community.Draw(worldSpriteBatch);
            }
            worldSpriteBatch.End();
            interfaceSpriteBatch.Begin();
            interfaceSpriteBatch.DrawString(date, String.Format("Day: {0}, Year: {1}, speed: {2}", day, year, speeds[speed]), new Vector2(10, 10), Color.Black);
            foreach (Dot dot in foodGraph)
            {
                dot.Draw(interfaceSpriteBatch);
            }
            foreach (Dot dot in woodGraph)
            {
                dot.Draw(interfaceSpriteBatch);
            }
            foreach (Dot dot in tradeGraph)
            {
                dot.Draw(interfaceSpriteBatch);
            }


            interfaceSpriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
