﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Diverse
{
    /// <summary>
    /// Allows to generate lots of combination of things. <see cref="Fuzzer"/> are very useful to detect hard coded values in our implementations.
    /// Note: you can instantiate another Deterministic Fuzzer by providing it the Seed you want to reuse.
    /// </summary>
    public partial class Fuzzer : IFuzz
    {
        private Random _internalRandom;

        private static char[] _specialCharacters = "+-_$%£&!?*$€'|[]()".ToCharArray();
        private static char[] _upperCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static char[] _lowerCharacters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static char[] _numericCharacters = "0123456789".ToCharArray();

        /// <summary>
        /// Generates a DefaultSeed. Important to keep a trace of the used seed so that we can reproduce a failing situation with <see cref="Fuzzer"/> involved.
        /// </summary>
        public int Seed { get; }
        
        /// <summary>
        /// Gets the name of this <see cref="Fuzzer"/> instance.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the Random instance to be used when we want to create a new extension method for the <see cref="Fuzzer"/>.
        /// <remarks>The use of explicit interface implementation for this property is made on purpose in order to hide this internal mechanic details from the Fuzzer end-user code.</remarks>
        /// </summary>
        Random IFuzz.Random => _internalRandom;

        /// <summary>
        /// Gives easy access to the <see cref="IFuzz.Random"/> explicit implementation.
        /// </summary>
        private Random InternalRandom => ((IFuzz)this).Random;

        /// <summary>
        /// Sets the way you want to log or receive what the <see cref="Fuzzer"/> has to say about every generated seeds used for every fuzzer instance and test.
        /// </summary>
        public static Action<string> Log { get; set; }

        /// <summary>
        /// Instantiates a <see cref="Fuzzer"/>.
        /// </summary>
        /// <param name="seed">The seed if you want to reuse in order to reproduce the very same conditions of another (failing) test.</param>
        /// <param name="name">The name you want to specify for this <see cref="Fuzzer"/> instance (useful for debuging purpose).</param>
        public Fuzzer(int? seed = null, string name = null)
        {
            var seedWasProvided = seed.HasValue;

            seed = seed ?? new Random().Next(); // the seed is not specified? pick a random one for this Fuzzer instance.
            Seed = seed.Value;

            _internalRandom = new Random(seed.Value);

            _fuzzStrings = new FuzzerStrings(_internalRandom);

            name = name ?? GenerateFuzzerName();
            Name = name;

            LogSeedAndTestInformations(seed.Value, seedWasProvided, name);
        }

        /// <summary>
        /// Generates a random integer value between a min (inclusive) and a max (exclusive) value.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The inclusive upper bound of the random number returned.</param>
        /// <returns>An integer value generated randomly.</returns>
        public int GenerateInteger(int minValue, int maxValue)
        {
            // Adjust the inclusiveness of the Fuzzer API to the exclusiveness of the Random API.
            maxValue = (maxValue == int.MaxValue) ? maxValue : maxValue + 1;
            
            return InternalRandom.Next(minValue, maxValue);
        }

        /// <summary>
        /// Generates a random integer value.
        /// </summary>
        /// <returns>An integer value generated randomly.</returns>
        public int GenerateInteger()
        {
            return GenerateInteger(int.MinValue, int.MaxValue);
        }

        /// <summary>
        /// Generates a random positive integer value.
        /// </summary>
        /// <param name="maxValue">The inclusive upper bound of the random number returned.</param>
        /// <returns>A positive integer value generated randomly.</returns>
        public int GeneratePositiveInteger(int? maxValue = null)
        {
            maxValue = maxValue ?? int.MaxValue;

            return GenerateInteger(0, maxValue.Value);
        }

        /// <summary>
        /// Generates a random positive decimal value.
        /// </summary>
        /// <returns>A positive decimal value generated randomly.</returns>
        public decimal GeneratePositiveDecimal()
        {
            return Convert.ToDecimal(GenerateInteger(0, int.MaxValue));
        }

        /// <summary>
        /// Generates a 'Diverse' first name (i.e. from all around the world and different cultures).
        /// </summary>
        /// <param name="gender">The <see cref="Gender"/> to be used as indication (optional).</param>
        /// <returns>A 'Diverse' first name.</returns>
        public string GenerateFirstName(Gender? gender = null)
        {
            string[] firstNameCandidates;
            if (gender == null)
            {
                var isFemale = HeadsOrTails();
                firstNameCandidates = isFemale ? Female.FirstNames : Male.FirstNames;
            }
            else
            {
                firstNameCandidates = gender == Gender.Female ? Female.FirstNames : Male.FirstNames;
            }

            var randomLocaleIndex = InternalRandom.Next(0, firstNameCandidates.Length);

            return firstNameCandidates[randomLocaleIndex];
        }

        /// <summary>
        /// Generates a 'Diverse' first name (i.e. from all around the world and different cultures).
        /// </summary>
        /// <param name="firstName">The first name of this person.</param>
        /// <returns>A 'Diverse' last name.</returns>
        public string GenerateLastName(string firstName)
        {
            Continent continent = FindContinent(firstName);

            var lastNames = LastNames.PerContinent[continent];

            var randomLocaleIndex = InternalRandom.Next(0, lastNames.Length);

            return lastNames[randomLocaleIndex];
        }

        /// <summary>
        /// Generates a 'Diverse' <see cref="Person"/> (i.e. from all around the world and different cultures). 
        /// </summary>
        /// <param name="gender">The (optional) <see cref="Gender"/> of this <see cref="Person"/></param>
        /// <returns>A 'Diverse' <see cref="Person"/> instance.</returns>
        public Person GeneratePerson(Gender? gender = null)
        {
            if (gender == null)
            {
                var isFemale = HeadsOrTails();
                if (isFemale)
                {
                    gender = Gender.Female;
                }
                else
                {
                    var isNonBinary = HeadsOrTails();
                    gender = isNonBinary ? Gender.NonBinary : Gender.Male;
                }
            }

            var firstName = GenerateFirstName(gender);
            var lastName = GenerateLastName(firstName);
            var eMail = GenerateEMail(firstName, lastName);
            var isMarried = HeadsOrTails();
            var age = GenerateInteger(18, 97);

            return new Person(firstName, lastName, gender.Value, eMail, isMarried, age);
        }

        /// <summary>
        /// Generates a random Email.
        /// </summary>
        /// <param name="firstName">The (optional) first name for this Email</param>
        /// <param name="lastName">The (option) last name for this Email.</param>
        /// <returns>A random Email.</returns>
        public string GenerateEMail(string firstName = null, string lastName = null)
        {
            if (firstName == null)
            {
                firstName = GenerateFirstName();
            }

            if (lastName == null)
            {
                lastName = GenerateLastName(firstName);
            }

            var domainNames = new string[] { "kolab.com", "protonmail.com", "gmail.com", "yahoo.fr", "42skillz.com", "gmail.com", "ibm.com", "gmail.com", "yopmail.com", "microsoft.com", "gmail.com", "aol.com" };
            var index = InternalRandom.Next(0, domainNames.Length);

            var domainName = domainNames[index];


            if (HeadsOrTails())
            {
                var shortVersion = $"{firstName.Substring(0, 1)}{lastName}@{domainName}".ToLower();
                shortVersion = TransformIntoValidEmailFormat(shortVersion);
                return shortVersion;
            }

            var longVersion = $"{firstName}.{lastName}@{domainName}".ToLower();
            longVersion = TransformIntoValidEmailFormat(longVersion);
            return longVersion;
        }


        private static void LogSeedAndTestInformations(int seed, bool seedWasProvided, string fuzzerName)
        {
            var testName = FindTheNameOfTheTestInvolved();

            if (Log == null)
            {
                throw new FuzzerException($"You must (at least once) set a value for the static {nameof(Log)} property of the {nameof(Fuzzer)} type in order to be able to retrieve the seeds used for each one of your Fuzzer/Tests (which is a prerequisite for deterministic test runs). Suggested value: ex. {nameof(Fuzzer)}.{nameof(Log)} = TestContext.WriteLine; in the [OneTimeSetUp] initialization method of your [SetUpFixture] class.");
            }

            Log($"----------------------------------------------------------------------------------------------------------------------");
            if (seedWasProvided)
            {
                Log($"--- Fuzzer (\"{fuzzerName}\") instantiated from a provided seed ({seed})");
                Log($"--- from the test: {testName}()");
            }
            else
            {
                Log($"--- Fuzzer (\"{fuzzerName}\") instantiated with the seed ({seed})");
                Log($"--- from the test: {testName}()");
                Log($"--- Note: you can instantiate another Fuzzer with that very same seed in order to reproduce the exact test conditions");
            }

            Log($"----------------------------------------------------------------------------------------------------------------------");
        }

        private static string FindTheNameOfTheTestInvolved()
        {
            var testName = "(not found)";
            try
            {
                var stackTrace = new StackTrace();

                var testMethod = stackTrace.GetFrames()
                    .Select(sf => sf.GetMethod())
                    .First(IsATestMethod);

                testName = $"{testMethod.DeclaringType.Name}.{testMethod.Name}";
            }
            catch
            { }

            return testName;
        }
        private static bool IsATestMethod(MethodBase mb)
        {
            var attributeTypes = mb.CustomAttributes.Select(c => c.AttributeType);

            var hasACustomAttributeOfTypeTest = attributeTypes.Any(y => (y.Name == "TestAttribute" || y.Name == "TestCaseAttribute" || y.Name == "Fact"));

            if (hasACustomAttributeOfTypeTest)
            {
                return true;
            }

            return hasACustomAttributeOfTypeTest;
        }

        private string GenerateFuzzerName(bool upperCased = true)
        {
            // We are explicitly not using the Random field here to prevent from doing side effects on the deterministic fuzzer instances (depending on whether or not we specified a name)
            var index = new Random().Next(0, 1500);

            return $"fuzzer{index}";
        }

        private static Continent FindContinent(string firstName)
        {
            Continent continent;
            var contextualizedFirstName = Male.ContextualizedFirstNames.FirstOrDefault(c => c.FirstName == firstName);
            if (contextualizedFirstName != null)
            {
                continent = contextualizedFirstName.Origin;
            }
            else
            {
                contextualizedFirstName = Female.ContextualizedFirstNames.FirstOrDefault(c => c.FirstName == firstName);
                if (contextualizedFirstName != null)
                {
                    continent = contextualizedFirstName.Origin;
                }
                else
                {
                    continent = Continent.Africa;
                }
            }

            return continent;
        }

        private string TransformIntoValidEmailFormat(string eMail)
        {
            var validFormat = eMail.Replace(' ', '-');
            validFormat = RemoveDiacritics(validFormat);

            return validFormat;
        }

        // from https://stackoverflow.com/questions/249087/how-do-i-remove-diacritics-accents-from-a-string-in-net
        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Flips a coin.
        /// </summary>
        /// <returns><b>True</b> if Heads; <b>False</b> otherwise (i.e. Tails).</returns>
        public bool HeadsOrTails()
        {
            return InternalRandom.Next(0, 2) == 1;
        }

        /// <summary>
        /// Generates a password following some common rules asked on the internet.
        /// </summary>
        /// <returns>The generated password</returns>
        public string GeneratePassword(int? minSize = null, int? maxSize = null, bool? includeSpecialCharacters = null)
        {
            var defaultMinSize = 7;
            var defaultMaxSize = 12;

            var minimumSize = minSize ?? defaultMinSize;
            var maximumSize = maxSize ?? defaultMaxSize;

            CheckGuardMinAndMaximumSizes(minSize, maxSize, minimumSize, maximumSize, defaultMinSize, defaultMaxSize);

            var passwordSize = InternalRandom.Next(minimumSize, maximumSize + 1);

            var pwd = new StringBuilder(passwordSize);
            for (var i = 0; i < passwordSize; i++)
            {
                if ((i == 0 || i == 10) && (includeSpecialCharacters.HasValue && includeSpecialCharacters.Value))
                {
                    pwd.Append(_specialCharacters[InternalRandom.Next(0, _specialCharacters.Length)]);
                    continue;
                }

                if (i == 4 || i == 14)
                {
                    pwd.Append(_upperCharacters[InternalRandom.Next(1, 26)]);
                    continue;
                }

                if (i == 6 || i == 13)
                {
                    pwd.Append(_numericCharacters[InternalRandom.Next(4, 10)]);
                    continue;
                }

                if (i == 3 || i == 9)
                {
                    pwd.Append(_numericCharacters[InternalRandom.Next(1, 5)]);
                    continue;
                }

                // by default
                pwd.Append(_upperCharacters[InternalRandom.Next(1, 26)]);
            }

            return pwd.ToString();
        }        

        private static void CheckGuardMinAndMaximumSizes(int? minSize, int? maxSize, int minimumSize, int maximumSize, int defaultMinSize, int defaultMaxSize)
        {
            if (minimumSize > maximumSize)
            {
                var parameterName = minSize == null ? "maxSize" : "minSize";
                if (minSize.HasValue && maxSize.HasValue)
                {
                    parameterName = "maxSize";
                }

                throw new ArgumentOutOfRangeException(parameterName,
                    $"maxSize ({maximumSize}) can't be inferior to minSize({minimumSize}). Specify 2 values if you don't want to use the default values of the library (i.e.: [{defaultMinSize}, {defaultMaxSize}]).");
            }
        }

        /// <summary>
        /// Generates a random <see cref="DateTime"/>.
        /// </summary>
        /// <returns>A <see cref="DateTime"/> value generated randomly.</returns>
        public DateTime GenerateDateTime()
        {
            return GenerateDateTimeBetween(DateTime.MinValue, DateTime.MaxValue);
        }

        /// <summary>
        /// Generates a random <see cref="DateTime"/> in a Time Range.
        /// </summary>
        /// <param name="minValue">The minimum inclusive boundary of the Time Range for this <see cref="DateTime"/> generation.</param>
        /// <param name="maxValue">The maximum inclusive boundary of the Time Range for this <see cref="DateTime"/> generation.</param>
        /// <returns>A <see cref="DateTime"/> instance between the min and the max inclusive boundaries.</returns>
        public DateTime GenerateDateTimeBetween(DateTime minValue, DateTime maxValue)
        {
            var nbDays = (maxValue - minValue).Days;

            var midInterval = (minValue.AddDays(nbDays/2));

            var maxDaysAllowedBefore = (midInterval - minValue).Days;
            var maxDaysAllowedAfter = (maxValue - midInterval).Days;
            var maxDays = Math.Min(maxDaysAllowedBefore, maxDaysAllowedAfter);

            return midInterval.AddDays(GenerateInteger(-maxDays, maxDays));
        }

        /// <summary>
        /// Generates a random <see cref="DateTime"/> in a Time Range.
        /// </summary>
        /// <param name="minValue">The minimum inclusive boundary of the Time Range for this <see cref="DateTime"/> generation, specified as a yyyy/MM/dd string.</param>
        /// <param name="maxValue">The maximum inclusive boundary of the Time Range for this <see cref="DateTime"/> generation, specified as a yyyy/MM/dd string.</param>
        /// <returns>A <see cref="DateTime"/> instance between the min and the max inclusive boundaries.</returns>
        public DateTime GenerateDateTimeBetween(string minDate, string maxDate)
        {
            var minDateOk = DateTime.TryParseExact(minDate, "yyyy/MM/dd", null, DateTimeStyles.None,  out var minDateTime);
            var maxDateOk = DateTime.TryParseExact(maxDate, "yyyy/MM/dd", null, DateTimeStyles.None, out var maxDateTime);

            //if (!minDateOk || !maxDateOk)
            //{
            //    var paramName = string.Empty;
            //    var message = string.Empty;

            //    if (!minDateOk)
            //    {

            //    }
            //    throw new ArgumentException(message, paramName);
            //}

            return GenerateDateTimeBetween(minDateTime, maxDateTime);
        }

        private IFuzzStrings _fuzzStrings;
        public string GenerateString(Feeling? feeling = null)
        {
            return _fuzzStrings.GenerateString(feeling);
        }

        public T GenerateInstance<T>()
        {
            Func<string> defaultGenerateFirstName = () => GenerateFirstName();
            Func<string> defaultGenerateAdjective = () => GenerateString();

            Generators generators = new Generators() {
                AdjectiveGenerator = defaultGenerateAdjective,
                NameGenerator = defaultGenerateFirstName,
                LastNameGenerator = GenerateLastName
            };

            return new FuzzerClass().GenerateInstance<T>(generators);
        }
    }
}