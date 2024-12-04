using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace BG3Common
{
    /// <summary>
    /// A transform that can be applied to the resized <paramref name="image"/> before saving it as a DDS.
    /// </summary>
    public delegate void ImageTransform(Image<Rgba32> image);

    /// <summary>
    /// A UV Coordinate for the atlas.
    /// </summary>
    public struct UVCoordinate
    {
        /// <summary>
        /// Horizontal component of the UV Coordinate, as a fraction of the total length.
        /// </summary>
        public float U;
        /// <summary>
        /// Vertical component of the UV Coordinate, as a fraction of the total length.
        /// </summary>
        public float V;

        /// <summary>
        /// Constructor for the <seealso cref="UVCoordinate"/>.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        public UVCoordinate(float u, float v)
        {
            U = u;
            V = v;
        }
    }

    /// <summary>
    /// A set of extensions for working with graphics.
    /// </summary>
    public static class GraphicsExtensions
    {
        
        /// <summary>
        /// The following code assumes a square atlas is being used.
        /// The provided point represents the index of a given image in the atlas based on the x/y coordinates.
        /// This is mapped into the corresponding upper left, and lower right UV coordinates for the based on
        /// the denominator representing the number of images that are mappable in the horizontal and vertical.
        /// 
        /// For example, in a 4x4 atlas for up to 16 images, the 1,2 point would represent one image across and
        /// two images down, and the denominator would be 4 (for the side length).
        /// </summary>
        /// <param name="point">The representation of the image's index in the image grid of the atlas.</param>
        /// <param name="denominator">The number of images that are stored vertically/horizontally.</param>
        /// <returns>The corresponding upper left and lower right coordinates for the given image.</returns>
        public static (UVCoordinate, UVCoordinate) GetUVs(this Point point, int denominator)
        {
            float x = point.X, y = point.Y;
            return (new UVCoordinate(x / denominator, y / denominator), new UVCoordinate((x + 1) / denominator, (y + 1) / denominator));
        }

        /// <summary>
        /// Saves a given <paramref name="image"/> to the <paramref name="targetFullPath"/> as a DDS image with
        /// the given number of <paramref name="maxMipLevels"/>. If the mip levels are less than 1, then only one 
        /// level is stored. The format is Bc7, best quality.
        /// </summary>
        public static void SaveDdsImage(this Image<Rgba32> image, string targetFullPath, int maxMipLevels = -1)
        {
            var parentDirectory = Path.GetDirectoryName(targetFullPath);
            if (parentDirectory != null)
            {
                Directory.CreateDirectory(parentDirectory);
            }
            if (File.Exists(targetFullPath))
            {
                File.Delete(targetFullPath);
            }
            BcEncoder e = new BcEncoder(CompressionFormat.Bc7);
            e.OutputOptions.GenerateMipMaps = maxMipLevels > 1;
            if (e.OutputOptions.GenerateMipMaps)
            {
                e.OutputOptions.MaxMipMapLevel = maxMipLevels;
            }
            e.OutputOptions.Quality = CompressionQuality.BestQuality;
            e.OutputOptions.FileFormat = OutputFileFormat.Dds;

            using FileStream fs = File.OpenWrite(targetFullPath);
            var buffer = new byte[4 * image.Size.Width * image.Size.Height];
            image.CopyPixelDataTo(buffer);
            e.EncodeToStream(buffer, image.Size.Width, image.Size.Height, PixelFormat.Rgba32, fs);
        }

        /// <summary>
        /// Creates a task to generate a DDS icon of the given <paramref name="image"/>, converting it to an image with the
        /// given <paramref name="sideLength"/>. This gets saved to the combined path of <paramref name="basePath"/>/<paramref name="imageName"/>.DDS.
        /// </summary>
        /// <returns>A task to monitor the completion of creating an icon</returns>
        public static Task GenerateIcon(this Image<Rgba32> image, int sideLength, string basePath, string imageName, ImageTransform? transform = null)
        {
            var clone = image.Clone();
            return Task.Factory.StartNew(() =>
            {
                var targetPath = Path.Combine(basePath, imageName + ".DDS");
                clone.Mutate(x => x.Resize(sideLength, sideLength));
                if (transform != null)
                {
                    transform(clone);
                }
                clone.SaveDdsImage(targetPath);
                clone.Dispose();
                Console.WriteLine("Saved {1}x{1} icon to: {0}", targetPath, sideLength);

            }, TaskCreationOptions.LongRunning);
        }
    }
}