using NFluent;
using NUnit.Framework;

namespace Diverse.Tests
{
    [TestFixture]
    public class FuzzerWithClassShould
    {
        [Test]
        public void Be_able_to_handle_readonly_string_properties()
        {
            Fuzzer fuzzer = new Fuzzer(43789743);

            DummyReadOnlyClass dummyClass = fuzzer.GenerateInstance<DummyReadOnlyClass>();

            Check.That(dummyClass.StringProperty).IsNull();
        }
        
        [Test]
        public void Be_able_to_generate_a_random_adjective_for_a_writable_string_property()
        {
            Fuzzer fuzzer = new Fuzzer(6739478);

            DummyWritableClass dummyClass = fuzzer.GenerateInstance<DummyWritableClass>();

            Check.That(dummyClass.StringProperty).IsEqualTo("hopeful");
        }

        [Test]
        public void Be_able_to_generate_a_diverse_firstname_for_any_object_containing_a_firstname_property()
        {
            Fuzzer fuzzer = new Fuzzer(23984398);

            Player dummyClass = fuzzer.GenerateInstance<Player>();

            Check.That(dummyClass.FirstName).IsEqualTo("Kevin");
        }

        [Test]
        public void Be_able_to_generate_a_diverse_lastname_for_any_object_containing_a_lastname_property()
        {
            Fuzzer fuzzer = new Fuzzer(23984398);

            Player dummyClass = fuzzer.GenerateInstance<Player>();

            Check.That(dummyClass.LastName).IsEqualTo("Fantasia");
        }
    }

    internal class DummyWritableClass
    {
        public string StringProperty { get; set; }
    }

    internal class DummyReadOnlyClass
    {
        public string StringProperty { get; }
    }

    internal class Player
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}