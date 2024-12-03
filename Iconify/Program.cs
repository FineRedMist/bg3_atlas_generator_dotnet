using System;
using System.Linq;


namespace Iconify
{
    internal partial class Program
    {
        static int Main(string[] args)
        {
            if(args.Length < 3) 
            {
                Console.WriteLine("Usage:");
                Console.WriteLine(" atlas <input path of image file(s)> <module path>");
                Console.WriteLine(" actionresource <module path> <action resource name>=<image file path> (the last argument repeated as many times as necessary).");
                return -1;
            }

            switch(args[0].ToLowerInvariant())
            {
                // Example: atlas D:\GitHub\frm\bg3_mod_epic6\Icons D:\GitHub\frm\bg3_mod_epic6\DnD-Epic6
                case "atlas":
                    {
                        var inputPath = args[1];
                        var modPathRoot = args[2];

                        return CreateAtlas(inputPath, modPathRoot);
                    }
                // Example: actionresource D:\GitHub\frm\bg3_mod_epic6\Icons\E6_Logo.png D:\GitHub\frm\bg3_mod_epic6\DnD-Epic6 FeatPoint
                case "actionresource":
                    {
                        var modPathRoot = args[1];
                        var sources = args
                            .Skip(2)
                            .Select(x => x.Split('='))
                            .Select(x => new ActionResourceIconSource { ActionResourceName = x[0], SourceFile = x[1] });
                        return CreateActionResourceIcons(modPathRoot, sources);
                    }
                default:
                    Console.WriteLine("Unknown operation type: {0}", args[0]);
                    return -1;
            }
        }
    }
}
