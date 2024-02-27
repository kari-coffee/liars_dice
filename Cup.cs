using System;
namespace liars_dice;
public class Cup
{
	List<Dice> dice = new List<Dice>();
	int num_dice = 5;
	public Cup()
	{
		for (int i = 0; i < num_dice; i++)
		{
			dice.Add(new Dice());
		}
	}
	public void Shake()
	{
		for (int i = 0; i < num_dice; i++)
		{
			dice[i].Roll();
		}
	}
	public void RemoveDice()
	{
		dice.RemoveAt(0);
		num_dice--;
	}
	public string Reveal()
	{
		string output = "";
		for (int i = 0; i < num_dice; i++)
		{
			output += dice[i].Value().ToString() + " ";
		}
		return output.Substring(0, output.Length-1);
	}
	public int[] Contents()
	{
		int[] output = new int[num_dice];
		for (int i = 0; i < num_dice; i++)
		{
			output[i] = dice[i].Value();
		}
		return output;
	}
	public int Look(int check_rank)
	{
		int res = 0;
		for (int i = 0; i < num_dice; i++)
		{
			if (dice[i].Value() == check_rank) { res++; }
		}
		return res;
	}
}
