using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BG3Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Iconify
{
    /// <summary>
    /// Whether to include the "lslib_meta"="v1,bswap_guids" attribute for the xml doc.
    /// </summary>
    enum SwapTag
    {
        /// <summary>
        /// Don't include the attribute.
        /// </summary>
        Absent,
        /// <summary>
        /// Include the attribute.
        /// </summary>
        Present
    }

    internal partial class Program
    {
        private static int CreateAtlas(string inputPath, string modPathRoot)
        {
            var modName = Path.GetFileName(modPathRoot);

            // According to https://bg3.wiki/wiki/Modding:Creating_Item_Icons, we need to do multiple things:
            // Public\Game\GUI\Assets\ControllerUIIcons\skills_png\Spell_Evocation_PulseWave.DDS
            // Public\Game\GUI\Assets\Tooltips\Icons\Spell_Evocation_PulseWave.DDS
            // Public\<mod>\GUI\Icons_Skills.lsx
            // Path in Icons_Skills.lsx maps to path under Public\<mod>
            /*
    <region id="TextureAtlasInfo">
        <node id="root">
            <children>
                <node id="TextureAtlasIconSize">
                    <attribute id="Height" type="int32" value="64"/>
                    <attribute id="Width" type="int32" value="64"/>
                </node>
                <node id="TextureAtlasPath">
                    <attribute id="Path" type="string" value="Assets/Textures/Icons/Icons_5e_Spells.dds"/>
                    <attribute id="UUID" type="FixedString" value="10b8d5a8-c07e-4822-9e74-34bf22692833"/>
                </node>
                <node id="TextureAtlasTextureSize">
                    <attribute id="Height" type="int32" value="2048"/>
                    <attribute id="Width" type="int32" value="2048"/>
                </node>
            </children>
        </node>
    </region>				  				
            */

            var imageFileNames = GatherImageFiles(inputPath);
            if (imageFileNames.Length == 0)
            {
                Console.WriteLine($"No images found in {inputPath}!");
                return -1;
            }

            string controllerIconPath = Path.Combine(modPathRoot, "Public", "Game", "GUI", "Assets", "ControllerUIIcons", "skills_png");
            string tooltipIconPath = Path.Combine(modPathRoot, "Public", "Game", "GUI", "Assets", "Tooltips", "Icons");
            var skillsAtlasPartialPath = Path.Combine("Assets", "Textures", "Icons", modName.BG3Normalize() + "_Icons.DDS");
            var uiRegistrationAtlasPartialPath = Path.Combine("Public", modName, skillsAtlasPartialPath);
            var atlasPath = Path.Combine(modPathRoot, "Public", modName, "Assets", "Textures", "Icons");
            var uiPath = Path.Combine(modPathRoot, "Public", "Game", "Content", "UI", "[PAK]_UI");
            var skillsPath = Path.Combine(modPathRoot, "Public", modName, "GUI", "Icons_Skills.lsx");

            Preclean(controllerIconPath, tooltipIconPath, atlasPath, uiPath, skillsPath);

            var atlasAxisLength = Atlas.GetAtlasImageAxisLength(imageFileNames.Length);

            Image<Rgba32> joined = new Image<Rgba32>(64 * atlasAxisLength, 64 * atlasAxisLength);
            joined.Mutate(x => x.Opacity(0));

            Dictionary<string, Point> imageMap = new Dictionary<string, Point>();
            Size atlasImageSize = new Size(64, 64);
            Point current = new Point(0, 0);

            List<Task> tasks = new List<Task>();

            foreach (var imageFileName in imageFileNames)
            {
                var imageName = Path.GetFileNameWithoutExtension(imageFileName);
                using Image<Rgba32> image = Image.Load<Rgba32>(Path.Combine(inputPath, imageFileName));

                tasks.Add(image.GenerateIcon(380, tooltipIconPath, imageName));
                tasks.Add(image.GenerateIcon(144, controllerIconPath, imageName));

                image.Mutate(x => x.Resize(64, 64));
                joined.Mutate(x => x.DrawImage(image, current * 64, new GraphicsOptions()));

                imageMap[imageName] = current;
                current = new Point(current.X + 1, current.Y);
                if (current.X == atlasAxisLength)
                {
                    current = new Point(0, current.Y + 1);
                }
            }

            var atlasId = Guid.NewGuid();
            joined.SaveDdsImage(Path.Combine(modPathRoot, "Public", modName, skillsAtlasPartialPath));
            SaveAtlasImageMap(imageMap, atlasId, atlasAxisLength, skillsAtlasPartialPath, skillsPath);
            SaveImageBank(atlasId, SwapTag.Present, uiRegistrationAtlasPartialPath, Path.Combine(uiPath, atlasId.ForBG3() + ".lsf.lsx"));
            //SaveImageBank(atlasId, false, partialPath, Path.Combine(uiPath, "_merged.lsx"));

            Task.WaitAll(tasks.ToArray());

            return 0;
        }

        private static void Preclean(params string[] paths)
        {
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    continue;
                }
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
        }

        private static void SaveImageBank(Guid atlasId, SwapTag isPresent, string atlasPartialPath, string imageBankLsfFilePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(imageBankLsfFilePath)!);
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.Encoding = Encoding.UTF8;
            using XmlWriter writer = XmlWriter.Create(imageBankLsfFilePath, xmlWriterSettings);
            writer.WriteStartDocument();
            using (var save = writer.WriteScopedElement("save"))
            {
                using (var version = writer.WriteScopedElement("version"))
                {
                    writer.WriteAttributeString("major", "4");
                    writer.WriteAttributeString("minor", "0");
                    writer.WriteAttributeString("revision", "0");
                    writer.WriteAttributeString("build", "49");
                    if (isPresent == SwapTag.Present)
                    {
                        writer.WriteAttributeString("lslib_meta", "v1,bswap_guids");
                    }
                }
                using (var region = writer.WriteScopedElement("region"))
                {
                    writer.WriteAttributeString("id", "TextureBank");
                    using (var node = writer.WriteScopedElement("node"))
                    {
                        writer.WriteAttributeString("id", "TextureBank");
                        using (var children = writer.WriteScopedElement("children"))
                        {
                            using (var resource = writer.WriteScopedElement("node"))
                            {
                                writer.WriteAttributeString("id", "Resource");
                                writer.WriteLsxAttribute("ID", "FixedString", atlasId.ForBG3());
                                writer.WriteLsxAttribute("Localized", false);
                                writer.WriteLsxAttribute("Name", "LSString", Path.GetFileNameWithoutExtension(atlasPartialPath));
                                writer.WriteLsxAttribute("SRGB", true);
                                writer.WriteLsxAttribute("SourceFile", "LSString", atlasPartialPath.Replace('\\', '/'));
                                writer.WriteLsxAttribute("Streaming", true);
                                writer.WriteLsxAttribute("Template", "FixedString", "Icons_Skills");
                                writer.WriteLsxAttribute("Type", 0);
                                //writer.WriteLsxAttribute("_OriginalFileVersion_", "Int64", "0");
                            }
                        }
                    }
                }
            }
            writer.WriteEndDocument();
        }

        private static void SaveAtlasImageMap(Dictionary<string, Point> imageMap, Guid atlasId, int atlasAxisLength, string atlasPartialPath, string imageMapLsxFilePath)
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
                }

                using (var region = writer.WriteScopedElement("region"))
                {
                    writer.WriteAttributeString("id", "IconUVList");
                    using (var node = writer.WriteScopedElement("node"))
                    {
                        writer.WriteAttributeString("id", "root");
                        using (var children = writer.WriteScopedElement("children"))
                        {
                            foreach (var image in imageMap)
                            {
                                Point p = image.Value;
                                using (var iconUV = writer.WriteScopedElement("node"))
                                {
                                    writer.WriteAttributeString("id", "IconUV");

                                    var (upperLeft, lowerRight) = p.GetUVs(atlasAxisLength);
                                    writer.WriteLsxAttribute("MapKey", "FixedString", image.Key);
                                    writer.WriteLsxAttribute("U1", upperLeft.U);
                                    writer.WriteLsxAttribute("U2", lowerRight.U);
                                    writer.WriteLsxAttribute("V1", upperLeft.V);
                                    writer.WriteLsxAttribute("V2", lowerRight.V);
                                }
                            }
                        }
                    }
                }

                using (var region = writer.WriteScopedElement("region"))
                {
                    writer.WriteAttributeString("id", "TextureAtlasInfo");
                    using (var node = writer.WriteScopedElement("node"))
                    {
                        writer.WriteAttributeString("id", "root");
                        using (var children = writer.WriteScopedElement("children"))
                        {
                            using (var TextureAtlasIconSize = writer.WriteScopedElement("node"))
                            {
                                writer.WriteAttributeString("id", "TextureAtlasIconSize");
                                writer.WriteLsxAttribute("Height", 64);
                                writer.WriteLsxAttribute("Width", 64);
                            }
                            using (var TextureAtlasPath = writer.WriteScopedElement("node"))
                            {
                                writer.WriteAttributeString("id", "TextureAtlasPath");
                                writer.WriteLsxAttribute("Path", "string", atlasPartialPath.Replace('\\', '/'));
                                writer.WriteLsxAttribute("UUID", "FixedString", atlasId.ForBG3());
                            }
                            using (var TextureAtlasTextureSize = writer.WriteScopedElement("node"))
                            {
                                writer.WriteAttributeString("id", "TextureAtlasTextureSize");
                                var atlasSize = atlasAxisLength * 64;
                                writer.WriteLsxAttribute("Height", atlasSize);
                                writer.WriteLsxAttribute("Width", atlasSize);
                            }
                        }
                    }
                }
            }
            writer.WriteEndDocument();
        }

        private static readonly string[] mSupportedExtensions = [".png", ".gif", ".jpg", ".bmp", ".webp"];
        static string[] GatherImageFiles(string path)
        {
            return Directory.EnumerateFiles(path)
                .Where(file => mSupportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                .ToArray();
        }
    }
}