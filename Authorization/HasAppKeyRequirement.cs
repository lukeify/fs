using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Fs.Authorization
{
    public class HasAppKeyRequirement : IAuthorizationRequirement
    {
        public string Key { get; }

        public string KeyHeader { get; }

        public HasAppKeyRequirement(string key)
        {
            this.Key = key;
            this.KeyHeader = "Authorization";
        }

        /// <summary>
        /// Is the key correct?
        /// </summary>
        /// <returns><c>true</c>, if key correct was ised, <c>false</c> otherwise.</returns>
        /// <param name="keyClaim">Key claim.</param>
        public bool IsKeyCorrect(string keyClaim)
        {
            return "Bearer " + this.Key == keyClaim;
        }
    }
}
