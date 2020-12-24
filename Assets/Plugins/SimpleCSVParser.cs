/*
 * A simple CSV parser taking in csv data and parses it into a List of List of strings.
 * 
 * Adapted from the CSVReader.cs from https://wiki.unity3d.com/index.php/CSVReader.
 * I made the following changes:
 * 1. I Removed the parts relating to Monobehaviours. The only part I want is the csv parsing.
 * 2. Changed it so that it builds a list of list of strings rather than a 2d array, the code for this is much simpler, I think.
 * 3. The original version was adding 1 to the height and width of the 2d array, meaning the parsed data had a full extra row and column for seemingly no reason.
 * 4. The initial string.split in the original version was only using a \n as the splitter, which was causing my data to have a \r at the end of each row, which the row parsing would
 *		interpret as a separate cell, causing the parsed data to have an extra blank item in each row. By having the split use both \n and \r\n as the splitters (and using RemoveEmptyEntries 
 *		to get rid of the blank trailing row) everything is correctly parsed the way I want now.
 */

using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SimpleCSVParser
{
	static public List<List<string>> ParseCSV(string csvData)
	{
		List<List<string>> parsedCsv = new List<List<string>>();

		string[] splitters = { "\n", "\r\n" };
		string[] rows = csvData.Split(splitters, System.StringSplitOptions.RemoveEmptyEntries);
		foreach (string row in rows)
		{
			parsedCsv.Add(ParseCsvRow(row));
		}

		return parsedCsv;
	}

	static public List<string> ParseCsvRow(string row)
	{
		List<string> parsedRow = new List<string>();
		string regexPattern = @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)";
		foreach (Match m in Regex.Matches(row, regexPattern, RegexOptions.ExplicitCapture))
		{
			parsedRow.Add(m.Groups[1].Value);
		}

		return parsedRow;
	}
}
