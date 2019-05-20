using System;
using System.IO;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Fs.Helpers
{
    public static class MultipartRequestHelper
    {
        /// <summary>
        /// Does the request contain a multipart <paramref name="contentType"/>.
        /// </summary>
        /// 
        /// <returns><c>true</c>, if multipart content type was ised, <c>false</c> otherwise.</returns>
        /// 
        /// <param name="contentType">Content type.</param>
        /// 
        public static bool IsMultipartContentType(string contentType)
        {
            return !String.IsNullOrEmpty(contentType) && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Hases the file content disposition.
        /// </summary>
        /// 
        /// <returns><c>true</c>, if file content disposition was hased, <c>false</c> otherwise.</returns>
        /// 
        /// <param name="disposition">Disposition.</param>
        /// 
        public static bool HasFileContentDisposition(ContentDispositionHeaderValue disposition)
        {
            return disposition != null
                && disposition.DispositionType.Equals("form-data")
                && (!String.IsNullOrEmpty(disposition.FileName.Value)
                || !String.IsNullOrEmpty(disposition.FileNameStar.Value));
        }

        /// <summary>
        /// Hases the form data content disposition.
        /// </summary>
        /// 
        /// <returns><c>true</c>, if form data content disposition was hased, <c>false</c> otherwise.</returns>
        /// 
        /// <param name="disposition">Form.</param>
        /// 
        public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue disposition)
        {
            return disposition != null
                && disposition.DispositionType.Equals("form-data")
                && String.IsNullOrEmpty(disposition.FileName.Value)
                && String.IsNullOrEmpty(disposition.FileNameStar.Value);
        }

        /// <summary>
        /// Gets the boundary of the Content Type.
        /// </summary>
        /// 
        /// <returns>The boundary.</returns>
        /// 
        /// <param name="contentType">Content type.</param>
        /// <param name="lengthLimit">Length limit.</param>
        /// 
        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            StringSegment boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary);
            if (String.IsNullOrWhiteSpace(boundary.Value))
            {
                throw new InvalidDataException("Missing Content-Type boundary.");
            }

            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException($"Multipart boundary length limit {lengthLimit} exceeded.");
            }

            return boundary.Value;
        }
    }
}
