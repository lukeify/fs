using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Fs.Authorization
{
    public class HasAppKeyHandler : AuthorizationHandler<HasAppKeyRequirement>
    {
        private HttpContext HttpContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Fs.Authorization.HasAppKeyHandler"/> class.
        /// </summary>
        /// <param name="httpContext">Http context.</param>
        public HasAppKeyHandler(HttpContext httpContext)
        {
            this.HttpContext = httpContext;
        }

        /// <summary>
        /// If the key does not exist on the header, or the key is not correct, return
        /// a failure, which will result in a 403 Forbidden error. Otherwise, the 
        /// policy will succeed.
        /// </summary>
        /// <returns>The requirement async.</returns>
        /// <param name="context">Context.</param>
        /// <param name="requirement">Requirement.</param>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasAppKeyRequirement requirement)
        {
            if (!this.HttpContext.Request.Headers.Keys.Contains(requirement.KeyHeader))
            {
                context.Fail();
            }

            if (!requirement.IsKeyCorrect(this.HttpContext.Request.Headers[requirement.KeyHeader]))
            {
                context.Fail();
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
