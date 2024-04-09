namespace RacerBooksAPI.Models
{
    public class FirebaseErrorModel
    {
        public FError error { get; set; }
    }

    public class FError
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<Error> errors { get; set; }
    }
}
