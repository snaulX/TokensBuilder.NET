namespace TokensAPI
{
    public enum Token
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
        /// Set to varible new value
        /// </summary>
        WRITEVAR,
        /// <summary>
        /// Create new default class
        /// </summary>
        NEWCLASS,
        /// <summary>
        /// Create new variable
        /// </summary>
        NEWVAR,
        /// <summary>
        /// Create new function
        /// </summary>
        NEWFUNC,
        /// <summary>
        /// End of block
        /// </summary>
        END,
        /// <summary>
        /// Get class
        /// </summary>
        GETCLASS,
        /// <summary>
        /// Get variable
        /// </summary>
        GETVAR,
        /// <summary>
        /// Get function
        /// </summary>
        GETFUNC,
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
        /// Get link
        /// </summary>
        GETLINK,
        /// <summary>
        /// Write value to variable by pointer
        /// </summary>
        WRITEINPOINTER,
        /// <summary>
        /// Create structure
        /// </summary>
        NEWSTRUCT,
        /// <summary>
        /// Create interface
        /// </summary>
        NEWINTERFACE,
        /// <summary>
        /// Create enumeration
        /// </summary>
        NEWENUM,
        /// <summary>
        /// Create module
        /// </summary>
        NEWMODULE,
        /// <summary>
        /// Create constructor
        /// </summary>
        NEWCONSTRUCTOR,
        /// <summary>
        /// Create attribute
        /// </summary>
        NEWATTRIBUTE,
        /// <summary>
        /// Find and get attribute
        /// </summary>
        GETATTRIBUTE,
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
        NEWEVENT,
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
        THROW,
        CALLCONSTRUCTOR,
        OVERRIDE,
        GET,
        SET,
        TYPEOF,
        CONST,
        OPERATOR,
        ASYNC,
        AWAIT,
        SWITCH,
        CASE,
        DEFAULT,
        NEWPOINTER,
        STARTBLOCK,
        /// <summary>
        /// Pre-compiler directiva
        /// </summary>
        DIRECTIVA,
        ENDCLASS,
        ENDMETHOD,
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
        ENDNAMESPACE
    }
}
