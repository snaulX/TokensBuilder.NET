# TokensBuilder.NET
 TokensBuilder - is a builder of tokens for .NET Framework  
 Wiki - https://github.com/world-of-legends/tokensbuilder-dotnet/wiki
 ## What is it?
**TokensBuilder.NET** - is a program (in common just .NET library) for build tokens set to .NET Framework program/library/Windows GUI Application.
For example file with the tokens can look like:
```
USE System
LIB SampleTokensLib
RUNFUNC Console.WriteLine "Hello World"
DIRECTIVA include YourDll.dll
DIRECTIVA version 1.0.0
DIRECTIVA outtype Exe
NEWCLASS ListExample<T> Public 
IMPLEMENTS IEnumerable<T>
ENDCLASS
COMMENT "It`s just crazy example"
MULTICOMMENT "It`s will be ignored by TokensBuilder"
```
### Project using:
* [Mono.Cecil](https://github.com/jbevain/cecil)
* [TokensAPI](https://github.com/world-of-legends/tokensbuilder-dotnet/tree/master/TokensAPI)
* .NET Framework 4.6
