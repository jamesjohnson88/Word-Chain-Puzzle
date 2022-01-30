#region Setup

using System.Diagnostics;

var dirPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"../../../"));
var textPath = $@"{dirPath}/assets/dictionary.txt";
var wordBank = File.ReadLines(textPath)
    .Where(x => !string.IsNullOrWhiteSpace(x))
    .Distinct()
    .ToList();

var solvedChains = new List<List<string>>();

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Please enter a start word: ");
 var startWord = Console.ReadLine()
     ?.Replace(" ", "")
     .ToLowerInvariant();

while (!wordBank.Contains(startWord))
{
    Console.WriteLine("You entered an invalid word, please choose another: ");
    startWord = Console.ReadLine()
        ?.Replace(" ", "")
        .ToLowerInvariant();
}

Console.WriteLine("Now, enter an end word: ");
 var endWord = Console.ReadLine()
     ?.Replace(" ", "")
     .ToLowerInvariant();

while (!wordBank.Contains(endWord))
{
    Console.WriteLine("You entered an invalid word, please choose another: ");
    endWord = Console.ReadLine()
        ?.Replace(" ", "")
        .ToLowerInvariant();
}

#endregion
#region Display Logic

var sw = new Stopwatch();
sw.Start();
var solvedValue = RecursivelyNavigateChain(startWord, endWord);
sw.Stop();

Console.WriteLine(solvedValue);
Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms");
Console.WriteLine("\nPress any key to exit.");
Console.ReadLine();

#endregion

string RecursivelyNavigateChain(string startWord, string targetWord)
{
    Console.WriteLine("Running...");
    
    var successfulPaths = SolveChain(startWord, endWord);

    var closestMatch = successfulPaths
        .OrderBy(x => x.Count)
        .FirstOrDefault();

    return closestMatch != null && closestMatch.Any() ? string.Join(" -> ", closestMatch) : "Could Not Solve";
}

IEnumerable<List<string>> SolveChain(string starterWord, string targetWord)
{
    var changeMaps = GenerateSingleChangeMaps(starterWord);

    foreach (var map in changeMaps)
    {
        var wordsInChain = new List<string> {starterWord, map};
        var maxRecursion = (starterWord.Length > targetWord.Length ? starterWord.Length : targetWord.Length) * 2;
        
        RecursivelySolveChain(map, targetWord, ref wordsInChain, maxRecursion);
        
        if (wordsInChain.LastOrDefault()!.Equals(targetWord))
        {
            solvedChains.Add(wordsInChain);
        }
    }
    
    return solvedChains;
}

void RecursivelySolveChain(string starterWord, string targetWord, ref List<string> wordTracker, int maxRecursion)
{
    if (maxRecursion == 0 || wordTracker.LastOrDefault() == targetWord)
    {
        return;
    }

    var list = wordTracker;
    var changeMaps = GenerateSingleChangeMaps(starterWord).ToList();
    
    // TODO - check maps for target word before bothering to loop through
    
    var validMaps = changeMaps.Where(x => !list.Contains(x)); 
    // TODO - use deviations to reduce number of branches we need to traverse
    //&& IsExpectedDeviations(list[^2].ToArray(), x.ToArray(), 2));
    
    foreach (var map in validMaps)
    {
        wordTracker.Add(map);
        RecursivelySolveChain(map, targetWord, ref wordTracker, maxRecursion-1);

        if (wordTracker.LastOrDefault() != targetWord)
        {
            continue;
        }
        
        // todo - group after all processing to avoid duplication
        var solvedRaw = new List<string>();
        
        // Console.WriteLine("RAW:");
        // Console.WriteLine($"{string.Join(" -> ", wordTracker)}");
        // Console.WriteLine("becomes...");

        var mutableList = wordTracker.ToList();
        
        foreach (var word in wordTracker)
        {
            var index = mutableList.IndexOf(word);
            var lastIndex = mutableList.LastIndexOf(mutableList.LastOrDefault()!);
                
            if (index == 0)
            {
                solvedRaw.Add(word);
            } 
            else if (index == lastIndex)
            {
                if (IsSingleDigitDifference(word.ToArray(), mutableList[lastIndex - 1].ToArray()))
                {
                    solvedRaw.Add(word);
                }
                else
                {
                    mutableList.Remove(word);
                }
            }
            else
            {
                if (IsSingleDigitDifference(word.ToArray(), mutableList[index-1].ToArray()) 
                    && IsSingleDigitDifference(word.ToArray(), mutableList[index+1].ToArray()))
                {
                    solvedRaw.Add(word);
                }
                else
                {
                    mutableList.Remove(word);
                }
            }
        }
            
        //Console.WriteLine($"{string.Join(" -> ", solvedRaw)}");

        if (solvedRaw.LastOrDefault() == targetWord)
        {
            solvedChains.Add(RemoveUnnecessaryLinks(solvedRaw));
        }
    }
}

List<string> RemoveUnnecessaryLinks(IEnumerable<string> links)
{
    var linkArray = links.ToArray();
    var length = linkArray.Length - 1;
    
    var reverseChain = new List<string>();
    
    for (var i = length; i >= 0; i--)
    {
        if (i == length || i == 0)
        {
            reverseChain.Add(linkArray[i]);
        }
        else
        {
            var preceding = linkArray[i - 1];
            var succeeding = linkArray[i + 1];

            if (!IsSingleDigitDifference(preceding.ToArray(), succeeding.ToArray()))
            {
                reverseChain.Add(linkArray[i]);
            }
        }

    }

    reverseChain.Reverse();
    
    return reverseChain.ToList();
}

IEnumerable<string> GenerateSingleChangeMaps(string? startWord)
{
    var startArray = startWord.ToArray();
    var examples = wordBank!
        .Where(x => IsSingleDigitDifference(startArray, x.ToArray()))
        .ToList();

    return examples;
}

bool IsExpectedDeviations(char[] startArray, char[] targetArray, int expectedDeviations)
{
    char[] longestWord;
    char[] shortestWord;
    var deviation = 0;

    if (startArray.Length == targetArray.Length)
    {
        longestWord = targetArray;
        shortestWord = startArray;
    }
    else
    {
        if (startArray.Length > targetArray.Length)
        {
            longestWord = startArray;
            shortestWord = targetArray;
        }
        else
        {
            longestWord = targetArray;
            shortestWord = startArray;
        }

        var buffer = shortestWord;
        for (var i = shortestWord.Length-1; i < longestWord.Length-1; i++)
        {
            buffer = buffer.Append(' ').ToArray();
        }

        shortestWord = buffer;
    }
    
    for (var i = 0; i < longestWord.Length; i++)
    {
        if (longestWord[i] != shortestWord[i])
        {
            deviation++;
        }

        if (deviation > expectedDeviations)
        {
            return false;
        }
    }

    return deviation == expectedDeviations;
}

bool IsSingleDigitDifference(char[] startArray, char[] targetArray)
{
    char[] longestWord;
    char[] shortestWord;
    var deviation = 0;

    if (startArray.Length == targetArray.Length)
    {
        longestWord = targetArray;
        shortestWord = startArray;
    }
    else
    {
        if (startArray.Length > targetArray.Length)
        {
            longestWord = startArray;
            shortestWord = targetArray;
        }
        else
        {
            longestWord = targetArray;
            shortestWord = startArray;
        }

        var buffer = shortestWord;
        for (var i = shortestWord.Length-1; i < longestWord.Length-1; i++)
        {
            buffer = buffer.Append(' ').ToArray();
        }

        shortestWord = buffer;
    }
    
    for (var i = 0; i < longestWord.Length; i++)
    {
        if (longestWord[i] != shortestWord[i])
        {
            deviation++;
        }

        if (deviation > 1)
        {
            return false;
        }
    }

    return deviation == 1;
}