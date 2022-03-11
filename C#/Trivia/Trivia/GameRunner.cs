using System;
using System.Collections.Generic;

namespace Trivia
{
    public class GameRunner
    {
        public static void Main(string[] args)
        {
            var players = new List<string> { "Chet", "Pat", "Sue" };
            if (Game.IsPlayable(players.Count))
            {
                Console.WriteLine("Choose a maximum score");
                var maxScore = Console.ReadLine();
                int value;
                bool minimumValue = true;
                while (!int.TryParse(maxScore, out value) && !string.IsNullOrWhiteSpace(maxScore) || minimumValue)
                {
                    if (value == 0 || value < 6)
                    {
                        Console.WriteLine("Only number above 6 accepted or enter to use default value");
                    }
                    else
                    {
                        minimumValue = false;
                        break;
                    }
                    maxScore = Console.ReadLine();
                }

                Console.WriteLine("Would you like to play with techno questions instead of rock questions ? (Y/N)");
                var key = ConsoleKey.Enter;
                while (key is not (ConsoleKey.Y or ConsoleKey.N))
                {
                    key = Console.ReadKey().Key;
                    Console.WriteLine();
                }

                bool isRockSelected = key == ConsoleKey.N;

                ConsoleKey res;
                do {

                    var aGame = new Game(maxScore == string.Empty ? 6 : value, isRockSelected);
                    aGame.Add(players);
                    aGame.Play();
                    aGame.stat();
                    Console.WriteLine("Would you like to play again ? (Y/N)");
                    res = Console.ReadKey().Key;
                    Console.Clear();
                } while (res == ConsoleKey.Y);
                
            }
            else
            {
                Console.WriteLine("This trivia only supports a minimum of 2 and up to 6 players");
            }
        }
    }
}
