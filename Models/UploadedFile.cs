using System;
using Microsoft.Net.Http.Headers;

namespace Fs.Models
{
    public class UploadedFile
    {
        public string TempPath { get; set; }

        public string UserProvidedName { get; set; }

        public MediaTypeHeaderValue UserProvidedMimetype { get; set; }

        public string UserProvidedExtension { get; set; }
    }
}
