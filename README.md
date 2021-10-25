# JBSnorro.LayoutEngine

A standalone commandline tool with a single purpose: given an html page, print to stdout the `boundingClientRectangle` of all HTML elements on the page.
Example usage:

```bash
layoutengine --dir "/path/to/dir/containing/an/index.html"
```

or on windows:

```bash
layoutengine.exe --dir "C:\\path\\to\\dir\\containing\\an\\index.html"
```

The `--dir` argument need not be absolute, `--file` must be though. At least one of each must be provided.

A rectangle is printed in the format `<tagname>,<x>,<y>,<width>,<height>\n` where each number is a float printed with `.` as decimal point, if required.
The rectangles are sorted by xpath, and printed after the line `########## RECTANGLES INCOMING (V1) ##########`. Before that, Selenium output is printed, which I failed to redirect.
Skipping until that line can be achieved for example like
```bash
layoutengine --dir "/path/to/dir/containing/an/index.html" | sed "0,/########## RECTANGLES INCOMING/d"
```

The html page must be a local file. It may refer to other files like `*.css`.

### Notes to developer:
When publishing, make sure the Configuration is set to Release, otherwise all kinds of errors can occur.

Publishing the linux configuration doesn't work in the VS UI. 
Use this bash command instead:
```
cd LayoutEngine; dotnet publish -c Release -r linux-x64 -o publish
```

(or with runtime `win10-x64` from linux also seems to work)

When testing from Linux, please run this instead:
```
dotnet test --configuration CI --runtime linux-x64
```
