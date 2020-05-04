namespace TokensBuilder.Errors
{
    public class InvalidMethodError : TokensError
    {
        public InvalidMethodError(uint _line, string _message) : base(_line, _message)
        {
        }
    }
}
