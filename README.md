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

A rectangle is printed in the format `<x>,<y>,<width>,<height>\n` where each number is a float printed with `.` as decimal point, if required.
The rectangles are sorted by xpath.

The html page must be a local file. It may refer to other files like `*.css`.
