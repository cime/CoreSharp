using System;

namespace CoreSharp.DataAccess
{
    public interface IUser : IEntity<long>
    {
        string UserName { get; set; }
        string NormalizedUserName { get; set; }

        string Password { get; set; }


        string Email { get; set; }
        string NormalizedEmail { get; set; }
        bool EmailConfirmed { get; set; }

        string PhoneNumber { get; set; }
        bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// Gets or sets the date and time, in UTC, when any user lockout ends.
        /// </summary>
        /// <remarks>A value in the past means the user is not locked out.</remarks>
        DateTimeOffset? LockoutEnd { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the user could be locked out.
        /// </summary>
        /// <value>True if the user could be locked out, otherwise false.</value>
        bool LockoutEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of failed login attempts for the current user.
        /// </summary>
        int AccessFailedCount { get; set; }

        bool Active { get; set; }
    }
}
