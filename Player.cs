using System;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Emit;

namespace liars_dice;
public class Player
{
	bool human = false;
	Cup cup = new Cup();
	int num_dice = 5;
	public int player_num;
	public Player(bool human, int player_num)
	{
		this.human = human;
		this.player_num = player_num;
	}

	// Cup functions
	public void ShakeCup()
	{
		if (num_dice > 0)
		{
			cup.Shake();
		}
	}
	public string Reveal()
	{
		return cup.Reveal();
	}
	public int[] CupContents()
	{
		return cup.Contents();
	}
    public bool LoseDice()
    {

        for (int i = 0; i < 2; i++)
        {
            cup.RemoveDice();
            num_dice--;
            if (num_dice <= 0) return true;
        }
        return false;
    }

    Dictionary<(double, double), double> cache = new(); 
	// cache stores previously computed factorials to avoid re-calculating them
	private double factorial(double n, double m)
	{
		if (cache.ContainsKey((n, m))) return cache[(n, m)];
		if (m == 0)
		{
			cache[(n, m)] = n;
			return n;
		}
		cache[(n, m)] = factorial(n * m, m - 1);
		return cache[(n, m)];
	}
	public (int, int) Turn(int player_count, int prev_bid_rank, int prev_bid_num)
	{
		if (num_dice > 0)
		{
			int bid_rank, bid_num;
			bid_num = bid_rank = 0;
            if (human)
            {
                Console.WriteLine($"Player {player_num}: Raise [R] or Call Liar [L]?");
                string input = " ";
                while (input != "r" && input != "l")
                {
                    input = Console.ReadKey(true).KeyChar.ToString().ToLower();
                }
                if (input == "r") // raise bid
                {
					while (bid_rank < 1 || bid_rank > 6)
					{
                        Console.WriteLine("Enter number on dice/rank: ");
                        bid_rank = int.Parse(Console.ReadLine());
                    }
					while (bid_num < 1) // could add more conditions but not necessary
					{
                        Console.WriteLine("Enter number of dice: ");
                        bid_num = int.Parse(Console.ReadLine());
                    }
                    
					return (bid_rank, bid_num);
					
                }
                else // Call liar on previous bidder
                {
					return (-1, -1);
                }
            }
			else // if computer player
			{
				double k = prev_bid_num-cup.Look(prev_bid_rank);
				double prob;
				if (k > 0) // calculate probability based on combinatorics maths
				{
					double n = player_count * num_dice - (num_dice - k);
					prob = (factorial(n, n) / (factorial(k, k) * (factorial(n, n) - factorial(k, k)))) * Math.Pow(1 / 6, Math.Pow(k * (5 / 6), n - k));
				}
				else prob = 1.0;
				if (prob < 0.5)
				{
					return (-1, -1);
				}
				// computer will call higher number if possesses call die rank in cup
				// otherwise calls next highest rank possessed
				// if none possessed, calls random higher call
				// number called is random between min possible num and 1/2 number of dice in play but less than total dice in play
				bid_rank = prev_bid_rank;
				while (cup.Look(bid_rank) == 0 && bid_rank < 6) bid_rank++;
                Random random = new Random();
				int res_rank, res_num;
                if (bid_rank == prev_bid_rank)
                {
                    (res_rank, res_num) = (bid_rank, random.Next(prev_bid_num + 1, prev_bid_num + player_count * num_dice / 2));
                }
                else if (cup.Look(bid_rank) == 0) // none possessed
				{

                    (res_rank, res_num) = (Math.Min(6, bid_rank + random.Next(bid_rank + 1, bid_rank + 1 + (6 - bid_rank))),
						random.Next(1, player_count*num_dice/2));
				}
				else
				{
                    (res_rank, res_num) = (bid_rank, random.Next(1, player_count * num_dice / 2));
                }
				res_num = Math.Min(player_count * num_dice, res_num);
				return (res_rank, res_num);
                
			}

        }
		else // player is dead
		{
			Console.WriteLine($"Player {player_num} watches.");
			return (-1, -1);
		}
    }
}
