using System;
using System.Collections.Generic;
using TokensAPI;

namespace TokensBuilder
{
    interface TokensTemplate
    {
        /// <summary>
        /// Parse expression by template
        /// </summary>
        /// <param name="expression">Getted expression</param>
        /// <param name="expression_end">After getting expression has expression end (true) or open block (false)</param>
        /// <returns>Getted expression equals current template</returns>
        bool Parse(TokensReader expression, bool expression_end);

        /// <summary>
        /// Run expression by this template
        /// </summary>
        /// <param name="expression">Expression for run</param>
        /// <returns>List of errors when was throwed in running</returns>
        List<TokensError> Run(TokensReader expression);
    }
}
