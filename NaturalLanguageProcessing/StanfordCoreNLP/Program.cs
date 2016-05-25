namespace NLP
{
    using System;
    using System.IO;

    using edu.stanford.nlp.pipeline;

    using java.io;
    using java.util;

    using Console = System.Console;

    public static class Program
    {
        public static void Main()
        {
            // Path to the folder with models extracted from `stanford-corenlp-3.6.0-models.jar`
            var jarRoot = @"data";

            // Text for processing
            var text = "Kosgi Santosh sent an email to Stanford University. He didn't get a reply.";
            // var text = "The following transaction was received from Admin entered on 04/30/2016 at 11:31 PM CDT and filed on 04/28/2016 ";

            // Annotation pipeline configuration
            var props = new Properties();
            props.setProperty("annotators", "tokenize, ssplit, pos, lemma, ner, parse, dcoref");
            props.setProperty("ner.useSUTime", "0");

            // We should change current directory, so StanfordCoreNLP could find all the model files automatically
            var curDir = Environment.CurrentDirectory;
            Directory.SetCurrentDirectory(jarRoot);
            var pipeline = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(curDir);

            // Annotation
            var annotation = new Annotation(text);
            pipeline.annotate(annotation);

            // Result - Pretty Print
            using (var stream = new ByteArrayOutputStream())
            {
                pipeline.prettyPrint(annotation, new PrintWriter(stream));
                Console.WriteLine(stream.toString());
                stream.close();
            }
        }
    }
}
