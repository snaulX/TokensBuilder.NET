namespace TokensAPI
{
    public enum Token : byte
    {
        /// <summary>
        /// Token does not exist. Just none
        /// </summary>
        NULL,
        /// <summary>
        /// Create ability to seacrh in linked namespaces
        /// </summary>
        USE,
        /// <summary>
        /// Import all classes and methods and modules from other assembly (in compilation-time)
        /// </summary>
        INCLUDE,
        /// <summary>
        /// Set to varible new value
        /// </summary>
        WRITEVAR,
        /// <summary>
        /// Create new default class
        /// </summary>
        CLASS,
        /// <summary>
        /// Create new variable
        /// </summary>
        FIELD,
        /// <summary>
        /// Create new function
        /// </summary>
        METHOD,
        /// <summary>
        /// End of block
        /// </summary>
        END,
        /// <summary>
        /// Run function
        /// </summary>
        RUNFUNC,
        /// <summary>
        /// Cycle 'while'
        /// </summary>
        WHILE,
        /// <summary>
        /// Cycle 'for'
        /// </summary>
        FOR,
        /// <summary>
        /// Cycle 'foreach'
        /// </summary>
        FOREACH,
        /// <summary>
        /// Break out of body of cycle or 'switch'
        /// </summary>
        BREAK,
        /// <summary>
        /// Go to next iteration of cycle
        /// </summary>
        CONTINUE,
        /// <summary>
        /// Return value and exit from function
        /// </summary>
        RETURN,
        /// <summary>
        /// Operator 'if'
        /// </summary>
        IF,
        /// <summary>
        /// Operator 'else'
        /// </summary>
        ELSE,
        /// <summary>
        /// Operator 'else if'
        /// </summary>
        ELIF,
        /// <summary>
        /// Return to label
        /// </summary>
        GOTO,
        /// <summary>
        /// Create label at this moment
        /// </summary>
        LABEL,
        /// <summary>
        /// Return iteration object and not exit from function
        /// </summary>
        YIELD,
        /// <summary>
        /// Create structure
        /// </summary>
        STRUCT,
        /// <summary>
        /// Create interface
        /// </summary>
        INTERFACE,
        /// <summary>
        /// Create enumeration
        /// </summary>
        ENUM,
        /// <summary>
        /// Create module
        /// </summary>
        MODULE,
        /// <summary>
        /// Create constructor
        /// </summary>
        CONSTRUCTOR,
        /// <summary>
        /// Use attribute
        /// </summary>
        ATTRIBUTE,
        /// <summary>
        /// Find and get constructor
        /// </summary>
        GETCONSTRUCTOR,
        /// <summary>
        /// Write OpCode to block
        /// </summary>
        OPCODEADD,
        /// <summary>
        /// Create new event type
        /// </summary>
        EVENT,
        /// <summary>
        /// Get event 
        /// </summary>
        GETEVENT,
        /// <summary>
        /// Create block 'try'
        /// </summary>
        TRY,
        /// <summary>
        /// Create block 'catch'
        /// </summary>
        CATCH,
        /// <summary>
        /// Create implementation of interface in the class block
        /// </summary>
        IMPLEMENTS,
        /// <summary>
        /// Operator throw
        /// </summary>
        THROW,
        OVERRIDE,
        /// <summary>
        /// Create getter of property
        /// </summary>
        GET,
        /// <summary>
        /// Create setter of property
        /// </summary>
        SET,
        /// <summary>
        /// Operator typeof
        /// </summary>
        TYPEOF,
        /// <summary>
        /// Create constant
        /// </summary>
        CONST,
        /// <summary>
        /// Create operator
        /// </summary>
        OPERATOR,
        /// <summary>
        /// Create async method
        /// </summary>
        ASYNC,
        /// <summary>
        /// Operator await
        /// </summary>
        AWAIT,
        /// <summary>
        /// Operator awitch
        /// </summary>
        SWITCH,
        CASE,
        DEFAULT,
        STARTBLOCK,
        /// <summary>
        /// Pre-compiler directiva
        /// </summary>
        DIRECTIVA,
        /// <summary>
        /// Get all classes, methods and etc from other tokens
        /// </summary>
        LIB,
        /// <summary>
        /// Write code on low-level programming language
        /// </summary>
        VIRTUAL,
        /// <summary>
        /// Set creating namespace
        /// </summary>
        NAMESPACE,
        /// <summary>
        /// It`s just single commentary
        /// </summary>
        COMMENT,
        /// <summary>
        /// It`s just multiline commentary
        /// </summary>
        MULTICOMMENT,
        /// <summary>
        /// Mark breakpoint
        /// </summary>
        BREAKPOINT,
        /// <summary>
        /// Operator for addition '+'
        /// </summary>
        ADD,
        /// <summary>
        /// Operator for substraction '-'
        /// </summary>
        SUB,
        /// <summary>
        /// Operator for divise '/'
        /// </summary>
        DIV,
        /// <summary>
        /// Operator for multiply '*'
        /// </summary>
        MUL,
        /// <summary>
        /// Operator for modulo '%'
        /// </summary>
        MOD,
        /// <summary>
        /// Operator greater '>'
        /// </summary>
        CGT,
        /// <summary>
        /// Operator lower
        /// </summary>
        CLT,
        /// <summary>
        /// Operator equals '=='
        /// </summary>
        CEQ,
        /// <summary>
        /// Operator and '&&'
        /// </summary>
        AND,
        /// <summary>
        /// Operator or '||'
        /// </summary>
        OR,
        /// <summary>
        /// Operator and '|'
        /// </summary>
        XOR,
        /// <summary>
        /// Operator and '!'
        /// </summary>
        NOT,
        /// <summary>
        /// Operator sizeof
        /// </summary>
        SIZEOF
    }
}
