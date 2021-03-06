﻿namespace Diverse
{
    /// <summary>
    /// Fuzz <see cref="Person"/> related information.
    /// </summary>
    public interface IFuzzPersons
    {
        /// <summary>
        /// Generates a 'Diverse' first name (i.e. from all around the world and different cultures).
        /// </summary>
        /// <param name="gender">The <see cref="Gender"/> to be used as indication (optional).</param>
        /// <returns>A 'Diverse' first name.</returns>
        string GenerateFirstName(Gender? gender = null);

        /// <summary>
        /// Generates a 'Diverse' last name (i.e. from all around the world and different cultures).
        /// </summary>
        /// <param name="firstName">The first name of this person.</param>
        /// <returns>A 'Diverse' last name.</returns>
        string GenerateLastName(string firstName);

        /// <summary>
        /// Generates a 'Diverse' <see cref="Person"/> (i.e. from all around the world and different cultures). 
        /// </summary>
        /// <param name="gender">The (optional) <see cref="Gender"/> of this <see cref="Person"/></param>
        /// <returns>A 'Diverse' <see cref="Person"/> instance.</returns>
        Person GeneratePerson(Gender? gender = null);

        /// <summary>
        /// Generates a random Email.
        /// </summary>
        /// <param name="firstName">The (optional) first name for this Email</param>
        /// <param name="lastName">The (option) last name for this Email.</param>
        /// <returns>A random Email.</returns>
        string GenerateEMail(string firstName = null, string lastName = null);

        /// <summary>
        /// Generates a password following some common rules asked on the internet.
        /// </summary>
        /// <returns>The generated password</returns>
        string GeneratePassword(int? minSize = null, int? maxSize = null, bool? includeSpecialCharacters = null);
    }
}