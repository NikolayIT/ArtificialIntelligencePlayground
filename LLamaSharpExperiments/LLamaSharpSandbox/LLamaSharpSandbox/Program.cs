using LLama.OldVersion;

namespace LLamaSharpSandbox
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var model = new LLamaModel(new LLamaParams(
                model: Path.Combine(@"K:\AI Models\pytorch_model-00001-of-00007.bin"),
                n_ctx: 512,
                interactive: true,
                repeat_penalty: 1.0f,
                temp: 0.4f,
                n_gpu_layers: 24,
                verbose_prompt: false));

            var session = new ChatSession<LLamaModel>(model)
    .WithPrompt("""
        You are helpful assistant. You know everything about everything.

        User:
        """)
    .WithAntiprompt(new[] { "User: " });

            Console.WriteLine();
            Console.Write("User: ");
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                var prompt = Console.ReadLine() + "\n";

                Console.ForegroundColor = ConsoleColor.White;
                foreach (var output in session.Chat(prompt, encoding: "UTF-8"))
                {
                    Console.Write(output);
                }
            }
        }
    }
}
