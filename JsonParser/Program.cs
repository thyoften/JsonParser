using JsonParser;

/*
 
    thyoften's JSON parser
    
    Reads in a JSON file and prints it
    Could be used also to generate JSON files by serializing it or doing something in Tree.Visit
    ( *Should* support all standard features of JSON files )
*/

if (args.Length > 0)
{
    string file = args[0];

    if (File.Exists(file))
    {
        string json = File.ReadAllText(file);

        JsonReader reader = new JsonReader(json);
        JsonViewer viewer = new JsonViewer();

        viewer.ViewJson(reader.Read());

        Console.ReadLine();
    }
    else
    {
        Console.WriteLine("File does not exist: " + file);
    }
}
else
    Console.WriteLine("No file given, exiting.");
