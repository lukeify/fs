using System;
using System.IO;
using Fs.Enums;

namespace Fs.Models
{
    public class File
    {
        public FileCategory Category { get; set; }

        public string Name { get; set; }

        public string UserProvidedName { get; set; }

        public string Mimetype { get; set; }

        public string Extension { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public long Filesize { get; set; }

        public int Length { get; set; }

        public bool HasThumbnail { get; set; }

        private int _views;

        public int Views {
            get
            {
                return this._views;
            }
        }

        public string Url
        {
            get
            {
                return Path.ChangeExtension(this.Name, this.Extension);
            }
        }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Increments the view counter by one.
        /// </summary>
        /// 
        /// <returns>
        /// The new number of views the file has.
        /// </returns>
        /// 
        public int IncrementViews()
        {
            return ++this._views;
        }
    }
}
