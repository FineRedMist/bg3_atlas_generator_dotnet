namespace BG3Common
{
    /// <summary>
    /// Helper functions for managing texture atlases.
    /// </summary>
    public static class Atlas
    {
        /// <summary>
        /// Determines the side length of the atlas in terms of the number of images contained within.
        /// </summary>
        /// <param name="imageCount">The count of uniform images to store in the atlas.</param>
        /// <returns>The side length (measured in images) for a square atlas to contain the images.</returns>
        public static int GetAtlasImageAxisLength(int imageCount)
        {
            int size = 1;
            while (imageCount > size * size)
            {
                size *= 2;
            }
            return size;
        }

    }
}
