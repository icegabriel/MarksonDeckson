using System;
using System.IO;

namespace MarksonDeckson.Utils
{
    public static class ExceptionLog
    {
        public static void Write(Exception e)
        {
            using (var file = File.Open("log.txt", FileMode.OpenOrCreate))
            using (var writer = new StreamWriter(file))
            {
                writer.WriteLine($"Message: {e.Message}\nStackTrace: {e.StackTrace}\n\n\n");
            }
        }
    }
}
