namespace TokensBuilder.Errors
{
    public class InvalidOutTypeError : TokensError
    {
        public InvalidOutTypeError(uint _line, string outType) : base(_line, $"Not correct output type {outType}")
        {
        }
    }
}
