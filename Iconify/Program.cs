using System;

namespace Iconify
{

    internal partial class Program
    {
        static int Main(string[] args)
        {
            if(args.Length < 3) 
            {
                Console.WriteLine("The operation type, input path of image file(s), and the path to the module are required.");
                return -1;
            }

            var inputPath = args[1];
            var modPathRoot = args[2];

            switch(args[0].ToLowerInvariant())
            {
                // Example: atlas D:\GitHub\frm\bg3_mod_epic6\Icons D:\GitHub\frm\bg3_mod_epic6\DnD-Epic6
                case "atlas":
                    return CreateAtlas(inputPath, modPathRoot);
                // Example: actionresource D:\GitHub\frm\bg3_mod_epic6\Icons\E6_Logo.png D:\GitHub\frm\bg3_mod_epic6\DnD-Epic6 FeatPoint
                case "actionresource":
                    if (args.Length < 4)
                    {
                        Console.WriteLine("The operation type, input path of image file(s), the path to the module, and the name of the action resource (as specified in ActionResourceDefinitions.xml) are required.");
                        return -1;
                    }
                    return CreateActionResourceIcons(inputPath, modPathRoot, args[3]);
                default:
                    Console.WriteLine("Unknown operation type: {0}", args[0]);
                    return -1;
            }
        }
    }
}
