using System;
using System.Collections.Generic;
using System.Linq;

namespace Trivia
{
    public class Game
    {
        private readonly List<Player> _players = new();

        private readonly LinkedList<Question> _questionList = new();
        private Player _currentPlayer;
        private int _amountOfGoldToWin;
        public bool isRockSelected;
        private List<Player> leaderBoard = new();

        public Game(int amountOfGoldToWin, bool isRockSelected)
        {
            _amountOfGoldToWin = amountOfGoldToWin;
            this.isRockSelected = isRockSelected;

            List<ECategory> categories = Enum.GetValues(typeof(ECategory)).Cast<ECategory>().ToList();
            FillQuestions(categories);
        }

        /// On vérifie si la partie est possible a lancer
        public static bool IsPlayable(int numberPlayer)
        {
            return numberPlayer is >= 2 and < 7;
        }

        private void FillQuestions(List<ECategory> categoryList)
        {
            foreach (var category in categoryList)
            {
                for (var i = 0; i < 1; i++)
                {
                    _questionList.AddLast(
                        new Question(
                            i * (1 + (int)category),
                            category,
                            "Question " + (i + 1),
                            "Answer " + (i + 1)
                        )
                    );
                }
            }
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

            return false;
        }


        /// Lance le tour du joueur
        public void Play()
        {
            while (_players.Count > 0 && leaderBoard.Count != 3)
            {
                if (_currentPlayer == null)
                    _currentPlayer = _players.FirstOrDefault();

                Console.WriteLine(_currentPlayer.Name + " is the current player");

                ConsoleKey key = ConsoleKey.Enter;

                Console.WriteLine("Action : ");
                Console.WriteLine("1- Jouer 2- Quitter la partie");


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

                IncrementPlayer();
            }
            ShowLeaderBoard();
            
        }

        public void ShowLeaderBoard()
        {
            foreach (Player player in leaderBoard)
            {
                int place = leaderBoard.IndexOf(player) + 1;
                Console.WriteLine($"{place}- {player.Name} ---- Score : {player.Points}");
            }
        }

        public void Roll(int maxCases = 12)
        {
            var roll = new Random().Next(5) + 1;
            Console.WriteLine("He has rolled a " + roll);

            if (_currentPlayer.IsInPrison)
            {
                int outChance = (100 / _currentPlayer.TimeInPrison) + (_currentPlayer.TimeInARowInPrison * 10);
                int rollOut = new Random().Next(100);

                Console.WriteLine($"{_currentPlayer.Name} was in prison {_currentPlayer.TimeInPrison} times and have {outChance}% chance to escape it (Time in prison : {_currentPlayer.TimeInPrison}, Time in a row : {_currentPlayer.TimeInARowInPrison})");
                Console.WriteLine($"Roll {rollOut}");

                if (outChance < rollOut)
                {
                    Console.WriteLine(_currentPlayer.Name + " is not getting out of prison");
                    _currentPlayer.TimeInARowInPrison++;
                    _currentPlayer.WillQuitPrison = false;
                    return;
                }

                _currentPlayer.WillQuitPrison = true;
                _currentPlayer.IsInPrison = false;
                _currentPlayer.TimeInARowInPrison = 0;
                Console.WriteLine(_currentPlayer.Name + " is getting out of prison");

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
                {
                    UseJoker();
                    return;
                }
            }
            var random = new Random().Next(9);
            if (IsWinner(random))
                AddToLeaderBoard();
        }

        public void AddToLeaderBoard()
        {
            leaderBoard.Add(_currentPlayer);
            _players.Remove(_currentPlayer);
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
        }

        /// Répond à une question choisi en fonction la catégorie
        private void AskQuestion()
        {
            var category = (_currentPlayer.IsInPrison && !_currentPlayer.WillQuitPrison)
                ? _currentPlayer.QuestionInPrison
                : _currentPlayer.GetCategory(isRockSelected);

            _currentPlayer.LHistorique.Add(category);

            Console.WriteLine("The category is " + category);

            if (_questionList.Count(x => x.answeredBy == 0) <= _players.Count * 2) // si on a pas au moins 2 questions par joueur
            {
                Console.WriteLine("Deck ran out of questions! Refueling!");

                List<ECategory> categories = Enum.GetValues(typeof(ECategory)).Cast<ECategory>().ToList();
                FillQuestions(categories);
            }

            Question findQuestion = _questionList.FirstOrDefault(q => q.category == category && q.answeredBy == 0);
            if (findQuestion == null)
            {
                Console.WriteLine("Question not found");
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
            return _currentPlayer.DidWin(_amountOfGoldToWin); ;
        }

        /// Retourne vrai si la réponse est fausse 
        private bool WrongAnswer()
        {
            _currentPlayer.Streak = 0;

            Console.WriteLine("Question was incorrectly answered");
            Console.WriteLine(_currentPlayer.Name + " was sent to the prison");
            if (!_currentPlayer.IsInPrison)
            {
                _currentPlayer.IsInPrison = true;
                _currentPlayer.TimeInPrison++;
            }

            if (_currentPlayer.TimeInARowInPrison == 0)
            {
                _currentPlayer.QuestionInPrison = SelectQuestionInPrison();
                Console.WriteLine("Vous venez de choisir la catégorie " + _currentPlayer.QuestionInPrison.ToString());
            }

            IncrementPlayer();
            return false;
        }

        private ECategory SelectQuestionInPrison()
        {
            Console.WriteLine("Vous vous dirigez en prison, veuillez choisir la catégorie de votre prochaine question :");
            Console.WriteLine("Pop (A), Science (B), Sport (C), Rock/Techno (D), Rap (E), Philosophie (F), Geographie (G), People (H) ou Littérature (I)");
            var key = ConsoleKey.Enter;
            while (key is not (ConsoleKey.A or ConsoleKey.B or ConsoleKey.C or ConsoleKey.D or ConsoleKey.E or ConsoleKey.F or ConsoleKey.G or ConsoleKey.H or ConsoleKey.I))
            {
                key = Console.ReadKey().Key;
                Console.WriteLine();
            }

            switch (key)
            {
                case ConsoleKey.A:
                    return ECategory.Pop;
                case ConsoleKey.B:
                    return ECategory.Science;
                case ConsoleKey.C:
                    return ECategory.Sport;
                case ConsoleKey.D:
                    return isRockSelected ? ECategory.Rock : ECategory.Techno;
                case ConsoleKey.E:
                    return ECategory.Rap;
                case ConsoleKey.F:
                    return ECategory.Philosophy;
                case ConsoleKey.G:
                    return ECategory.Geography;
                case ConsoleKey.H:
                    return ECategory.People;
                case ConsoleKey.I:
                    return ECategory.Litterature;
                default:
                    return ECategory.Science;
            }
        }


        private void IncrementPlayer()
        {
            if (_players.Count == 0)
                return;

            _currentPlayer = _players.FirstOrDefault(p => p.Id > _currentPlayer.Id);
            if (_currentPlayer == null)
                _currentPlayer = _players.FirstOrDefault();

        }

        public void stat()
        {
            foreach (var player in leaderBoard)
            {
                Console.WriteLine(player.Name);
                Console.WriteLine("Rock :" + player.LHistorique.FindAll(s => s == ECategory.Rock).Count());
                Console.WriteLine("Science :" + player.LHistorique.FindAll(s => s == ECategory.Science).Count());
                Console.WriteLine("Pop :" + player.LHistorique.FindAll(s => s == ECategory.Pop).Count());
                Console.WriteLine("Sport :" + player.LHistorique.FindAll(s => s == ECategory.Sport).Count());
                Console.WriteLine("Techno :" + player.LHistorique.FindAll(s => s == ECategory.Techno).Count());
            }

            foreach (var question in _questionList.GroupBy(x => x.category))
            {
                Console.WriteLine(question.Key + " total number of question is " + question.Count() + " out of " + _questionList.Count);
            }
        }
    }
}
