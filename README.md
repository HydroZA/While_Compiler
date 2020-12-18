# While Compiler
Created as part of a Compilers and Formal Language course during my MSc. It's written in C# 9 targeting .NET 5.0.

# WhileLang
WhileLang is a imperative language featuring a single type, i32. 

## Running
`cd While_Compiler/While_Compiler`

`dotnet run [args] <file>`


## Grammar
### Arithmetic Expressions
```
<aexp> ::== <num>
<aexp> ::== <var>
<aexp> ::== ( <aexp> )
<aexp> ::== <aop>
<aop> ::== <aexp> + <aexp>
<aop> ::== <aexp> - <aexp>
<aop> ::== <aexp> * <aexp>
<aop> ::== <aexp> / <aexp>
<aop> ::== <aexp> % <aexp>
```
### Boolean Expressions
```
<bexp> ::== true
<bexp> ::== false
<bexp> ::== ( <bexp> )
<bexp> ::== <bop>
<bop> ::== <aexp> == <aexp>
<bop> ::== <aexp> < <aexp>
<bop> ::== <aexp> > <aexp>
<bop> ::== <aexp> <= <aexp>
<bop> ::== <aexp> >= <aexp>
<bop> ::== <aexp> != <aexp>
<bexp> ::== <and>
<bexp> ::== <or>
<and> ::== <bexp> && <bexp>
<or> ::== <bexp> || <bexp>
```
### Statements
```
<statement> ::== if <bexp> then <block> else <block> <statement> ::== skip
<statement> ::== <var> := <aexp>
<statement> ::== while <bexp> do <block>
<statement> ::== for <var> := <aexp> upto <aexp> do <block>
<statement> ::== read <var>
<statement> ::== write <str>
<statement> ::== write <var>
```

### Compound Statements
```
<statements> ::== <statement>
<statements> ::== <statement> ; <statements>
```

### Block
`<block> ::== { <statements> }`

## Quirks
**The final statement of every block must NOT end with a semi-colon**

```
{
  write "test!";
  read x
}
```





