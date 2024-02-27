using System.Net.NetworkInformation;

namespace liars_dice;
class Program
{
    static void Quit()
    {
        Console.WriteLine("Quitting...press any key to close console.");
        Console.ReadLine();
        Environment.Exit(1);
    }
    public static void ClearCurrentConsoleLine()
    {
        int currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLineCursor);
    }
    static void ShiftPlayerOrder(List<int> players_left, int n) 
    {
        // shifts all players to make sure correct player starts each turn
        for (int i = 0; i < n; i++)
        {
            players_left.Add(players_left[0]);
            players_left.RemoveAt(0);
        }
    }
    static void Game()
    {
        int player_count = 0;
        while (player_count < 2 || player_count > 5)
        {
            Console.WriteLine("Enter number of players (both human & computer, between 2-5 inclusive): ");
            player_count = int.Parse(Console.ReadLine());
        }
        int human_count = -1;
        while (human_count < 0 || human_count > player_count)
        {
            Console.WriteLine("Enter number of human players: ");
            human_count = int.Parse(Console.ReadLine());
        }
        Player[] players = new Player[player_count];
        for (int i = 0; i < human_count; i++)
        {
            players[i] = new Player(true, i + 1);
        }
        for (int i = human_count; i < player_count; i++)
        {
            players[i] = new Player(false, i + 1);
        }
        Console.WriteLine("------------------------\n      Game Start!\n------------------------");

        // initialise player + score tracker
        List<int> players_left = new List<int>();
        int[] rounds_won = new int[player_count];
        bool liar_called;
        for (int i = 0; i < player_count; i++)
        {
            players_left.Add(i);
            rounds_won[i] = 0;
        }
        while (players_left.Count() != 1) // game rounds loop
        {
            liar_called = false;
            Console.WriteLine("Cups have been shaken!\nPlayer's die will be revealed, press enter to reveal next player's die set");
            Console.ReadLine();
            foreach (int i in players_left) // reveal each player's dice set in turn
            {
                Player p = players[i];
                p.ShakeCup();
                Console.WriteLine($"Player {p.player_num} has die set: [{p.Reveal()}] (Press Enter to hide)");
                Console.ReadLine();
                Console.SetCursorPosition(0, Console.CursorTop - 2);
                ClearCurrentConsoleLine();
            }
            Console.WriteLine("Players have all checked hands, calling will begin.");
            int prev_bid_rank, prev_bid_num, cur_bid_rank, cur_bid_num, cur_player;
            prev_bid_rank = prev_bid_num = 0;
            cur_player = 1;
            while (!liar_called)
            {
                foreach (int i in players_left) // go through each player's turn
                {
                    (cur_bid_rank, cur_bid_num) = players[i].Turn(players_left.Count(), prev_bid_rank, prev_bid_num);
                    if (cur_bid_rank == -1) // called liar
                    {
                        liar_called = true;
                        Console.WriteLine($"Player {i + 1} calls Liar!");
                        cur_player = i + 1;
                        break;
                    }
                    if (cur_bid_num == 1) Console.WriteLine($"Player {i + 1} called 1 {cur_bid_rank}");
                    else Console.WriteLine($"Player {i + 1} called {cur_bid_num} {cur_bid_rank}s");
                    prev_bid_num = cur_bid_num;
                    prev_bid_rank = cur_bid_rank;
                }
            }
            // deal with liar call
            if (liar_called)
            {
                foreach (int i in players_left)
                {
                    Console.WriteLine($"Player {i + 1} had die set: [{players[i].Reveal()}]");
                }
                Dictionary<int, int> dice_count = new();
                for (int i = 1; i < 7; i++) dice_count[i] = 0;
                foreach (int i in players_left) // count total dice of each rank
                {
                    foreach (int d in players[i].CupContents())
                    {
                        dice_count[d]++;
                    }
                }
                if (dice_count[prev_bid_rank] == 1) Console.WriteLine($"There was 1 {prev_bid_rank}");
                else Console.WriteLine($"There were {dice_count[prev_bid_rank]} {prev_bid_rank}s");

                // initialise prev player wrapping array if necessary
                int prev_player = players_left.IndexOf(cur_player - 1)-1;
                if (prev_player == -1) prev_player = players_left.Count() - 1;
                prev_player = players_left[prev_player]+1;
                
                if (dice_count[prev_bid_rank] >= prev_bid_num) // bid was valid, remove 2 dice from calling player
                {
                    rounds_won[prev_player - 1]++;
                    Console.WriteLine($"Bid was valid, player {cur_player} loses 2 dice");

                    if (players[cur_player - 1].LoseDice())
                    {
                        Console.WriteLine($"Player {cur_player} has no dice! They are out of the game.");
                        players_left.Remove(cur_player - 1);
                        if (players_left.Count() == 1) break;
                        if (cur_player != players_left.Count() - 1) ShiftPlayerOrder(players_left, cur_player - 1);
                    }
                    else
                    {
                        Console.WriteLine($"Player {cur_player} will go first next round.");
                        ShiftPlayerOrder(players_left, cur_player - 1);
                    }
                }
                else // bid was invalid, remove 2 dice from bidding player
                {
                    rounds_won[cur_player - 1]++;
                    Console.WriteLine($"Bid was invalid, player {prev_player} loses 2 dice");
                    if (players[prev_player - 1].LoseDice())
                    {
                        Console.WriteLine($"Player {prev_player} has no dice! They are out of the game.");
                        players_left.Remove(prev_player - 1);
                        if (players_left.Count() == 1) break;
                        if (prev_player != players_left.Count() - 1) ShiftPlayerOrder(players_left, prev_player - 1);
                    }
                    else
                    {
                        Console.WriteLine($"Player {prev_player} will go first next round.");
                        ShiftPlayerOrder(players_left, prev_player - 1);
                    }

                }
                Console.WriteLine("------------------------\n       Next Round\n------------------------");
            }
        }
        Console.WriteLine($"Player {players_left[0]+1} wins!");
        for (int i = 0; i < player_count; i++)
        {
            Console.WriteLine($"Player {i + 1} won {rounds_won[i]} rounds");
        }
        Console.WriteLine("------------------------\n      Game Over!\n------------------------");
    }
    static void Main(string[] args)
    {
        bool stop = false;
        while (!stop)
        {
            Console.WriteLine("Welcome to Liar's Dice.\nOptions:\n - Start game [S]\n - Instructions[I]\n - Exit [E]");
            char input = 'a';
            while (!new string[3] { "s", "i", "e" }.Contains(input.ToString().ToLower())) input = Console.ReadKey(true).KeyChar;
            Console.WriteLine();
            switch (input)
            {
                case 's':
                    Game();
                    break;
                case 'i':
                    Console.WriteLine("Instructions: Select menu option with corresponding input key. " +
                        "\nEnter number of players (2-5 inclusive), followed by number of human players." +
                        "\nPress enter to display and again to hide each player's dice set, so you don't see other player's sets" +
                        "\nEach human player enters bid when prompted to, and loser of each round is evaluated until only 1 player is left.");
                    break;
                case 'e':
                    stop = true;
                    Quit();
                    break;
            }
        }
        
    }
}
