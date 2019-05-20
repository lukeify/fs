using System;
using System.Security.Cryptography;
using System.Text;
using Fs.Models;
using Microsoft.Extensions.Configuration;
using RethinkDb.Driver.Net;

namespace Fs.Services
{
    public class FilenameGenerator
    {
        public RethinkDb.Driver.RethinkDB R = RethinkDb.Driver.RethinkDB.R;

        public IConfiguration Configuration { get; set; }

        public IConnection Conn { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fs.Services.FilenameGenerator"/> class.
        /// </summary>
        /// 
        /// <param name="configuration">Configuration.</param>
        /// <param name="conn">Conn.</param>
        /// 
        public FilenameGenerator(IConfiguration configuration, IConnection conn)
        {
            this.Configuration = configuration;
            this.Conn = conn;
        }

        /// <summary>
        /// Generates a name for the file.
        /// </summary>
        /// 
        /// <returns>The name that has been generated.</returns>
        /// 
        public String GenerateName()
        {
            int size = 1;
            int passesHad = 0;
            int passesAllowedPerSize = 3;

            string potentialName = String.Empty;

            do
            {
                if (passesHad == passesAllowedPerSize)
                {
                    size++;
                    passesHad = 0;
                }

                potentialName = this.GenerateNameOption(size);

                passesHad++;
            }
            while (this.IsNameTaken(potentialName));

            return potentialName;
        }

        /// <summary>
        /// Produces a cryptographically-generated name option which may or may not
        /// be valid.
        /// </summary>
        /// 
        /// <returns>The potential name.</returns>
        /// 
        /// <param name="size">The size of the name to be generated.</param>
        /// 
        private string GenerateNameOption(int size)
        {
            char[] chars = "abcdefghijklmnopqrstuvwxyz0123456789-_".ToCharArray();
            byte[] data = new byte[size];

            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Evaluates whether the name option is taken, or has already been used.
        /// </summary>
        /// 
        /// <returns>
        /// <c>true</c> if the name has been used, or <c>false</c> if it 
        /// has not.
        /// </returns>
        /// 
        /// <param name="name">The name to check for whether it's been taken.</param>
        /// 
        private bool IsNameTaken(string name)
        {
            File preexistingFile = this.R
                .Db(this.Configuration["Database:Name"])
                .Table(this.Configuration["Database:Table"])
                .Get(name)
                .RunAtom<File>(this.Conn);

            return preexistingFile != null;
        }
    }
}
