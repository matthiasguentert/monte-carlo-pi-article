using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using System.Linq;

namespace Tests
{
    /// <summary>
    ///     Taken from Azure-SDK
    /// </summary>
    public static class TestUtils
    {
        public const string DummyBaseUrl = "https://batch-test.windows-int.net";
        public const string DummyAccountName = "Dummy";
        public const string DummyAccountKey = "ZmFrZQ==";
        public const string DummyToken = "ZmFrZQ==";

        public static BatchSharedKeyCredentials CreateDummySharedKeyCredential()
        {
            BatchSharedKeyCredentials credentials = new BatchSharedKeyCredentials(
                TestUtils.DummyBaseUrl,
                TestUtils.DummyAccountName,
                TestUtils.DummyAccountKey);

            return credentials;
        }

        public static BatchClient CreateDummyClient()
        {
            var client = BatchClient.Open(CreateDummySharedKeyCredential());
            client.CustomBehaviors = client.CustomBehaviors.Where(behavior => !(behavior is RetryPolicyProvider)).ToList();

            return client;
        }
    }
}
