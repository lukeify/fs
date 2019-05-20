using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Fs.Helpers;
using Fs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Fs.Services
{
    public class FileUploadService
    {
        private static readonly FormOptions DefaultFormOptions = new FormOptions();

        private HttpContext Http { get; set; }

        private KeyValueAccumulator FormAccumulator { get; set; } = new KeyValueAccumulator();

        private IConfiguration Configuration { get; set; }

        private IList<UploadedFile> UploadedFiles { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fs.Services.FileUploadService"/> class.
        /// </summary>
        /// 
        /// <param name="configuration">Configuration of the application.</param>
        /// 
        public FileUploadService(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Handle uploading files to the system.
        /// </summary>
        /// 
        /// <returns>A Task resolving to an object representing the file that was uploaded.</returns>
        /// 
        /// <param name="httpContext">Http context.</param>
        /// 
        public async Task<IEnumerable<UploadedFile>> Upload(HttpContext httpContext)
        {
            this.Http = httpContext;
            this.UploadedFiles = new List<UploadedFile>();

 
            // Ensure we have a Multipart Content-Type.
            if (!MultipartRequestHelper.IsMultipartContentType(httpContext.Request.ContentType))
            {
                throw new Exception($"Incorrect Content Type. Got {httpContext.Request.ContentType} Expected MultipartContent.");
            }

            // Retrieve the media type, and the string up to the boundary.
            MediaTypeHeaderValue mediaType = MediaTypeHeaderValue.Parse(httpContext.Request.ContentType);
            string boundary = MultipartRequestHelper.GetBoundary(mediaType, DefaultFormOptions.MultipartBoundaryLengthLimit);

            // Create our reader
            MultipartReader reader = new MultipartReader(boundary, httpContext.Request.Body);

            // Read each section, and loop over each section.
            MultipartSection section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                UploadedFile file = new UploadedFile();
                file = await this.IterateOverSection(section, file);
                this.UploadedFiles.Add(file);

                section = await reader.ReadNextSectionAsync();
            }

            return this.UploadedFiles;
        }

        /// <summary>
        /// Moves the file that can be found in the <paramref name="currentDirectoryAndName"/> 
        /// to a directory specified by the system configuration with the filename provided
        /// by <paramref name="fileName"/>, and with extension from <paramref name="fileExtension"/>.
        /// </summary>
        /// 
        /// TODO: Docblocks
        /// 
        /// 
        public string Move(UploadedFile file, string newFileNameWithoutExtension)
        {
            // Build the new directory and name, and then append an extension to the file.
            string newPathWithoutExtension = Path.Combine(this.Configuration["Fs:Dir"], newFileNameWithoutExtension);
            string newPath = Path.ChangeExtension(newPathWithoutExtension, file.UserProvidedExtension);

            // Move the file from tempoerary storage to its permanent location.
            System.IO.File.Move(file.TempPath, newPath);

            // Return the new permanent location
            return newPath;
        }

        /// <summary>
        /// Iterates the over a Multipart section of the form.
        /// </summary>
        /// 
        /// <returns>A task.</returns>
        /// 
        /// <param name="section">The MultipartSection to process.</param>
        /// 
        private async Task<UploadedFile> IterateOverSection(MultipartSection section, UploadedFile file)
        {
            bool hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(
                section.ContentDisposition, 
                out ContentDispositionHeaderValue contentDisposition
            );

            if (hasContentDispositionHeader && MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
            {
                // Set the uploaded file properties.
                file.UserProvidedName      = contentDisposition.FileName.Value;
                file.UserProvidedExtension = Path.GetExtension(contentDisposition.FileName.Value);
                file.UserProvidedMimetype  = MediaTypeHeaderValue.Parse(section.ContentType);
                file.TempPath              = Path.GetTempFileName();

                // Copy the file to disk.
                using (FileStream targetStream = System.IO.File.Create(file.TempPath))
                {
                    await section.Body.CopyToAsync(targetStream);
                }

            } 
            else if (hasContentDispositionHeader && MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
            {
                StringSegment key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                Encoding encoding = this.GetEncoding(section);

                using (StreamReader streamReader = new StreamReader(
                    section.Body,
                    encoding,
                    detectEncodingFromByteOrderMarks: true,
                    bufferSize: 1024,
                    leaveOpen: true
                ))
                {
                    string value = await streamReader.ReadToEndAsync();
                    if (String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                    {
                        value = String.Empty;
                    }
                    this.FormAccumulator.Append(key.Value, value);

                    if (this.FormAccumulator.ValueCount > DefaultFormOptions.ValueCountLimit)
                    {
                        throw new InvalidDataException($"Form key count limit {DefaultFormOptions.ValueCountLimit} exceeded.");
                    }
                }
            }

            return file;
        }

        /// <summary>
        /// Retrieves the encoding for the provided MultipartSection.
        /// </summary>
        /// 
        /// <returns>The encoding.</returns>
        /// 
        /// <param name="section">Section.</param>
        /// 
        private Encoding GetEncoding(MultipartSection section)
        {
            bool hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(
                section.ContentType, 
                out MediaTypeHeaderValue mediaType
            );

            // Ensure UTF7 encodings return UTF8 Encoding type.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }

            return mediaType.Encoding;
        }
    }
}
