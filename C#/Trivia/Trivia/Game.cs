﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace Trivia
{
    public class Game
    {
        private readonly List<Player> _players = new();
        
        private readonly LinkedList<string> _popQuestions = new();
        private readonly LinkedList<string> _scienceQuestions = new();
        private readonly LinkedList<string> _sportsQuestions = new();
        private readonly LinkedList<string> _rockQuestions = new();
        private readonly LinkedList<string> _technoQuestions = new();

        private readonly LinkedList<Question> _questionList = new();
        private Player _currentPlayer;
        private int _amountOfGoldToWin;
        private readonly bool _isRockSelected;
        private bool _hasWinner;

        public Game(int amountOfGoldToWin)
        {

            _amountOfGoldToWin = amountOfGoldToWin;

            Console.WriteLine("Would you like to play with techno questions instead of rock questions ? (Y/N)");
            var key = ConsoleKey.Enter;
            while (key is not (ConsoleKey.Y or ConsoleKey.N))
            {
                key = Console.ReadKey().Key;
                Console.WriteLine();
            }

            List<int> categoryList = Enum.GetValues(typeof(ECategory))
                .Cast<ECategory>()
                .Select(v => (int) v)
                .ToList();
            
            if (key == ConsoleKey.Y)
                categoryList.RemoveAt(3);
            else
            {
                _isRockSelected = true;
                categoryList.RemoveAt(4);
            }
                
            foreach (int category in categoryList)
            {
                for (var i = 0; i < 50; i++)
                {
                    _questionList.AddLast(
                        new Question(
                            i * (1 + category), 
                            category, 
                            "Question " + (i + 1),
                            "Answer " + (i + 1)
                        )
                    );
                }
            }
        }

        /// On vérifie si la partie est possible a lancer
        public static bool IsPlayable(int numberPlayer)
        {
            return numberPlayer is >= 2 and < 7;
        }

        public void Add(List<string> playersName)
        {
            for (var i = 0; i < playersName.Count; i++)
            {
                var player = new Player(i + 1, playersName[i]);
                _players.Add(player);

                Console.WriteLine(player.Name + " was added");
                Console.WriteLine("He is player number " + player.Id);
            }
        }

        public bool RemovePlayer()
        {
            if (_currentPlayer == null)
                _currentPlayer = _players.FirstOrDefault();

            Console.WriteLine("Le joueur " + _currentPlayer.Name + " a décidé de quitter la partie.");
            _players.Remove(_currentPlayer);
            Console.WriteLine("Le joueur a été supprimé. ------------------------------------");
            if (_players.Count == 0)
                Environment.Exit(0);

            IncrementPlayer();
            return false;
        }


        /// Lance le tour du joueur
        public void Play()
        {
            _hasWinner = false;
            while (!_hasWinner)
            {
                if (_currentPlayer == null)
                    _currentPlayer = _players.FirstOrDefault();

                Console.WriteLine(_currentPlayer.Name + " is the current player");

                Console.WriteLine("Action : ");
                Console.WriteLine("1- Jouer 2- Quitter la partie");
                ConsoleKey key = ConsoleKey.Enter;

                while (key != ConsoleKey.D1 && key != ConsoleKey.D2)
                {
                    key = Console.ReadKey().Key;
                    Console.WriteLine();
                }

                switch (key)
                {
                    case ConsoleKey.D1:
                        Roll();
                        break;
                    case ConsoleKey.D2:
                        RemovePlayer();
                        break;
                }
            }
        }

        public void Roll(int maxCases = 12)
        {
            var roll = new Random().Next(5) + 1;
            Console.WriteLine("He has rolled a " + roll);

            if (_currentPlayer.IsInPrison)
            {
                if (roll % 2 == 0)
                {
                    _currentPlayer.WillQuitPrison = true;
                    Console.WriteLine(_currentPlayer.Name + " is getting out of prison yet");
                    return;
                }

                Console.WriteLine(_currentPlayer.Name + " is not getting out of prison");
                _currentPlayer.WillQuitPrison = false;
            }

            _currentPlayer.Position += roll;
            if (_currentPlayer.Position > maxCases - 1)
                _currentPlayer.Position -= maxCases;

            Console.WriteLine(_currentPlayer.Name + "'s new location is " + _currentPlayer.Position);
            AskQuestion();
            if (!_currentPlayer.IsJokerUsed)
            {
                Console.WriteLine("Voulez-vous utiliser votre Joker ?(Y/N)");
                ConsoleKey res = ConsoleKey.Enter;
                while (res != ConsoleKey.Y && res != ConsoleKey.N)
                {
                    res = Console.ReadKey().Key;
                    Console.WriteLine();
                }
                if (res == ConsoleKey.Y)
                    UseJoker();
            }

            var random = new Random().Next(9);
            _hasWinner = IsWinner(random);
        }

        public bool IsWinner(int random)
        {
            switch (random)
            {
                case 7:
                    return WrongAnswer();
                default:
                    return WasCorrectlyAnswered();
            }
        }

        public void UseJoker()
        {
            Console.WriteLine(_currentPlayer.Name + " is using its joker!");
            _currentPlayer.IsJokerUsed = true;
            IncrementPlayer();
        }

        /// Répond à une question choisi en fonction la catégorie
        private void AskQuestion()
        {
            var category = _currentPlayer.GetCategory(_isRockSelected);
            _currentPlayer.LHistorique.Add(category);
            Console.WriteLine("The category is " + category);
            Question findQuestion = _questionList.FirstOrDefault(q => q.category == (int) category && q.answeredBy == 0);
            if (findQuestion == null)
            {
                Console.WriteLine("Not found question");
                return;
            }

            _questionList.Find(findQuestion).Value.answeredBy = _currentPlayer.Id;
            Console.WriteLine(findQuestion.question);
        }

        /// Retourne vrai si la réponse est bonne
        private bool WasCorrectlyAnswered()
        {
            if (_currentPlayer.IsInPrison)
            {
                if (_currentPlayer.WillQuitPrison)
                {
                    _currentPlayer.IsInPrison = false;
                    Console.WriteLine("Answer was correct!!!!");
                    Console.WriteLine(_currentPlayer.Name + " now has " + ++_currentPlayer.Points + " Gold Coins.");
                    return _currentPlayer.DidWin(_amountOfGoldToWin);
                }
            }

            _currentPlayer.Streak++;
            _currentPlayer.Points += _currentPlayer.Streak;

            Console.WriteLine("Answer was correct!!!!");
            Console.WriteLine(_currentPlayer.Name + " now has " + _currentPlayer.Points + " Gold Coins.");

            var hasWinner = _currentPlayer.DidWin(_amountOfGoldToWin);
            IncrementPlayer();
            return hasWinner;        }

        /// Retourne vrai si la réponse est fausse 
        private bool WrongAnswer()
        {
            _currentPlayer.Streak = 0;

            Console.WriteLine("Question was incorrectly answered");
            Console.WriteLine(_currentPlayer.Name + " was sent to the prison");
            _currentPlayer.IsInPrison = true;

            IncrementPlayer();
            return false;
        }


        private void IncrementPlayer()
        {
            int currentPlayer = _players.IndexOf(_currentPlayer) + 1;
            if (currentPlayer == _players.Count)
                currentPlayer = 0;

            _currentPlayer = _players[currentPlayer];
        }

        public void stat()
        {
            foreach (var player in _players)
            {
                Console.WriteLine(player.Name);
                Console.WriteLine("Rock :" + player.LHistorique.FindAll(delegate (ECategory s) { return s == ECategory.Rock; }).Count());
                Console.WriteLine("Science :" + player.LHistorique.FindAll(delegate (ECategory s) { return s == ECategory.Science; }).Count());
                Console.WriteLine("Pop :" + player.LHistorique.FindAll(delegate (ECategory s) { return s == ECategory.Pop; }).Count());
                Console.WriteLine("Sport :" + player.LHistorique.FindAll(delegate (ECategory s) { return s == ECategory.Sport; }).Count());
                Console.WriteLine("Techno :" + player.LHistorique.FindAll(delegate (ECategory s) { return s == ECategory.Techno; }).Count());
            }

        }
    }

}
