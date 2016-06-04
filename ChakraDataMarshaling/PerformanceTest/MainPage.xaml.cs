using Chakra;
using ChakraDataMarshaling;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PerformanceTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            TestingProgressRing.IsActive = true;
            var output = await RunAsync();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                OutputTextBlock.Text = output;
                TestingProgressRing.IsActive = false;
            });
        }

        private async void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            TestingProgressRing.IsActive = true;
            InputTextBox.Text = "";
            var output = await RunAsync();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                OutputTextBlock.Text = output;
                TestingProgressRing.IsActive = false;
            });
        }

        private Task<string> RunAsync()
        {
            var json = InputTextBox.Text;
            var repeat = int.Parse(RepeatTextBox.Text);

            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    return "";
                }

                var sb = new StringBuilder();

                using (var runtime = JavaScriptRuntime.Create())
                {
                    var context = runtime.CreateContext();
                    JavaScriptContext.Current = context;

                    var test = new Test(runtime, repeat);
                    var parseConverter = new JsonParseConverter();
                    var stringifyConverter = new JsonStringifyConverter();
                    var dotNetValue = JToken.Parse(json);
                    var chakraValue = parseConverter.Convert(dotNetValue);

                    sb.AppendLine("Json.NET -> Chakra with JSON.parse");
                    sb.AppendLine("----------------------------------");
                    RunDotNetToChakra(dotNetValue, test, parseConverter.Convert);
                    sb.AppendLine(test.ToString());

                    sb.AppendLine("Chakra -> Json.NET with JSON.stringify");
                    sb.AppendLine("--------------------------------------");
                    RunChakraToDotNet(chakraValue, test, stringifyConverter.Convert);
                    sb.AppendLine(test.ToString());

                    sb.AppendLine("Json.NET -> Chakra with object model");
                    sb.AppendLine("------------------------------------");
                    RunDotNetToChakra(dotNetValue, test, JTokenToJavaScriptValueConverter.Convert);
                    sb.AppendLine(test.ToString());

                    sb.AppendLine("Chakra -> Json.NET with object model");
                    sb.AppendLine("------------------------------------");
                    RunChakraToDotNet(chakraValue, test, JavaScriptValueToJTokenConverter.Convert);
                    sb.AppendLine(test.ToString());

                    JavaScriptContext.Current = JavaScriptContext.Invalid;
                }

                return sb.ToString();
            });
        }

        private void RunDotNetToChakra(JToken value, Test test, Func<JToken, JavaScriptValue> func)
        {
            test.Start();

            for (var i = 0; i < test.Repeat; ++i)
            {
                func(value);
            }

            test.Stop();
        }

        private void RunChakraToDotNet(JavaScriptValue value, Test test, Func<JavaScriptValue, JToken> func)
        {
            test.Start();

            for (var i = 0; i < test.Repeat; ++i)
            {
                func(value);
            }

            test.Stop();
        }

        public class Test
        {
            private readonly Stopwatch _stopwatch = new Stopwatch();
            private readonly JavaScriptBeforeCollectCallback _callback;

            private long _chakraCollections;
            private long _dotNetGen0;
            private long _dotNetGen1;
            private long _dotNetGen2;

            public Test(JavaScriptRuntime runtime, int repeat)
            {
                _callback = OnCollect;
                runtime.SetBeforeCollectCallback(IntPtr.Zero, _callback);

                Repeat = repeat;
            }

            public int Repeat { get; }

            public void Start()
            {
                GC.Collect();

                _chakraCollections = 0;
                _dotNetGen0 = GC.CollectionCount(0);
                _dotNetGen1 = GC.CollectionCount(1);
                _dotNetGen2 = GC.CollectionCount(2);
                _stopwatch.Restart();
            }

            public void Stop()
            {
                _stopwatch.Stop();
                _dotNetGen0 = GC.CollectionCount(0) - _dotNetGen0;
                _dotNetGen1 = GC.CollectionCount(1) - _dotNetGen1;
                _dotNetGen2 = GC.CollectionCount(2) - _dotNetGen2;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Duration:     {_stopwatch.ElapsedMilliseconds} ms");
                sb.AppendLine($"Chakra GC:    {_chakraCollections}");
                sb.AppendLine($".NET GC Gen0: {_dotNetGen0}");
                sb.AppendLine($".NET GC Gen1: {_dotNetGen1}");
                sb.AppendLine($".NET GC Gen2: {_dotNetGen2}");
                return sb.ToString();
            }

            private void OnCollect(IntPtr state)
            {
                _chakraCollections++;
            }
        } 
    }
}
