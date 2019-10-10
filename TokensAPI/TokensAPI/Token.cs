using System;

namespace TokensAPI
{
    public abstract class Token
    {
        public abstract Identifer Parse(params Identifer[] identifers);
    }
}
