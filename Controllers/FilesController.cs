using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fs.Models;
using Microsoft.AspNetCore.Mvc;
using RethinkDb.Driver.Net;
using Microsoft.Extensions.Configuration;
using Fs.Attributes;
using Fs.Services;
using RethinkDb.Driver.Ast;
using Fs.Enums;
using Fs.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Fs.Controllers
{
    [Route("api/[controller]")]
    public class FilesController : Controller
    {
        public RethinkDb.Driver.RethinkDB R = RethinkDb.Driver.RethinkDB.R;

        public IConnection Conn { get; set; }

        public IConfiguration Configuration { get; set; }

        public FileUploadService FileUploadService { get; set; }

        public FilenameGenerator FilenameGenerator { get; set; }

        public FileAttributesHelper FileAttributesHelper { get; set; }

        public ThumbnailGenerator ThumbnailGenerator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fs.Controllers.FilesController"/> class.
        /// </summary>
        /// 
        /// <param name="configuration">The configuration of the application.</param>
        /// <param name="conn">A reference to a RethinkDB connection.</param>
        /// 
        public FilesController(
            FileUploadService fileUploadService, 
            FilenameGenerator filenameGenerator,
            FileAttributesHelper fileAttributesHelper,
            ThumbnailGenerator thumbnailGenerator,
            IConfiguration configuration, 
            IConnection conn)
        {
            this.FileUploadService = fileUploadService;
            this.FilenameGenerator = filenameGenerator;
            this.FileAttributesHelper = fileAttributesHelper;
            this.ThumbnailGenerator = thumbnailGenerator;
            this.Conn = conn;
            this.Configuration = configuration;
        }

        [Route("meta")]
        [HttpGet]
        public FilesystemData Meta()
        {
            int count = this.FilesTable().Count()
                .RunAtom<int>(this.Conn);

            int bytes = this.FilesTable().Sum("Filesize")
                .RunAtom<int>(this.Conn);

            return new FilesystemData()
            {
                BytesUsed = bytes,
                Count = count
            };
        }

        /// <summary>
        /// GET: /api/files
        /// 
        /// Returns all the files. If from and paginate parameters are specified,
        /// then a chunk of files will be returned.
        /// </summary>
        /// 
        /// <returns>The get.</returns>
        /// 
        /// <param name="from">The ID from which to retrieve files from.</param>
        /// <param name="paginate">The number of results to retrieve in this request.</param>
        [HttpGet]
        public IEnumerable<File> Get(string from, string paginate)
        {
            IList<File> files = this.FilesTable()
                .OrderBy(this.R.Desc("CreatedAt"))
                .RunAtom<IList<File>>(this.Conn);

            return files.ToList();
        }

        /// <summary>
        /// GET: /api/files/1
        /// 
        /// Returns a specific file by an id.
        /// </summary>
        /// 
        /// <returns>The file that matches the <paramref name="id"/>.</returns>
        /// 
        /// <param name="id">The ID of the file to retrieve.</param>
        [HttpGet("{id}")]
        public File Get(int id)
        {
            File file = this.FilesTable()
                .Get(id)
                .RunAtom<File>(this.Conn);

            return file;
        }

        /// <summary>
        /// Uploads one or more files to the application.
        /// </summary>
        /// 
        /// <returns>
        /// The response from the server upon uploading and handling files.
        /// </returns>
        [HttpPut]
        [DisableFormValueModelBinding]
        public async Task<IEnumerable<File>> Put()
        {
            // Set the datetime this message was called.
            DateTime now = DateTime.Now;

            // Our list of files we return to the client
            IList<File> files = new List<File>();

            // Retrieve the files from the client, then iterate over each file, generate a name for it, 
            // and move the file  to its final directory.
            IEnumerable<UploadedFile> uploadedFiles = await this.FileUploadService.Upload(HttpContext);

            foreach (UploadedFile uploadedFile in uploadedFiles)
            {
                string newFileNameWithoutExtension = this.FilenameGenerator.GenerateName();
                string newFilePath = this.FileUploadService.Move(uploadedFile, newFileNameWithoutExtension);

                // TODO: Sniff the actual mimetype of the file, and use that instead of the user-provided
                // mime type.

                // TODO: Generate the File model based on the FileCategory.

                // Generate a thumbnail here.
                int preferredThumbnailHeight = int.Parse(this.Configuration["App:PreferredThumbnailHeight"]);

                bool hasThumbnail = this.ThumbnailGenerator.CreateDefaultThumbnail(
                    newFilePath,
                    preferredThumbnailHeight,
                    this.Configuration["Fs:DefaultThumbDir"]
                );

                // Create a File Model
                File file = new File()
                {
                    Category            = FileCategory.Image,
                    Name                = newFileNameWithoutExtension,
                    UserProvidedName    = uploadedFile.UserProvidedName,
                    Mimetype            = uploadedFile.UserProvidedMimetype.ToString(),
                    Extension           = uploadedFile.UserProvidedExtension,
                    Width               = this.FileAttributesHelper.GetImageWidth(newFilePath),
                    Height              = this.FileAttributesHelper.GetImageHeight(newFilePath),
                    Filesize            = this.FileAttributesHelper.GetFilesize(newFilePath),
                    HasThumbnail        = hasThumbnail,
                    CreatedAt           = now,
                    UpdatedAt           = now
                };

                // Insert into RethinkDB.
                this.FilesTable()
                    .Insert(file)
                    .RunWrite(this.Conn);


                files.Add(file);
            }

            return files;
        }

        /// <summary>
        /// Uploads a snippet to the server.
        /// </summary>
        /// <returns>The snippet.</returns>
        [HttpPut]
        [Route("Snippet")]
        public File Snippet(UploadedSnippet snippet)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Patch the specified id.
        /// </summary>
        /// <returns>The patch.</returns>
        /// <param name="id">Identifier.</param>
        [HttpPatch("{id}")]
        public File Patch(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// DELETE: /api/files/{id}
        /// 
        /// Deletes the specified file.
        /// </summary>
        /// 
        /// <returns>204 if the file was deleted successfully.</returns>
        /// 
        /// <param name="id">The file that should be deleted.</param>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {


            this.FilesTable()
                .Get(id)
                .Delete()
                .RunWrite(this.Conn);

            return StatusCode(204);
        }

        /// <summary>
        /// Shorthand method to retrieve a reference to the table where file
        /// metadata is stored.
        /// </summary>
        /// <returns>The table.</returns>
        private Table FilesTable()
        {
            return this.R
                .Db(this.Configuration["Database:Name"])
                .Table(this.Configuration["Database:Table"]);
        }
    }
}
