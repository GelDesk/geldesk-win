using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace GelDesk.FrameworkTests
{
    public class RpcMessageTest
    {
        #region Setup
        public RpcMessageTest(ITestOutputHelper output)
        {
            this.output = output;
            RpcSerializerConfig.ApplyGlobalDefaultSerializerSettings();
        }

        readonly ITestOutputHelper output;

        static string JsonFrame(string path, string data, int requestId)
        {
            // frame: [ "path (optional)", [data], requestId ]
            var frame = "[";
            if (path != null)
                frame += "\"" + path + "\",";
            frame += (data ?? "[]") + ","
                + requestId.ToString()
                + "]";
            return frame;
        }

        [Fact]
        public void Test_JsonFrame()
        {
            var data = JsonFrame("path/to/action", "[]", -1);
            Assert.Equal(@"[""path/to/action"",[],-1]", data);
        }

        #endregion

        [Fact]
        public void Serialize_NotificationWithLineBreaks()
        {
            var message = new RpcMessage()
            {
                Path = "path/to/action",
                Arguments = new JArray(@"A string
that has
line breaks.")
            };
            var data = message.Serialize();
            Assert.True(data != null, "Data should NOT be null.");
            output.WriteLine(data);
            Assert.True(data != null && data.IndexOf(Environment.NewLine) < 0, "Data frame should NOT have line breaks.");
        }

        [Fact]
        public void TryParse_NotificationNoArgs()
        {
            var path = "path/to/action";
            var data = JsonFrame(path, null, -1);
            RpcMessage message;
            Exception err;
            var parsed = RpcMessage.TryParse(data, out message, out err);
            Assert.True(parsed, "Could not parse notification message");
            Assert.True(message.IsNotification, "Should be a notification.");
            Assert.True(!message.IsRequest, "Should NOT be a request.");
            Assert.True(!message.IsResponse, "Should NOT be a response.");
            Assert.True(!message.HasError, "Should NOT have an error.");
            Assert.True(message.Arguments != null, "Should have data.");
            Assert.True(message.Arguments != null && message.Arguments.Count == 0,
                "Should have zero data elements.");
            Assert.True(message.Path == path, "Path doesn't match input.");
        }

        [Fact]
        public void TryParse_RequestNoArgs()
        {
            var path = "path/to/action";
            var data = JsonFrame(path, null, 99);
            RpcMessage message;
            Exception err;
            var parsed = RpcMessage.TryParse(data, out message, out err);
            Assert.True(parsed, "Could not parse request message");
            Assert.True(message.IsRequest, "Should be a request.");
            Assert.True(!message.IsNotification, "Should NOT be a notification.");
            Assert.True(!message.HasError, "Should NOT have an error.");
            Assert.True(message.Arguments != null, "Should have data.");
            Assert.True(message.Arguments != null && message.Arguments.Count == 0,
                "Should have zero data elements.");
        }

        [Fact]
        public void TryParse_ResponseWithError()
        {
            var data = JsonFrame(null,
                @"[{""message"": ""The message."", ""stack"": ""The stack.""}]",
                99);
            RpcMessage message;
            Exception err;
            var parsed = RpcMessage.TryParse(data, out message, out err);
            Assert.True(parsed, "Could not parse response with error message.");
            Assert.True(message.IsResponse, "Should be a response.");
            Assert.True(!message.IsRequest, "Should NOT be a request.");
            Assert.True(!message.IsNotification, "Should NOT be a notificaiton.");
            Assert.True(message.HasError, "Should have an error.");
            Assert.True(message.Arguments != null, "Should have data.");
            Assert.True(message.Arguments != null && message.Arguments.Count == 0,
                "Should have zero data elements.");
            Assert.True(message.Error.Message == "The message.",
                "Error message doesn't match input.");
            var errRpc = message.Error as RpcException;
            Assert.True(errRpc != null, "Should have an RpcException error.");
            Assert.True(errRpc.Stack == "The stack.",
                "Stack doesn't match input.");
        }

        [Fact]
        public void TryParse_ResponseWithErrorCode()
        {
            var data = JsonFrame(null, @"[500]", 99);
            RpcMessage message;
            Exception err;
            var parsed = RpcMessage.TryParse(data, out message, out err);
            Assert.True(parsed, "Could not parse response with error code.");
            Assert.True(message.IsResponse, "Should be a response.");
            Assert.True(!message.IsRequest, "Should NOT be a request.");
            Assert.True(!message.IsNotification, "Should NOT be a notification.");
            Assert.True(message.HasError, "Should have an error.");
            Assert.True(message.Arguments != null, "Should have data.");
            Assert.True(message.Arguments != null && message.Arguments.Count == 0,
                "Should have zero data elements.");
            Assert.True(message.Error.Message == "Internal error",
                "Error message doesn't match input.");
            var errRpc = message.Error as RpcException;
            Assert.True(errRpc != null, "Should have an RpcException error.");
            Assert.True(errRpc.Code == 500, "RpcException.Code does not match input.");
        }

        [Fact]
        public void TrySerialize_RequestWithArgs()
        {
            var message = SetupRequest("path/to/action", 0, "hello", null);
            var json = message.Serialize();
            Assert.True(json != null, "JSON should not be null.");
            output.WriteLine(json);
        }

        RpcMessage SetupRequest(string path, params JToken[] data)
        {
            var message = new RpcMessage();
            message.Path = path;
            message.Arguments = new JArray(data);
            message.RequestId = 99;
            return message;
        }
    }
}
