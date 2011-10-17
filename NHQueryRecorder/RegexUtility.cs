using System.Linq;
using System.Text.RegularExpressions;

namespace NHQueryRecorder
{
    public class RegexUtility
    {
        public static string[] GetCapturedValuesInGroup(Match match, string groupName)
        {
            return (from Capture capture in match.Groups[groupName].Captures
                    select capture.Value).ToArray();
        } 
    }
}