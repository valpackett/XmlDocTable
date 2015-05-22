# XmlDocTable

A *very simple* API docs generator for C#, based on [Roslyn].  
Currently outputs [LaTeX] tables, but you can easily change the format.  
Works on both Mono and Microsoft .NET.

[Roslyn]: https://github.com/dotnet/roslyn
[LaTeX]: https://en.wikibooks.org/wiki/LaTeX

## Usage (Mono)

Generate docs for XmlDocTable itself (`.` is the directory):

```bash
$ nuget restore
$ xbuild
$ mono XmlDocTableCli/bin/Debug/XmlDocTableCli.exe . > generated.tex
$ lualatex docs.tex
```

See `docs.tex` for an example file that includes the generated docs using `\input`.

## License

Copyright 2014-2015 Greg V <greg@unrelenting.technology>  
Available under the ISC license, see the `COPYING` file
