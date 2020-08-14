namespace TokensBuilder.Errors
{
    public class InvalidTypeError : TokensError
    {
        public InvalidTypeError(uint _line, string _message) : base(_line, _message)
        {
        }
    }
}
