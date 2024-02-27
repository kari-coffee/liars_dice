using System;
namespace liars_dice;
public class Dice
{
	int value;
	public Dice()
	{
		value = Roll();
	}
	public int Value()
	{
		return value;
	}
	public int Roll()
	{
		Random random = new Random();
		return random.Next(1, 7);
	}
}
