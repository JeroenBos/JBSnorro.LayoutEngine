using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
public static class IJavaScriptExecutorExtensions
{
	/// <summary>
	/// JS source code of a function `getXPath` that gets the XPath of a given xml node.
	/// </summary>
	private static readonly string GetXPathJSSourceCode = @"
function getXPath(node) {
    var comp, comps = [];
    var parent = null;
    var xpath = '';
    var getPos = function(node) {
        var position = 1, curNode;
        if (node.nodeType == Node.ATTRIBUTE_NODE) {
            return null;
        }
        for (curNode = node.previousSibling; curNode; curNode = curNode.previousSibling) {
            if (curNode.nodeName == node.nodeName) {
                ++position;
            }
        }
        return position;
     }

    if (node instanceof Document) {
        return '/';
    }

    for (; node && !(node instanceof Document); node = node.nodeType == Node.ATTRIBUTE_NODE ? node.ownerElement : node.parentNode) {
        comp = comps[comps.length] = {};
        switch (node.nodeType) {
            case Node.TEXT_NODE:
                comp.name = 'text()';
                break;
            case Node.ATTRIBUTE_NODE:
                comp.name = '@' + node.nodeName;
                break;
            case Node.PROCESSING_INSTRUCTION_NODE:
                comp.name = 'processing-instruction()';
                break;
            case Node.COMMENT_NODE:
                comp.name = 'comment()';
                break;
            case Node.ELEMENT_NODE:
                comp.name = node.nodeName;
                break;
        }
        comp.position = getPos(node);
    }

    for (var i = comps.length - 1; i >= 0; i--) {
        comp = comps[i];
        xpath += '/' + comp.name;
        if (comp.position != null) {
            xpath += '[' + comp.position + ']';
        }
    }
    return xpath;
}
";


	/// <summary>
	/// Executes a JS function for each element on the page, and returns the result by the element's XPath.
	/// </summary>
	/// <typeparam name="T"> The return type of the function invoked on each element. </typeparam>
	/// <param name="jsExecutor"> The JS environment in which to invoke the function. </param>
	/// <param name="functionJSSourceCode"> The source code of the function to invoke. </param>
	/// <param name="functionName"> The name of the function to invoke. </param>
	/// <param name="converter">A function that converts results of the function to <typeparamref name="T"/>. </param>
	/// <returns> Returns the converted return values for each element in the JS environment, keyed by the element's XPath. </returns>
	public static IReadOnlyDictionary<string, T> ForeachXPaths<T>(this IJavaScriptExecutor jsExecutor, string functionJSSourceCode, string functionName, Func<object, T> converter)
	{
		string js = GetXPathJSSourceCode +
			functionJSSourceCode +
		@"
var all = document.getElementsByTagName(""*"");

var result = {};
for (var i = 0, max = all.length; i < max; i++)
{
    var xpath = getXPath(all[i]);
    var value = " + functionName + @"(all[i]);

    result[xpath] = value;

    // console.log('{');
    // console.log('    \""' + xpath + '\"",');
    // console.log('    \""' + value + '\""');
    // console.log('}' + (i + 1 == max ? ',' : ''));
}
return result;";

		var result = (IReadOnlyDictionary<string, object>)jsExecutor.ExecuteScript(js);
		var castResult = new Dictionary<string, T>(result.Select(kvp => KeyValuePair.Create(kvp.Key, converter(kvp.Value))));
		return castResult;

	}

	/// <summary>
	/// NOT TESTED. Executes the specified function on the specified element.
	/// </summary>
	/// <param name="driver"> The JS environment to invoke the function in. </param>
	/// <param name="functionName"> The name of the function on the element to invoke. </param>
	/// <param name="this"> The element to invoke the function on. </param>
	/// <param name="arguments"> Additional arguments to be passed to the function. </param>
	private static object ThisExecuteScript(this IJavaScriptExecutor driver, string functionName, IWebElement @this, params object[] arguments)
	{
		var allArguments = new List<object>();
		allArguments.Add(@this);
		allArguments.AddRange(arguments);

		var parameterList = string.Join(",", allArguments.Select((_, i) => $"arg{i}"));
		string js = @$"
            function executeOnSelf({parameterList}) 
            {{ 
                return arg0.{functionName}({parameterList.Substring("arg0,".Length)}; 
            }}
            return executeOnSelf(...arguments);";
		return driver.ExecuteScript(js, allArguments.ToArray());
	}

	private static string GetElementByXPathJSSourceCode(string path)
	{
		//Not really implemented but interesting nonetheless
		return $"document.evaluate({path}, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue;";
	}
}
