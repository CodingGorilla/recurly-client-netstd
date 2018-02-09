using System;
using Xunit;

namespace Recurly.Tests
{
    public class AccountsTests
    {
        public AccountsTests()
        {
            RecurlyClient = new RecurlyClient("client-lib-test", "8f1359864cfa4f378542d639e655229c");
        }

        public RecurlyClient RecurlyClient { get; set; }
    }
}
