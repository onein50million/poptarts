using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace pop
{
    class Dot
    {
        double value;
        int initialX, initialY;
        int x, y;
        public int height, width;
        double peak;
        static Texture2D dot;
        List<Dot> graph;
        Color color;
        public Dot(List<Dot> graph, int initialX, int initialY, double value, double peak, int height, int width, Color color)
        {
            this.initialX = initialX;
            this.initialY = initialY;
            this.value = value;
            this.peak = peak;
            this.width = width;
            this.height = height;
            x = initialX;
            y = (int)(initialY - (value/peak)*height);
            this.graph = graph;
            this.color = color;
        }
        public static void Load(Game game)
        {
            dot = game.Content.Load<Texture2D>("dot");
        }
        public void Update()
        {
            if (x < initialX - width)
            {
                graph.Remove(this);
            }
            x--;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(dot, new Rectangle(x, y, 2, 2), color);
        }
    }
}
