using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class Utils 
{
    public static string PathSimplify(string path)
    {
        Regex regex = new Regex(@"[^\\/]+(?<!\.\.)[\\/]\.\.[\\/]");

        while (true)
        {
            string newPath = regex.Replace(path, string.Empty);
            if (newPath.Equals(path)) break;
            path = newPath;
        }

        return path;
    }
}
