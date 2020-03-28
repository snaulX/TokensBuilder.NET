using TokensAPI;

namespace TokensBuilder.Errors
{
    public class InvalidTokenError : TokensError
    {
        public InvalidTokenError(uint _line, string _message) : base(_line, _message)
        {
        }

        public InvalidTokenError(uint _line, TokenType token) : base(_line, $"Invalid token {token}")
        {
        }
    }
}
