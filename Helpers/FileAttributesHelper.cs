using System;
using Fs.Enums;
using Fs.Models;
using SixLabors.ImageSharp;

namespace Fs.Helpers
{
    public class FileAttributesHelper
    {
        /// <summary>
        /// Determines the category of a file.
        /// </summary>
        /// 
        /// <returns>The file category.</returns>
        /// 
        public FileCategory DetermineFileCategory(string filePath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sniffs the mimetype of the provided file.
        /// </summary>
        /// <returns>The mimetype.</returns>
        public string SniffMimetype(string filePath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the filesize of the provided file at filePath.
        /// </summary>
        /// 
        /// <returns>The filesize of the file at the path.</returns>
        /// 
        /// <param name="filePath">The path to the file to retrieve the size of.</param>
        /// 
        public long GetFilesize(string filePath)
        {
            return new System.IO.FileInfo(filePath).Length;
        }

        /// <summary>
        /// Retrieves the width of the image.
        /// </summary>
        /// 
        /// <returns>The image width.</returns>
        /// 
        /// <param name="filePath">The path to the file on disk.</param>
        /// 
        public int GetImageWidth(string filePath)
        {
            System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            IImageInfo imageInfo = Image.Identify(fs);
            return imageInfo.Width;
        }

        /// <summary>
        /// Retrieves the height of the image.
        /// </summary>
        /// 
        /// <returns>The image height.</returns>
        /// 
        /// <param name="filePath">The path to the file on disk.</param>
        /// 
        public int GetImageHeight(string filePath)
        {
            System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            IImageInfo imageInfo = Image.Identify(fs);
            return imageInfo.Height;
        }

        /// <summary>
        /// Gets the length of the video.
        /// </summary>
        /// 
        /// <returns>The video length.</returns>
        /// 
        public int GetVideoLength()
        {
            throw new NotImplementedException();
        }
    }
}
