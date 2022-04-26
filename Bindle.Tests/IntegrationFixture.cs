using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Xunit;

namespace Deislabs.Bindle.Tests
{
    public class IntegrationFixture : IDisposable
    {
        private readonly Process process;
        private readonly string dataPath;
        private readonly string testDataCopy;

        // private readonly string[] testDirs;
        public IntegrationFixture()
        {
            var bindlePath = Environment.GetEnvironmentVariable("BINDLE_SERVER_PATH");
            if (!string.IsNullOrEmpty(bindlePath))
            {
                var fullPath = Path.GetFullPath(Path.Join(bindlePath, "bindle-server"));
                dataPath = Path.GetFullPath(Path.Join("../../../data"));
                Console.WriteLine($"Using Bindle Server Path: {fullPath}");
                Console.WriteLine($"Using Test Data Path: {dataPath}");

                // Take a copy of the test data to be used by the tests
                testDataCopy = $"{Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())}/test-data-copy";
                Console.WriteLine($"Copying Test Data to: {testDataCopy}");
                CopyDirectory(dataPath, testDataCopy);

                var psi = new ProcessStartInfo
                {
                    FileName = fullPath,
                    Arguments = $"--unauthenticated -i 127.0.0.1:14044 -d {dataPath}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };
                psi.Environment["RUST_LOG"] = "info";
                process = new Process
                {
                    StartInfo = psi
                };
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine(e.Data);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine(e.Data);
                    }
                };
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                // TODO: something a bit less crappy that this to wait for server to start:
                Thread.Sleep(5000);
                Assert.False(process.HasExited);             
            }
        }

        public void Dispose()
        {
            // Stop the bindle server
            try
            {
                process?.Kill();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to stop bindle process:{ex}");
            }

            // Revert the modifications to test data

            var testOutput =  $"{Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())}/test-output";
            Console.WriteLine($"Copying Test Output to: {testOutput}");

            // Copy the output of the test to the temp directory and then delete it and replace with the original, then delete the copies

            CopyDirectory(dataPath, testOutput); 
            Directory.Delete(dataPath, true);  
            CopyDirectory(testDataCopy, dataPath);
            Directory.Delete(testDataCopy, true);
            Directory.Delete(testOutput, true);
        }

        private void CopyDirectory(string source, string dest)
        {
             Directory.CreateDirectory(dest);
             foreach (var file in Directory.GetFiles(source))
             {
                 File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), true);
             }

             foreach (var dir in Directory.GetDirectories(source))
             {
                 CopyDirectory(dir, Path.Combine(dest, Path.GetFileName(dir)));
             }
        }
    }
}