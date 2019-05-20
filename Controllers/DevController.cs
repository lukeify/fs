﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RethinkDb.Driver.Model;
using RethinkDb.Driver.Net;

namespace Fs.Controllers
{
    [Route("api/[controller]/[action]")]
    public class DevController : Controller
    {
        private RethinkDb.Driver.RethinkDB R = RethinkDb.Driver.RethinkDB.R;

        private IHostingEnvironment Environment { get; set; }

        private IConfiguration Configuration { get; set; }

        private IConnection Conn { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fs.Controllers.DevController"/> class.
        /// </summary>
        /// 
        public DevController(IHostingEnvironment environment, IConfiguration configuration, IConnection conn)
        {
            this.Environment = environment;
            this.Configuration = configuration;
            this.Conn = conn;
        }

        /// <summary>
        /// // GET: api/dev/wipe
        /// 
        /// Wipes all content from the RethinkDB instance, and then removes all files.
        /// Note that this is only possible in development. Attempting to call this method
        /// in production will fail.
        /// </summary>
        /// 
        /// <returns>
        /// An HTTP status code indicating the outcome of the wipe attempt.
        /// </returns>
        /// 
        [HttpGet]
        public IActionResult Wipe()
        {
            if (!this.Environment.IsDevelopment())
            {
                return StatusCode(403);
            }

            this.R
                .Db(this.Configuration["Database:Name"])
                .Table(this.Configuration["Database:Table"])
                .Delete()
                .RunWrite(this.Conn)
                .AssertNoErrors();

            string[] directoriesToClear = new string[2];
            directoriesToClear[0] = this.Configuration["Fs:Dir"];
            directoriesToClear[1] = this.Configuration["Fs:DefaultThumbDir"];

            foreach (string directoryToClear in directoriesToClear)
            {
                System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(directoryToClear);
                foreach (System.IO.FileInfo file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }
            }

            return NoContent();
        }

        /// <summary>
        /// GET: /api/dev/init
        /// 
        /// Initialize the system, by creating the necessary database and table
        /// if it doesn't already exist.
        /// </summary>
        /// 
        /// <returns>A status code representing the outcome of the operation</returns>
        /// 
        [HttpGet]
        public IActionResult Init()
        {
            if (!this.Environment.IsDevelopment())
            {
                return StatusCode(403);
            }

            // Chech for existence of the database.
            List<string> databases = this.R
                .DbList()
                .RunResult<List<string>>(this.Conn);

            string foundDb = databases
                .FirstOrDefault(db => db == this.Configuration["Database:Name"]);

            // Create the db.
            if (foundDb == null)
            {
                this.R
                    .DbCreate(this.Configuration["Database:Name"])
                    .RunWrite(this.Conn)
                    .AssertDatabasesCreated(1);
            }

            // Check for the existence of the table.
            List<string> tables = this.R
                .Db(this.Configuration["Database:Name"])
                .TableList()
                .RunResult<List<string>>(this.Conn);

            string foundTable = tables
                .FirstOrDefault(t => t == this.Configuration["Database:Table"]);

            // Create the table.
            if (foundTable == null)
            {
                this.R
                    .Db(this.Configuration["Database:Name"])
                    .TableCreate(this.Configuration["Database:Table"])
                    .OptArg("primary_key", "Name")
                    .RunWrite(this.Conn)
                    .AssertTablesCreated(1);
            }

            return NoContent();
        }
    }
}
