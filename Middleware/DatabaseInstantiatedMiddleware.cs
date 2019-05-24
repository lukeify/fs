using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RethinkDb.Driver.Model;
using RethinkDb.Driver.Net;

namespace Fs.Middleware
{
    public class DatabaseInstantiatedMiddleware
    {
        private RequestDelegate Next { get; set; }

        private IConfiguration Configuration { get; set; }

        private RethinkDb.Driver.RethinkDB R = RethinkDb.Driver.RethinkDB.R;

        private IConnection Conn { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fs.Middleware.DatabaseInstantiatedMiddleware"/> class.
        /// </summary>
        /// 
        /// <param name="next">When called, continues the request pipeline.</param>
        /// 
        public DatabaseInstantiatedMiddleware(RequestDelegate next, IConfiguration configuration, IConnection conn)
        {
            this.Next = next;
            this.Configuration = configuration;
            this.Conn = conn;
        }

        /// <summary>
        /// Invokes the middleware asynchronously. We check if the rethinkdb database exists and the items table
        /// exists. If either don't, they are instantiated.
        /// </summary>
        /// 
        /// <returns>The async.</returns>
        /// 
        /// <param name="context">Context.</param>
        /// 
        public async Task InvokeAsync(HttpContext context)
        {
            if (!this.DoesDatabaseExist())
            {
                this.CreateDatabase();
            }

            if (!this.DoesTableExist())
            {
                this.CreateTable();
            }

            await this.Next(context);
        }

        /// <summary>
        /// Checks if the database exists.
        /// </summary>
        /// 
        /// <returns><c>true</c>, if it exists, <c>false</c> otherwise.</returns>
        /// 
        private bool DoesDatabaseExist()
        {
            List<string> databases = this.R
                .DbList()
                .RunResult<List<string>>(this.Conn);

            try
            {
                databases.First(db => db == this.Configuration["Database:Name"]);
                return true;
            }
            catch (InvalidOperationException e)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates the specified database.
        /// </summary>
        private void CreateDatabase()
        {
            this.R
                .DbCreate(this.Configuration["Database:Name"])
                .RunWrite(this.Conn)
                .AssertDatabasesCreated(1);
        }


        /// <summary>
        /// Checks if the database table specified exists.
        /// </summary>
        /// 
        /// <returns><c>true</c>, if it exists, <c>false</c> otherwise.</returns>
        /// 
        private bool DoesTableExist()
        {
            List<string> tables = this.R
                .Db(this.Configuration["Database:Name"])
                .TableList()
                .RunResult<List<string>>(this.Conn);

            try
            {
                tables.First(t => t == this.Configuration["Database:Table"]);
                return true;
            }
            catch (InvalidOperationException e)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates the specified table in the database.
        /// </summary>
        private void CreateTable()
        {
            this.R
                .Db(this.Configuration["Database:Name"])
                .TableCreate(this.Configuration["Database:Table"])
                .OptArg("primary_key", "Name")
                .RunWrite(this.Conn)
                .AssertTablesCreated(1);
        }
    }

    public static class DatabaseInstantiatedMiddlewareExtensions
    {
        public static IApplicationBuilder EnsureDatabaseExists(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DatabaseInstantiatedMiddleware>();
        }
    }
}
