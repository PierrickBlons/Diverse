﻿using System;

namespace Diverse
{
    /// <summary>
    /// Represent a <see cref="Person"/> with some common characteristics.
    /// Very useful to test most of the applications dealing with humans.
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Gets the first name of the <see cref="Person"/> instance.
        /// </summary>
        public string FirstName { get; }

        /// <summary>
        /// Gets the last name of the <see cref="Person"/> instance.
        /// </summary>
        public string LastName { get; }

        /// <summary>
        /// Gets the gender of the <see cref="Person"/> instance.
        /// </summary>
        public Gender Gender { get; }
        
        /// <summary>
        /// Gets the title of the <see cref="Person"/> instance.
        /// </summary>
        public GenderTitle Title { get; }

        /// <summary>
        /// Gets the email of the <see cref="Person"/> instance.
        /// </summary>
        public string EMail { get; }

        /// <summary>
        /// Gets whether or not the <see cref="Person"/> instance is married.
        /// </summary>
        public bool IsMarried { get;  }

        /// <summary>
        /// Gets the age of the <see cref="Person"/> instance.
        /// </summary>
        public int Age { get; }

        internal Person(string firstName, string lastName, Gender gender, string eMail, bool isMarried, int age)
        {
            FirstName = firstName;
            LastName = lastName;
            Gender = gender;
            EMail = eMail;
            IsMarried = isMarried;
            Age = age;
            switch (gender)
            {
                case Gender.Male:
                    Title = GenderTitle.Mr;
                    break;
                case Gender.Female:
                    Title = isMarried ? GenderTitle.Mrs : GenderTitle.Ms;
                    break;
                case Gender.NonBinary:
                    Title = GenderTitle.Mx;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gender), gender, "not handled properly yet.");
            }
        }

        /// <summary>
        /// Returns this instance of <see cref="string"/>.
        /// </summary>
        /// <returns>The current string./</returns>
        public override string ToString()
        {
            var marriageStatus = IsMarried ? "married - " :string.Empty;
            var status = $"({marriageStatus}age: {Age} years)";

            return $"{Title}. {FirstName} {LastName.ToUpper()} ({Gender}) {EMail} {status}";
        }
    }
}