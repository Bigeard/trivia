namespace Trivia {
    public class Question
    {

        public int id;
        public ECategory category;
        public string question;
        public string answer;
        public int answeredBy;

        public Question(int id, ECategory category, string question, string answer)
        {
            this.id = id;
            this.category = category;
            this.question = question;
            this.answer = answer;
        }
    }
}