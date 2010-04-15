using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Globalization;
using SourceAFIS.Simple;

namespace FvcMatch
{
    class Match
    {
        static Person LoadTemplate(string path)
        {
            Fingerprint fp = new Fingerprint();
            fp.Template = File.ReadAllBytes(path);
            Person person = new Person();
            person.Add(fp);
            return person;
        }

        static void WriteLog(string[] args, string status, float similarity)
        {
            using (FileStream stream = File.Open(args[2], FileMode.Append))
            {
                using (TextWriter writer = new StreamWriter(stream))
                    writer.WriteLine("{0} {1} {2} {3:F5}", args[0], args[1], status, similarity);
            }
        }

        static void Main(string[] args)
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                AfisEngine afis = new AfisEngine();
                Person probe = LoadTemplate(args[0]);
                Person candidate = LoadTemplate(args[1]);
                afis.Threshold = 0;
                float score = afis.Verify(probe, candidate);
                float similarity = score / 100;
                WriteLog(args, "OK", similarity);
            }
            catch (Exception)
            {
                WriteLog(args, "FAIL", 0);
            }
        }
    }
}