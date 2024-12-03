using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BG3Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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
                IconPath = "Mods\\{0}\\GUI\\Assets\\Shared\\Resources\\{1}.DDS",
                SideLength = 48,
                Type = IconType.Standard,
                Optional = false
            },
            new ActionResourceIconSetting
            {
                IconPath = "Mods\\{0}\\GUI\\Assets\\Shared\\Resources\\Highlight\\{1}.DDS",
                SideLength = 48,
                Type = IconType.Highlight,
                Optional = false
            },
            new ActionResourceIconSetting
            {
                IconPath = "Mods\\{0}\\GUI\\Assets\\Shared\\Resources\\Missing\\{1}.DDS",
                SideLength = 48,
                Type = IconType.Missing,
                Optional = false
            },
            new ActionResourceIconSetting
            {
                IconPath = "Mods\\{0}\\GUI\\Assets\\Shared\\Resources\\Used\\{1}.DDS",
                SideLength = 48,
                Type = IconType.Used,
                Optional = false
            },
            new ActionResourceIconSetting
            {
                IconPath = "Mods\\{0}\\GUI\\Assets\\CC\\icons_resources\\{1}.DDS",
                SideLength = 128,
                Type = IconType.Standard,
                Optional = false
            },
            new ActionResourceIconSetting
            {
                IconPath = "Mods\\{0}\\GUI\\Assets\\ActionResources_c\\Icons\\{1}.DDS",
                SideLength = 80,
                Type = IconType.Standard,
                Optional = false
            },
            new ActionResourceIconSetting
            {
                IconPath = "Mods\\{0}\\GUI\\Assets\\ActionResources_c\\Icons\\Resources\\{1}.DDS",
                SideLength = 64,
                Type = IconType.Standard,
                Optional = false
            },
            new ActionResourceIconSetting
            {
                IconPath = "Mods\\{0}\\GUI\\Assets\\ActionResources_c\\Icons\\Resources\\Highlight\\{1}.DDS",
                SideLength = 64,
                Type = IconType.Highlight,
                Optional = true
            },
            new ActionResourceIconSetting
            {
                IconPath = "Mods\\{0}\\GUI\\Assets\\ActionResources_c\\Icons\\Resources\\Missing\\{1}.DDS",
                SideLength = 64,
                Type = IconType.Missing,
                Optional = true
            },
            new ActionResourceIconSetting
            {
                IconPath = "Mods\\{0}\\GUI\\Assets\\ActionResources_c\\Icons\\Resources\\Used\\{1}.DDS",
                SideLength = 64,
                Type = IconType.Used,
                Optional = true
            }
        };

        /// <summary>
        /// From https://mod.io/g/baldursgate3/r/implementing-custom-action-resources-with-impui
        /// </summary>
        /// <param name="inputPath">The path to the image file to generate action resource icons for.</param>
        /// <param name="modPathRoot">The path to the root of the mod.</param>
        /// <param name="actionResourceName">The name of the action resource to generate icons for.</param>
        /// <returns></returns>
        private static int CreateActionResourceIcons(string inputPath, string modPathRoot, string actionResourceName)
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

            // First pass just duplicate the image to the right size to all locations.
            using Image<Rgba32> image = Image.Load<Rgba32>(inputPath);

            List<Task> tasks = new List<Task>();

            foreach (var setting in ActionResourceIconSettings)
            {
                if (setting.Optional) // Skipping for now.
                {
                    continue;
                }

                string iconPath = Path.Combine(modPathRoot, string.Format(setting.IconPath, modName, actionResourceName));
                tasks.Add(image.GenerateIcon(setting.SideLength, Path.GetDirectoryName(iconPath)!, actionResourceName));
            }

            Task.WaitAll(tasks.ToArray());

            return 0;
        }

    }
}
