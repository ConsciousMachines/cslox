# cslox
C# implementation of Jlox from Bob Nystrom's "Crafting Interpreters"


Ideas for future Lox additions: <as of completing part II>
- add eval
- add lists & dicts
- make it pure OO: replace C# object with a JloxObject, similar to PyObject in Python
- redesign environments, functional style.
- use Resolver for static analysis only, not for env
- add features from Philip Guo's CPython internals
	- and also https://devguide.python.org/exploring/ - see bottom of page for resources
	- see also https://leanpub.com/insidethepythonvirtualmachine
- for clox later: add enough library functions to make it self-hosting
- add types (an extra field on the JloxObject, and then a type checker pass). start w explicit types, then inferred
- instead of adding an extra bool to represent if  fn is an initializer, just add to static analysis that we can't define fns called "init". or use dunderscore methods :3
- see if it's possible to remove the super . rule from the grammar, esp with a redesigned env. 
