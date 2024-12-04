using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BG3Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Iconify
{

    internal partial class Program
    {
        enum IconType
        {
            Standard,
            Highlight,
            Missing,
            Used
        }

        struct ActionResourceIconSource
        {
            public string SourceFile;
            public string ActionResourceName;
        }

        /// <summary>
        /// From https://mod.io/g/baldursgate3/r/implementing-custom-action-resources-with-impui
        /// </summary>
        /// <param name="modPathRoot">The path to the root of the mod.</param>
        /// <param name="sources">Mapping of action resource name to source file to use.</param>
        /// <returns></returns>
        private static int CreateActionResourceIcons(string modPathRoot, IEnumerable<ActionResourceIconSource> sources)
        {
            /* From the documentation:
                Icons are in BC6H/BC7 format
            
               There are 4 different types of action resource icon:
                1st type -Keyboard icon.Appears on top of hotbar, spellbook, reactions and in tooltips. Has 3 alternative versions for missing (it's been taken away),
                    used (it's been expended) and unavailable.Size 48x48.
                2nd type - Keyboard CC / level up icon.Appears in character creation or level up when a progression / passive grants an action resource.A progression
                    description is also required. Size 128x128.
                3rd type - Controller front icon.Appears on the left hand side of the action resource listing in both action radial "Cost" and "Action Resources"
                    section.Size 80x80.
                4th type - Controller resource icon.Appears on the right hand side of the action resource listing in both action radial "Cost" and "Action Resources"
                    section. Has 3 OPTIONAL alternative versions for missing(it's been taken away), used (it's been expended) and unavailable.If you don't add a version
                    for these, it'll look the same done for most of the base icons except action point, bonus action and movement.Size 44x64.
            
               Locations:
                1st type - Keyboard icon:
                    Mods\MODNAME\GUI\Assets\Shared\Resources\ACTIONRESOURCENAME.DDS
                    Mods\MODNAME\GUI\Assets\Shared\Resources\Highlight\ACTIONRESOURCENAME.DDS
                    Mods\MODNAME\GUI\Assets\Shared\Resources\Missing\ACTIONRESOURCENAME.DDS
                    Mods\MODNAME\GUI\Assets\Shared\Resources\Used\ACTIONRESOURCENAME.DDS
                 2nd type - Keyboard CC/level up icon:
                    Mods\MODNAME\GUI\Assets\CC\icons_resources\ACTIONRESOURCENAME.DDS
                 3rd type - Controller front icon:
                    Mods\MODNAME\GUI\Assets\ActionResources_c\Icons\ACTIONRESOURCENAME.DDS
                 4th type - Controller resource icon:
                    Mods\MODNAME\GUI\Assets\ActionResources_c\Icons\Resources\ACTIONRESOURCENAME.DDS
                 OPTIONAL
                    Mods\MODNAME\GUI\Assets\ActionResources_c\Icons\Resources\Highlight\ACTIONRESOURCENAME.DDS
                    Mods\MODNAME\GUI\Assets\ActionResources_c\Icons\Resources\Missing\ACTIONRESOURCENAME.DDS
                    Mods\MODNAME\GUI\Assets\ActionResources_c\Icons\Resources\Used\ACTIONRESOURCENAME.DDS
            */
            var modName = Path.GetFileName(modPathRoot);
            var guiBase = "Mods\\" + modName + "\\GUI";

            List<Task> tasks = new List<Task>();
            SortedDictionary<string, int> imageMap = new SortedDictionary<string, int>();

            foreach (var source in sources)
            {
                // First pass just duplicate the image to the right size to all locations.
                using Image<Rgba32> standardImage = Image.Load<Rgba32>(source.SourceFile); // Standard image source

                foreach (var setting in ActionResourceIconSettings)
                {
                    var image = standardImage; // Use the standard image if we can't find and override image.
                    ImageTransform? transform = null; // The transform to apply if we can't find a specific image.

                    if (setting.Type != IconType.Standard)
                    {
                        // Check to see if there is an override image for this icon type.
                        var alternateImage = GetImagePath(source.SourceFile, setting.Type);
                        if (File.Exists(alternateImage))
                        {
                            image = Image.Load<Rgba32>(alternateImage);
                        }
                        else // If not, apply a default transform.
                        {
                            transform = GetTransform(setting.Type);
                        }
                    }

                    string partialPath = string.Format(setting.IconPath, source.ActionResourceName);
                    string iconPath = Path.Combine(modPathRoot, guiBase, partialPath);
                    imageMap[partialPath] = setting.SideLength;
                    tasks.Add(image.GenerateIcon(setting.SideLength, Path.GetDirectoryName(iconPath)!, source.ActionResourceName, transform));
                }
            }

            SaveActionResourceImageMap(imageMap, Path.Combine(modPathRoot, guiBase, "metadata.lsf.lsx")!);
            Task.WaitAll(tasks.ToArray());

            return 0;
        }

        private static string GetImagePath(string standardImagePath, IconType type)
        {
            if(type == IconType.Standard)
            {
                return standardImagePath;
            }
            return Path.Combine(Path.GetDirectoryName(standardImagePath)!, Path.GetFileNameWithoutExtension(standardImagePath) + "_" + type.ToString() + Path.GetExtension(standardImagePath));
        }

        private static string FixActionResourceIconPathName(string partialPath)
        {
            string result = partialPath.Replace('\\', '/');
            result = Path.ChangeExtension(result, ".png");
            return result;
        }

        private static void SaveActionResourceImageMap(SortedDictionary<string, int> imageMap, string imageMapLsxFilePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(imageMapLsxFilePath)!);
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.Encoding = Encoding.UTF8;
            using XmlWriter writer = XmlWriter.Create(imageMapLsxFilePath, xmlWriterSettings);
            writer.WriteStartDocument();
            using (var save = writer.WriteScopedElement("save"))
            {
                using (var version = writer.WriteScopedElement("version"))
                {
                    writer.WriteAttributeString("major", "4");
                    writer.WriteAttributeString("minor", "0");
                    writer.WriteAttributeString("revision", "0");
                    writer.WriteAttributeString("build", "49");
                    writer.WriteAttributeString("lslib_meta", "v1,bswap_guids,lsf_keys_adjacency");
                }

                using (var region = writer.WriteScopedElement("region"))
                {
                    writer.WriteAttributeString("id", "config");
                    using (var nodeConfig = writer.WriteScopedElement("node"))
                    {
                        writer.WriteAttributeString("id", "config");
                        using (var childrenConfig = writer.WriteScopedElement("children"))
                        {
                            using (var nodeEntries = writer.WriteScopedElement("node"))
                            {
                                writer.WriteAttributeString("id", "entries");
                                using (var childrenEntries = writer.WriteScopedElement("children"))
                                {
                                    foreach (var image in imageMap)
                                    {
                                        using (var nodeImage = writer.WriteScopedElement("node"))
                                        {
                                            writer.WriteAttributeString("id", "Object");
                                            writer.WriteLsxAttribute("MapKey", "FixedString", FixActionResourceIconPathName(image.Key));

                                            using(var childrenImage = writer.WriteScopedElement("children"))
                                            {
                                                using (var attribute = writer.WriteScopedElement("node"))
                                                {
                                                    writer.WriteAttributeString("id", "entries");
                                                    writer.WriteLsxAttribute("h", "int16", image.Value.ToString());
                                                    writer.WriteLsxAttribute("mipcount", "int8", "1");
                                                    writer.WriteLsxAttribute("w", "int16", image.Value.ToString());
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            writer.WriteEndDocument();
        }

        struct ActionResourceIconSetting
        {
            public string IconPath;
            public int SideLength;
            public IconType Type;
            public bool Optional;
        }

        private static readonly ActionResourceIconSetting[] ActionResourceIconSettings = new ActionResourceIconSetting[]
        {
            new ActionResourceIconSetting
            {
                IconPath = "Assets\\Shared\\Resources\\{0}.DDS",
                SideLength = 48,
                Type = IconType.Standard,
                Optional = false
            },
            new ActionResourceIconSetting
            {
                IconPath = "Assets\\Shared\\Resources\\Highlight\\{0}.DDS",
                SideLength = 48,
                Type = IconType.Highlight,
                Optional = false
            },
            new ActionResourceIconSetting
            {
                IconPath = "Assets\\Shared\\Resources\\Missing\\{0}.DDS",
                SideLength = 48,
                Type = IconType.Missing,
                Optional = false
            },
            new ActionResourceIconSetting
            {
                IconPath = "Assets\\Shared\\Resources\\Used\\{0}.DDS",
                SideLength = 48,
                Type = IconType.Used,
                Optional = false
            },
            new ActionResourceIconSetting
            {
                IconPath = "Assets\\CC\\icons_resources\\{0}.DDS",
                SideLength = 128,
                Type = IconType.Standard,
                Optional = false
            },
            new ActionResourceIconSetting
            {
                IconPath = "Assets\\ActionResources_c\\Icons\\{0}.DDS",
                SideLength = 80,
                Type = IconType.Standard,
                Optional = false
            },
            new ActionResourceIconSetting
            {
                IconPath = "Assets\\ActionResources_c\\Icons\\Resources\\{0}.DDS",
                SideLength = 64,
                Type = IconType.Standard,
                Optional = false
            },
            new ActionResourceIconSetting
            {
                IconPath = "Assets\\ActionResources_c\\Icons\\Resources\\Highlight\\{0}.DDS",
                SideLength = 64,
                Type = IconType.Highlight,
                Optional = true
            },
            new ActionResourceIconSetting
            {
                IconPath = "Assets\\ActionResources_c\\Icons\\Resources\\Missing\\{0}.DDS",
                SideLength = 64,
                Type = IconType.Missing,
                Optional = true
            },
            new ActionResourceIconSetting
            {
                IconPath = "Assets\\ActionResources_c\\Icons\\Resources\\Used\\{0}.DDS",
                SideLength = 64,
                Type = IconType.Used,
                Optional = true
            }
        };

        private static ImageTransform? GetTransform(IconType type)
        {
            switch(type )
            {
                case IconType.Missing: // Transform to near black (haven't seen an example to sample yet).
                    return (image) =>
                    {
                        for (int y = 0; y < image.Height; y++)
                        {
                            for (int x = 0; x < image.Width; x++)
                            {
                                var pixel = image[x, y];
                                image[x, y] = new Rgba32(46, 44, 42, pixel.A);
                            }
                        }
                    };
                case IconType.Used: // Transform to grey (similar shade for actions and bonus actions)
                    return (image) =>
                    {
                        for(int y = 0; y < image.Height; y++)
                        {
                            for (int x = 0; x < image.Width; x++)
                            {
                                var pixel = image[x, y];
                                image[x, y] = new Rgba32(120, 118, 116, pixel.A);
                            }
                        }
                    };
                case IconType.Highlight: // Transform to near white (similar shade for actions and bonus actions)
                    return (image) =>
                    {
                        for (int y = 0; y < image.Height; y++)
                        {
                            for (int x = 0; x < image.Width; x++)
                            {
                                var pixel = image[x, y];
                                image[x, y] = new Rgba32(209, 207, 205, pixel.A);
                            }
                        }
                    };
            }
            return null;
        }
    }
}
