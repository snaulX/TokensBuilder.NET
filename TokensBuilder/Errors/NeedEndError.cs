namespace TokensBuilder.Errors
{
    public class NeedEndError : TokensError
    {
        public NeedEndError(uint _line, string _message) : base(_line, _message)
        {
        }
    }
}
