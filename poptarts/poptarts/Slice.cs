using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using poptarts;

namespace pop
{

    class Slice
    {
        public Dictionary<Good, double> needs = new Dictionary<Good, double>(); //per person
        public Dictionary<Good, double> metNeeds = new Dictionary<Good, double>(); //ratio
        public Dictionary<Good, double> producedGoods = new Dictionary<Good, double>();
        public Dictionary<Good, double> wastedGoods = new Dictionary<Good, double>();

        public double population;
        public double money;
        public Job job;
        public double dailyBirthRate = 0;
        public double dailyDeathRate = 0;

        private const double BirthRate = 60; //annual birth rate per 1000 +- 50%
        private const double DeathRate = 30; //annual birth rate per 1000 +- 50%
        private const double variance = 0.5;

        public Dictionary<Good, double> goodProduction = new Dictionary<Good, double>();
        private double deathModifier = 1;
        private double birthModifer = 1;
        private double starvationSeverity = 0;
        private double coldness = 0;
        private double happiness = 1;
        private double produceRevenue = 0; //profit of last days production
        private Community community;


        public Slice(int population, Job job, Community community)
        {
            this.population = population;
            this.job = job;
            this.community = community;

            goodProduction.Add(Good.Food, 0.3);
            goodProduction.Add(Good.Wood, 0.23);
            goodProduction.Add(Good.Trade, 0.08);
            foreach (Good good in Enum.GetValues(typeof(Good)))
            {
                if(good != Good.None)
                {
                    producedGoods.Add(good, 0);
                    wastedGoods.Add(good, 0);
                    metNeeds.Add(good, 0);
                }

            }
            switch (job)
            {
                case Job.Farmer:
                    money = population * 1.0;

                    needs.Add(Good.Food, 0.2);//600
                    needs.Add(Good.Wood, 0.05);//150
                    needs.Add(Good.Trade, 0.01);//30
                    break;
                case Job.Labourer:
                    money = population * 1.0;

                    needs.Add(Good.Food, 0.2); //200
                    needs.Add(Good.Wood, 0.05);//50
                    needs.Add(Good.Trade, 0.01);//10
                    break;
                case Job.Tradesman:
                    money = population * 1.0;

                    needs.Add(Good.Food, 0.15);
                    needs.Add(Good.Wood, 0.2); //100
                    needs.Add(Good.Trade, 0.02);//10
                    break;
                case Job.Miner:
                    money = population * 1.0;

                    needs.Add(Good.Food, 0.2);
                    needs.Add(Good.Wood, 0.05); 
                    needs.Add(Good.Trade, 0.01);
                    break;
                case Job.Noble:
                    money = population * 2000.0;

                    needs.Add(Good.Food, 0.1);
                    needs.Add(Good.Wood, 0.01);
                    needs.Add(Good.Trade, 0.05);
                    break;
                default:
                    break;
            }
        }
        public void Update(GameTime gameTime)
        {
            ProduceGoods();
            UseGoods();
            SellGoods();
            UpdatePopulation();
            ResetGoods();
            //Console.WriteLine("Food: {0:0.0} Wood: {1:1.1}",ownedGoods[Good.Food],ownedGoods[Good.Wood]);

        }
        public double GetNeed(Good good)
        {
            return needs[good] * population;
        }
        public double GetNeedCost()
        {
            double needCost = 0; //Cost to buy all needs
            for (Good i = 0; i < (Good)needs.Count; i++)
            {
                needCost += GetNeed(i) * community.market[i].cost;
            }
            return needCost;
        }
        public double GetSingleNeed(double time)
        {
            double sum = 0;
            for (Good need = 0; need < (Good)needs.Count; need++)
            {
                sum += community.market[need].cost * time;
            }
            return sum;
        }

        public void ProduceGoods()
        {
            Good good = Good.None;
            double productionModifier = 0;

            switch (job)
            {

                case Job.Farmer:
                    good = Good.Food;
                    break;
                case Job.Labourer:
                    good = Good.Wood;
                    break;
                case Job.Tradesman:
                    good = Good.Trade;
                    break;
                case Job.Noble:
                    money += 10 * population;
                    break;
                case Job.Miner:
                    money += Helper.Logistic(population,10000.0,0,0.01);
                    break;
                default:
                    break;
            }
            if (good != Good.None && producedGoods[good] < population * goodProduction[good] * 10 && population>0)
            {

                //double costSum = 0;
                //for (Good i = 0; i < (Good)community.market.Count; i++)
                //{
                //    if(i != good)
                //    {
                //        costSum += community.market[i].cost;
                //    }
                //}
                //double averageCost = costSum / (community.market.Count-1);
                //double relativeCost = community.market[good].cost / averageCost;
                ////sigmoid
                //productionModifier += relativeCost / (1 + Math.Abs(relativeCost));
                //Console.WriteLine("r = {0:0.0}, sigmoid = {1:0.00}", relativeCost, relativeCost / (1 + Math.Abs(relativeCost)));


                //double projectedProfitMargin = 0;
                //double profitSigmoid = 0;
                //if (produceRevenue > 0)
                //{
                //    projectedProfitMargin = produceRevenue - GetNeedCost() / produceRevenue; //projected profit

                //    profitSigmoid = ((projectedProfitMargin) / (1 + Math.Abs(projectedProfitMargin)))*0.25; //s shaped function

                //}
                //productionModifier += profitSigmoid;
                //productionModifier += 0.1 * happiness - 0.00;
                //productionModifier += 0.2 * metNeeds[Good.Food] - 0.00;
                //productionModifier += 0.1 * metNeeds[Good.Wood] - 0.00;
                //productionModifier

                //productionModifier += Math.Min(0.5,projectedProfitMargin);
                productionModifier = Math.Max(-0.9, productionModifier); //always produce a little
                productionModifier = Math.Min(3, productionModifier); //don't produce outrageous amounts
                
                double producedGood = Math.Pow(population,0.99) * goodProduction[good] * (1 + productionModifier);
                this.producedGoods[good] += producedGood;
                //Console.WriteLine("{0} modifier {1:0.00}, produced {2:0.00} profitRatio {3:0.0}", good.ToString(), productionModifier,producedGoods,projectedProfitMargin);

            }
        }
        public void UseGoods()
        {
            for(Good iGood = 0; iGood < (Good)needs.Count; iGood++)
            {

                double need = GetNeed(iGood);
                double ownedGood = producedGoods[iGood];

                //Owned goods used for needs first (ie farmers eat before selling food)
                producedGoods[iGood] -= Math.Min(need, producedGoods[iGood]);
                need -= Math.Min(need, ownedGood);

                //Buy goods when they don't have enough
                double boughtGoods = community.BuyGood(iGood, need, this);
                need -= boughtGoods;
                metNeeds[iGood] = 1 - need/GetNeed(iGood);
                switch (iGood)
                {
                    case Good.Food:
                        starvationSeverity =
                            Math.Max(0, starvationSeverity
                            + (1 - metNeeds[iGood]) * 2
                            - (Math.Pow(2, starvationSeverity / 100) - 1)
                            - 1);
                        break;
                    case Good.Wood:
                        coldness += (1 - metNeeds[iGood])*2 - 1;
                        coldness = Math.Max(0, coldness);
                        coldness = Math.Min(30, coldness);
                        break;
                    case Good.Trade:
                        happiness = metNeeds[iGood];
                        break;
                    default:
                        break;
                }


            }
        }

        public void SellGoods()
        {

            produceRevenue = 0;
            for (int i = 0; i < producedGoods.Count; i++)
            {
                if (producedGoods[(Good)i] > 0)
                {
                    double soldGoods = community.SellGood((Good)i, producedGoods[(Good)i], this);
                    producedGoods[(Good)i] -= soldGoods;
                    produceRevenue += soldGoods * community.market[(Good)i].cost;

                }
            }
        }
        public void UpdatePopulation()
        {
            double projectedProfitMargin = 0;
            if (produceRevenue > GetNeedCost())
            {
                projectedProfitMargin = produceRevenue - GetNeedCost() / produceRevenue; //projected profit ratio
            }

            birthModifer = Math.Max(0, 40 * (metNeeds[Good.Food]+metNeeds[Good.Wood])/2 + projectedProfitMargin);
            deathModifier = Math.Max(0, starvationSeverity * 100 + coldness*10);
            //Console.WriteLine("{0} Cold: {1:0.0}", job.ToString(),coldness);

            dailyBirthRate =
                ((BirthRate + birthModifer) * (population / 1000.0) / 365.0)
                * (Game1.random.NextDouble() + variance);
            dailyDeathRate =
                ((DeathRate + deathModifier) * (population / 1000.0) / 365.0)
                * (Game1.random.NextDouble() + variance);

            population += dailyBirthRate - dailyDeathRate;
            //money *= 0.1; //lose track of money or something
        }
        public void ResetGoods() //sets all owned goods to 0;
        {
            for(Good i = 0; i < (Good)producedGoods.Count; i++)
            {
                wastedGoods[i] = producedGoods[i];
                Console.WriteLine("Wasted: {0:0.0} {1}", wastedGoods[i], i.ToString());
                producedGoods[i] = 0;
            }
        }

    }
}
