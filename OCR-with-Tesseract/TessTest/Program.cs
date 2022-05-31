using System.Text;
using TesseractOCR;
using TesseractOCR.Enums;
using TesseractOCR.Pix;

namespace TessTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            using var engine = new Engine(@"./tessdata", Language.Bulgarian, EngineMode.Default);
            var img = Image.LoadFromFile(@"C:\Users\Nikolay.IT\Downloads\test.JPG");
            // img = img.ConvertRGBToGray();
            using var page = engine.Process(img);
            Console.WriteLine("Mean confidence: {0}", page.MeanConfidence);
            Console.WriteLine("Text: \r\n{0}", page.Text);
            // Console.WriteLine(ToString(page));
        }

        static string ToString(Page page)
        {
            var result = new StringBuilder();
            foreach (var block in page.Layout)
            {
                result.AppendLine($"Block confidence: {block.Confidence}");
                if (block.BoundingBox != null)
                {
                    var boundingBox = block.BoundingBox.Value;
                    result.AppendLine($"Block bounding box X1 '{boundingBox.X1}', Y1 '{boundingBox.Y2}', X2 " +
                                      $"'{boundingBox.X2}', Y2 '{boundingBox.Y2}', width '{boundingBox.Width}', height '{boundingBox.Height}'");
                }
                result.AppendLine($"Block text: {block.Text}");

                foreach (var paragraph in block.Paragraphs)
                {
                    result.AppendLine($"Paragraph confidence: {paragraph.Confidence}");
                    if (paragraph.BoundingBox != null)
                    {
                        var boundingBox = paragraph.BoundingBox.Value;
                        result.AppendLine($"Paragraph bounding box X1 '{boundingBox.X1}', Y1 '{boundingBox.Y2}', X2 " +
                                          $"'{boundingBox.X2}', Y2 '{boundingBox.Y2}', width '{boundingBox.Width}', height '{boundingBox.Height}'");
                    }
                    var info = paragraph.Info;
                    result.AppendLine($"Paragraph info justification: {info.Justification}");
                    result.AppendLine($"Paragraph info is list item: {info.IsListItem}");
                    result.AppendLine($"Paragraph info is crown: {info.IsCrown}");
                    result.AppendLine($"Paragraph info first line ident: {info.FirstLineIdent}");
                    result.AppendLine($"Paragraph text: {paragraph.Text}");

                    foreach (var textLine in paragraph.TextLines)
                    {
                        if (textLine.BoundingBox != null)
                        {
                            var boundingBox = textLine.BoundingBox.Value;
                            result.AppendLine($"Text line bounding box X1 '{boundingBox.X1}', Y1 '{boundingBox.Y2}', X2 " +
                                              $"'{boundingBox.X2}', Y2 '{boundingBox.Y2}', width '{boundingBox.Width}', height '{boundingBox.Height}'");
                        }
                        result.AppendLine($"Text line confidence: {textLine.Confidence}");
                        result.AppendLine($"Text line text: {textLine.Text}");

                        foreach (var word in textLine.Words)
                        {
                            result.AppendLine($"Word confidence: {word.Confidence}");
                            if (word.BoundingBox != null)
                            {
                                var boundingBox = word.BoundingBox.Value;
                                result.AppendLine($"Word bounding box X1 '{boundingBox.X1}', Y1 '{boundingBox.Y2}', X2 " +
                                                  $"'{boundingBox.X2}', Y2 '{boundingBox.Y2}', width '{boundingBox.Width}', height '{boundingBox.Height}'");
                            }
                            result.AppendLine($"Word is from dictionary: {word.IsFromDictionary}");
                            result.AppendLine($"Word is numeric: {word.IsNumeric}");
                            result.AppendLine($"Word language: {word.Language}");
                            result.AppendLine($"Word text: {word.Text}");

                            foreach (var symbol in word.Symbols)
                            {
                                result.AppendLine($"Symbol confidence: {symbol.Confidence}");
                                if (symbol.BoundingBox != null)
                                {
                                    var boundingBox = symbol.BoundingBox.Value;
                                    result.AppendLine($"Symbol bounding box X1 '{boundingBox.X1}', Y1 '{boundingBox.Y2}', X2 " +
                                                      $"'{boundingBox.X2}', Y2 '{boundingBox.Y2}', width '{boundingBox.Width}', height '{boundingBox.Height}'");
                                }
                                result.AppendLine($"Symbol is superscript: {symbol.IsSuperscript}");
                                result.AppendLine($"Symbol is dropcap: {symbol.IsDropcap}");
                                result.AppendLine($"Symbol text: {symbol.Text}");
                            }
                        }
                    }
                }
            }

            return result.ToString();
        }
    }
}
