﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using MonoGame.Extended.ViewportAdapters;
using System.Collections;
using System.Collections.Generic;

namespace Myplatformer
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Player player = new Player();

        List<Enemy> enemies = new List<Enemy>();
        public Chest goal = null;

        Camera2D camera = null;
        TiledMap map = null;
        TiledMapRenderer mapRenderer = null;
        TiledMapTileLayer collisionLayer;
        public ArrayList allCollisionTiles = new ArrayList();
        public Sprite[,] levelGrid;

        public int tileHeight = 0;
        public int levelTileWidth = 0;
        public int levelTileHeight = 0;

        public Vector2 gravity = new Vector2(0, 1500);

        Song gameMusic;


        SpriteFont arialFont;
        int score = 0;
        int lives = 3;
        Texture2D heart = null;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

      
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

       
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            player.Load(Content, this); // call load function

            arialFont = Content.Load<SpriteFont>("Arial");
            heart = Content.Load<Texture2D>("heart");

            BoxingViewportAdapter viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);

            camera = new Camera2D(viewportAdapter);
            camera.Position = new Vector2(0, GraphicsDevice.Viewport.Height);

            map = Content.Load<TiledMap>("level1");
            mapRenderer = new TiledMapRenderer(GraphicsDevice); //draws map


            gameMusic = Content.Load<Song>("scifi");
            MediaPlayer.Play(gameMusic);

            SetUpTiles();
            LoadObjects();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
           
        }

        
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            player.Update(deltaTime);

            foreach(Enemy enemy in enemies)
            {
                enemy.Update(deltaTime);
            }

            camera.Position = player.playerSprite.position - new Vector2(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);

            base.Update(gameTime);
        }//call updatefrom player class

        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //view and projection matrix
            var viewMatrix = camera.GetViewMatrix();
            var projectionMatrix = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0f, -1f);

            //begin drawing
            spriteBatch.Begin(transformMatrix: viewMatrix);

            mapRenderer.Draw(map, ref viewMatrix, ref projectionMatrix);
            //call draw function 
            player.Draw(spriteBatch);

            foreach(Enemy enemy in enemies)
            {
                enemy.Draw(spriteBatch);
            }
            goal.Draw(spriteBatch);
            // finish drawing
            spriteBatch.End();
            //drawing UI
            spriteBatch.Begin();
            spriteBatch.DrawString(arialFont, "Score: " + score.ToString(), new Vector2(20, 20), Color.Yellow);


            int loopCount = 0;
            while (loopCount < lives)
            {
                spriteBatch.Draw(heart, new Vector2(GraphicsDevice.Viewport.Width - 80 - loopCount * 40, 20), Color.White);
                loopCount++;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void SetUpTiles()
        {
            tileHeight = map.TileHeight;
            levelTileHeight = map.Height;
            levelTileWidth = map.Width;
            levelGrid = new Sprite[levelTileWidth, levelTileHeight];

            foreach (TiledMapTileLayer layer in map.TileLayers)
            {
                if (layer.Name == "collision")
                {
                    collisionLayer = layer;
                }
            }

            int columns = 0;
            int rows = 0;
            int loopCount = 0;

            while (loopCount < collisionLayer.Tiles.Count)
            {
                if (collisionLayer.Tiles[loopCount].GlobalIdentifier != 0)
                {
                    Sprite tileSprite = new Sprite();
                    tileSprite.position.X = columns * tileHeight;
                    tileSprite.position.Y = rows * tileHeight;
                    tileSprite.width = tileHeight;
                    tileSprite.height = tileHeight;
                    tileSprite.UpdateHitbox();
                    allCollisionTiles.Add(tileSprite);
                    levelGrid[columns, rows] = tileSprite;
                }

                columns++;

                if(columns == levelTileWidth)
                {
                    columns = 0;
                    rows++;
                }

                loopCount++;
            }

        
        }

        void LoadObjects()
        {
            foreach(TiledMapObjectLayer layer in map.ObjectLayers)
            {
                if (layer.Name == "enemies")
                {
                    foreach(TiledMapObject thing in layer.Objects)
                    {
                        Enemy enemy = new Enemy();
                        Vector2 tiles = new Vector2((int)(thing.Position.X / tileHeight), (int)(thing.Position.Y / tileHeight));
                        enemy.enemySprite.position = tiles * tileHeight;
                        enemy.Load(Content, this);
                        enemies.Add(enemy);
                    }
                }
                if (layer.Name == "goal")
                {
                    TiledMapObject thing = layer.Objects[0];
                    if (thing !=null)
                    {
                        Chest chest = new Chest();
                        chest.chestSprite.position = new Vector2(thing.Position.X, thing.Position.Y);
                        chest.Load(Content, this);
                        goal = chest;
                    }
                }
            }
        }
    }
}
