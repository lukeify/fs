using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Fs.Services
{
    public class ThumbnailGenerator
    {
        public bool CreateDefaultThumbnail(string filePath, int desiredThumbnailHeight, string thumbnailDir)
        {
            // Firstly retrieve the image metadata, and check to see if a thumbnail
            // even needs to be genrated. It might not be if the image height is already
            // less than the desired thumbnail height.
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            if (Image.Identify(fs).Height < (desiredThumbnailHeight * 2))
            {
                return false;
            }

            // The image is larger, we need to scale it down.
            using (Image<Rgba32> image = Image.Load(filePath))
            {
                // Calculate our scale factor.
                double scaleFactor = (double) image.Height / (desiredThumbnailHeight * 2);

                // Calculate our thumb width and thumb height.
                int thumbWidth = Convert.ToInt32((double) image.Width / scaleFactor);
                int thumbHeight = Convert.ToInt32((double) image.Height / scaleFactor);

                // Clone, Resize, & Save.
                image.Mutate(ctx => ctx.Resize(thumbWidth, thumbHeight));
                image.Save(Path.Combine(thumbnailDir, Path.GetFileName(filePath)));
            }

            return true;
        }
    }
}
