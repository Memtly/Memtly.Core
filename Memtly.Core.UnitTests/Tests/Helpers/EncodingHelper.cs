using Memtly.Core.Helpers;

namespace Memtly.Core.UnitTests.Tests.Helpers
{
    public class EncodingHelperTests
    {
        public EncodingHelperTests() 
        {
        }

        [SetUp]
        public void Setup()
        {
        }

        [TestCase("This is a test...", "VGhpcyBpcyBhIHRlc3QuLi4=")]
        [TestCase("TESTING", "VEVTVElORw==")]
        [TestCase("kdsjfhksjhfkdshfkjhskjhfkhskjdfhksdhfkhsdkjfhksdhfkjdshf@Asd!!&$^$$**==", "a2RzamZoa3NqaGZrZHNoZmtqaHNramhma2hza2pkZmhrc2RoZmtoc2RramZoa3NkaGZramRzaGZAQXNkISEmJF4kJCoqPT0=")]
        public void EncrytpionHelper_Base64Encode(string input, string output)
        {
            var actual = EncodingHelper.Base64Encode(input);

            Assert.That(actual, Is.EqualTo(output));
        }

        [TestCase("VGhpcyBpcyBhIHRlc3QuLi4=", "This is a test...")]
        [TestCase("VEVTVElORw==", "TESTING")]
        [TestCase("a2RzamZoa3NqaGZrZHNoZmtqaHNramhma2hza2pkZmhrc2RoZmtoc2RramZoa3NkaGZramRzaGZAQXNkISEmJF4kJCoqPT0=", "kdsjfhksjhfkdshfkjhskjhfkhskjdfhksdhfkhsdkjfhksdhfkjdshf@Asd!!&$^$$**==")]
        public void EncrytpionHelper_Base64Decode(string input, string output)
        {
            var actual = EncodingHelper.Base64Decode(input);

            Assert.That(actual, Is.EqualTo(output));
        }
    }
}