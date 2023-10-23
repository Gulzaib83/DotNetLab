namespace AWSLambdaAPI.Models
{
    public class ToDos
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public bool? IsCompleted { get; set; }

        public int Owner { get; set; }
    }
}
