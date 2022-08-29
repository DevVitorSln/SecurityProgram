using SecurityProgram.Enums;
using System.Text.RegularExpressions;

namespace SecurityProgram.Funcoes
{
    public static class MenuUsuario
    {

        private static string _menu;

        private static string _menuLogin;

        private static string _messagemSelecionOpcao = "\nInforme uma opção: ";

        static MenuUsuario()
        {
            _menu = "[OPÇÕES]";

            foreach (var value in Enum.GetValues(typeof(OpcoesUsuario)))
            {
                string enumName = Enum.GetName(typeof(OpcoesUsuario), value);

                var names = Regex.Split(enumName, @"(?<!^)(?=[A-Z]|[0-9])");

                enumName = string.Empty;

                foreach (var name in names)
                {
                    enumName += $"{name} ";
                }

                _menu += $"\n{(int)value}.{enumName}";

            }

            _menuLogin = "[OPÇÕES]";

            foreach (var valueLogado in Enum.GetValues(typeof(OpcoesUsuarioLogado)))
            {
                string enumNameLogado = Enum.GetName(typeof(OpcoesUsuarioLogado), valueLogado);

                var namesLogado = Regex.Split(enumNameLogado, @"(?<!^)(?=[A-Z]|[0-9])");

                enumNameLogado = string.Empty;

                foreach (var nameLogado in namesLogado)
                {
                    enumNameLogado += $"{nameLogado} ";
                }

                _menuLogin += $"\n{(int)valueLogado}.{enumNameLogado}";
            }
        }

        public static OpcoesUsuario ReceberOpcaoUsuario()
        {
            Console.Clear();
            ImprimirMensagem($"{_menu}\n{_messagemSelecionOpcao}");

            string opcao = ReceberValorInserido();
            if (string.IsNullOrWhiteSpace(opcao))
            {
                throw new ArgumentNullException("\n[ERROR]: Informe uma opção.");
            }

            int nroOpcao;

            var opcaoIntValida = Int32.TryParse(opcao, out nroOpcao);

            if (opcaoIntValida)
            {
                bool enumValido = Enum.IsDefined(typeof(OpcoesUsuario), nroOpcao);

                if (enumValido)
                    return (OpcoesUsuario)nroOpcao;
            }

            throw new ArgumentException("\nInforme uma opção válida.");
        }
        public static OpcoesUsuarioLogado ReceberOpcaoUsuarioLogado()
        {
            Console.Clear();
            ImprimirMensagem($"{_menuLogin}\n{_messagemSelecionOpcao}");

            string opcao = ReceberValorInserido();

            int nroOpcao;

            bool opcaoIntValida = Int32.TryParse(opcao, out nroOpcao);

            OpcoesUsuarioLogado enumOpcoesUsuarioLogado = (OpcoesUsuarioLogado)nroOpcao;

            return enumOpcoesUsuarioLogado;
        }
        public static string ReceberValorInserido()
        {
            return Console.ReadLine();
        }

        public static void ImprimirErro(string mensagem)
        {
            Console.WriteLine($"\n\n[ERROR] {mensagem}");
        }
        public static void ImprimirMensagem(string mensagem)
        {
            Console.Write($"{mensagem}");
        }
        public static void Continuar(string mensagem = null)
        {
            if (mensagem == null)
            {
                ImprimirMensagem("\nAperte uma tecla para voltar ao menu...");
            }
            else
            {
                ImprimirMensagem($"{mensagem}");
            }

            Console.ReadKey();
        }
    }
}