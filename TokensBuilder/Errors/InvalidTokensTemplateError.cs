namespace TokensBuilder.Errors
{
    public class InvalidTokensTemplateError : TokensError
    {
        public InvalidTokensTemplateError(uint _line, string _message) : base(_line, _message)
        {
        }
    }
}
