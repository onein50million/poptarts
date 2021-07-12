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
    public struct marketGood
    {
        public double quantity, cost, maximum, demand, supply;
        private double[] costHistory;
        public marketGood(double quantity, double cost, double maximum)
        {
            this.quantity = quantity;
            this.cost = cost;
            this.maximum = maximum;
            demand = 0;
            supply = 0;
            costHistory = new double[10];
            for (int i = 0; i < costHistory.Length; i++)
            {
                costHistory[i] = cost;
            }
        }
        public void DayTick()
        {
            double movedCost = costHistory[costHistory.Length-1];
            costHistory[costHistory.Length - 1] = cost;
            for(int i = costHistory.Length-2; i >= 0; i--)
            {
                double temp = costHistory[i];
                costHistory[i] = movedCost;
                movedCost = temp;
            }
            //Console.WriteLine(string.Join(", ", costHistory));
        }
        public double AverageCost()
        {
            double sum = 0;
            for(int i = 0; i< costHistory.Length; i++)
            {
                sum += costHistory[i];
            }
            return sum / costHistory.Length;
        }
    }
    class Community
    {
        public List<Slice> slices = new List<Slice>();
        public Dictionary<Good, marketGood> market = new Dictionary<Good, marketGood>(); // Good, (quantity, cost, max, total bought that day , total sold that day)
        public CommunityType type;
        public string name;
        static SpriteFont text;
        static Texture2D town;
        public Point position;
        
        public Community(CommunityType type, String name)
        {
            this.type = type;
            this.name = name;
            //slices.Add(new Slice(100, Job.Noble, this));
            slices.Add(new Slice(200, Job.Tradesman, this));
            slices.Add(new Slice(1000, Job.Labourer, this));
            slices.Add(new Slice(3000, Job.Farmer, this));
            //slices.Add(new Slice(90, Job.Miner, this));
            market.Add(Good.Food, new marketGood(0.0, 10.0, 3000)); 
            market.Add(Good.Wood, new marketGood(0.0, 10.0, 3000));
            market.Add(Good.Trade, new marketGood(0.0, 50.0, 3000));
        }
        public void Update(GameTime gameTime)
        {
            ResetMarket();
            foreach(Slice slice in slices)
            {
                //Console.WriteLine("Available Food: {0:0.00}", market[Good.Food].quantity);
                slice.Update(gameTime);
            }
            MarketUpdate();
        }
        public void Initialize(Game game)
        {
            position = new Point(Game1.random.Next(-1000, 1000), Game1.random.Next(-1000,1000));
            double sum = 0;
            foreach(Slice slice in slices)
            {
                sum += slice.GetNeed(Good.Wood);
            }
            //Console.WriteLine("SUM: {0:0.0}",sum);
            //Food 885
            //Wood 251
        }
        public static void Load(Game game)
        {
            town = game.Content.Load<Texture2D>("town");
            text = game.Content.Load<SpriteFont>("population");
            
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            String data = String.Format("'{0}' - Total Pop: {1:0} Total Wealth: {2:0}\n", name,GetTotalPopulation(), GetTotalMoney());
            for(Good i =0; i < (Good)market.Count; i++)
            {
                data += String.Format("{0} - $: {1:0.0} Supply: {2:0.0} Demand {3:0.0}\n",i, market[i].cost, market[i].supply, market[i].demand);
            }
            foreach(Slice slice in slices)
            {
                data += String.Format("{6} {0:0} - Change: {1:0.00}, Money: {2:0.00} Food: {3:0.0} Wood: {4:0.0} Trade: {5:0.0}\n",slice.population, slice.dailyBirthRate-slice.dailyDeathRate, slice.money,slice.metNeeds[Good.Food] * 100, slice.metNeeds[Good.Wood]*100,slice.metNeeds[Good.Trade]*100, slice.job.ToString());
            }
            spriteBatch.DrawString(text, data, new Vector2(position.X + 40, position.Y), Color.Black);
            spriteBatch.Draw(town, new Rectangle((int)position.X, (int)position.Y, town.Width, town.Height), Color.White);                
        }

        //returns the amount purchased
        public double BuyGood(Good good, double quantity, Slice slice)
        {

            marketGood targetGood = market[good]; //structs are passed by value and compiler is nice and tells you that you shouldn't edit it

            double affordableGoods = slice.money / targetGood.cost;
            double availableGoods = Math.Min(affordableGoods, targetGood.quantity);
            double boughtGoods = Math.Min(quantity, availableGoods);
            targetGood.demand += Math.Min(quantity, affordableGoods);

            targetGood.quantity = targetGood.quantity - boughtGoods;
            slice.money -= boughtGoods * targetGood.cost;

            market[good] = targetGood;
            return boughtGoods;
        }
        public double SellGood(Good good, double quantity, Slice slice)
        {
            //double demandedGoods = market[good].demand;

            //double sellQuantity = Math.Min(demandedGoods, quantity);

            marketGood targetGood = market[good];
            targetGood.supply += quantity;

            double marketSpace = Math.Max(0, targetGood.maximum - targetGood.quantity);
            slice.money += Math.Min(quantity, marketSpace) * targetGood.cost;

            double soldGoods = Math.Min(quantity, marketSpace);
            targetGood.quantity += soldGoods;

            market[good] = targetGood;
            return soldGoods; //goods actually sold
        }
        public double GetTotalPopulation()
        {
            double sum = 0;
            foreach (Slice slice in slices)
            {
                sum += slice.population;
            }
            return sum;
        }
        public double GetTotalMoney()
        {
            double sum = 0;
            foreach (Slice slice in slices)
            {
                sum += slice.money;
            }
            return sum;
        }
        public void ResetMarket()
        {
            for(Good i = 0; i < (Good)market.Count; i++)
            {
                marketGood currentGood = market[i];
                currentGood.demand = 0;
                currentGood.supply = 0;
                market[i] = currentGood;

            }
        }
        public void MarketUpdate()
        {
            for(int i = 0; i < market.Count; i++)
            {
                marketGood currentGood = market[(Good)i];
                currentGood.DayTick();

                double marketActivity = currentGood.demand + currentGood.supply;
                if(currentGood.demand != 0 && currentGood.supply != 0)
                {
                    double proportional_error = currentGood.demand/currentGood.supply;
                    double proportional_constant = 1;
                    //double integral_error =
                    //double integral_constant = 1.0;
                    //double derivative_error =
                    //double derivative_constant = 1.0;
                    currentGood.cost += (proportional_error - 1.0) * proportional_constant;
                    //currentGood.cost *= Math.Clamp(Math.Pow(currentGood.demand/currentGood.supply,0.02), 0.99, 1.01);
                    currentGood.cost = Math.Max(currentGood.cost, 0.1);
                    //Console.WriteLine("{0} cost: {1:0.000}, supply {2:0.00}, demand {3:0.00}", ((Good)i).ToString(), currentGood.cost, currentGood.supply, currentGood.demand);
                    market[(Good)i] = currentGood;
                    Console.WriteLine("{0} avg $: {1:0.0}", (Good)i, currentGood.AverageCost());
                }
                else {
                    //Console.WriteLine("No activity for {0}", (Good)i);
                }

            }

        }
    }
}
